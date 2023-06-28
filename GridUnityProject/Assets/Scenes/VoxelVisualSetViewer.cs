using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VoxelVisualSetViewer : MonoBehaviour
{
    private VoxelVisualComponentSet model;

    [SerializeField]
    private VoxelDesignationDisplay X0Y0Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X0Y0Z1Display;
    [SerializeField]
    private VoxelDesignationDisplay X0Y1Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X0Y1Z1Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y0Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y0Z1Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y1Z0Display;
    [SerializeField]
    private VoxelDesignationDisplay X1Y1Z1Display;

    [SerializeField]
    private Transform contents;

    private List<GameObject> objectsUnderContents;

    public void Initialize(VoxelVisualComponentSet model)
    {
        this.model = model;
        objectsUnderContents = new List<GameObject>();
        SetDesignationDisplay();
        UpdateVisuals();
        gameObject.name = model.Designation.ToDesignation().Key;
    }

    private void UpdateVisuals()
    {
        ClearContents();
        foreach (ComponentInSet item in model.Components)
        {
            GameObject component = CreateComponent(item);
            objectsUnderContents.Add(component);
        }
    }

    private GameObject CreateComponent(ComponentInSet item)
    {
        GameObject objRoot = new GameObject(item.Component.name);
        objRoot.transform.SetParent(contents, false);
        GameObject componentObj = new GameObject(item.Component.name);
        componentObj.transform.SetParent(objRoot.transform, false);
        MeshFilter filter = componentObj.AddComponent<MeshFilter>();
        filter.sharedMesh = item.Component.Mesh;
        MeshRenderer renderer = componentObj.AddComponent<MeshRenderer>();
        renderer.materials = GetComponentMaterials(item.Component.Materials).ToArray();
        objRoot.transform.Rotate(0, 90 * item.Rotations, 0);
        componentObj.transform.localScale = new Vector3(item.Flipped ? -1 : 1, 1, 1);
        return objRoot;
    }

    private IEnumerable<Material> GetComponentMaterials(Material[] materials)
    {
        foreach (Material material in materials)
        {
            yield return VoxelVisualBaseAssets.Instance.GetMaterialFor(material);
        }
    }

    private void ClearContents()
    {
        foreach (GameObject obj in objectsUnderContents)
        {
            Destroy(obj);
        }
        objectsUnderContents.Clear();
    }

    private void SetDesignationDisplay()
    {
        VoxelVisualDesignation designation = model.Designation.ToDesignation();
        X0Y0Z0Display.UpdateDisplayContent(designation.X0Y0Z0);
        X0Y0Z1Display.UpdateDisplayContent(designation.X0Y0Z1);
        X0Y1Z0Display.UpdateDisplayContent(designation.X0Y1Z0);
        X0Y1Z1Display.UpdateDisplayContent(designation.X0Y1Z1);
        X1Y0Z0Display.UpdateDisplayContent(designation.X1Y0Z0);
        X1Y0Z1Display.UpdateDisplayContent(designation.X1Y0Z1);
        X1Y1Z0Display.UpdateDisplayContent(designation.X1Y1Z0);
        X1Y1Z1Display.UpdateDisplayContent(designation.X1Y1Z1);
    }
}
