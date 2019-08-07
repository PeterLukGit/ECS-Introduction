using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Unity.Transforms;

public class MoveSystem : ComponentSystem
{
    //移動System
    protected override void OnUpdate()
    {
        //尋找有 Translation MoveSpeedComponent 兩個組件的Entities
        Entities.ForEach((ref Translation translation , ref MoveSpeedComponent moveSpeedComponent) =>
        {
            //移動
            translation.Value.y += moveSpeedComponent.speed * Time.deltaTime;

            //當超過一定範圍就反過來跑
            if((translation.Value.y > 5f)|| (translation.Value.y < (-5f)))
            {
                //Debug.Log(moveSpeedComponent.speed + " " + moveSpeedComponent.speed);
                moveSpeedComponent.speed = -moveSpeedComponent.speed;
            }

        });

    }
}
