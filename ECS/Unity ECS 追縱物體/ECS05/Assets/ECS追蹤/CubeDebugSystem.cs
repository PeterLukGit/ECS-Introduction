using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

/// <summary>
/// 顯示方塊找到的圓球目標
/// </summary>
public class CubeDebugSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation translation, ref Cube_Look_Target CubeTarget) => {
            if (World.Active.EntityManager.Exists(CubeTarget.targetEntity))
            {
                Translation targetTranslation = World.Active.EntityManager.GetComponentData<Translation>(CubeTarget.targetEntity);
                Debug.DrawLine(translation.Value, targetTranslation.Value);
               // Debug.Log("輸出測試線");
            }
        });
    }

}
