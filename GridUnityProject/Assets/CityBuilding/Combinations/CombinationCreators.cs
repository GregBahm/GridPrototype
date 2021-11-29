using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class CombinationCreators : MonoBehaviour
{
    private List<VoxelDesignation> baseBlueprints;
    private List<VoxelDesignation> invariants;

    public GameObject BlueprintDisplayPrefab;

    public GameObject[] ArtContent;
    public VoxelBlueprint[] Blueprints;
    private VoxelBlueprintDisplay[] displays;

    private void Start()
    {
        baseBlueprints = GetAllUniqueCombinations().ToList();
        invariants = GetInvariants().ToList();
        VerifyInvariants();
        displays = CreateDisplayPrefabs().ToArray();
        //SaveMeshes();
        SetUpBlueprints();
    }

    private void SaveMeshes(string outputFolder)
    {
        for (int i = 0; i < displays.Length; i++)
        {
            GameObject obj = displays[i].gameObject;
            string outputPath = outputFolder + i + ".obj";
            ObjExporter.GameObjectToFile(obj, i, outputPath);
        }
    }

    private void SetUpBlueprints()
    {
        for (int i = 0; i < invariants.Count; i++)
        {
            VoxelBlueprint blueprint = Blueprints[i];
            VoxelDesignation invariant = invariants[i];
            blueprint.DesignationValues = invariant.GetFlatValues();
            if(ArtContent[i] != null)
            {
                blueprint.ArtContent = ArtContent[i].GetComponent<MeshFilter>().sharedMesh;
            }
            else
            {
                blueprint.ArtContent = null;
            }
        }
    }

    private IEnumerable<VoxelBlueprintDisplay> CreateDisplayPrefabs()
    {
        List<VoxelBlueprintDisplay> ret = new List<VoxelBlueprintDisplay>();
        int index = 0;
        int rootCount = Mathf.CeilToInt(Mathf.Sqrt(invariants.Count));
        foreach (VoxelDesignation item in invariants)
        {
            GameObject obj = Instantiate(BlueprintDisplayPrefab);
            VoxelBlueprintDisplay behavior = obj.GetComponent<VoxelBlueprintDisplay>();
            behavior.SetBlueprint(item);
            int x = Mathf.FloorToInt(index / rootCount);
            int y = index % rootCount;
            obj.transform.position = new Vector3(x * 5, 0, y * 5);
            obj.name = item.ToString();
            index++;
            ret.Add(behavior);
        }
        return ret;
    }

    private void VerifyInvariants()
    {
        HashSet<string> invariantsTest = new HashSet<string>();
        foreach (VoxelDesignation item in invariants)
        {
            if (!invariantsTest.Add(item.ToString()))
            {
                throw new Exception("Not Unique");
            }
            List<GeneratedVoxelDesignation> variants = item.GetUniqueVariants().ToList();
            foreach (VoxelDesignation variant in variants)
            {
                if (!invariantsTest.Add(variant.ToString()))
                {
                    throw new Exception("Not Unique");
                }
            }
        }
        if(invariantsTest.Count != 256)
        {
            throw new Exception("Doesn't sum up to the right amout");
        }
    }

    public IEnumerable<VoxelDesignation> GetInvariants()
    {
        Dictionary<string, VoxelDesignation> invariantsDictionary = baseBlueprints.ToDictionary(item => item.ToString(), item => item);
        foreach (VoxelDesignation item in baseBlueprints)
        {
            if(invariantsDictionary.ContainsKey(item.ToString()))
            {
                List<GeneratedVoxelDesignation> variants = item.GetUniqueVariants().ToList();
                foreach (VoxelDesignation variant in variants)
                {
                    if (invariantsDictionary.ContainsKey(variant.ToString()))
                    {
                        invariantsDictionary.Remove(variant.ToString());
                    }
                }
            }
        }
        return invariantsDictionary.Values;
    }

    public static IEnumerable<VoxelDesignation> GetAllUniqueCombinations() // I know. Silly. But it's one-use code and I don't want to think.
    {
        for (int a = 0; a < 2; a++)
        {
            for (int b = 0; b < 2; b++)
            {
                for (int c = 0; c < 2; c++)
                {
                    for (int d = 0; d < 2; d++)
                    {
                        for (int e = 0; e < 2; e++)
                        {
                            for (int f = 0; f < 2; f++)
                            {
                                for (int g = 0; g < 2; g++)
                                {
                                    for (int h = 0; h < 2; h++)
                                    {

                                        bool[] vals = new bool[] { 
                                            a == 0, 
                                            b == 0, 
                                            c == 0, 
                                            d == 0, 
                                            e == 0, 
                                            f == 0, 
                                            g == 0, 
                                            h == 0 };
                                        yield return new VoxelDesignation(vals);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
