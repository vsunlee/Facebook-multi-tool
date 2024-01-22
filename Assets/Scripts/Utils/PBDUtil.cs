using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PBDUtil
{
    /// <summary>
    /// 内联函数，使用顶点索引获取顶点位置，并以Vector3类型返回
    /// </summary>
    /// <param name="model">顶点所归属的模型</param>
    /// <param name="nodeIdx">顶点索引</param>
    /// <returns>顶点的坐标</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 NodeIdxToVector3(TetModel model, int nodeIdx)
    {
        return new Vector3(model.NodePos[nodeIdx * 3 + 0], model.NodePos[nodeIdx * 3 + 1], model.NodePos[nodeIdx * 3 + 2]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 NewNodeIdxToVector3(TetModel model, int nodeIdx)
    {
        return new Vector3(model.NewNodePos[nodeIdx * 3 + 0], model.NewNodePos[nodeIdx * 3 + 1], model.NewNodePos[nodeIdx * 3 + 2]);
    }

    /// <summary>
    /// 内联函数，使用顶点索引获取顶点速度，并以Vector3类型返回
    /// </summary>
    /// <param name="model">顶点所归属的模型</param>
    /// <param name="nodeIdx">顶点索引</param>
    /// <returns>顶点的速度</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3 NodeIdxToVelVector3(TetModel model, int nodeIdx)
    {
        return new Vector3(model.NodeVel[nodeIdx * 3 + 0], model.NodeVel[nodeIdx * 3 + 1], model.NodeVel[nodeIdx * 3 + 2]);
    }

    /// <summary>
    /// Matrix4x4的加法函数
    /// </summary>
    /// <param name="A">加数矩阵</param>
    /// <param name="B">加数矩阵</param>
    /// <returns>和矩阵</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 MatrixAdd(Matrix4x4 A, Matrix4x4 B)
    {
        Matrix4x4 C = new Matrix4x4();
        C.SetRow(0, A.GetRow(0) + B.GetRow(0));
        C.SetRow(1, A.GetRow(1) + B.GetRow(1));
        C.SetRow(2, A.GetRow(2) + B.GetRow(2));
        C.SetRow(3, A.GetRow(3) + B.GetRow(3));

        return C;
    }

    /// <summary>
    /// Matrix4x4的倍数函数
    /// </summary>
    /// <param name="A">矩阵</param>
    /// <param name="t">倍数</param>
    /// <returns>结果矩阵</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Matrix4x4 MatrixMultiple(Matrix4x4 A, float t)
    {
        Matrix4x4 B = new Matrix4x4();

        B.SetRow(0, A.GetRow(0) * t);
        B.SetRow(1, A.GetRow(1) * t);
        B.SetRow(2, A.GetRow(2) * t);
        B.SetRow(3, A.GetRow(3) * t);

        return B;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float CalculateVolume(TetModel model, int eleIdx)
    {
        Vector3 nodeA = NodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 0]);
        Vector3 nodeB = NodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 1]);
        Vector3 nodeC = NodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 2]);
        Vector3 nodeD = NodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 3]);

        return Vector3.Dot(Vector3.Cross(nodeB - nodeA, nodeC - nodeA), (nodeD - nodeA)) / 6;
    }
    /// <summary>
    /// 计算模型四面体体积，
    /// tetgen四面体模型文件中的四面体是逆时针的，在右手系右手定则法向量朝外
    /// 导入到Unity之后，yz坐标互换，变为左手系，采用左手定则的叉积法向量依旧朝外
    /// 所以要得到朝内的法向，就需要对结果取相反数
    /// </summary>
    /// <param name="model">四面体所归属的模型</param>
    /// <param name="eleIdx">四面体编号</param>
    /// <returns>四面体体积</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float GetNewElePosAndCalculateVolume(TetModel model, int eleIdx, out Vector3 nodeA, out Vector3 nodeB, out Vector3 nodeC, out Vector3 nodeD)
    {
        nodeA = NewNodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 0]);
        nodeB = NewNodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 1]);
        nodeC = NewNodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 2]);
        nodeD = NewNodeIdxToVector3(model, model.EleIdx[eleIdx * 4 + 3]);

        return Vector3.Dot(Vector3.Cross(nodeB - nodeA, nodeC - nodeA), (nodeD - nodeA)) / 6;
    }
}
