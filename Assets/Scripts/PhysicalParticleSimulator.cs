using System;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalParticleSimulator
{
    float GRAVITATIONAL_CONSTANT = 6.67408e-11f;
    Vector3 center_of_mass;
    float total_mass;
    uint num_particles;
    PhysicalParticle[] physical_particles;

    class ParticleAggregate
    {
        public List<PhysicalParticle> particles = new List<PhysicalParticle>();
        public Vector3 center_of_mass = new Vector3();
        public float total_mass = 0;
    }

    public enum SimulatorAlgorithm
    {
        NAIVE,
        HASHING
    };
    SimulatorAlgorithm simulation_algorithm;

    public PhysicalParticleSimulator(SimulatorAlgorithm method = SimulatorAlgorithm.NAIVE, PhysicalParticle[] particles = null, float gravitational_constant = 6.67408e-11f)
    {
        simulation_algorithm = method;

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

    public void SetSimulationAlgorithm(SimulatorAlgorithm method)
    {
        simulation_algorithm = method;
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
        if (simulation_algorithm == SimulatorAlgorithm.NAIVE)
        {
            for (uint i = 0; i < num_particles; ++i)
            {
                Vector3 force_on_particle_i = NaiveForceOnParticle(i);
                ApplyForce(physical_particles[i], force_on_particle_i, deltaT);
            }
        } else if (simulation_algorithm == SimulatorAlgorithm.HASHING)
        {
            Dictionary<int, ParticleAggregate> spatial_hash = PartitionParticles(physical_particles, new Vector3(1f, 1f, 1f));
            foreach (KeyValuePair<int, ParticleAggregate> curr_cell in spatial_hash)
            {
                ParticleAggregate curr_bucket = curr_cell.Value;
                foreach (PhysicalParticle particle in curr_bucket.particles)
                {
                    Vector3 force = HashingForce(spatial_hash, curr_cell.Value, particle, deltaT);
                    ApplyForce(particle, force, deltaT);
                }
            }
        }
    }

    void ApplyForce(PhysicalParticle particle, Vector3 total_force, float dt)
    {
        particle.position += particle.velocity * dt;
        particle.velocity += total_force / particle.mass * dt;
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
            force += p_i.mass * physical_particles[j].mass / pd_cubed * position_difference;
        }
        for (uint j = index + 1; j < num_particles; ++j) //messier to write 2 separate for loops but more optimized
        {
            Vector3 position_difference = physical_particles[j].position - p_i.position;
            float distance = position_difference.magnitude;
            float pd_cubed = distance * distance * distance;
            force += p_i.mass * physical_particles[j].mass / pd_cubed * position_difference;
        }
        return force * GRAVITATIONAL_CONSTANT;
    }

    Vector3 HashingForce(Dictionary<int, ParticleAggregate> spatial_hash, ParticleAggregate curr_bucket, PhysicalParticle particle, float deltaT)
    {
        ParticleAggregate other_bucket;
        Vector3 total_force = Vector3.zero;
        foreach (KeyValuePair<int, ParticleAggregate> other_cell in spatial_hash)
        {
            other_bucket = other_cell.Value;
            if (curr_bucket == other_bucket)
            {
                continue;
            }
            Vector3 position_difference = other_bucket.center_of_mass - particle.position;
            float distance = position_difference.magnitude;
            float pd_cubed = distance * distance * distance;
            total_force += other_bucket.total_mass * particle.mass / pd_cubed * position_difference;
        }
        foreach (PhysicalParticle other_particle in curr_bucket.particles)
        {
            if (other_particle == particle)
            {
                continue;
            }
            Vector3 position_difference = other_particle.position - particle.position;
            float distance = position_difference.magnitude;
            float pd_cubed = distance * distance * distance;
            total_force += other_particle.mass * particle.mass / pd_cubed * position_difference;
        }
        total_force *= GRAVITATIONAL_CONSTANT;
        particle.velocity += total_force / particle.mass * deltaT;
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

    Dictionary<int, ParticleAggregate> PartitionParticles(PhysicalParticle[] particles, Vector3 cell_size)
    {

        Dictionary<int, ParticleAggregate> spatial_hash = new Dictionary<int, ParticleAggregate>();
        for (int i = 0; i < particles.Length; i++)
        {
            PhysicalParticle p = particles[i];
            int x_ind = Mathf.FloorToInt(p.position.x / cell_size.x);
            int y_ind = Mathf.FloorToInt(p.position.y / cell_size.y);
            int z_ind = Mathf.FloorToInt(p.position.z / cell_size.z);
            int hash = x_ind * 113 + y_ind * 12769 + z_ind * 1442897;

            ParticleAggregate bucket;
            if (!spatial_hash.TryGetValue(hash, out bucket))
            {
                bucket = new ParticleAggregate();
                spatial_hash[hash] = bucket;
            }
            bucket.particles.Add(p);
            bucket.center_of_mass += p.position * p.mass;
            bucket.total_mass += p.mass;
        }
        foreach (KeyValuePair<int, ParticleAggregate> pair in spatial_hash)
        {
            pair.Value.center_of_mass /= pair.Value.total_mass;
        }
        return spatial_hash;
    }

}
