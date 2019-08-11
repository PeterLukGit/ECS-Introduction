using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;
using Unity.Burst;

/// <summary>
///Job多線程
///講解:
///這邊跟單線程比就看的到差異
///原因很簡單，因為函式不是一個而是十個
///單線程必須一個一個執行
///多線程就可以多執行所以比單線程快
///至於Burst這邊用另Bool控制讀入的Job函式來表現
///
/// PS:這邊根據教學是60ms ==> 14ms
///       但我測到市 60ms ==> 30ms
///       推論是電腦差異
///       
///PS2:Burst不知為何測試三次後才有效果出來，推論是電腦差異
///
/// </summary>
public class JobTest2 : MonoBehaviour
{
    [SerializeField]
    [Header("啟用Job系統")]
    private bool useJobs;

   [SerializeField]
    [Header("啟用Burst系統")]
    private bool useBurst;

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
            //建立陣列存放Job函式
            //Allocator.Temp是臨時陣列，所以需要釋放掉
            NativeList<JobHandle> jobHandleList = new NativeList<JobHandle>(Allocator.Temp);

            //是否啟用Burst
            if (useBurst == true)
            {
                for (int i = 0; i < 10; i++)
                {
                    //Job+Burst加入陣列
                    JobHandle jobHandle_BurstCompile = ReadyJob_BurstCompile();
                    jobHandleList.Add(jobHandle_BurstCompile);
                }
            }
            else
            {
                for (int i = 0; i < 10; i++)
                {
                    //單純Job加入陣列
                    JobHandle jobHandle = ReadyJob();
                    jobHandleList.Add(jobHandle);
                }
            }

            //執行
            JobHandle.CompleteAll(jobHandleList);
            //釋放陣列
            jobHandleList.Dispose();

        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                ReallyToughTask();
            }
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
        ReallyJob_2 job = new ReallyJob_2();
        return job.Schedule();
    }

    //設定Job 因為Job用struct設定，所以要先實例化
    private JobHandle ReadyJob_BurstCompile()
    {
        ReallyJob_2_Burst job = new ReallyJob_2_Burst();
        return job.Schedule();
    }



}

/// <summary>
/// 設定Job
/// </summary>
public struct ReallyJob_2 : IJob
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

/// <summary>
/// 設定BurstCompile Job
/// </summary>
[BurstCompile]
public struct ReallyJob_2_Burst: IJob
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
