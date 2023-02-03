using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PortalTextureSetup : MonoBehaviour
{
    [SerializeField] List<Camera> cameras = new List<Camera>();
    [SerializeField] List<Material> materials = new List<Material>();

    private void Start()
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            if (cameras[i].targetTexture != null)
                cameras[i].targetTexture.Release();

            cameras[i].targetTexture = new RenderTexture(Screen.width, Screen.height, 24);
            materials[i].mainTexture = cameras[i].targetTexture;
        }
    }
}
