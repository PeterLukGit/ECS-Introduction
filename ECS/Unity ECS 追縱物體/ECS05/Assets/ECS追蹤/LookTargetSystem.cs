using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 搜尋目標
/// </summary>
public class LookTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //搜尋，目前沒有找到目標的方塊
        Entities.
            WithNone<Cube_Look_Target>().
            WithAll<Cube_Unit>().
            ForEach((Entity entity, ref Translation unitTranslation) =>
        {
            float3 Cube_Pos = unitTranslation.Value;

            Entity Target = Entity.Null;

            //Debug.Log("搜尋");

            //搜尋目標
            Entities.WithAll<Ball_Target>().ForEach((Entity targetEntity, ref Translation targetTranslation) =>
            {
                float3 Target_Pos = targetTranslation.Value;

                //Target = targetEntity;
               
                //當方塊與目標小於2(這只是隨便設定，能用一Component設定)
                if (math.distance(Cube_Pos, Target_Pos) < 2)
                {
                    Target = targetEntity;
                    //Debug.Log("找到目標小於<2");
                }
                //else
                //{
                //    Target = targetEntity;
                //    Debug.Log("找到目標大於>2");
                //}
            });

            //放入敵人數據
            if (Target != Entity.Null)
            {
                //Debug.DrawLine(unitPosition, closestTargetPosition);
                //在PostUpdatecommand中添加Instantiate指令，这些指令会在Update执行完毕后再执行
                //在PostUpdatecommand，这些指令会在Update执行完毕后再执行
                //在主线程 update 的 ComponentSystem 子类有一个 PostUpdateCommands（其本身是一个EntityCommandBuffer ） 可以用，
                //我们只要简单地把变化按顺序放进去即可。在系统的 Update 调用之后，它会立刻自动在世界（World）中进行所有数据更改。这样可以防止数组数据无效，API 也和 EntityManager 很相似。
                PostUpdateCommands.AddComponent(entity, new Cube_Look_Target { targetEntity = Target });
                //Debug.Log("加上Cube_Look_Target");
            }
           
        });
    }
}
