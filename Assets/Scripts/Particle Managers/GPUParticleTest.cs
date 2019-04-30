using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticleTest : MonoBehaviour
{
    GPUParticleManager manager;

    int amount = 2;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<GPUParticleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            Vector3[] particles = new Vector3[amount];
            for (int i = 0; i < particles.Length; i++)
            {
                particles[i] = 1 * (new Vector3(Random.value, Random.value, Random.value));
            }
            manager.AddPoints(particles);
            //amount += 10;
        }
    }
}
