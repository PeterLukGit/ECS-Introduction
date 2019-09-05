using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 方塊移動和刪除目標
/// </summary>
public class CubeMoveSystem : ComponentSystem
{

    protected override void OnUpdate()
    {
        Entities.ForEach((Entity unitEntity, ref Cube_Look_Target LookTarget, ref Translation translation) =>
        {
            //EntityManager.Exists 報告Entity對像是否仍然有效。
            if (World.Active.EntityManager.Exists(LookTarget.targetEntity))
            {
                //Debug.Log(World.Active.EntityManager.Exists(LookTarget.targetEntity) + " " + LookTarget.targetEntity);
                //取得hasTarget.targetEntity的位置
                Translation targetTranslation =
                World.Active.EntityManager.
                GetComponentData<Translation>(LookTarget.targetEntity);

                //取得目標與人物之間的向量(標準化)
                float3 targetDir = math.normalize(targetTranslation.Value - translation.Value);
                //速度
                float moveSpeed = 5f;
                //位移
                translation.Value += targetDir * moveSpeed * Time.deltaTime;

                //當他們距離小於0.2
                if (math.distance(translation.Value, targetTranslation.Value) < .2f)
                {
                    // Close to target, destroy it
                    //刪除目標物件
                    PostUpdateCommands.DestroyEntity(LookTarget.targetEntity);
                    //刪除人物組件
                    PostUpdateCommands.RemoveComponent(unitEntity, typeof(Cube_Look_Target));
                }

            }
            else
            {
                //假如目標不見，刪除組件
                PostUpdateCommands.RemoveComponent(unitEntity, typeof(Cube_Look_Target));
            }
        });

    }
}

