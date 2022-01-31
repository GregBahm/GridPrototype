using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    public Toggle ExteriorsTabButton;
    public Toggle InteriorsTabButton;
    public Toggle FoundationTabButton;
    public GameObject ExteriorsTab;
    public GameObject InteriorsTab;
    public GameObject FoundationTab;

    public UiTab SelectedTab;

    public enum UiTab
    {
        Exteriors,
        Interiors,
        Foundation
    }

    private void Start()
    {
        ExteriorsTabButton.isOn = SelectedTab == UiTab.Exteriors;
        InteriorsTabButton.isOn = SelectedTab == UiTab.Interiors;
        FoundationTabButton.isOn = SelectedTab == UiTab.Foundation;
    }

    private void Update()
    {

        ExteriorsTab.SetActive(SelectedTab == UiTab.Exteriors);
        InteriorsTab.SetActive(SelectedTab == UiTab.Interiors);
        FoundationTab.SetActive(SelectedTab == UiTab.Foundation);
    }

    public void SetTabToExteriors() { SelectedTab = UiTab.Exteriors; }
    public void SetTabToInteriors() { SelectedTab = UiTab.Interiors; }
    public void SetTabToFoundation() { SelectedTab = UiTab.Foundation; }
}
