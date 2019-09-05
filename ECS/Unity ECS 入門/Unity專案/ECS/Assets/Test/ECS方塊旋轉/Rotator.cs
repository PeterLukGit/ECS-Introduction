using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;


public class Rotator : MonoBehaviour
{
    //Component 資料
    public float speed;
}

class RotatorSystem : ComponentSystem
{

    //struct Component
    //{
    //    public Rotator rotator;
    //    public Transform transform;
    //}

    /// <summary>
    /// System裡的OnUpdate 跟Update一樣
    /// </summary>
    protected override void OnUpdate()
    {
        //deltaTime 拉出來
        float deltaTime = Time.deltaTime;

        //var entitis = GetEntities<Component>();

        //foreach (var entity in GetEntities<Component>())//获取所有指定的Entity
        //{

        //}

        //每禎搜尋含有Transform組件 , Rotator 組件的Entity實體
        //然後旋轉，速度是吃Rotator 組件裡的變數
        //不用加ref是因為Transform 屬於 Class 如果是struct就需要
        Entities.ForEach((Entity entity, Transform transform, Rotator rotator) =>
        {
            transform.Rotate(0f, rotator.speed * deltaTime, 0f);
        });

        //Entities.ForEach(( Transform transform, Rotator rotator) =>
        //{
        //    transform.Rotate(0f, rotator.speed * deltaTime, 0f);
        //});


    }
}