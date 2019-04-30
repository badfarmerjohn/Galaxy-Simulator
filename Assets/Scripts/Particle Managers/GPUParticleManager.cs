using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class GPUParticleManager : MonoBehaviour
{
    public VisualEffect visualEffect;

    //VFXEventAttribute spawn_attr;

    Texture2D new_point_positions;
    Texture2D new_point_scales;

    // Start is called before the first frame update
    void Start()
    {
        //spawn_attr = visualEffect.CreateVFXEventAttribute();

        new_point_positions = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
        new_point_scales = new Texture2D(SystemInfo.maxTextureSize, 1, TextureFormat.RGBAFloat, false);
    }

    public int i = 0;
    public void AddPoints(Vector3[] newPositions)
    {
        NativeArray<Color> rawData = new_point_positions.GetRawTextureData<Color>();
        for (int begin = 0; begin < newPositions.Length; begin += SystemInfo.maxTextureSize)
        {
            int end = Mathf.Min(begin + SystemInfo.maxTextureSize, newPositions.Length);
            for (int j = begin, k = 0; j < end; j++, k++)
            {
                rawData[k] = new Color(newPositions[j].x, newPositions[j].y, newPositions[j].z);
            }
            new_point_positions.Apply();
            visualEffect.SetInt("num_new_particles", end - begin);
            visualEffect.SetTexture("new_particle_positions", new_point_positions);
            visualEffect.SetVector3("offset", i * Vector3.one);
            visualEffect.SendEvent("AddParticles");
            Debug.Log("Adding " + (end - begin));
        }
        i += 1;
    }
}
