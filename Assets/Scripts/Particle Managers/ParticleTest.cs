using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleTest : MonoBehaviour
{
    CPUParticleManager manager;

    // Start is called before the first frame update
    void Start()
    {
        manager = GetComponent<CPUParticleManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            ParticleSystem.Particle p = new ParticleSystem.Particle();
            p.position = new Vector3(Random.value, Random.value, Random.value);
            p.startSize = 1f;
            p.startColor = new Color(1, 0, 0, 0.2f);
            manager.AddParticle(p);
        }
    }
}
