# Galaxy-Simulator
A visually compelling galaxy simulator. Not physically accurate though.

# Final Project - Galaxy Simulator
## Jeromy Lui | Ryan Meyer | Eric Zhang

---

### Summary
In this project, we hope to build a simple galaxy simulator that is visually compelling. Galaxies are a collection of stars, solar systems, dust, and other celestial bodies, which at the scale of the visualization, can be thought of as individual particles. In this project, we will explore methods of approximating the gravitational interactions between the particles to obtain a visually compelling simulation without the computational expense of a physically accurate one. Specifically, we will examine ways to accelerate the simulation calculations through efficient data structures as well as what corrections are needed to account for discrete approximation errors (similar to in spring simulation).

### Problem Description
Theoretically, simulating galaxies can be done precisely with enough particle granularity.
* Simulate each gaseous body as a system of particles.
* Simulate space dust as individual particles.
* Simulate all solid celestial bodies as single particles.

However, as there are millions of such such celestial bodies in a galaxy, this would be highly impractical to do all the gravity/collision calculations on a normal computer for visuals.  

One problem we will try to solve is finding the right granularity of simulation to allow for realtime, pseudo-realistic visuals on a typical computer (with GPU). An example of this could be abstracting gaseous celestial objects (like stars) as a single light-emitting particle. we will be exploring other visual tricks, such as rendering space dust as some combination of particles and fog to reduce number of particles necessary while still maintaining the visual appearance.  

Another problem we will tackle is accelerating the physics calculations. For example, in a physically accurate simulation, each particle's gravitational pull is applied to every other particle in the simulation. However, for visual purposes, we may limit a particle's effect to particles within a limited vicinity. As such, new data structures will need to be applied to support such speed-ups.

### Goals and Deliverables
1) **Galaxy Simulator**: interactive executable as described in the summary.
    * Default mode displays the rotating galaxy in all its glory.
    * Interactive mode will allow the user to control a free-floating first person camera. 
2) **Performance Measurement**: We will measure the performance of the system by running our code and seeing how many particles we can simulate. We will create a scatterplot of the particle/object count aginst framerate, and then compare the performance for a CPU-based simulator versus GPU-based simulator.
    * The results should be that the framerate for the CPU-based simulator will decrease drastically after around 100,000 particles.
    * However, for the GPU-based simulator, the decrease in framerate should be much less impacted/drastic.

   In terms of the quality, we will be comparing the images from our simulator with that of real images of galaxies.
3) **Screensaver Feature**: If we get ahead of schedule and all things go according to plan, we hope to add an additional screen save feature.
4) **Increased Variation**: We may also add more variation to the procedurally-generated galaxies.
5) **Zoom Feature (stretch goal)**: If the other deliverables and goals are met ahead of schedule, we may add a feature to zoom into galactic objects, using some LOD heuristic to switch between rendering celestial bodies as tiny particles, and rendering them as meshed and/or textured objects at different distances.

### Schedule
* **4/14-4/20:** Research and get familiar Unity, visual effects graphs, and the other resources listed. Get basic particles to appear in the simulation and apply shaders.
* **4/21-4/27:** Have a basic implementation of a galaxy simulator for a CPU-based approach and be around halfway done with GPU-based approach.
* **4/28-5/4:** Finish GPU-based approach for galaxy simulator, along with the performance measurements.
* **5/5-5/9:** Prepare presentation for poster session. Work on any additional features beyond planned ones.
* **5/10-5/14** Finish remaining deliverables (video, webpage) and submit.

### Resources
* **Unity Game Engine**: we will be using Unity as our development platform as it has a robust rendering and physics pipeline, and has cross-platform compatibility.
  * Visual Effects Graph: This is a new tool developed by Unity for its game engine. One of its core features is being able to render and manage a large number of particles.
* **Literature**: we will refer to some published work on the topics of computational astrophysics and methods of gaining efficiency in particle simulation to guide our project.
    * [PKDGRAV3](https://comp-astrophys-cosmol.springeropen.com/track/pdf/10.1186/s40668-017-0021-1): This research details the methods used by its authors to produce a highly performant simulation on an order of trillions of particles. While these computations were made on a supercomputer, we may still be able adapt many of their ideas to a smaller scale simulation on common hardware.
    * [Strategies for Efficient Particle Resolution in the Direct Simulation Monte Carlo Method](https://www.sciencedirect.com/science/article/pii/S0021999199963970): This paper reflects on the computational cost of Monte Carlo simulation for particle regions of varying density.
    * [An Efficient Program for Many-Body Simulation](https://epubs.siam.org/doi/10.1137/0906008): The authors of this research use a tree structure to improve the asymptotic runtime complexity of N-Body dynamics systems. This result may aid in computing gravitational forces for millions of particles in a galaxy.
