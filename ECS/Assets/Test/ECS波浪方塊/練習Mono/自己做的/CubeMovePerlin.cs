using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovePerlin : MonoBehaviour
{
    //紀錄躁點位置
    public Vector2 PerlinVec2;
   
    //方塊震幅多大
    public float high;
    //時間
    public float elepsedTime = 0;
    //躁點output
    public float perlinNoise = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //獲取時間
        elepsedTime = Time.time;
        //取得躁點的下一步(這裡下一步是用Time來代替)
        perlinNoise = Mathf.PerlinNoise(PerlinVec2.x + elepsedTime, PerlinVec2.y+ elepsedTime);
        //偏移
        transform.position = new Vector3(
          transform.position.x,
          perlinNoise * high,
          transform.position.z);

    }
}
