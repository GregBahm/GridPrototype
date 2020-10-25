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
        Image image = GetComponent<Image>();
        image.material = new Material(image.material);
        cloudsMat = image.material; // Don't want the cloud .mat file to change every time the thing runs
    }
    void Update()
    {
        cloudOffset = (cloudOffset + CloudSpeed) % 1;
        cloudsMat.SetTextureOffset("_MainTex", new Vector2(cloudOffset, 0));
    }
}
