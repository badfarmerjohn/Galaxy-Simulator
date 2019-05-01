using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticleTest : MonoBehaviour
{
    public int amount = 1000;

    GPUParticleManager manager;

    int iteration = 0;

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
                particles[i] = new Vector3(Random.value, Random.value, Random.value);
            }
            Color[] colors = new Color[amount];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Random.ColorHSV();
            }
            manager.AddPoints(particles, colors: colors);
        }
        if (Input.GetKeyUp(KeyCode.Delete))
        {
            manager.Clear();
        }
    }
}
