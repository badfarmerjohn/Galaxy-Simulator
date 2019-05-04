using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GPUParticleTest : MonoBehaviour
{
    public int amount = 1000;

    GPUParticleManager manager;

    //int iteration = 0;

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
                particles[i] = 5 * new Vector3(Random.value, Random.value, Random.value);
            }
            Color[] colors = new Color[amount];
            for (int i = 0; i < colors.Length; i++)
            {
                colors[i] = Random.ColorHSV();
            }
            Vector3[] scales = new Vector3[amount];
            Vector3 baseSize = 0.01f * Vector3.one;
            for (int i = 0; i < particles.Length; i++)
            {
                scales[i] = 0.05f * (new Vector3(Random.value, Random.value, Random.value)) + baseSize;
            }
            manager.AddPoints(particles, scales: scales, colors: colors);
        }
        if (Input.GetKeyUp(KeyCode.Delete))
        {
            manager.Clear();
        }
    }
}
