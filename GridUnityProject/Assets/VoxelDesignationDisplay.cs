using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoxelDesignationDisplay : MonoBehaviour
{
    public GameObject Content;

    public void UpdateDisplayContent(SlotType slotType)
    {
        if (slotType == SlotType.Empty)
        {
            Content.SetActive(false);
            return;
        }
        Content.SetActive(true);
    }
}
