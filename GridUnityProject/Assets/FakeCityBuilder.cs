using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FakeCityBuilder : MonoBehaviour
{
    [SerializeField]
    private int towersCount;
    
    [SerializeField]
    private int gridSize;

    [SerializeField]
    private float heightRamp;

    [SerializeField]
    private Transform theRedCube;

    [SerializeField]
    private GameObject cellPrefab;

    private IEnumerable<Tower> city;
    
    [SerializeField]
    private Mesh cube;
    [SerializeField]
    private Material cubeMat;
    
    private Bounds bounds;
    public int CubeCount;

    private ComputeBuffer positionsBuffer;
    private const int positionsBufferStride = sizeof(float) * 3;

    private void Start()
    {
        this.city = MakeCity();
        CubeCount = city.SelectMany(item => item.TowerPoints).Count();
        positionsBuffer = GetPositionsBuffer();
    }

    private ComputeBuffer GetPositionsBuffer()
    {
        Vector3[] positions = city.SelectMany(item => item.TowerPoints)
            .Select(item => item.transform.position)
            .ToArray();

        ComputeBuffer ret = new ComputeBuffer(positions.Length, positionsBufferStride);
        ret.SetData(positions);
        return ret;
    }

    private void OnDestroy()
    {
        positionsBuffer.Release();
    }
    
    public void Update()
    {
        PlaceRedCube();
        HandleCubeVisiblity();
    }

    private void HandleCubeVisiblity()
    {
        cubeMat.SetBuffer("positionBuffer", positionsBuffer);
        Graphics.DrawMeshInstancedProcedural(cube, 0, cubeMat, bounds, CubeCount);
    }

    private void PlaceRedCube()
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
        {
            theRedCube.position = hit.collider.gameObject.transform.position;
        }
    }

    private IEnumerable<Tower> MakeCity()
    {
        List<Tower> ret = new List<Tower>();
        for (int i = 0; i < towersCount; i++)
        {
            ret.Add(MakeTower());
        }
        return ret;
    }

    private Tower MakeTower()
    {
        float xPos = UnityEngine.Random.value;
        float zPos = UnityEngine.Random.value;

        float towerHeightRamp = Mathf.Pow(UnityEngine.Random.value, heightRamp);
        int towerHeight = Mathf.CeilToInt(gridSize * towerHeightRamp);

        List<MeshRenderer> towerPoints = new List<MeshRenderer>();
        for (int i = 0; i < towerHeight; i++)
        {
            towerPoints.Add(MakeTowerCell(xPos, zPos, i));
        }
        return new Tower(towerPoints);
    }

    private MeshRenderer MakeTowerCell(float x, float z, int yCell)
    {
        float y = (float)yCell / gridSize;
        GameObject retObj = Instantiate(cellPrefab);
        retObj.transform.position = new Vector3(x, y, z);
        float scale = 1f / gridSize;
        retObj.transform.localScale = new Vector3(scale, scale, scale);
        return retObj.GetComponent<MeshRenderer>();
    }

    private class Tower
    {
        public IEnumerable<MeshRenderer> TowerPoints { get; }

        public Tower(IEnumerable<MeshRenderer> towerPoints)
        {
            TowerPoints = towerPoints;
        }
    }
}