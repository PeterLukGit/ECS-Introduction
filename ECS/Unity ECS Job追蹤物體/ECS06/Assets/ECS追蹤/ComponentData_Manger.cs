using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

//把所有會用到的Component寫在這

//人物Tag標籤
public struct Cube_Unit : IComponentData { }

//目標Tag標籤
public struct Ball_Target : IComponentData { }

//人物鎖定的目標，避免一直切換目標
public struct Cube_Look_Target : IComponentData
{
    public Entity targetEntity;
}

