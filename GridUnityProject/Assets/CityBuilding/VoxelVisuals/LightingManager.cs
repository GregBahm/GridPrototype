using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightingManager : MonoBehaviour
{
    public Texture TopLighting;
    public Texture Bottomlighting;
    public bool ConstantlyUpdateTextures;

    private void Start()
    {
        Shader.SetGlobalTexture("_BottomLighting", Bottomlighting);
        Shader.SetGlobalTexture("_TopLighting", TopLighting);
    }

    private void Update()
    {
        if(ConstantlyUpdateTextures)
        {
            Shader.SetGlobalTexture("_BottomLighting", Bottomlighting);
            Shader.SetGlobalTexture("_TopLighting", TopLighting);
        }
    }

    public void UpdatePostion(MainGrid grid)
    {
        Vector2[] borderPositions = GetBorderPositions(grid).ToArray();
        float minX = float.MaxValue;
        float maxX = float.MinValue;
        float minY = float.MaxValue;
        float maxY = float.MinValue;
        foreach (Vector2 position in borderPositions)
        {
            minX = Mathf.Min(position.x, minX);
            maxX = Mathf.Max(position.x, maxX);
            minY = Mathf.Min(position.y, minY);
            maxY = Mathf.Max(position.y, maxY);
        }
        SetLightingBox(grid, minX, maxX, minY, maxY);

        Shader.SetGlobalMatrix("_LightBoxTransform", transform.worldToLocalMatrix);
    }

    private void SetLightingBox(MainGrid grid, float minX, float maxX, float minZ, float maxZ)
    {
        float x = (maxX + minX) / 2;
        float z = (maxZ + minZ) / 2;
        float y = (float)grid.MaxHeight / 2;
        transform.position = new Vector3(x, y, z);

        float xScale = maxX - minX;
        float zScale = maxZ - minZ;
        transform.localScale = new Vector3(xScale, grid.MaxHeight, zScale);
    }

    private IEnumerable<Vector2> GetBorderPositions(MainGrid grid)
    {
        HashSet<GroundPoint> borderPoints = new HashSet<GroundPoint>();
        foreach (GroundEdge edge in grid.BorderEdges)
        {
            borderPoints.Add(grid.Points[edge.PointA.Index]);
            borderPoints.Add(grid.Points[edge.PointB.Index]);
        }
        return borderPoints.Select(item => item.Position);
    }
}