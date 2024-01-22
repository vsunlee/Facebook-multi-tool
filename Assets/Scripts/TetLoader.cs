using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class TetLoader
{
    /// <summary>
    /// 读取node文件的函数,目前仅生成位置数组。
    /// 速度和力的数组和它大小一致，但是先不生成。
    /// </summary>
    public static void ReadNodeFile(string nodeFileName, out int nodeNum, out float[] nodePos, Vector3 basePosition)
    {
        //读取node文本文件
        string[] textTxt = File.ReadAllLines(nodeFileName);
        //解析出顶点数量
        nodeNum = int.Parse(textTxt[0].Split(' ')[0]);
        //开辟位置数组空间
        nodePos = new float[nodeNum * 3];

        for (int i = 0; i < textTxt.Length; i++)
        {
            string[] splitTxt = textTxt[i + 1].Split(' ', options: StringSplitOptions.RemoveEmptyEntries);
            if (splitTxt[0] == "#") break;
            //for (int j = 0; j < splitTxt.Length; j++) Debug.Log(splitTxt[j]);
            nodePos[i * 3 + 0] = float.Parse(splitTxt[1]) + basePosition.x;
            nodePos[i * 3 + 1] = float.Parse(splitTxt[2]) + basePosition.y;
            nodePos[i * 3 + 2] = float.Parse(splitTxt[3]) + basePosition.z;
        }
    }

    /// <summary>
    /// 读取四面体信息文件
    /// </summary>
    public static void ReadEleFile(string eleFileName, out int eleNum, out int[] eleIdx)
    {
        //读取node文本文件
        string[] textTxt = File.ReadAllLines(eleFileName);
        //解析出顶点数量
        eleNum = int.Parse(textTxt[0].Split(' ')[0]);
        //开辟位置数组空间
        eleIdx = new int[eleNum * 4];

        for (int i = 0; i < textTxt.Length; i++)
        {
            string[] splitTxt = textTxt[i + 1].Split(' ', options: StringSplitOptions.RemoveEmptyEntries);
            if (splitTxt[0] == "#") break;
            eleIdx[i * 4 + 0] = int.Parse(splitTxt[1]);
            eleIdx[i * 4 + 1] = int.Parse(splitTxt[2]);
            eleIdx[i * 4 + 2] = int.Parse(splitTxt[3]);
            eleIdx[i * 4 + 3] = int.Parse(splitTxt[4]);
        }
    }

    /// <summary>
    /// 读取表面信息文件
    /// </summary>
    public static void ReadFaceFile(string faceFileName, out int faceNum, out int[] faceIdx)
    {
        //读取node文本文件
        string[] textTxt = File.ReadAllLines(faceFileName);
        //解析出顶点数量
        faceNum = int.Parse(textTxt[0].Split(' ')[0]);
        //开辟位置数组空间
        faceIdx = new int[faceNum * 3];

        for (int i = 0; i < textTxt.Length; i++)
        {
            string[] splitTxt = textTxt[i + 1].Split(' ', options: StringSplitOptions.RemoveEmptyEntries);
            if (splitTxt[0] == "#") break;
            faceIdx[i * 3 + 0] = int.Parse(splitTxt[1]);
            faceIdx[i * 3 + 2] = int.Parse(splitTxt[2]);
            faceIdx[i * 3 + 1] = int.Parse(splitTxt[3]);
        }
    }

    public static void ReadEdgeFile(string edgeFileName, out int edgeNum, out int[] edgeIdx)
    {
        //读取node文本文件
        string[] textTxt = File.ReadAllLines(edgeFileName);
        //解析出顶点数量
        edgeNum = int.Parse(textTxt[0].Split(' ')[0]);
        //开辟位置数组空间
        edgeIdx = new int[edgeNum * 2];

        for (int i = 0; i < textTxt.Length; i++)
        {
            string[] splitTxt = textTxt[i + 1].Split(' ', options: StringSplitOptions.RemoveEmptyEntries);
            if (splitTxt[0] == "#") break;
            edgeIdx[i * 2 + 0] = int.Parse(splitTxt[1]);
            edgeIdx[i * 2 + 1] = int.Parse(splitTxt[2]);
        }
    }
}
