using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

/// <summary>
///講解
/// 這邊要使用 IJobParallelFor 實現物件位移
/// IJobParallelFor 理解成Job版For迴圈
/// 輸入數據後，會自動分開多核執行For迴圈
/// 但是IJobParallelFor 無法使用Transform組件
/// 所以IJobParallelFor只是負責運算，
/// 位移還是要而外去做
/// 
///PS: Job要在一定複雜度情況下，就能顯現出Job功用
///若是複雜度不夠，則Job反而會拖累禎數，反而Update還比較快
///在這邊是用
///        float value = 0f;
//          for (int i = 0; i < 1000; i++){value = math.exp10(math.sqrt(value));}
///        提升複雜度
///        所以去掉的話，Update反而跑的比Job快
///        因為Job會多一層分配手續，所以拖慢了速度
/// </summary>
public class JobTest3: MonoBehaviour
{
    [SerializeField]
    [Header("啟用Job系統")]
    private bool useJobs;

    [SerializeField]
    [Header("啟用Burst系統")]
    private bool useBurst;

    [SerializeField]
    [Header("測試方塊")]
    private GameObject cube_gameObject;

    [SerializeField]
    [Header("方塊數量")]
    private int cube_num = 100;

    //存放Cube的List
    private List<cube_data> cube_gameObject_List;

    //存放Cube的data
    class cube_data
    {
        //Transform組件
        public Transform transform;
        //移動用Y座標
        public float moveY;
    }

    // Start is called before the first frame update
    void Start()
    {
        cube_gameObject_List = new List<cube_data>();
        //生成方塊
        for (int i = 0; i < cube_num; i++)
        {
            //生成
            GameObject Cube_Transform = Instantiate(cube_gameObject, new Vector3(UnityEngine.Random.Range(-8f, 8f), UnityEngine.Random.Range(-5f, 5f), UnityEngine.Random.Range(-8f, 8f)), Quaternion.identity);
            //存放數據
            cube_gameObject_List.Add
                (new cube_data
                { transform = Cube_Transform.GetComponent<Transform>(),
                    moveY = UnityEngine.Random.Range(1f, 2f)});
        }

    }

    // Update is called once per frame
    void Update()
    {
        float startTime = Time.realtimeSinceStartup;


        if (useJobs == true)
        {
            //設定臨時陣列，XYZ座標
            NativeArray<float3> positionArray_Update = new NativeArray<float3>(cube_gameObject_List.Count, Allocator.TempJob);
            //設定臨時陣列Y軸移動速度
            NativeArray<float> moveYArray_Update = new NativeArray<float>(cube_gameObject_List.Count, Allocator.TempJob);

            //將數據導入
            //原因在於要把Transform的Vec3數據轉成float3
            //另一個是Job不支持Transform直接運算
            for (int i = 0; i < cube_gameObject_List.Count; i++)
            {
                positionArray_Update[i] = cube_gameObject_List[i].transform.position;
                moveYArray_Update[i] = cube_gameObject_List[i].moveY;
            }


            //是否啟用Burst
            if (useBurst == true)
            {
                ReallyJob_3_IJobParallelFor_BurstCompile reallyJob_3_IJobParallelFor_BurstCompile = new ReallyJob_3_IJobParallelFor_BurstCompile
                {
                    //導入數據
                    deltaTime = Time.deltaTime,
                    positionArray = positionArray_Update,
                    moveYArray = moveYArray_Update,
                };

                //啟用Job
                //Schedule(cube_gameObject_List.Count, 100);
                //IJobParallelFor類似For迴圈所以要設定跑到多少
                //cube_gameObject_List.Count,就是只說跑完整個陣列，數值超過會報例外
                JobHandle jobHandle = reallyJob_3_IJobParallelFor_BurstCompile.Schedule(cube_gameObject_List.Count, 100);
                jobHandle.Complete();
            }
            else
            {
                ReallyJob_3_IJobParallelFor reallyJob_3_IJobParallelFor = new ReallyJob_3_IJobParallelFor
                {
                    //導入數據
                    deltaTime = Time.deltaTime,
                    positionArray = positionArray_Update,
                    moveYArray = moveYArray_Update,
                };

                //啟用Job
                //Schedule(cube_gameObject_List.Count, 100);
                //IJobParallelFor類似For迴圈所以要設定跑到多少
                //cube_gameObject_List.Count,就是只說跑完整個陣列，數值超過會報例外
                JobHandle jobHandle = reallyJob_3_IJobParallelFor.Schedule(cube_gameObject_List.Count, 100);
                jobHandle.Complete();
            }

            //讓方塊移動
           // 因為IJobParallelFor只算好數據，移動還是要用For迴圈在處理一次
            for (int i = 0; i < cube_gameObject_List.Count; i++)
            {
                //print(cube_gameObject_List[i].transform.position + "  " +positionArray_Update[i]);
                cube_gameObject_List[i].transform.position = positionArray_Update[i];
                cube_gameObject_List[i].moveY = moveYArray_Update[i];
            }
            //釋放陣列
            positionArray_Update.Dispose();
            moveYArray_Update.Dispose();
        }
        else
        {
            //這邊有點問題
            //我使用foreach時5000個方塊為600ms
            //但使用for迴圈時能降到<1ms狀況，不只這樣，還跑得比Job快
            //推測問題是for迴圈根本沒執行，因為5000個方塊只有1個在動
            //所以這邊5000個方塊移動應該用foreach
            foreach (cube_data cube in cube_gameObject_List)
            {
                cube.transform.position += new Vector3(0, cube.moveY * Time.deltaTime, 0);
                if (cube.transform.position.y > 5f)
                {
                    cube.moveY = -math.abs(cube.moveY);
                }
                if (cube.transform.position.y < -5f)
                {
                    cube.moveY = +math.abs(cube.moveY);
                }
                float value = 0f;
                for (int i = 0; i < 1000; i++)
                {
                    value = math.exp10(math.sqrt(value));
                }
            }



            //for (int i = 0; i < cube_gameObject_List.Count; i++)
            //{
            //    //移動
            //    cube_gameObject_List[i].transform.position += new Vector3(0, cube_gameObject_List[i].moveY * Time.deltaTime, 0f);

            //    //Y軸範圍
            //    if (cube_gameObject_List[i].transform.position.y > 5f)
            //    {
            //        cube_gameObject_List[i].moveY = -math.abs(cube_gameObject_List[i].moveY);
            //    }
            //    if (cube_gameObject_List[i].transform.position.y < -5f)
            //    {
            //        cube_gameObject_List[i].moveY = +math.abs(cube_gameObject_List[i].moveY);
            //    }

            //    float value = 0f;
            //    for (int j = 0; i < 1000; i++)
            //    {
            //        value = math.exp10(math.sqrt(value));
            //    }
            //}

        }


        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }



}

/// <summary>
/// IJobParallelFor講解
/// Execute(int index)意思是
/// 就像For迴圈一樣，跑設定好的程式
/// 也因為IJobParallelFor是struct所以
/// Transform 之類在Job裡不能用
/// </summary>
public struct ReallyJob_3_IJobParallelFor : IJobParallelFor
{
    //存放的三維數據
    public NativeArray<float3> positionArray;
    //Y軸移動速度
    public NativeArray<float> moveYArray;

    //readonly屬性僅在應用於本機字段時才有意義，
    //它允許多個不同的作業同時在同一數據上工作
    //非本機字段始終只是副本而不是引用因此在這裡添加（readonly）到float deltaTime是不必要的
    //以上是元影片註解，但看不懂寫什麼
    //以下原文
    //the readonly attribute only makes sense when applied to native 
    //fields it allow multiple different jobs to work concurrently on the 
    //same data Non-Native fields Are always just copies and not 
    //refernces so in here adding (readonly) to float deltaTime is unnecessary
    [ReadOnly] public float deltaTime;

    public void Execute(int index)
    {
        //移動
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);
        //Y軸範圍
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        //增加運算複雜度，凸顯Job
        //原因是不加點複雜度，Job跑得還會比foreach慢
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

}

[BurstCompile]
public struct ReallyJob_3_IJobParallelFor_BurstCompile : IJobParallelFor
{

    //存放的三維數據
    public NativeArray<float3> positionArray;
    //Y軸移動速度
    public NativeArray<float> moveYArray;

    //readonly屬性僅在應用於本機字段時才有意義，
    //它允許多個不同的作業同時在同一數據上工作
    //非本機字段始終只是副本而不是引用因此在這裡添加（readonly）到float deltaTime是不必要的
    //以上是元影片註解，但看不懂寫什麼
    //以下原文
    //the readonly attribute only makes sense when applied to native 
    //fields it allow multiple different jobs to work concurrently on the 
    //same data Non-Native fields Are always just copies and not 
    //refernces so in here adding (readonly) to float deltaTime is unnecessary
    [ReadOnly] public float deltaTime;

    public void Execute(int index)
    {
        //移動
        positionArray[index] += new float3(0, moveYArray[index] * deltaTime, 0f);
        //Y軸範圍
        if (positionArray[index].y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (positionArray[index].y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }

        //增加運算複雜度，凸顯Job
        //原因是不加點複雜度，Job跑得還會比foreach慢
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

}



