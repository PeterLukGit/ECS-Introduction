using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

public class Testing : MonoBehaviour
{

    [SerializeField]//要渲染的模型
    [Header("要渲染的模型")]
    private Mesh Privatemesh;

    [SerializeField]//模型的材質球
    [Header("模型的材質球")]
    private Material Privatematerial;

    [SerializeField]//生成數量
    [Header("生成數量")]
    private int num;

    // Start is called before the first frame update
    void Start()
    {
        //EntityManager 管理Entity
        EntityManager entityManager = World.Active.EntityManager;

        //EntityArchetype 製作Entity原型 當成Entity Prefab就好理解
        //而   typeof(LevelComponent)就是加入的Component組件
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(LevelComponent),
            typeof(Translation),
            typeof(RenderMesh),
             typeof(LocalToWorld),
            typeof(MoveSpeedComponent));

        //宣告存放Entity陣列
        //程式做完後陣列要釋放掉
        //Allocator就是設定要如何使用這陣列Temp是臨時陣列，所以要釋放
        NativeArray<Entity> entitieArray = new NativeArray<Entity>(num, Allocator.Temp);

        //生成Entity用entityArchetype當模板，並放入entitieArray陣列裡
        entityManager.CreateEntity(entityArchetype, entitieArray);

        //修正Entity陣列裡Entity的組件數據
        for (int i = 0; i < entitieArray.Length; i++)
        {
            Entity entity = entitieArray[i];

            //修正組件數據
            entityManager.SetComponentData(entity, new LevelComponent { level = Random.Range(10, 20) });
            entityManager.SetComponentData(entity, new MoveSpeedComponent { speed = Random.Range(1f, 2f) });
            entityManager.SetComponentData(entity, new Translation { Value = new Unity.Mathematics.float3(Random.Range(0, 10), Random.Range(0, 0), Random.Range(0, 10)) });

            //設定"共用組件"(Set"Shared"ComponentData)
            //因為全部Entity都有RenderMesh(渲染組件)，每個都渲染同一個模型，用"共用組件"省資源
            entityManager.SetSharedComponentData(entity, new RenderMesh
            {
                mesh = Privatemesh,
                material = Privatematerial
            }
            );
        }
        //釋放掉陣列
        entitieArray.Dispose();

    }

}