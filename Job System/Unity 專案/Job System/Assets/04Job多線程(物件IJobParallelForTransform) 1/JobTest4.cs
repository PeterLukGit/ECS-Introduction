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
///這邊要使用 IJobParallelForTransform 實現物件位移
///這跟IJobParallelFor類似
///依樣理解成Job版For迴圈，只是在Job裡可以使用Transform組件
///但是Transform組件要預先輸入進去給Job
///
/// PS: Job要在一定複雜度情況下，就能顯現出Job功用
///若是複雜度不夠，則Job反而會拖累禎數，反而Update還比較快
///在這邊是用
///        float value = 0f;
//          for (int i = 0; i < 1000; i++){value = math.exp10(math.sqrt(value));}
///        提升複雜度
///        所以去掉的話，Update反而跑的比Job快
///        因為Job會多一層分配手續，所以拖慢了速度
/// </summary>
public class JobTest4 : MonoBehaviour
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
                {
                    transform = Cube_Transform.GetComponent<Transform>(),
                    moveY = UnityEngine.Random.Range(1f, 2f)
                });
        }

    }

    // Update is called once per frame
    void Update()
    {
        float startTime = Time.realtimeSinceStartup;


        if (useJobs == true)
        {
            //設定臨時陣列Y軸移動速度
            NativeArray<float> moveYArray_Update = new NativeArray<float>(cube_gameObject_List.Count, Allocator.TempJob);
            //設定臨時陣列Transform組件
            TransformAccessArray transformAccessArray = new TransformAccessArray(cube_gameObject_List.Count);

            //將數據導入
            for (int i = 0; i < cube_gameObject_List.Count; i++)
            {
                transformAccessArray.Add(cube_gameObject_List[i].transform);
                moveYArray_Update[i] = cube_gameObject_List[i].moveY;
            }


            //是否啟用Burst
            if (useBurst == true)
            {
                ReallyJob_4_IJobParallelForTransform_BurstCompile reallyJob_4_IJobParallelForTransform_BurstCompile = new ReallyJob_4_IJobParallelForTransform_BurstCompile
                {
                    deltaTime = Time.deltaTime,
                    moveYArray = moveYArray_Update,
                };

                JobHandle jobHandle = reallyJob_4_IJobParallelForTransform_BurstCompile.Schedule(transformAccessArray);
                jobHandle.Complete();
            }
            else
            {
                ReallyJob_4_IJobParallelForTransform reallyJob_4_IJobParallelForTransform = new ReallyJob_4_IJobParallelForTransform
                {
                    deltaTime = Time.deltaTime,
                    moveYArray = moveYArray_Update,
                };

                JobHandle jobHandle = reallyJob_4_IJobParallelForTransform.Schedule(transformAccessArray);
                jobHandle.Complete();
            }

            //讓方塊移動
            // 因為IJobParallelFor只算好數據，移動還是要用For迴圈在處理一次
            for (int i = 0; i < cube_gameObject_List.Count; i++)
            {
                //print(cube_gameObject_List[i].transform.position + "  " +positionArray_Update[i]);
                //cube_gameObject_List[i].transform.position = positionArray_Update[i];
                cube_gameObject_List[i].moveY = moveYArray_Update[i];
            }
            //釋放陣列
            transformAccessArray.Dispose();
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

        }


        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }



}

/// <summary>
///
/// </summary>
public struct ReallyJob_4_IJobParallelForTransform : IJobParallelForTransform
{
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

    public void Execute(int index, TransformAccess transform)
    {
        //位移
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0f);
        //檢查速度
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        //增加複雜度
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}


[BurstCompile]
public struct ReallyJob_4_IJobParallelForTransform_BurstCompile : IJobParallelForTransform
{
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

    public void Execute(int index, TransformAccess transform)
    {
        //位移
        transform.position += new Vector3(0, moveYArray[index] * deltaTime, 0f);
        //檢查速度
        if (transform.position.y > 5f)
        {
            moveYArray[index] = -math.abs(moveYArray[index]);
        }
        if (transform.position.y < -5f)
        {
            moveYArray[index] = +math.abs(moveYArray[index]);
        }
        //增加複雜度
        float value = 0f;
        for (int i = 0; i < 1000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }
}
