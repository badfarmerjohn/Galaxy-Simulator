using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.Experimental.VFX;

public class ParticleSpawner : MonoBehaviour
{
    public VisualEffect visualEffect;
    public int particleCount = 0;
    public int numPoints = 1;

    private int iterations = 0;

    private VFXEventAttribute attr;
    private float offset = 0.5f;
    private Vector3 scale = new Vector3(10, 0, 5);

    private Texture2D pointCloudTex;

    // Start is called before the first frame update
    void Start()
    {
        attr = visualEffect.CreateVFXEventAttribute();
        pointCloudTex = new Texture2D(Mathf.Min(SystemInfo.maxTextureSize, numPoints), 1, TextureFormat.RGBAFloat, false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            int newParticleCount = Mathf.Min(SystemInfo.maxTextureSize, numPoints);
            visualEffect.SetInt("SpawnCount", newParticleCount);

            NativeArray<Color> texels = pointCloudTex.GetRawTextureData<Color>();
            for (int i = 0; i < texels.Length; i++)
            {
                texels[i] = new Color((Random.value - offset) * scale.x, (Random.value - offset) * scale.y, (Random.value - offset) * scale.z);
            }
            pointCloudTex.Apply();

            visualEffect.SetTexture("TexturePoints", pointCloudTex);
            visualEffect.SetVector3("Bias", iterations * scale);
            visualEffect.SendEvent("AddParticles", attr);

            particleCount += newParticleCount;
            iterations += 1;
        }
    }
}
