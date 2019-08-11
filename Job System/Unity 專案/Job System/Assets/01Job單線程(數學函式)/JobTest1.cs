using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

/// <summary>
/// Job單線程
/// 講解:這邊用數學函式來測試
///普通Mono執行 跟 Job執行差異
///但看出來並沒有差，原因在於這是單線程
///因為函式只執行一組，所以Job設定成單線程情況下，
///運行並沒有差
/// </summary>
public class JobTest1 : MonoBehaviour
{
    [SerializeField]
    [Header("啟用Job系統")]
    private bool useJobs;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float startTime = Time.realtimeSinceStartup;

        if (useJobs == true)
        {
            JobHandle job = ReadyJob();
            //確保Job工作已完成，不加Job不執行函式運算
            job.Complete();
        }
        else
        {
            ReallyToughTask();
        }

        //計算用時
        Debug.Log(((Time.realtimeSinceStartup - startTime) * 1000f) + "ms");
    }

    /// <summary>
    /// 複雜運算，在這邊用於測試Job性能
    /// </summary>
    private void ReallyToughTask()
    {
        // Represents a tough task like some pathfinding or a really complex calculation
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

    //設定Job 因為Job用struct設定，所以要先實例化
    private JobHandle ReadyJob()
    {
        ReallyJob_1 job = new ReallyJob_1();
        return job.Schedule();
    }

 

}

/// <summary>
/// 設定Job
/// </summary>
public struct ReallyJob_1 : IJob
{
    /// <summary>
    /// 設定Job執行的函式
    /// </summary>
    public void Execute()
    {
        //複雜函式
        // Represents a tough task like some pathfinding or a really complex calculation
        float value = 0f;
        for (int i = 0; i < 50000; i++)
        {
            value = math.exp10(math.sqrt(value));
        }
    }

}
