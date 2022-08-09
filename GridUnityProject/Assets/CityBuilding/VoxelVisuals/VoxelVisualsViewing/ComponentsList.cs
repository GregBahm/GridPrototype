using System;
using UnityEngine;

public class ComponentsList : MonoBehaviour
{
    private VoxelVisualSetupManager manager;

    [SerializeField]
    private GameObject ComponentUiPrefab;
    [SerializeField]
    private RectTransform ComponentsListUi;


    private void Start()
    {
        manager = GetComponent<VoxelVisualSetupManager>();
        InstantiateComponentPefabs();
    }

    private void InstantiateComponentPefabs()
    {
        foreach (VoxelVisualComponent component in manager.SourceComponents)
        {
            GameObject componentUi = Instantiate(ComponentUiPrefab);
            componentUi.transform.SetParent(ComponentsListUi, false);
        }
    }
}