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
            for (int i = 0; i < default_point_sizes.Length; i++)
            {
                default_point_sizes[i] = new Color(_default_size, _default_size, _default_size);
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
    Texture2D new_point_sizes;

    NativeArray<Color> default_point_colors;
    NativeArray<Color> default_point_sizes;
    Texture2D _default_point_colors;
    Texture2D _default_point_sizes;

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

        new_point_positions = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        new_point_colors = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        new_point_sizes = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RFloat, false);

        _default_point_sizes = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RFloat, false);
        default_point_sizes = _default_point_sizes.GetRawTextureData<Color>();
        
        _default_point_colors = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        default_point_colors = _default_point_colors.GetRawTextureData<Color>();

        default_size = 0.1f;
        default_color = new Color(1, 1, 1, 1);
    }
    
    public void AddPoints(Vector3[] positions, float[] sizes = null, Color[] colors = null)
    {
        for (int begin = 0; begin < positions.Length; begin += SystemInfo.maxTextureSize)
        {
            int end = Mathf.Min(begin + SystemInfo.maxTextureSize, positions.Length);
            if (end - begin != new_point_positions.width)
            {
                new_point_positions = new Texture2D(end - begin, 1, TextureFormat.RGBAFloat, false);
                new_point_colors = new Texture2D(end - begin, 1, TextureFormat.RGBAFloat, false);
                if (colors == null)
                {
                    NativeArray<Color>.Copy(default_point_colors, new_point_colors.GetRawTextureData<Color>(), end - begin);
                    new_point_colors.Apply();
                }
                //new_point_sizes = new Texture2D(end - begin, 1, TextureFormat.RFloat, false);
                //if (sizes == null)
                //{
                //    NativeArray<Color>.Copy(default_point_sizes, new_point_sizes.GetRawTextureData<Color>(), end - begin);
                //}
            }

            NativeArray<Color> rawData = new_point_positions.GetRawTextureData<Color>();
            for (int j = begin, k = 0; j < end; j++, k++)
            {
                rawData[k] = new Color(positions[j].x, positions[j].y, positions[j].z);
            }
            new_point_positions.Apply();

            if (colors != null)
            {
                NativeArray<Color>.Copy(colors, begin, new_point_colors.GetRawTextureData<Color>(), 0, end - begin);
                new_point_colors.Apply();
            }

            //if (sizes != null)
            //{
            //    NativeArray<Color> scaleData = new_point_sizes.GetRawTextureData<Color>();
            //    for (int j = begin, k = 0; j < end; j++, k++)
            //    {
            //        scaleData[k] = new Color(sizes[j], sizes[j], sizes[j]);
            //    }
            //    new_point_sizes.Apply();
            //}

            visualEffect.SetInt("num_new_particles", end - begin);
            visualEffect.SetTexture("new_particle_positions", new_point_positions);
            visualEffect.SetTexture("new_particle_colors", new_point_colors);
            //visualEffect.SetTexture("new_particle_scales", new_point_scales);
            visualEffect.SendEvent("AddParticles");
        }
    }

    public void Clear()
    {
        visualEffect.Reinit();
    }
}
