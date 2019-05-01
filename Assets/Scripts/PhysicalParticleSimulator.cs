﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicalParticleSimulator : MonoBehaviour
{
  public const float GRAVITATIONAL_CONSTANT = 1.0f;
  public PhysicalParticle[] physical_particles;

  void Update()
  {
    float dt = Time.deltaTime;
    for (uint i = 0; i < physical_particles.Length; ++i)
    {
      Vector3 force_on_particle_i = NaiveForceOnParticle(i);
      ApplyForce(i, force_on_particle_i, dt);
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
      Vector3 position_difference = p_i.position - physical_particles[j].position;
      float pd_cubed = position_difference.magnitude * position_difference.magnitude * position_difference.magnitude;
      force += GRAVITATIONAL_CONSTANT * physical_particles[j].mass * (position_difference / pd_cubed);
    }
    return force;
  }

}
