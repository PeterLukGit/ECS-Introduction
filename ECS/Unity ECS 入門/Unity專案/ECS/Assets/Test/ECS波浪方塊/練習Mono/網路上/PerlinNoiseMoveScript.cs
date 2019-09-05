using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseMoveScript : MonoBehaviour
{
    public float elepsedTime = 0;
    public float perlinNoise = 0;
    public float multiplier = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        elepsedTime = Time.time;
        perlinNoise = Mathf.PerlinNoise(transform.position.x * multiplier + elepsedTime, transform.position.z * multiplier + elepsedTime);

        transform.position = new Vector3(
            transform.position.x,
            perlinNoise,
            transform.position.z);
    }
}
