using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

 public struct PerlinComponent : IComponentData
{
    //紀錄躁點位置
    public Vector2 PerlinVec2;
    //方塊震幅多大
    public float high;
    //時間
    public float elepsedTime;
    //躁點output
    public float perlinNoise;
}
