using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CloudsBehavior : MonoBehaviour
{
    public float CloudSpeed;
    float cloudOffset;
    private Material cloudsMat;

    private void Start()
    {
        cloudsMat = GetComponent<Image>().material;
    }
    void Update()
    {
        cloudOffset = (cloudOffset + CloudSpeed) % 1;
        cloudsMat.SetTextureOffset("_MainTex", new Vector2(cloudOffset, 0));
    }
}
