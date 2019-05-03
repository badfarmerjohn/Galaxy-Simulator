using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class GalaxyInitializer : MonoBehaviour
{
    public string galaxyFolder = "Assets/Galaxy Files/galaxy - highres";

    string galaxyCPUFileSuffix = "/pix_array.txt";
    string galaxyHMeanFileSuffix = "/h_mean_array.txt";
    string galaxySMeanFileSuffix = "/s_mean_array.txt";
    string galaxyVMeanFileSuffix = "/v_mean_array.txt";

    string galaxyHStdFileSuffix = "/h_std_array.txt";
    string galaxySStdFileSuffix = "/s_std_array.txt";
    string galaxyVStdFileSuffix = "/v_std_array.txt";

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

        ReadDistributionFile(galaxyFolder + galaxyCPUFileSuffix);
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
            GenerateGpuParticles(numGpuParticles);
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
        float tileRadiusSqr1 = (center_offset * 0.2f).sqrMagnitude;
        float tileRadiusSqr2 = (center_offset * 2).sqrMagnitude;
        float gaussFactor1 = 1.0f / Mathf.Sqrt(Mathf.PI * tileRadiusSqr1) * 5000 * cellSize;
        float gaussFactor2 = 1.0f / Mathf.Sqrt(Mathf.PI * tileRadiusSqr2) * 10000 * cellSize;

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

            Vector3 unit_position = new Vector3(row + Random.value, 0, col + Random.value);
            float y_range_1 = Mathf.Exp(-(unit_position - center_offset).sqrMagnitude / tileRadiusSqr1) * gaussFactor1;
            float y_range_2 = Mathf.Exp(-(unit_position - center_offset).sqrMagnitude / tileRadiusSqr2) * gaussFactor2;
            p.position = cellSize * (new Vector3(unit_position.x, sampleGaussian(0, Mathf.Max(y_range_1, y_range_2)), unit_position.z) - center_offset);

            float hue_mean = h_mean[row, col];
            float hue_std = h_std[row, col];
            float hue_sample = sampleGaussian(hue_mean, hue_std);

            float sat_mean = s_mean[row, col];
            float sat_std = s_std[row, col];
            float sat_sample = sampleGaussian(sat_mean, sat_std);
            
            float value_mean = v_mean[row, col];
            float value_std = v_std[row, col];
            float value_sample = sampleGaussian(value_mean, value_std);
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
        float tileRadiusSqr1 = (center_offset * 0.2f).sqrMagnitude;
        float tileRadiusSqr2 = (center_offset * 2).sqrMagnitude;
        float gaussFactor1 = 1.0f / Mathf.Sqrt(Mathf.PI * tileRadiusSqr1) * 5000 * cellSize;
        float gaussFactor2 = 1.0f / Mathf.Sqrt(Mathf.PI * tileRadiusSqr2) * 10000 * cellSize;

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
            float y_range_1 = Mathf.Exp(-(unit_position - center_offset).sqrMagnitude / tileRadiusSqr1) * gaussFactor1;
            float y_range_2 = Mathf.Exp(-(unit_position - center_offset).sqrMagnitude / tileRadiusSqr2) * gaussFactor2;
            positions[i] = cellSize * (new Vector3(unit_position.x, sampleGaussian(0, Mathf.Max(y_range_1, y_range_2)), unit_position.z) - center_offset);

            float hue_mean = h_mean[row, col];
            float hue_std = h_std[row, col];
            float hue_sample = sampleGaussian(hue_mean, hue_std);

            float sat_mean = s_mean[row, col];
            float sat_std = s_std[row, col];
            float sat_sample = sampleGaussian(sat_mean, sat_std);
            
            float value_mean = v_mean[row, col];
            float value_std = v_std[row, col];
            float value_sample = sampleGaussian(value_mean, value_std);

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
            Debug.LogWarning("Empty file: " + galaxyFolder + galaxyCPUFileSuffix);
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
        int[] dimensions = FindDimension(galaxyFolder + galaxyHMeanFileSuffix);
        int numRow = dimensions[0];
        int numCol = dimensions[1];
        h_mean = new float[numRow, numCol];
        s_mean = new float[numRow, numCol];
        v_mean = new float[numRow, numCol];
        h_std = new float[numRow, numCol];
        s_std = new float[numRow, numCol];
        v_std = new float[numRow, numCol];
        ReadHSVFile(galaxyFolder + galaxyHMeanFileSuffix, h_mean);
        ReadHSVFile(galaxyFolder + galaxySMeanFileSuffix, s_mean);
        ReadHSVFile(galaxyFolder + galaxyVMeanFileSuffix, v_mean);
        ReadHSVFile(galaxyFolder + galaxyHStdFileSuffix, h_std);
        ReadHSVFile(galaxyFolder + galaxySStdFileSuffix, s_std);
        ReadHSVFile(galaxyFolder + galaxyVStdFileSuffix, v_std);
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
