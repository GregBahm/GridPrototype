using UnityEngine;

public class ComponentSetsList : MonoBehaviour
{
    private VoxelVisualSetupManager manager;

    [SerializeField]
    private GameObject ComponentSetUiPrefab;
    [SerializeField]
    private RectTransform ComponentSetUiGroup;

    private void Start()
    {
        manager = GetComponent<VoxelVisualSetupManager>();
        InstantiateComponentPefabs();
    }

    private void InstantiateComponentPefabs()
    {
        foreach (VoxelVisualComponentSet set in manager.VisualSetup.ComponentSets)
        {
            GameObject setUi = Instantiate(ComponentSetUiPrefab);
            setUi.transform.SetParent(ComponentSetUiGroup, false);
        }
    }
}
