using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GalaxyInitializer : MonoBehaviour
{
    public string galaxyFolder = "Assets/Galaxy Files/galaxy";

    string galaxyCPUFile;
    string galaxyHMeanFile;
    string galaxySMeanFile;
    string galaxyVMeanFile;

    string galaxyHStdFile;
    string galaxySStdFile;
    string galaxyVStdFile;

    public int numCpuParticles = 0;
    public int numGpuParticles = 0;

    public float cellSize = 0.1f;

    float[] cumulative_row_densities;
    float[,] marginal_cumulative_col;

    float[,] h_mean;
    float[,] s_mean;
    float[,] v_mean;

    float[,] h_std;
    float[,] s_std;
    float[,] v_std;

    CPUParticleManager cpu_particles_manager;
    GPUParticleManager gpu_particles_manager;

    bool added = false;

    // Start is called before the first frame update
    void Start()
    {
        galaxyCPUFile = galaxyFolder + "/pix_array.txt";
        galaxyHMeanFile = galaxyFolder + "/h_mean_array.txt";
        galaxySMeanFile = galaxyFolder + "/s_mean_array.txt";
        galaxyVMeanFile = galaxyFolder + "/v_mean_array.txt";

        galaxyHStdFile = galaxyFolder + "/h_std_array.txt";
        galaxySStdFile = galaxyFolder + "/s_std_array.txt";
        galaxyVStdFile = galaxyFolder + "/v_std_array.txt";

        ReadDistributionFile(galaxyCPUFile);
        SetUpHSVStructures();

        cpu_particles_manager = GetComponent<CPUParticleManager>();
        if (cpu_particles_manager == null)
        {
            cpu_particles_manager = gameObject.AddComponent<CPUParticleManager>();
        }

        gpu_particles_manager = GetComponent<GPUParticleManager>();
        if (gpu_particles_manager == null)
        {
            gpu_particles_manager = gameObject.AddComponent<GPUParticleManager>();
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

        if (Input.GetKeyUp(KeyCode.Space))
        {
            gpu_particles_manager.Clear();
            GenerateGpuParticles(numGpuParticles);
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

            float hue_mean = h_mean[row, col];
            float hue_std = h_std[row, col];
            float hue_sample = sampleGaussian(hue_mean, hue_std);

            float sat_mean = s_mean[row, col];
            float sat_std = s_std[row, col];
            float sat_sample = sampleGaussian(sat_mean, sat_std);
            
            float value_mean = v_mean[row, col];
            float value_std = v_std[row, col];
            float value_sample = sampleGaussian(value_mean, value_std);

            PhysicalParticle p = new PhysicalParticle();
            p.position = cellSize * (new Vector3(unit_position.x, (Random.value - 0.5f) * y_range, unit_position.z) - center_offset);
            //p.color = Color.white;
            p.color = Color.HSVToRGB(hue_sample, sat_sample, value_sample);
            p.mass = 1;
            p.size = 0.01f;
            p.velocity = Vector3.zero;
            cpu_particles[i] = p;
        }
        return cpu_particles;
    }

    void GenerateGpuParticles(int num_particles)
    {
        Debug.Log("Generating GPU Particles: " + num_particles);

        Vector3[] positions = new Vector3[num_particles];
        //Vector3[] scales = new Vector3[num_particles];
        Color[] colors = new Color[num_particles];

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

            float hue_mean = h_mean[row, col];
            float hue_std = h_std[row, col];
            float hue_sample = sampleGaussian(hue_mean, hue_std);

            float sat_mean = s_mean[row, col];
            float sat_std = s_std[row, col];
            float sat_sample = sampleGaussian(sat_mean, sat_std);
            
            float value_mean = v_mean[row, col];
            float value_std = v_std[row, col];
            float value_sample = sampleGaussian(value_mean, value_std);

            Vector3 unit_position = new Vector3(row + Random.value, 0, col + Random.value);
            float y_range = Mathf.Exp(-(unit_position - center_offset).sqrMagnitude / tileRadius2) * gaussFactor;
            
            positions[i] = cellSize * (new Vector3(unit_position.x, (Random.value - 0.5f) * y_range, unit_position.z) - center_offset);
            colors[i] = Color.HSVToRGB(hue_sample, sat_sample, value_sample);
            //colors[i] = Color.white;
            //scales[i] = 0.01f * Vector3.one;
        }

        gpu_particles_manager.AddPoints(positions, colors: colors);
    }

    void ReadDistributionFile(string file_path)
    {
        StreamReader reader = new StreamReader(file_path);
        List<float[]> densities = new List<float[]>();
        string line = reader.ReadLine();
        while (line != null)
        {
            string[] tokens = line.Split(new char[] {' '} , System.StringSplitOptions.RemoveEmptyEntries);
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
            Debug.LogWarning("Empty file: " + galaxyCPUFile);
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

    void SetUpHSVStructures() {
        int[] dimensions = FindDimension(galaxyHMeanFile);
        int numRow = dimensions[0];
        int numCol = dimensions[1];
        h_mean = new float[numRow, numCol];
        s_mean = new float[numRow, numCol];
        v_mean = new float[numRow, numCol];
        h_std = new float[numRow, numCol];
        s_std = new float[numRow, numCol];
        v_std = new float[numRow, numCol];
        ReadHSVFile(galaxyHMeanFile, h_mean);
        ReadHSVFile(galaxySMeanFile, s_mean);
        ReadHSVFile(galaxyVMeanFile, v_mean);
        ReadHSVFile(galaxyHStdFile, h_std);
        ReadHSVFile(galaxySStdFile, s_std);
        ReadHSVFile(galaxyVStdFile, v_std);
    }


    int[] FindDimension(string file_path) {
        int numRow = File.ReadAllLines(file_path).Length;
        StreamReader reader = new StreamReader(file_path);
        string line = reader.ReadLine();
        int numCol = -1;
        if (line != null)
        {
            string[] tokens = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            numCol = tokens.Length;
            line = reader.ReadLine();
        }

        if (numCol < 0)
        {
            Debug.LogWarning("Empty file: " + file_path);
            return new int[]  { 0, 0 };
        }
        return new int[] { numRow, numCol };
    }

    void ReadHSVFile(string file_path, float[,] data) {
        StreamReader reader = new StreamReader(file_path);
        string line = reader.ReadLine();
        int currRow = 0;
        while (line != null)
        {
            string[] tokens = line.Split(new char[] { ' ' }, System.StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
            {
                continue;
            }

            for (int currCol = 0; currCol < tokens.Length; currCol++)
            {
                data[currRow, currCol] = float.Parse(tokens[currCol]);
            }
            line = reader.ReadLine();
            currRow += 1;
        }
    }


    float computeGaussian(float x, float mean, float std) {
        return (1.0f / (std * Mathf.Sqrt(2 * Mathf.PI))) * Mathf.Exp(-0.5f * Mathf.Pow((x - mean) / std, 2));
    }

    float sampleGaussian(float mean, float std)
    {
        float r = Mathf.Sqrt(-2 * Mathf.Log(Random.value));
        float theta = 2 * Mathf.PI * Random.value;
        return r * Mathf.Sin(theta) * std + mean;
    }

}
