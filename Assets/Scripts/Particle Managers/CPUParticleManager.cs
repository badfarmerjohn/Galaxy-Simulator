using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUParticleManager : MonoBehaviour
{
    public int initialParticleCount = 20000;
    public string shader = "Legacy Shaders/Particles/Alpha Blended";

    ParticleSystem ps;
    ParticleSystemRenderer ps_renderer;
    Material ps_material;

    ParticleSystem.Particle[] _particles;
    int num_particles = 0;

    bool needs_update = false;
    int new_particle_count = 0;


    // Start is called before the first frame update
    void Start()
    {
        _particles = new ParticleSystem.Particle[initialParticleCount];
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }
        ps_renderer = GetComponent<ParticleSystemRenderer>();
        InitializeParticleSystem(ps, ps_renderer, initialParticleCount);
        ps.SetParticles(_particles, num_particles, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if (needs_update)
        {
            num_particles = Mathf.Clamp(new_particle_count, 0, _particles.Length);
            ps.SetParticles(_particles, num_particles, 0);
            needs_update = false;
        }
    }

    public ParticleSystem.Particle[] GetParticles()
    {
        return _particles;
    }

    public int Size()
    {
        return num_particles;
    }

    public void SetParticles(ParticleSystem.Particle[] particles, int num_particles)
    {
        this._particles = particles;
        OnParticlesChanged(num_particles);
    }

    public void AddParticle(ParticleSystem.Particle new_particle)
    {
        EnsureSize(new_particle_count + 1);
        _particles[num_particles] = new_particle;
        OnParticlesChanged(num_particles + 1);
    }

    public void AddParticles(ParticleSystem.Particle[] new_particles)
    {
        int new_particle_count = num_particles + new_particles.Length;
        EnsureSize(num_particles + new_particles.Length);
        System.Array.Copy(new_particles, 0, _particles, num_particles, new_particles.Length);
        OnParticlesChanged(new_particle_count);
    }

    public void OnParticlesChanged(int new_particle_count)
    {
        this.new_particle_count = new_particle_count;
        needs_update = true;
    }

    public void EnsureSize(int new_size)
    {
        ParticleSystem.Particle[] new_container = _particles;
        while (num_particles >= new_container.Length)
        {
            new_container = new ParticleSystem.Particle[new_container.Length * 2];
        }
        if (new_container != _particles)
        {
            for (int i = 0; i < num_particles; i++)
            {
                new_container[i] = _particles[i];
            }
            _particles = new_container;
        }
    }

    public void Clear()
    {
        OnParticlesChanged(0);
    }

    public void SetDefaultColor(Color color)
    {
        ps_material.SetColor("_Color", color);
        ps_renderer.material = ps_material;
        Debug.Log("Setting color");
    }

    public void SetDefaultEmission(Color color)
    {
        ps_material.SetColor("_EmissionColor", color);
    }

    /****************
     * The Following code is taken from the git repository 
     * https://github.com/jeshlee121/ISAACS-RadiationVisualization.git
     * *************/

    protected static void PrepareMaterial(Material particleMaterial)
    {
        // Make it transparent
        particleMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        particleMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.DstAlpha);
        particleMaterial.SetInt("_ZWrite", 0);
        particleMaterial.EnableKeyword("_ALPHATEST_ON");
        particleMaterial.EnableKeyword("_ALPHABLEND_ON");
        particleMaterial.EnableKeyword("_ALPHAPREMULTIPLY_ON");
        particleMaterial.renderQueue = 3000;
    }

    protected void InitializeParticleSystem(ParticleSystem ps, ParticleSystemRenderer renderer, int max_particles)
    {
        ParticleSystem.MainModule main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.maxParticles = max_particles;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(1.0f, 1.0f, 1.0f, 0.0f));

        renderer.sortMode = ParticleSystemSortMode.Distance;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        renderer.renderMode = ParticleSystemRenderMode.Mesh;

        GameObject gameObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        renderer.mesh = gameObj.GetComponent<MeshFilter>().mesh;
        Destroy(gameObj);
        renderer.enableGPUInstancing = true;

        renderer.alignment = ParticleSystemRenderSpace.World;

        ps_material = new Material(Shader.Find(shader));
        renderer.material = ps_material;

        PrepareMaterial(ps_material);

        SetDefaultColor(new Color(1.0f, 1.0f, 1.0f, 1.0f));
        SetDefaultEmission(new Color(0.5f, 0.5f, 0.5f, 0.8f));

        ParticleSystem.EmissionModule em = ps.emission;
        em.enabled = false;
        ParticleSystem.ShapeModule sh = ps.shape;
        sh.enabled = false;
        ParticleSystem.VelocityOverLifetimeModule vol = ps.velocityOverLifetime;
        vol.enabled = false;
        ParticleSystem.LimitVelocityOverLifetimeModule lvol = ps.limitVelocityOverLifetime;
        lvol.enabled = false;
        ParticleSystem.InheritVelocityModule ivm = ps.inheritVelocity;
        ivm.enabled = false;
        ParticleSystem.ForceOverLifetimeModule fol = ps.forceOverLifetime;
        fol.enabled = false;
        ParticleSystem.ColorOverLifetimeModule col = ps.colorOverLifetime;
        col.enabled = false;
        ParticleSystem.ColorBySpeedModule cbs = ps.colorBySpeed;
        cbs.enabled = false;
        ParticleSystem.SizeOverLifetimeModule sol = ps.sizeOverLifetime;
        sol.enabled = false;
        ParticleSystem.SizeBySpeedModule sbs = ps.sizeBySpeed;
        sbs.enabled = false;
        ParticleSystem.RotationOverLifetimeModule rol = ps.rotationOverLifetime;
        rol.enabled = false;
        ParticleSystem.RotationBySpeedModule rbs = ps.rotationBySpeed;
        rbs.enabled = false;
        ParticleSystem.ExternalForcesModule extf = ps.externalForces;
        extf.enabled = false;
        ParticleSystem.NoiseModule noise = ps.noise;
        noise.enabled = false;
        ParticleSystem.CollisionModule collision = ps.collision;
        collision.enabled = false;
        ParticleSystem.TriggerModule triggers = ps.trigger;
        triggers.enabled = false;
        ParticleSystem.SubEmittersModule subem = ps.subEmitters;
        subem.enabled = false;
        ParticleSystem.TextureSheetAnimationModule tsa = ps.textureSheetAnimation;
        tsa.enabled = false;
        ParticleSystem.LightsModule lights = ps.lights;
        lights.enabled = false;
        ParticleSystem.CustomDataModule cd = ps.customData;
        cd.enabled = false;
    }
}
