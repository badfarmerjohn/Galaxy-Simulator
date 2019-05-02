using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GalaxyInitializer : MonoBehaviour
{
    public string galaxyFile = "Assets/Galaxy Files/galaxy_pix_array.txt";

    public int numCpuParticles = 0;
    public int numGpuParticles = 0;

    public float cellSize = 0.1f;

    float[] cumulative_row_densities;
    float[,] marginal_cumulative_col;

    CPUParticleManager cpu_particles_manager;
    GPUParticleManager gpu_particles_manager;

    bool added = false;

    // Start is called before the first frame update
    void Start()
    {
        ReadDistributionFile(galaxyFile);

        cpu_particles_manager = GetComponent<CPUParticleManager>();
        if (cpu_particles_manager == null)
        {
            cpu_particles_manager = gameObject.AddComponent<CPUParticleManager>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!added)
        {
            cpu_particles_manager.AddParticles(GenerateCpuParticles(numCpuParticles), 0, numCpuParticles);
            added = true;
        }
    }

    PhysicalParticle[] GenerateCpuParticles(int num_particles)
    {
        Debug.Log("Generating CPU Particles: " + num_particles);
        PhysicalParticle[] cpu_particles = new PhysicalParticle[num_particles];

        Vector3 center_offset = new Vector3(cumulative_row_densities.Length * 0.5f, 0, marginal_cumulative_col.GetLength(1) * 0.5f);
        float tileRadius2 = (0.15f * center_offset).sqrMagnitude;
        float gaussFactor = 1.0f / Mathf.Sqrt(Mathf.PI * tileRadius2) * 10000 * cellSize;

        for (int i = 0; i < num_particles; i++)
        {
            float rand1 = Random.value, rand2 = Random.value;
            int row;
            for (row = 1; row < cumulative_row_densities.Length; row++)
            {
                if (cumulative_row_densities[row] > rand1)
                {
                    break;
                }
            }
            row -= 1;

            int col, width = marginal_cumulative_col.GetLength(1);
            for (col = 1; col < width; col++)
            {
                if (marginal_cumulative_col[row, col] > rand2)
                {
                    break;
                }
            }
            col -= 1;

            Vector3 unit_position = new Vector3(row + Random.value, 0, col + Random.value);
            float y_range = Mathf.Exp(-(unit_position - center_offset).sqrMagnitude / tileRadius2) * gaussFactor;

            PhysicalParticle p = new PhysicalParticle();
            p.position = cellSize * (new Vector3(unit_position.x, (Random.value - 0.5f) * y_range, unit_position.z) - center_offset);
            p.color = Color.white;
            p.mass = 1;
            p.size = 0.01f;
            p.velocity = Vector3.zero;
            cpu_particles[i] = p;
        }
        return cpu_particles;
    }

    PhysicalParticle[] GenerateGpuParticles()
    {
        return null;
    }

    void ReadDistributionFile(string file_path)
    {
        StreamReader reader = new StreamReader(file_path);
        List<float[]> densities = new List<float[]>();
        string line = reader.ReadLine();
        while (line != null)
        {
            string[] tokens = line.Split(new char[] {' '}, System.StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                continue;
            }

            float[] currRow = new float[tokens.Length];
            for (int i = 0; i < tokens.Length; i++)
            {
                currRow[i] = float.Parse(tokens[i]);
            }
            densities.Add(currRow);

            line = reader.ReadLine();
        }

        if (densities.Count == 0)
        {
            Debug.LogWarning("Empty file: " + galaxyFile);
            return;
        }

        cumulative_row_densities = new float[densities.Count];
        marginal_cumulative_col = new float[densities.Count, densities[0].Length];
        float total = 0;
        for (int i = 0; i < densities.Count; i++)
        {
            cumulative_row_densities[i] = total;

            float[] currRow = densities[i];

            float rowTotal = 0;
            for (int j = 0; j < currRow.Length; j++)
            {
                rowTotal += currRow[j];
            }
            float currTotal = 0;
            for (int j = 0; j < currRow.Length; j++)
            {
                marginal_cumulative_col[i, j] = currTotal / rowTotal;
                currTotal += currRow[j];
            }

            total += rowTotal;
        }

        for (int i = 0; i < cumulative_row_densities.Length; i++)
        {
            cumulative_row_densities[i] /= total;
        }
    }
}
