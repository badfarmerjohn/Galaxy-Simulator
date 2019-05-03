using System;
using UnityEngine;

public class PhysicalParticleSimulator
{
    float GRAVITATIONAL_CONSTANT = 6.67408e-11f;
    Vector3 center_of_mass;
    float total_mass;
    uint num_particles;
    PhysicalParticle[] physical_particles;

    public PhysicalParticleSimulator(PhysicalParticle[] particles = null, float gravitational_constant = 6.67408e-11f)
    {
        physical_particles = particles;
        GRAVITATIONAL_CONSTANT = gravitational_constant;
        if (physical_particles != null)
        {
            num_particles = (uint) physical_particles.Length;
            ComputeCenterOfMass();
            SetInitialVelocities();
        }
        else
        {
            num_particles = 0;
            total_mass = 0;
            center_of_mass = new Vector3();
        }
    }

    public void SetParticles (PhysicalParticle[] particles)
    {
        physical_particles = particles;
        num_particles = (uint) physical_particles.Length;
    }

    public void PerformTimestep(float deltaT)
    {
        if (physical_particles == null)
        {
            return;
        }
        for (uint i = 0; i < num_particles; ++i)
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
            Vector3 position_difference = physical_particles[j].position - p_i.position;
            float distance = position_difference.magnitude;
            float pd_cubed = distance * distance * distance;
            force += GRAVITATIONAL_CONSTANT * p_i.mass * physical_particles[j].mass / pd_cubed * position_difference;
        }
        for (uint j = index + 1; j < num_particles; ++j) //messier to write 2 separate for loops but more optimized
        {
            Vector3 position_difference = physical_particles[j].position - p_i.position;
            float distance = position_difference.magnitude;
            float pd_cubed = distance * distance * distance;
            force += GRAVITATIONAL_CONSTANT * p_i.mass * physical_particles[j].mass / pd_cubed * position_difference;
        }
        return force;
    }

    void ComputeCenterOfMass()
    {
        total_mass = 0f;
        Vector3 weighted_positions = new Vector3();
        for (uint i = 0; i < physical_particles.Length; ++i)
        {
            total_mass += physical_particles[i].mass;
            weighted_positions += physical_particles[i].mass * physical_particles[i].position;
        }
        center_of_mass = weighted_positions / total_mass;
    }

    void SetInitialVelocities()
    {
        if (total_mass == 0f || num_particles == 0)
        {
            for (uint i = 0; i < num_particles; ++i)
            {
                physical_particles[i].velocity = new Vector3();
            }
            return;
        }
        if (center_of_mass == null)
        {
            ComputeCenterOfMass();
        }
        for (uint i = 0; i < num_particles; ++i)
        {
            Vector3 dist_vector = center_of_mass - physical_particles[i].position;
            float R = dist_vector.magnitude;
            float velocity_magnitude = (float) Math.Sqrt(GRAVITATIONAL_CONSTANT * total_mass / R);
            Vector3 y_normal = new Vector3(0, 1, 0);
            Vector3 velocity_direction = new Vector3(dist_vector.x / dist_vector.z, 0, dist_vector.z / dist_vector.x);
            Vector3.OrthoNormalize(ref y_normal, ref dist_vector, ref velocity_direction);
            physical_particles[i].velocity = velocity_direction * velocity_magnitude;
        }
    }

}
