using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInsPerlin : MonoBehaviour
{
    //躁點
    public float perlinNoise = 0f;
    //躁點偏移
    //(平滑度(因為Mathf.PerlinNoise在噪點圖來看灰階很平滑，所以i * refinement, j * refinement 來看成的數值越大月不平滑))
    public float refinement = 0f;
    //高度
    public float multiplier = 0f;
    //方塊數量
    public int cube = 0;
    //方塊
    public GameObject cubeprefab;


    // Start is called before the first frame update
    void Start()
    {
        //生成方塊
        for (int i = 0; i < cube; i++)
        {
            for (int j = 0; j < cube; j++)
            {
                //生成方塊躁點
                perlinNoise = Mathf.PerlinNoise(i * refinement, j * refinement);
                //生成方塊
                GameObject go = GameObject.Instantiate(cubeprefab);
                //偏移位置
                go.transform.position = new Vector3(
                    i, perlinNoise * multiplier, j);
                //把躁點資訊家道加到腳本裡用於移動
                go.GetComponent<CubeMovePerlin>().high = multiplier;
                go.GetComponent<CubeMovePerlin>().PerlinVec2 = new Vector2(i * refinement, j * refinement);
            }
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
