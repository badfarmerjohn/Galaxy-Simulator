using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalParticleSimulator
{
    float GRAVITATIONAL_CONSTANT = 6.67408e-11f;

    PhysicalParticle[] physical_particles;

    void PhysicsParticleSimulator(PhysicalParticle[] particles = null, float gravitational_constant = 1.0f)
    {
        physical_particles = particles;
        GRAVITATIONAL_CONSTANT = gravitational_constant;
    }

    public void SetParticles (PhysicalParticle[] particles)
    {
        physical_particles = particles;
    }

    public void PerformTimestep(float deltaT)
    {
        if (physical_particles == null)
        {
            return;
        }
        for (uint i = 0; i < physical_particles.Length; ++i)
        {
            Vector3 force_on_particle_i = NaiveForceOnParticle(i);
            ApplyForce(i, force_on_particle_i, deltaT);
            physical_particles[i].position += physical_particles[i].velocity * deltaT;

            Debug.Log(physical_particles[i].velocity);
        }
    }

    void ApplyForce(uint index, Vector3 total_force, float dt)
    {
        physical_particles[index].position += physical_particles[index].velocity * dt;
        physical_particles[index].velocity += total_force / physical_particles[index].mass * dt;
    }

    Vector3 NaiveForceOnParticle(uint index)
    {
        Vector3 force = new Vector3();
        PhysicalParticle p_i = physical_particles[index];
        for (uint j = 0; j < index; ++j)
        {
            if (j == index)
            {
                continue;
            }
            Vector3 position_difference = physical_particles[j].position - p_i.position;
            float distance = position_difference.magnitude;
            float pd_cubed = distance * distance * distance;
            force += GRAVITATIONAL_CONSTANT * p_i.mass * physical_particles[j].mass / pd_cubed * position_difference;
        }
        return force;
    }

}
