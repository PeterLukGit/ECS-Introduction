using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;


/// <summary>
/// 搜尋目標
/// </summary>
public class LookTargetSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        //是否啟動JobSystem還是ComponentSystem
        bool JobEnabled = GameHander_test.GameHander_test_ins.JobBool;

        if(JobEnabled ==true)
        {
            return;
        }


        //搜尋，目前沒有找到目標的方塊
        Entities.
            WithNone<Cube_Look_Target>().
            WithAll<Cube_Unit>().
            ForEach((Entity entity, ref Translation unitTranslation) =>
            {
                float3 Cube_Pos = unitTranslation.Value;
                float3 closestTargetPosition = float3.zero;
                Entity Target = Entity.Null;

                //Debug.Log("搜尋");

                //搜尋目標
                Entities.WithAll<Ball_Target>().ForEach((Entity targetEntity, ref Translation targetTranslation) =>
                {
                    if (Target == Entity.Null)
                    {
                        // No target
                        Target = targetEntity;
                        closestTargetPosition = targetTranslation.Value;
                    }
                    else
                    {
                        if (math.distance(Cube_Pos, targetTranslation.Value) < math.distance(Cube_Pos, closestTargetPosition))
                        {
                            // This target is closer
                            Target = targetEntity;
                            closestTargetPosition = targetTranslation.Value;
                        }
                    }
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


/// <summary>
/// 這是找目標(圓球)的Job化
/// 分為兩種方法
/// 1.一條龍
/// 也就是在找到目標十，就在Job中執行新增組件
/// 
/// 2.分開式
/// Job找到目標後，並沒有新增，
/// 而是用一陣列保存，Job結束後
/// 再用另一Job新增組件
/// 
/// 這兩種方法在數量少時並沒差多少
/// 但在於BurstCompile無法與entityCommandBuffer同時再一起
/// 所以分開來的話，搜尋就能用BurstCompile來減少時間
/// 添加組件雖無法BurstCompile但減少程式流程來增加速度
/// 
/// 注意:Job是有其極限，當到依程度，Job也沒多大用處
/// </summary>
public class CubeFindJobSystem : JobComponentSystem
{
    //共通struct----------------------------

    //這用於紀錄目標位置
    private struct EntityJobArray
    {
        public Entity entity;
        public float3 position;
    }

    //共通---結束------------------------------

    //第一種作法----------------------------

    // 意思是執行此struct時必須含有該組建 
    //這邊意思就是只有方塊才能執行
    [RequireComponentTag(typeof(Cube_Unit))]
    //這句意思是排除持有目標組件的方塊
    [ExcludeComponent(typeof(Cube_Look_Target))]
    //綜合兩句意思就是只有方塊中並沒有找到目標的方塊執行此函示
    private struct FindTargetJobSystem_No1 : IJobForEachWithEntity<Translation>
    {

        [ReadOnly]
        //如果在JobSystem字段中設置屬性，則作業結束時似乎會自動釋放NativeArray
        //作業聲明將在作業中訪問的所有數據
        // //通過將其聲明為只讀，允許多個作業並行訪問數據
        [DeallocateOnJobCompletion]
        public NativeArray<EntityJobArray> targetarray;

        //該EntityCommandBuffer課程解決了兩個重要問題：
        //        在工作時，您無法訪問EntityManager。
        //當您訪問EntityManager（例如，創建實體）時，會使所有註入的數組和EntityQuery對象無效。
        //通過EntityCommandBuffer抽象，您可以將更改排隊（從作業或從主線程進行），以使更改可以稍後在主線程上生效。有兩種使用方法EntityCommandBuffer：
        //ComponentSystem在主線程上更新的子類有一個可用的自動調用PostUpdateCommands。要使用它，只需引用該屬性並使更改排隊。從系統Update功能返回後，它們會立即自動應用於世界。
        //------------------------------------------------------------------------------------------
        //EntityCommandBuffer.Concurrent允許並發（確定性）命令緩衝區記錄。
        //------------------------------------------------------------------------------------------
        //以上是Unity官方所寫的
        //經理解大致意思是在進入Job的多線程時，實際上是無法訪問EntityManager
        //也就是無法訪問到世界，所以必須在進入Job時宣告"EntityCommandBuffer實體命令緩衝"
        //而EntityCommandBuffer.Concurrent裡面掌管AddComponent所以，需要用這個方式新增組件
        //而不是向上一版
        //PostUpdateCommands.AddComponent(entity, new Cube_Look_Target { targetEntity = Target });
        //就可以，添加組件
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        public void Execute(Entity entity, int index, [ReadOnly]  ref Translation translation)
        {
            float3 Cube_Pos = translation.Value;
            float3 closestTargetPosition = float3.zero;
            Entity Target = Entity.Null;

            //Debug.Log("搜尋");

            for (int i = 0; i < targetarray.Length; i++)
            {
                EntityJobArray targetentityJobArray = targetarray[i];
                if (Target == Entity.Null)
                {
                    Target = targetentityJobArray.entity;
                    closestTargetPosition = targetentityJobArray.position;
                }
                else
                {
                    if (math.distance(Cube_Pos, targetentityJobArray.position) < math.distance(Cube_Pos, closestTargetPosition))
                    {
                        Target = targetentityJobArray.entity;
                        closestTargetPosition = targetentityJobArray.position;
                    }
                }
            }

            //放入敵人數據
            if (Target != Entity.Null)
            {
                //EntityCommandBuffer實體命令緩衝
                //使用了這個是無法使用BurstCompile
                entityCommandBuffer.AddComponent(index, entity, new Cube_Look_Target { targetEntity = Target });
            }



        }
    }

    //第二種作法--------------------------------

    // 意思是執行此struct時必須含有該組建 
    //這邊意思就是只有方塊才能執行
    [RequireComponentTag(typeof(Cube_Unit))]
    //這句意思是排除持有目標組件的方塊
    [ExcludeComponent(typeof(Cube_Look_Target))]
    //綜合兩句意思就是只有方塊中並沒有找到目標的方塊執行此函示
    [BurstCompile]
    private struct FindTargetJobSystem_No2 : IJobForEachWithEntity<Translation>
    {

        [ReadOnly]
        //如果在JobSystem字段中設置屬性，則作業結束時似乎會自動釋放NativeArray
        [DeallocateOnJobCompletion]
        public NativeArray<EntityJobArray> targetarray;

        //就是這邊跟第一種不一樣這是保存目標Entity的數據
        public NativeArray<Entity> TargetEntityArray;

        public void Execute(Entity entity, int index, [ReadOnly]  ref Translation translation)
        {
            float3 Cube_Pos = translation.Value;
            float3 closestTargetPosition = float3.zero;
            Entity Target = Entity.Null;

            //Debug.Log("搜尋");

            for (int i = 0; i < targetarray.Length; i++)
            {
                EntityJobArray targetentityJobArray = targetarray[i];
                if (Target == Entity.Null)
                {
                    // No target
                    Target = targetentityJobArray.entity;
                    closestTargetPosition = targetentityJobArray.position;
                }
                else
                {
                    if (math.distance(Cube_Pos, targetentityJobArray.position) < math.distance(Cube_Pos, closestTargetPosition))
                    {
                        // This target is closer
                        Target = targetentityJobArray.entity;
                        closestTargetPosition = targetentityJobArray.position;
                    }
                }
            }

            //這邊也跟第一種不一樣，將陣列保存目標Entity的數據
            TargetEntityArray[index] = Target;



        }
    }


    [RequireComponentTag(typeof(Cube_Unit))]
    [ExcludeComponent(typeof(Cube_Look_Target))]
    ///新增組件，將第一種作法的新增拉出來
    private struct AddComponentJob : IJobForEachWithEntity<Translation>
    {

        [DeallocateOnJobCompletion]
        [ReadOnly]
        //目標物
        public NativeArray<Entity> TargetEntityArray;

        //跟第一種一樣，需要實體命令緩衝
        public EntityCommandBuffer.Concurrent entityCommandBuffer;

        //將命令添加到並發命令緩衝區時，可以使用索引。運行並行處理實體的作業時，
        //可以使用並發命令緩衝區。在IJobForEachWithEntity作業中，
        //當您使用Schedule（）方法而不是上例中使用的ScheduleSingle（）方法時，
        //作業系統並行處理實體。並行命令緩衝區應始終使用並發命令緩衝區，
        //以確保線程安全和確定性地執行緩衝區命令。
        //您也可以使用索引來引用同一系統中所有Job中的相同實體。
        //例如，如果您需要多次處理一組實體並在此過程中收集臨時數據，
        //則可以使用索引將臨時數據插入一個Job中的NativeArray中，
        //然後使用該索引在一個Job中訪問該數據。隨後的工作。（自然，您必須將相同的NativeArray傳遞給兩個Job。）
        //以上是Unity官方寫的關於index的內容

        //簡單講就是，index就像For迴圈那樣
        public void Execute(Entity entity, int index, ref Translation translation)
        {
            if (TargetEntityArray[index] != Entity.Null)
            {
                //加入組件
                entityCommandBuffer.AddComponent(index, entity, new Cube_Look_Target { targetEntity = TargetEntityArray[index] });
            }
        }

    }

    //第二種作法---結束------------------------------

    //空Job
    //為了看清楚JobSystem還是ComponentSystem之間對比所用的

    [RequireComponentTag(typeof(Cube_Unit))]
    [ExcludeComponent(typeof(Cube_Look_Target))]
    private struct FindTargetJobSystem_Null : IJobForEachWithEntity<Translation>
    {
        public void Execute(Entity entity, int index, [ReadOnly]  ref Translation translation)
        {

        }
    }


    //這解釋比較長
    //1.當我們要在Job裡新增組件時需動用到EntityCommandBuffer
    //2.但是我們無法直接新增EntityCommandBuffer
    //基於以上2點所以必須要繞點路
    //EndSimulationEntityCommandBufferSystem是ECS中預設的System，並且需在OnCreate(初始化此命令緩衝區系統。)新增
    //透過他創建EntityCommandBuffer之後再丟到Job裡
    //entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
    //丟到Job裡面，讓Job擁有變數，才能新增組件
    //https://forum.unity.com/threads/addcomponent-from-inside-jobcomponentsystem-job.678730/
    private EndSimulationEntityCommandBufferSystem endSimulationEntityCommandBufferSystem;

    //因為EndSimulationEntityCommandBufferSystem要在一開始就創建出來
    protected override void OnCreate()
    {
        endSimulationEntityCommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        base.OnCreate();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        bool JobAdd = GameHander_test.GameHander_test_ins.JobAddComponent;

        bool JobEnabled = GameHander_test.GameHander_test_ins.JobBool;

        //Debug.Log(JobAdd);

        //共通部分，為了拿到目標物的Entity

        //因為Job中無法訪問世界所以要先訪問佇列
        //這句意思就是找有Ball_Target組件 並有Translation組件
        EntityQuery targetQuery = GetEntityQuery(typeof(Ball_Target), ComponentType.ReadOnly<Translation>());
        //轉成陣列
        NativeArray<Entity> targetEntityArray = targetQuery.ToEntityArray(Allocator.TempJob);
        NativeArray<Translation> targetTranslationArray = targetQuery.ToComponentDataArray<Translation>(Allocator.TempJob);
        //轉換完，這是目標圓球陣列
        NativeArray<EntityJobArray> targetArray = new NativeArray<EntityJobArray>(targetEntityArray.Length, Allocator.TempJob);
       
        //輸入資料
        for (int i = 0; i < targetEntityArray.Length; i++)
        {
            targetArray[i] = new EntityJobArray
            {
                entity = targetEntityArray[i],
                position = targetTranslationArray[i].Value,
            };
        }
        //釋放陣列
        targetEntityArray.Dispose();
        targetTranslationArray.Dispose();

        JobHandle jobHandle;
        //共通結束

        //是否啟動JobSystem還是ComponentSystem
        if (JobEnabled == true)
        {
            //啟動JobSystem
            if (JobAdd == false)
            {
                //第一種作法
                FindTargetJobSystem_No1 findTargetJobSystem_No1 = new FindTargetJobSystem_No1
                {
                    targetarray = targetArray,
                    entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent()
                };

                jobHandle = findTargetJobSystem_No1.Schedule(this, inputDeps);

            }
            else
            {
                //第二種作法

                //這句意思就是找有Cube_Unit組件 並排除Ball_Target組件
                EntityQuery unitQuery = GetEntityQuery(typeof(Cube_Unit), ComponentType.Exclude<Ball_Target>());
                NativeArray<Entity> closestTargetEntityArray = new NativeArray<Entity>(unitQuery.CalculateEntityCount(), Allocator.TempJob);

                FindTargetJobSystem_No2 findTargetJobSystem_No2 = new FindTargetJobSystem_No2
                {
                    targetarray = targetArray,
                    TargetEntityArray = closestTargetEntityArray
                };

                jobHandle = findTargetJobSystem_No2.Schedule(this, inputDeps);

                AddComponentJob addComponentJob = new AddComponentJob
                {
                    TargetEntityArray = closestTargetEntityArray,
                    entityCommandBuffer = endSimulationEntityCommandBufferSystem.CreateCommandBuffer().ToConcurrent(),
                };

                jobHandle = addComponentJob.Schedule(this, jobHandle);

            }
        }
        else
        {
            //啟動ComponentSystem但JobSystem不能return空，所以用一空Job賦值
            FindTargetJobSystem_Null findTargetJobSystem_Null = new FindTargetJobSystem_Null { };
            jobHandle = findTargetJobSystem_Null.Schedule(this, inputDeps);
        }
       




        // 備註
        //從作業寫入命令緩衝區時，必須添加 
        //通過調用此函數將該Job映射到此緩衝區系統的依賴項列表。
        //否則，當寫入作業仍在進行時，緩衝區系統可以執行命令緩衝區中當前的命令。
        //以上Unity官方
        //每當將命令緩衝區傳遞給系統時，您都需要將作業句柄返回到擁有命令緩衝區的系統（您可以簡單地將其返回到系統末尾傳遞最終的作業句柄。）
        //這是確保作業的必需條件在屏障系統運行之前，已完成使用命令緩衝區的操作。
        //以上Unity論壇
        //大致理解，就是為了保護，避免內存出意外，所以擁有EntityCommandBuffer的Job都必須加上這句，放在末尾
        endSimulationEntityCommandBufferSystem.AddJobHandleForProducer(jobHandle);

        return jobHandle;
    }
}
