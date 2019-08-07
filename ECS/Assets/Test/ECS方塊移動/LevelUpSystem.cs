using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;

public class LevelUpSystem : ComponentSystem
{
    //數值累加System
    protected override void OnUpdate()
    {
        //搜尋Entities找 LevelComponent組件
        Entities.ForEach((ref LevelComponent levelComponent) =>
        {
            levelComponent.level += 1 * Time.deltaTime;
            Debug.Log(levelComponent.level);
        });


    }
}
