using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionVelocity
{

    private int nodeIdx;

    private Vector3 velocity;

    /// <summary>
    /// 被撞到的顶点索引
    /// </summary>
    public int NodeIdx { get { return nodeIdx; } set { nodeIdx = value; } }
    /// <summary>
    /// 碰撞后的速度
    /// </summary>
    public Vector3 Velocity { get { return velocity; } set { velocity = value; } }
}
