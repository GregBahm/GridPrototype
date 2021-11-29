using UnityEngine;

public class VoxelBlueprintDisplay : MonoBehaviour
{
    public GameObject[] Indicators;

    public string Name;

    internal void SetBlueprint(VoxelDesignation item)
    {
        Name = item.ToString();
        int index = 0;
        for (int x = 0; x < 2; x++)
        {
            for (int y = 0; y < 2; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    bool val = item.Description[x, y, z];
                    Indicators[index].SetActive(val);
                    index++;
                }
            }
        }
    }
}