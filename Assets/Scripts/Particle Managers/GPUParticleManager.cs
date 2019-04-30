using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class GPUParticleManager : MonoBehaviour
{
    VisualEffect visualEffect;

    Texture2D new_point_positions;
    Texture2D new_point_scales;

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
        new_point_scales = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
    }
    
    public void AddPoints(Vector3[] newPositions)
    {
        for (int begin = 0; begin < newPositions.Length; begin += SystemInfo.maxTextureSize)
        {
            int end = Mathf.Min(begin + SystemInfo.maxTextureSize, newPositions.Length);
            if (end - begin != new_point_positions.width)
            {
                new_point_positions = new Texture2D(end - begin, 1, TextureFormat.RGBAFloat, false);
            }
            NativeArray<Color> rawData = new_point_positions.GetRawTextureData<Color>();
            for (int j = begin, k = 0; j < end; j++, k++)
            {
                rawData[k] = new Color(newPositions[j].x, newPositions[j].y, newPositions[j].z);
            }
            new_point_positions.Apply();

            visualEffect.SetInt("num_new_particles", end - begin);
            visualEffect.SetTexture("new_particle_positions", new_point_positions);
            visualEffect.SendEvent("AddParticles");
        }
    }

    public void Clear()
    {
        visualEffect.Reinit();
    }
}
