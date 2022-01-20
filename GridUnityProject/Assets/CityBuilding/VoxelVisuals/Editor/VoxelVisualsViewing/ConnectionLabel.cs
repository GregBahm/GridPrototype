using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class ConnectionLabel : MonoBehaviour
{
    public TextMeshPro Label;

    private void Update()
    {
        if(Camera.current != null && Label != null)
        {
            Label.transform.LookAt(Camera.current.transform, Camera.current.transform.up);
            Label.transform.Rotate(new Vector3(0, 180, 0));
        }    
    }
}