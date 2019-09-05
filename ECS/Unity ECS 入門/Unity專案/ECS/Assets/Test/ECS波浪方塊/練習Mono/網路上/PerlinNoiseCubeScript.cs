using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseCubeScript : MonoBehaviour
{
    public float perlinNoise = 0f;
    public float refinement = 0f;
    public float multiplier = 0f;
    public int cube = 0;

    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i<cube;i++)
        {
            for(int j = 0; j<cube;j++)
            {
                perlinNoise = Mathf.PerlinNoise(i * refinement, j * refinement);
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = new Vector3(
                    i,perlinNoise*multiplier,j);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
