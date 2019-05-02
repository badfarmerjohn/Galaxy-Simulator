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

    // Start is called before the first frame update
    void Start()
    {
        ReadDistributionFile(galaxyFile);

        cpu_particles_manager = GetComponent<CPUParticleManager>();
        if (cpu_particles_manager == null)
        {
            cpu_particles_manager = gameObject.AddComponent<CPUParticleManager>();
        }

        cpu_particles_manager.AddParticles(GenerateCpuParticles(numCpuParticles), 0, numCpuParticles);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    PhysicalParticle[] GenerateCpuParticles(int num_particles)
    {
        Debug.Log("Generating: " + num_particles);
        PhysicalParticle[] cpu_particles = new PhysicalParticle[num_particles];
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

            PhysicalParticle p = new PhysicalParticle();
            p.position = cellSize * (new Vector3(row + Random.value, 0, col + Random.value));
            p.color = Color.red;
            p.mass = 1;
            p.size = 0.1f;
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
