using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TetModel
{

    #region 文件和路径相关
    /// <summary>
    /// 待读取文件名字，无后缀
    /// </summary>
    private string fileName;
    /// <summary>
    /// 模型文件无后缀名字
    /// </summary>
    public string FileName { get { return fileName; } set { fileName = value; } }
    /// <summary>
    /// 模型文件路径变量
    /// </summary>
    private string dataPath;
    /// <summary>
    /// 模型文件路径属性
    /// </summary>
    public string DataPath { get { return dataPath; } set { dataPath = value; } }
    /// <summary>
    /// 路径+顶点文件名带后缀
    /// </summary>
    public string NodeFileName { get { return DataPath + "/" + FileName + ".node"; } }
    /// <summary>
    /// 路径+顶点文件名带后缀
    /// </summary>
    public string EleFileName { get { return DataPath + "/" + FileName + ".ele"; } }
    /// <summary>
    /// 路径+表面文件名带后缀
    /// </summary>
    public string FaceFileName { get { return DataPath + "/" + FileName + ".face"; } }
    /// <summary>
    /// 路径+表面文件名带后缀
    /// </summary>
    public string EdgeFileName { get { return DataPath + "/" + FileName + ".edge"; } }
    #endregion

    #region 顶点直接相关
    /// <summary>
    /// 顶点数量变量
    /// </summary>
    private int nodeNum;
    /// <summary>
    /// 顶点数量属性
    /// </summary>
    public int NodeNum { get { return nodeNum; } set { nodeNum = value; } }
    /// <summary>
    /// 顶点位置数组变量
    /// </summary>
    private float[] nodePos;
    /// <summary>
    /// 顶点位置数组属性
    /// </summary>
    public float[] NodePos { get { return nodePos; } set { nodePos = value; } }

    #endregion

    #region 四面体相关
    /// <summary>
    /// 四面体数量变量
    /// </summary>
    private int eleNum;
    /// <summary>
    /// 四面体数量属性
    /// </summary>
    public int EleNum { get { return eleNum; } set { eleNum = value; } }
    /// <summary>
    /// 四面体元素索引数组变量
    /// </summary>
    private int[] eleIdx;
    /// <summary>
    /// 四面体元素索引数组属性
    /// </summary>
    public int[] EleIdx { get { return eleIdx; } set { eleIdx = value; } }
    /// <summary>
    /// 四面体初始体积变量
    /// </summary>
    private float[] volume;
    /// <summary>
    /// 四面体初始体积属性
    /// </summary>
    public float[] Volume { get { return volume; } set { volume = value; } }

    #endregion

    #region 表面相关
    /// <summary>
    /// 表面三角形数量变量
    /// </summary>
    private int faceNum;
    /// <summary>
    /// 表面三角形数量属性
    /// </summary>
    public int FaceNum { get { return faceNum; } set { faceNum = value; } }
    /// <summary>
    /// 表面三角形索引数组变量
    /// </summary>
    private int[] faceIdx;
    /// <summary>
    /// 表面三角形索引数组属性
    /// </summary>
    public int[] FaceIdx { get { return faceIdx; } set { faceIdx = value; } }
    #endregion

    #region 边相关

    /// <summary>
    /// 边数量变量
    /// </summary>
    private int edgeNum;
    /// <summary>
    /// 边数量属性
    /// </summary>
    public int EdgeNum { get { return edgeNum; } set { edgeNum = value; } }
    /// <summary>
    /// 边索引数组变量
    /// </summary>
    private int[] edgeIdx;
    /// <summary>
    /// 边索引数组属性
    /// </summary>
    public int[] EdgeIdx { get { return edgeIdx; } set { edgeIdx = value; } }

    /// <summary>
    /// 每一条边的两个点的初始距离变量
    /// </summary>
    private float[] distance;
    /// <summary>
    /// 每一条边的两个点的初始距离属性
    /// </summary>
    public float[] Distance { get { return distance; } set { distance = value; } }

    #endregion

    /// <summary>
    /// 基准位置变量
    /// </summary>
    private Vector3 basePosition;
    /// <summary>
    /// 基准位置属性
    /// </summary>
    public Vector3 BasePosition { get { return basePosition; } set { basePosition = value; } }

    #region 物理量
    /// <summary>
    /// 顶点速度变量
    /// </summary>
    private float[] nodeVel;
    /// <summary>
    /// 顶点速度属性
    /// </summary>
    public float[] NodeVel { get { return nodeVel; } set { nodeVel = value; } }

    /// <summary>
    /// 顶点新位置变量
    /// </summary>
    private float[] newNodePos;
    /// <summary>
    /// 顶点新位置属性
    /// </summary>
    public float[] NewNodePos { get { return newNodePos; } set { newNodePos = value; } }

    /// <summary>
    /// 顶点力变量
    /// </summary>
    private float[] nodeForce;
    /// <summary>
    /// 顶点力属性
    /// </summary>
    public float[] NodeForce { get { return nodeForce; } set { nodeForce = value; } }

    #endregion

    #region 渲染相关
    /// <summary>
    /// 表面法线变量
    /// </summary>
    private Vector3[] faceNormal;
    /// <summary>
    /// 表面法线属性
    /// </summary>
    public Vector3[] FaceNormal { get { return faceNormal; } set { faceNormal = value; } }
    /// <summary>
    /// 表面UV坐标变量
    /// </summary>
    private float[] faceUv;
    /// <summary>
    /// 表面UV坐标属性
    /// </summary>
    public float[] FaceUv { get { return faceUv; } set { faceUv = value; } }
    #endregion

    /// <summary>
    /// 模型构造方法
    /// </summary>
    /// <param name="fileName">文件名，无后缀</param>
    /// <param name="dataPath">文件路径</param>
    /// <param name="basePosition">基准位置</param>
    public TetModel(string fileName, string dataPath, Vector3 basePosition)
    {
        FileName = fileName;
        DataPath = dataPath;
        BasePosition = basePosition;
    }


    /// <summary>
    /// 读取三种模型文件
    /// </summary>
    public void Load()
    {
        TetLoader.ReadNodeFile(NodeFileName, out nodeNum, out nodePos, basePosition);
        TetLoader.ReadEleFile(EleFileName, out eleNum, out eleIdx);
        TetLoader.ReadFaceFile(FaceFileName, out faceNum, out faceIdx);
        TetLoader.ReadEdgeFile(EdgeFileName, out edgeNum, out edgeIdx);
        CreateAllPhysicalArray();
    }

    public void CreateAllPhysicalArray()
    {
        NewNodePos = new float[NodeNum * 3];

        NodeVel = new float[NodeNum * 3];
        NodeForce = new float[NodeNum * 3];

        FaceNormal = new Vector3[FaceNum];
        CalculateFaceNormal();
        FaceUv = new float[FaceNum * 3 * 2];
        for (int i = 0; i < FaceNum; i++)
        {
            FaceUv[i * 6 + 0] = 0;
            FaceUv[i * 6 + 1] = 0;
            FaceUv[i * 6 + 2] = 1;
            FaceUv[i * 6 + 3] = 0;
            FaceUv[i * 6 + 4] = 0;
            FaceUv[i * 6 + 5] = 1;
        }

        //开辟边的初始长度数组空间
        Distance = new float[EdgeNum];
        for (int i = 0; i < EdgeNum; i++)
        {
            Distance[i] = CalculateDistance(EdgeIdx[i * 2], EdgeIdx[i * 2 + 1]);
        }

        //计算四面体初始体积
        Volume = new float[EleNum];
        for (int i = 0; i < EleNum; i++)
        {
            Volume[i] = PBDUtil.CalculateVolume(this, i);
        }
    }


    public void CalculateFaceNormal()
    {
        for (int i = 0; i < FaceNum; i++)
        {
            Vector3 p0 = new Vector3(NodePos[FaceIdx[i * 3 + 0] * 3 + 0], NodePos[FaceIdx[i * 3 + 0] * 3 + 1], NodePos[FaceIdx[i * 3 + 0] * 3 + 2]);
            Vector3 p1 = new Vector3(NodePos[FaceIdx[i * 3 + 1] * 3 + 0], NodePos[FaceIdx[i * 3 + 1] * 3 + 1], NodePos[FaceIdx[i * 3 + 1] * 3 + 2]);
            Vector3 p2 = new Vector3(NodePos[FaceIdx[i * 3 + 2] * 3 + 0], NodePos[FaceIdx[i * 3 + 2] * 3 + 1], NodePos[FaceIdx[i * 3 + 2] * 3 + 2]);
            FaceNormal[i] = Vector3.Cross(p1 - p0, p2 - p0);
        }
    }

    public float CalculateDistance(int nodeA, int nodeB)
    {
        float Ax = NodePos[nodeA * 3 + 0];
        float Ay = NodePos[nodeA * 3 + 1];
        float Az = NodePos[nodeA * 3 + 2];

        float Bx = NodePos[nodeB * 3 + 0];
        float By = NodePos[nodeB * 3 + 1];
        float Bz = NodePos[nodeB * 3 + 2];

        return Mathf.Sqrt(Mathf.Pow(Ax - Bx, 2) + Mathf.Pow(Ay - By, 2) + Mathf.Pow(Az - Bz, 2));
    }

}
