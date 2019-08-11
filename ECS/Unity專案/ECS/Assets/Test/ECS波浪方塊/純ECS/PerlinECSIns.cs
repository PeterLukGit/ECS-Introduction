using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Collections;
using Unity.Rendering;

public class PerlinECSIns : MonoBehaviour
{
    //躁點
    [Header("躁點")]
    public float perlinNoise = 0f;
    //躁點偏移
    //(平滑度(因為Mathf.PerlinNoise在噪點圖來看灰階很平滑，所以i * refinement, j * refinement 來看成的數值越大月不平滑))
    [Header("躁點偏移")]
    public float refinement = 0f;
    //高度
    [Header("高度")]
    public float multiplier = 0f;
    //方塊數量
    [Header("方塊數量")]
    public int cube = 0;

    [SerializeField]//要渲染的模型
    [Header("要渲染的模型")]
    private Mesh Privatemesh;

    [SerializeField]//模型的材質球
    [Header("模型的材質球")]
    private Material Privatematerial;


    // Start is called before the first frame update
    void Start()
    {
        //EntityManager 管理Entity
        EntityManager entityManager = World.Active.EntityManager;

        //EntityArchetype 製作Entity原型 當成Entity Prefab就好理解
        //而   typeof(LevelComponent)就是加入的Component組件
        EntityArchetype entityArchetype = entityManager.CreateArchetype(
            typeof(Translation),
            typeof(RenderMesh),
            typeof(LocalToWorld),
            typeof(PerlinComponent));

        //宣告存放Entity陣列
        //程式做完後陣列要釋放掉
        //Allocator就是設定要如何使用這陣列Temp是臨時陣列，所以要釋放
        NativeArray<Entity> entitieArray = new NativeArray<Entity>(cube * cube, Allocator.Temp);

        //生成Entity用entityArchetype當模板，並放入entitieArray陣列裡
        entityManager.CreateEntity(entityArchetype, entitieArray);

        //確定方塊的XY軸
        int array_x, array_y;
        //修正Entity陣列裡Entity的組件數據
        for (int i = 0; i < entitieArray.Length; i++)
        {
            Entity entity = entitieArray[i];
            //XY軸計算
            array_x = i % cube;
            array_y = i / cube;
            //噪點
            perlinNoise = Mathf.PerlinNoise(array_x * refinement, array_y * refinement);

            ////修正組件數據
            entityManager.SetComponentData(entity, 
                new Translation {
                    Value = new Unity.Mathematics.float3(array_x, perlinNoise * multiplier, array_y) });

            entityManager.SetComponentData(entity, 
                new PerlinComponent  { high = multiplier,
                    PerlinVec2 = new Vector2 (array_x * refinement, array_y * refinement) });

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
