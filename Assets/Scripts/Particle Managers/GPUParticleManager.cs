using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class GPUParticleManager : MonoBehaviour
{
    public float default_size
    {
        get
        {
            return _default_size;
        }
        set
        {
            _default_size = value;
            for (int i = 0; i < default_point_scales.Length; i++)
            {
                default_point_scales[i] = new Color(_default_size, _default_size, _default_size);
            }
        }
    }
    float _default_size;

    public Color default_color
    {
        get
        {
            return _default_color;
        }
        set
        {
            _default_color = value;
            for (int i = 0; i < default_point_colors.Length; i++)
            {
                default_point_colors[i] = _default_color;
            }
        }
    }
    Color _default_color;

    VisualEffect visualEffect;

    Texture2D new_point_positions;
    Texture2D new_point_colors;
    Texture2D new_point_scales;

    NativeArray<Color> default_point_colors;
    NativeArray<Color> default_point_scales;
    Texture2D _default_point_colors;
    Texture2D _default_point_scales;

    Queue<System.Tuple<Vector3[], Vector3[], Color[]>> point_queue = new Queue<System.Tuple<Vector3[], Vector3[], Color[]>>();
    int curr_index = 0;
    float wait_interval = 1f;
    float last_push_time;

    // Start is called before the first frame update
    void Start()
    {
        //spawn_attr = visualEffect.CreateVFXEventAttribute();
        visualEffect = GetComponent<VisualEffect>();
        if (visualEffect == null)
        {
            visualEffect = gameObject.AddComponent<VisualEffect>();
        }
        visualEffect.visualEffectAsset = AssetDatabase.LoadAssetAtPath<VisualEffectAsset>("Assets/Prefabs/GPUParticles.vfx");
        visualEffect.SetVector3("center_of_rotation", transform.position);

        new_point_positions = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        new_point_colors = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        new_point_scales = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);

        _default_point_scales = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        default_point_scales = _default_point_scales.GetRawTextureData<Color>();
        
        _default_point_colors = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        default_point_colors = _default_point_colors.GetRawTextureData<Color>();

        default_size = 0.01f;
        default_color = new Color(1, 1, 1, 1);

        last_push_time = Time.time - wait_interval;
    }

    void Update()
    {
        if (point_queue.Count > 0 && Time.time - last_push_time >= wait_interval)
        {
            System.Tuple<Vector3[], Vector3[], Color[]> currItem = point_queue.Peek();
            Vector3[] positions = currItem.Item1;
            Vector3[] scales = currItem.Item2;
            Color[] colors = currItem.Item3;
            
            int end = Mathf.Min(curr_index + SystemInfo.maxTextureSize - 1, positions.Length);
            if (end - curr_index != new_point_positions.width)
            {
                new_point_positions = new Texture2D(end - curr_index, 1, TextureFormat.RGBAFloat, false);
                new_point_colors = new Texture2D(end - curr_index, 1, TextureFormat.RGBAFloat, false);
                if (colors == null)
                {
                    NativeArray<Color>.Copy(default_point_colors, new_point_colors.GetRawTextureData<Color>(), end - curr_index);
                    new_point_colors.Apply();
                }
                new_point_scales = new Texture2D(end - curr_index, 1, TextureFormat.RGBAFloat, false);
                if (scales == null)
                {
                    NativeArray<Color>.Copy(default_point_scales, new_point_scales.GetRawTextureData<Color>(), end - curr_index);
                    new_point_scales.Apply();
                }
            }

            NativeArray<Color> rawData = new_point_positions.GetRawTextureData<Color>();
            for (int j = curr_index, k = 0; j < end; j++, k++)
            {
                rawData[k] = new Color(positions[j].x, positions[j].y, positions[j].z);
            }
            new_point_positions.Apply();

            if (colors != null)
            {
                NativeArray<Color>.Copy(colors, curr_index, new_point_colors.GetRawTextureData<Color>(), 0, end - curr_index);
                new_point_colors.Apply();
            }

            if (scales != null)
            {
                NativeArray<Color> scaleData = new_point_scales.GetRawTextureData<Color>();
                for (int j = curr_index, k = 0; j < end; j++, k++)
                {
                    scaleData[k] = new Color(scales[j].x, scales[j].y, scales[j].z);
                }
                new_point_scales.Apply();
            }
            
            visualEffect.SetInt("num_new_particles", end - curr_index);
            visualEffect.SetTexture("new_particle_positions", new_point_positions);
            visualEffect.SetTexture("new_particle_colors", new_point_colors);
            visualEffect.SetTexture("new_particle_scales", new_point_scales);
            visualEffect.SendEvent("AddParticles");

            curr_index = end;
            if (curr_index >= positions.Length)
            {
                point_queue.Dequeue();
                curr_index = 0;
            }
            last_push_time = Time.time;
        }
    }

    public void AddPoints(Vector3[] positions, Vector3[] scales = null, Color[] colors = null)
    {
        point_queue.Enqueue(new System.Tuple<Vector3[], Vector3[], Color[]>(positions, scales, colors));
    }

    public void Clear()
    {
        visualEffect.Reinit();
    }
}
