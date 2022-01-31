using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TabButtonViewModel : MonoBehaviour
{
    public GameObject InactiveBackground;
    private Toggle toggle;

    private void Start()
    {
        toggle = GetComponent<Toggle>();
    }

    void Update()
    {
        InactiveBackground.SetActive(!toggle.isOn);
    }
}
