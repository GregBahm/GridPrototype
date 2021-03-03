using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public Texture TopLighting;
    public Texture Bottomlighting;

    private void Update()
    {
        Shader.SetGlobalMatrix("_LightBoxTransform", transform.worldToLocalMatrix);
    }
}