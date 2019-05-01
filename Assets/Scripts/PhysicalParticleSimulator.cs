using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalParticleSimulator
{
  public const double GRAVITATIONAL_CONSTANT = 1.0;
  public PhysicalParticle[] physical_particles;

  void ApplyForce(uint index, Vector3 total_force, float dt)
  {
    physical_particles[i].position += physical_particles[i].velocity * dt;
    physical_particles[i].velocity += total_force / physical_particles[i].mass * dt;
  }

  Vector3 NaiveForceOnParticle(uint index)
  {
    Vector3 force = new Vector3();
    PhysicalParticle p_i = physical_particles[index];
    for (int j = 0; j < index; ++j)
    {
      Vector3 position_difference = p_i.position - physical_particles[j].position;
      force += GRAVITATIONAL_CONSTANT * physical_particles[j].mass * (position_difference / position_difference.mag);
    }
  }

}
