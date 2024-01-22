using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using UnityEngine;

public class PBD : MonoBehaviour
{
    public Material displayMaterial;
    public Texture texture0;

    public TetModel Sphere;
    /// <summary>
    /// 触地惩罚力度的系数
    /// </summary>
    public float penaltyK = 100f;
    /// <summary>
    /// 速度阻尼的系数
    /// </summary>
    public float dampingK = 0.1f;

    private ComputeBuffer nodePosBuffer;
    private ComputeBuffer faceIdxBuffer;
    private ComputeBuffer faceNormalBuffer;
    private ComputeBuffer faceUvBuffer;

    /// <summary>
    /// 节点质量
    /// </summary>
    public float m = 1f;
    /// <summary>
    /// 重力加速度
    /// </summary>
    public float g = 9.8f;
    /// <summary>
    /// 时间步长
    /// </summary>
    public float dt = 0.04f;
    /// <summary>
    /// 迭代次数
    /// </summary>
    public int iterations = 10;

    public int nodeToPrint = 120;

    private Hashtable collisionVelocityTable;

    public bool pointForceEnable = false;

    public int pointAtoAddForce = 50;
    public int pointBtoAddForce = 100;

    public Vector3 forceAddtoPintAB;

    // Start is called before the first frame update
    void Start()
    {
        Sphere = new TetModel("sphereMed", Application.dataPath + "/TetModel", transform.position);
        Sphere.Load();

        displayMaterial.SetTexture("Texture", texture0);

        #region 新建ComputeBuffer并设置数据
        nodePosBuffer = new ComputeBuffer(Sphere.NodeNum * 3, Marshal.SizeOf(typeof(float)));
        nodePosBuffer.SetData(Sphere.NodePos);

        faceIdxBuffer = new ComputeBuffer(Sphere.FaceNum * 3, Marshal.SizeOf(typeof(int)));
        faceIdxBuffer.SetData(Sphere.FaceIdx);

        faceNormalBuffer = new ComputeBuffer(Sphere.FaceNum, Marshal.SizeOf(typeof(Vector3)));
        faceNormalBuffer.SetData(Sphere.FaceNormal);

        faceUvBuffer = new ComputeBuffer(Sphere.FaceNum * 6, Marshal.SizeOf(typeof(float)));
        faceUvBuffer.SetData(Sphere.FaceUv);

        #endregion

        displayMaterial.SetBuffer("_vertices", nodePosBuffer);
        displayMaterial.SetBuffer("_idx", faceIdxBuffer);
        displayMaterial.SetBuffer("_normal", faceNormalBuffer);
        displayMaterial.SetBuffer("_uvs", faceUvBuffer);

        collisionVelocityTable = new Hashtable();
    }

    // Update is called once per frame
    void Update()
    {
        //添加点力
        if (pointForceEnable)
            AddForceToAB();
        //速度预测
        VelocityPredict();
        //速度阻尼
        dampingVelocity();
        //清空力
        ClearForce();
        //位置预测
        PositionPredict();

        //施加约束
        SolveConstraint();

        //更新位置
        UpdateVelocity();

        //更新位置
        UpdatePosition();
    }

    void SolveConstraint()
    {
        //施加位置和体积约束

        float k = 0.8f;
        k = 1 - Mathf.Pow(1 - k, 1f / iterations);
        //Debug.Log("转化后刚度：" + k);
        for (int i = 1; i <= iterations; i++)
        {
            Debug.Log("============第" + i + "轮==============");
            for (int j = 0; j < Sphere.EdgeNum; j++)
            {
                ConstrainPositions(j, k);

            }
            for (int j = 0; j < Sphere.EleNum; j++)
            {
                ConstrainVolume(j, k);
            }
        }
        CollisionConstraints();
    }

    void AddForceToAB()
    {
        Sphere.NodeForce[pointAtoAddForce * 3 + 0] = forceAddtoPintAB.x;
        Sphere.NodeForce[pointAtoAddForce * 3 + 1] = forceAddtoPintAB.y;
        Sphere.NodeForce[pointAtoAddForce * 3 + 2] = forceAddtoPintAB.z;

        Sphere.NodeForce[pointBtoAddForce * 3 + 0] = -forceAddtoPintAB.x;
        Sphere.NodeForce[pointBtoAddForce * 3 + 1] = -forceAddtoPintAB.y;
        Sphere.NodeForce[pointBtoAddForce * 3 + 2] = -forceAddtoPintAB.z;
    }

    void VelocityPredict()
    {
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            Sphere.NodeVel[i * 3 + 0] += Sphere.NodeForce[i * 3 + 0] / 1 * dt;
            Sphere.NodeVel[i * 3 + 1] += (Sphere.NodeForce[i * 3 + 1] - g) / 1 * dt;
            Sphere.NodeVel[i * 3 + 2] += Sphere.NodeForce[i * 3 + 2] / 1 * dt;
        }
    }

    void dampingVelocity()
    {
        float mI = 1f;
        float mSum = 0f;
        Vector3 xCM = Vector3.zero;
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            xCM.x += Sphere.NodePos[i * 3 + 0] * mI;
            xCM.y += Sphere.NodePos[i * 3 + 1] * mI;
            xCM.z += Sphere.NodePos[i * 3 + 2] * mI;

            mSum += mI;
        }

        xCM /= mSum;

        Vector3 vCM = Vector3.zero;
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            vCM.x += Sphere.NodeVel[i * 3 + 0] * mI;
            vCM.y += Sphere.NodeVel[i * 3 + 1] * mI;
            vCM.z += Sphere.NodeVel[i * 3 + 2] * mI;
        }

        vCM /= mSum;

        Vector3 L = Vector3.zero;
        Matrix4x4 I = Matrix4x4.zero;
        Vector3[] r = new Vector3[Sphere.NodeNum];

        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            r[i] = PBDUtil.NodeIdxToVector3(Sphere, i) - xCM;
            L += Vector3.Cross(r[i], PBDUtil.NodeIdxToVelVector3(Sphere, i) * mI);
            Matrix4x4 rIMat = new Matrix4x4();
            rIMat.SetRow(0, new Vector4(0, -r[i].z, r[i].y, 0));
            rIMat.SetRow(1, new Vector4(r[i].z, 0, -r[i].x, 0));
            rIMat.SetRow(2, new Vector4(-r[i].y, r[i].x, 0, 0));
            rIMat.SetRow(3, new Vector4(0, 0, 0, 0));

            if (i == nodeToPrint)
            {
                Debug.Log("顶点编号" + i + "速度阻尼中间变量：r[i]:\n" + r[i].ToString("E7") + "rIMat:\n" + rIMat.ToString("E7"));
            }

            I = PBDUtil.MatrixAdd(I, PBDUtil.MatrixMultiple((rIMat * rIMat.transpose), mI));
        }

        //满秩矩阵才可逆
        I[3, 3] = 1;

        Vector3 omega = I.inverse.MultiplyPoint3x4(L);
        //Debug.Log("速度阻尼" + "\nI:\n" + I.ToString("E7") + "I-1:\n" + I.inverse.ToString("E7") + "L:\n" + L.ToString("E7") + "omega:\n" + omega.ToString("E7"));

        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            //修改速度
            CorrectVelocity(i, dampingK * (vCM + Vector3.Cross(omega, r[i]) - PBDUtil.NodeIdxToVelVector3(Sphere, i)));
        }
    }

    void ClearForce()
    {
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            Sphere.NodeForce[i * 3 + 0] = 0;
            Sphere.NodeForce[i * 3 + 1] = 0;
            Sphere.NodeForce[i * 3 + 2] = 0;
        }
    }

    void PositionPredict()
    {
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            Sphere.NewNodePos[i * 3] = Sphere.NodePos[i * 3] + Sphere.NodeVel[i * 3] * dt;
            Sphere.NewNodePos[i * 3 + 1] = Sphere.NodePos[i * 3 + 1] + Sphere.NodeVel[i * 3 + 1] * dt;
            Sphere.NewNodePos[i * 3 + 2] = Sphere.NodePos[i * 3 + 2] + Sphere.NodeVel[i * 3 + 2] * dt;
        }
    }

    /// <summary>
    /// 生成碰撞约束，暂时未实现，先处理成触地反弹
    /// </summary>
    void CollisionConstraints()
    {
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            if (Sphere.NewNodePos[i * 3 + 1] < 0)
            {
                //if (nodeToPrint == i)
                //Debug.Log("触地反弹：顶点编号：" + i + "上步位置:" + Sphere.NodePos[i * 3 + 1] + "预测位置：" + Sphere.NewNodePos[i * 3 + 1]);

                //抬升入地部分，造成动量不守恒
                Sphere.NewNodePos[i * 3 + 1] = -Sphere.NewNodePos[i * 3 + 1];

                //整体抬升，造成刚性，且抖动
                //float depth = -Sphere.NewNodePos[i * 3 + 1];
                //for (int j = 0; j < Sphere.NodeNum; j++)
                //{
                //    Sphere.NewNodePos[j * 3 + 1] += 2 * depth;
                //}
                if (Sphere.NodeVel[i * 3 + 1] < 0)
                    collisionVelocityTable.Add(i, -Sphere.NodeVel[i * 3 + 1]);
            }

            //Sphere.NodeVel[i * 3 + 1] = -Sphere.NodeVel[i * 3 + 1];
            //Sphere.NodeForce[i * 3 + 1] = -Sphere.NewNodePos[i * 3 + 1] * penaltyK;

        }
    }

    void ConstrainPositions(int edgeIndex, float k)
    {
        int nodeIA = Sphere.EdgeIdx[edgeIndex * 2 + 0];
        int nodeIB = Sphere.EdgeIdx[edgeIndex * 2 + 1];

        //顶点质量的倒数
        float wA = 1;
        float wB = 1;

        Vector3 nodeA = new Vector3(Sphere.NewNodePos[nodeIA * 3], Sphere.NewNodePos[nodeIA * 3 + 1], Sphere.NewNodePos[nodeIA * 3 + 2]);
        Vector3 nodeB = new Vector3(Sphere.NewNodePos[nodeIB * 3], Sphere.NewNodePos[nodeIB * 3 + 1], Sphere.NewNodePos[nodeIB * 3 + 2]);
        //两点构成的向量
        Vector3 n = nodeB - nodeA;
        //存储两点当前距离
        float curDistance = n.magnitude;
        //归一化
        n.Normalize();

        Vector3 corr = k * n * (curDistance - Sphere.Distance[edgeIndex]) / (wA + wB);

        Vector3 deltaA = wA * corr;
        Vector3 deltaB = -wB * corr;

        //Debug.Log("edgeIndex:" + edgeIndex + "当前距离：" + curDistance + "初始距离：" + Sphere.Distance[edgeIndex] + "距离差：" + (curDistance - Sphere.Distance[edgeIndex]) + "\n纠正向量：(" + corr.x + "," + corr.y + "," + corr.z + ")\ndeltaA:" + deltaA + "  deltaB" + deltaB + "\n距离向量：(" + n.x + "," + n.y + "," + n.z + ")\n刚度k：" + k);

        //if (corr.magnitude > 0.0001f)
        //Debug.Log("距离约束：\n纠正向量：" + corr + "\ndeltaA:" + deltaA + "deltaB" + deltaB);

        CorrectNewPosition(nodeIA, deltaA);
        CorrectNewPosition(nodeIB, deltaB);
    }

    void ConstrainVolume(int eleIndex, float k)
    {
        //int nodeIA = Sphere.EleIdx[eleIndex * 4 + 0];
        //int nodeIB = Sphere.EleIdx[eleIndex * 4 + 1];
        //int nodeIC = Sphere.EleIdx[eleIndex * 4 + 2];
        //int nodeID = Sphere.EleIdx[eleIndex * 4 + 3];

        float wA = 1;
        float wB = 1;
        float wC = 1;
        float wD = 1;

        Vector3 nodeA;
        Vector3 nodeB;
        Vector3 nodeC;
        Vector3 nodeD;

        //当前体积
        float curVolume = PBDUtil.GetNewElePosAndCalculateVolume(Sphere, eleIndex, out nodeA, out nodeB, out nodeC, out nodeD);

        Vector3 gradA = Vector3.Cross(nodeD - nodeB, nodeC - nodeB);
        Vector3 gradB = Vector3.Cross(nodeC - nodeA, nodeD - nodeA);
        Vector3 gradC = Vector3.Cross(nodeD - nodeA, nodeB - nodeA);
        Vector3 gradD = Vector3.Cross(nodeB - nodeA, nodeC - nodeA);

        float corr = -k * 6 * (curVolume - Sphere.Volume[eleIndex]) / (wA * gradA.sqrMagnitude + wB * gradB.sqrMagnitude + wC * gradC.sqrMagnitude + wD * gradD.sqrMagnitude);

        Vector3 deltaA = corr * wA * gradA;
        Vector3 deltaB = corr * wB * gradB;
        Vector3 deltaC = corr * wC * gradC;
        Vector3 deltaD = corr * wD * gradD;

        //if (corr > 0.0001f)

        //if (343 == eleIndex)
        //Debug.Log("体积约束：\n" + "当前体积：" + curVolume + "初始体积：" + Sphere.Volume[eleIndex] + "体积差：" + (curVolume - Sphere.Volume[eleIndex]) + "纠正值：" + corr + "\ndeltaA:(" + deltaA.x + "," + deltaA.y + "," + deltaA.z + "\ndeltaB:(" + deltaB.x + "," + deltaB.y + "," + deltaB.z + "\ndeltaC:(" + deltaC.x + "," + deltaC.y + "," + deltaC.z + ")\ndeltaD:(" + deltaD.x + "," + deltaD.y + "," + deltaD.z + ")");

        CorrectNewPosition(Sphere.EleIdx[eleIndex * 4 + 0], corr * wA * gradA);
        CorrectNewPosition(Sphere.EleIdx[eleIndex * 4 + 1], corr * wB * gradB);
        CorrectNewPosition(Sphere.EleIdx[eleIndex * 4 + 2], corr * wC * gradC);
        CorrectNewPosition(Sphere.EleIdx[eleIndex * 4 + 3], corr * wD * gradD);

    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void CorrectNewPosition(int nodeIdex, Vector3 delta)
    {
        if (nodeToPrint == nodeIdex)
        {
            Vector3 newNodePos = new Vector3(Sphere.NewNodePos[nodeIdex * 3 + 0], Sphere.NewNodePos[nodeIdex * 3 + 1], Sphere.NewNodePos[nodeIdex * 3 + 2]);
            Debug.Log("预测位置的修改：顶点编号：" + nodeIdex + "顶点原位置：" + newNodePos.ToString("E6") + "修改后顶点的位置：" + (newNodePos + delta).ToString("E6") + "" + "\n调用堆栈：\n" + new System.Diagnostics.StackTrace().ToString());
        }
        Sphere.NewNodePos[nodeIdex * 3 + 0] += delta.x;
        Sphere.NewNodePos[nodeIdex * 3 + 1] += delta.y;
        Sphere.NewNodePos[nodeIdex * 3 + 2] += delta.z;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void CorrectVelocity(int nodeIdex, Vector3 delta)
    {
        Sphere.NodeVel[nodeIdex * 3 + 0] += delta.x;
        Sphere.NodeVel[nodeIdex * 3 + 1] += delta.y;
        Sphere.NodeVel[nodeIdex * 3 + 2] += delta.z;
    }

    void UpdateVelocity()
    {
        //利用位置更新速度
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            Sphere.NodeVel[i * 3 + 0] = (Sphere.NewNodePos[i * 3 + 0] - Sphere.NodePos[i * 3 + 0]) / dt;
            if (collisionVelocityTable.ContainsKey(i))
            {
                Sphere.NodeVel[i * 3 + 1] = (float)collisionVelocityTable[i];
                collisionVelocityTable.Remove(i);
            }
            else
            {
                Sphere.NodeVel[i * 3 + 1] = (Sphere.NewNodePos[i * 3 + 1] - Sphere.NodePos[i * 3 + 1]) / dt;
            }
            Sphere.NodeVel[i * 3 + 2] = (Sphere.NewNodePos[i * 3 + 2] - Sphere.NodePos[i * 3 + 2]) / dt;
        }
    }

    void UpdatePosition()
    {
        //更新位置
        for (int i = 0; i < Sphere.NodeNum; i++)
        {
            Sphere.NodePos[i * 3 + 0] = Sphere.NewNodePos[i * 3 + 0];
            Sphere.NodePos[i * 3 + 1] = Sphere.NewNodePos[i * 3 + 1];
            Sphere.NodePos[i * 3 + 2] = Sphere.NewNodePos[i * 3 + 2];
        }
    }

    void PassDataToRenderObject()
    {
        #region 给ComputeBuffer传入新数据，以供渲染
        nodePosBuffer.SetData(Sphere.NodePos);
        faceIdxBuffer.SetData(Sphere.FaceIdx);
        //计算表面法向
        Sphere.CalculateFaceNormal();
        faceNormalBuffer.SetData(Sphere.FaceNormal);
        faceUvBuffer.SetData(Sphere.FaceUv);
        #endregion
    }

    //是在摄像机渲染场景后，使用 displayMaterial 材质渲染一个由程序生成的三角形网格。
    private void OnRenderObject()
    {
        PassDataToRenderObject();
        displayMaterial.SetPass(0);
        Graphics.DrawProceduralNow(MeshTopology.Triangles, Sphere.FaceNum * 3, 1);
    }

    private void OnDestroy()
    {
        nodePosBuffer.Release();
        faceIdxBuffer.Release();
        faceNormalBuffer.Release();
        faceUvBuffer.Release();
    }
}
