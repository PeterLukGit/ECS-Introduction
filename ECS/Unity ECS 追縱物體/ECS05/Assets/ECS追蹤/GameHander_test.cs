using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;

/// <summary>
/// 建構環境
/// </summary>
public class GameHander_test : MonoBehaviour
{
    //追的方塊
    public Mesh CubeMesh;
    //方塊材質
    public Material CubeMaterial;
    //目標圓球
    public Mesh TargetMesh;
    //圓球材質
    public Material TargetMaterial;

    private static EntityManager entityManager;

    // Start is called before the first frame update
    void Start()
    {
        entityManager = World.Active.EntityManager;

        for (int i = 0; i < 10; i++)
        {
            Ins_Cube_Entity();
        }

        for (int i = 0; i < 5; i++)
        {
            Ins_Target_Entity();
        }



    }

    private float spawnTargetTimer;

    private void Update()
    {
        //生成標把
        spawnTargetTimer -= Time.deltaTime;
        if (spawnTargetTimer < 0)
        {
            spawnTargetTimer = .1f;

            for (int i = 0; i < 10; i++)
            {
                Ins_Target_Entity();
            }
        }
    }

    /// <summary>
    /// 生成方塊
    /// </summary>
    void Ins_Cube_Entity()
    {
        //生成人物
        Cube_rule_Entity(new float3(UnityEngine.Random.Range(-8, +8f), UnityEngine.Random.Range(-5, +5f), UnityEngine.Random.Range(-5, +5f)));
    }

    /// <summary>
    /// Entity方塊設定
    /// </summary>
    void Cube_rule_Entity(float3 position)
    {
        Entity entity = entityManager.CreateEntity(
            typeof(Translation),//位置
            typeof(LocalToWorld),//轉換座標
            typeof(RenderMesh),//渲染
            typeof(Scale),//大小（這是等比縮放）
            typeof(Cube_Unit)//這是Tag用於區分Entity
            );

        //輸入數據
        entityManager.SetSharedComponentData<RenderMesh>(entity,
              new RenderMesh
              {
                  material = CubeMaterial,
                  mesh = CubeMesh,
              });

        entityManager.SetComponentData<Translation>(entity,
    new Translation
    {
        Value = position
    });

        //設定縮放
        entityManager.SetComponentData(entity, new Scale { Value = 1.5f });

    }

    /// <summary>
    /// 生成目標
    /// </summary>
    void Ins_Target_Entity()
    {
        //生成目標
        Target_rule_Entity(new float3(UnityEngine.Random.Range(-8, +8f), UnityEngine.Random.Range(-5, +5f), UnityEngine.Random.Range(-5, +5f)));
    }

    /// <summary>
    /// Entity目標設定
    /// </summary>
    void Target_rule_Entity(float3 position)
    {
        Entity entity = entityManager.CreateEntity(
          typeof(Translation),//位置
          typeof(LocalToWorld),//轉換座標
          typeof(RenderMesh),//渲染
          typeof(Scale),//大小（這是等比縮放）
          typeof(Ball_Target)//這是Tag用於區分Entity
          );

        //輸入數據
        entityManager.SetSharedComponentData<RenderMesh>(entity,
              new RenderMesh
              {
                  material = TargetMaterial,
                  mesh = TargetMesh,
              });

        entityManager.SetComponentData<Translation>(entity,
            new Translation
            {
                Value = position
            }
        );

        //設定縮放
        entityManager.SetComponentData(entity, new Scale { Value = 0.5f });
    }

}
