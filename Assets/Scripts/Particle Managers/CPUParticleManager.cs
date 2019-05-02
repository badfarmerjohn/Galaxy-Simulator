using UnityEngine;
using UnityEngine.Assertions;

public class CPUParticleManager : MonoBehaviour
{
    public int initialParticleCount = 20000;
    public string shader = "Legacy Shaders/Particles/Alpha Blended";

    ParticleSystem ps;
    ParticleSystemRenderer ps_renderer;
    Material ps_material;

    ParticleSystem.Particle[] _particles = null;
    int num_particles = 0;

    bool needs_update = false;
    int new_particle_count = 0;

    float last_update_time;
    float update_interval = 5;


    // Start is called before the first frame update
    void Start()
    {
        if (_particles != null)
        {
            _particles = new ParticleSystem.Particle[initialParticleCount];
        }
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
        {
            ps = gameObject.AddComponent<ParticleSystem>();
        }
        ps_renderer = GetComponent<ParticleSystemRenderer>();
        InitializeParticleSystem(ps, ps_renderer, initialParticleCount);
        ps.SetParticles(_particles, num_particles, 0);

        last_update_time = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (_particles != null && (needs_update || Time.time - last_update_time > update_interval))
        {
            num_particles = Mathf.Clamp(new_particle_count, 0, _particles.Length);
            ps.SetParticles(_particles, num_particles, 0);

            needs_update = false;
            last_update_time = Time.time;
        }
    }

    public int Size()
    {
        return num_particles;
    }

    public void SetParticles(PhysicalParticle[] particles)
    {
        EnsureSize(particles.Length);
        for (int i = 0; i < particles.Length; i++)
        {
            _particles[i].position = particles[i].position;
            _particles[i].startColor = particles[i].color;
            _particles[i].startSize = particles[i].size;
        }
        OnParticlesChanged(particles.Length);
    }

    public void AddParticles(PhysicalParticle[] particles, int start_ind, int length)
    {
        EnsureSize(num_particles + length);
        for (int i = 0, j = start_ind, k = num_particles; i < length; i++, j++, k++)
        {
            _particles[k].position = particles[j].position;
            _particles[k].startColor = particles[j].color;
            _particles[k].startSize = particles[j].size;
        }
        OnParticlesChanged(num_particles + length);
    }

    public void UpdateParticlePositions(PhysicalParticle[] particles)
    {
        Assert.AreEqual(particles.Length, _particles.Length,
            "Number of PHysicalParticles must == number of ParticleSystem.Particles");
        for (int i = 0; i < particles.Length; i++)
        {
            _particles[i].position = particles[i].position;
        }
        OnParticlesChanged(particles.Length);
    }

    void OnParticlesChanged(int new_particle_count)
    {
        this.new_particle_count = new_particle_count;
        needs_update = true;
    }

    void EnsureSize(int new_size)
    {
        if (_particles == null)
        {
            _particles = new ParticleSystem.Particle[new_size];
            return;
        }
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
