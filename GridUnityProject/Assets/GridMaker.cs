using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridMaker : MonoBehaviour
{
    [SerializeField]
    private int gridRows;
    [SerializeField]
    private int gridColumns;
    [SerializeField]
    [Range(0,1)]
    private float easingWeight;

    private BaseGrid baseGrid;
    private EasedGrid easedGrid;

    private void Start()
    {
        baseGrid = new BaseGrid(gridRows, gridColumns);
        easedGrid = new EasedGrid(baseGrid);
    }

    private void Update()
    {
        easedGrid.DoEase(easingWeight);
        //DisplayBaseConnections();
        DisplayEasedConnections();
    }

    private void DisplayEasedConnections()
    {
        foreach (var item in easedGrid.EasedEdges)
        {
            Vector3 start = new Vector3(item.PointA.CurrentPos.x, 0, item.PointA.CurrentPos.y);
            Vector3 end = new Vector3(item.PointB.CurrentPos.x, 0, item.PointB.CurrentPos.y);
            Debug.DrawLine(start, end);
        }
    }

    private void DisplayBaseConnections()
    {
        foreach (Edge item in baseGrid.CulledConnections)
        {
            Vector3 start = new Vector3(item.PointA.PosX, 0, item.PointA.PosY);
            Vector3 end = new Vector3(item.PointB.PosX, 0, item.PointB.PosY);
            Debug.DrawLine(start, end);
        }
    }
}

class BaseGrid
{
    public TrianglePoint[,] Points { get; }
    public IEnumerable<Edge> BaseConnections { get; }
    public IEnumerable<Edge> CulledConnections { get; }

    private int gridRows;
    private int gridColumns;

    public BaseGrid(int rows, int columns)
    {
        gridRows = rows;
        gridColumns = columns;
        Points = CreateTrianglePoints();
        BaseConnections = CreateConnectins(Points);
        CulledConnections = DestroyConnections(BaseConnections);
    }

    private TrianglePoint[,] CreateTrianglePoints()
    {
        TrianglePoint[,] ret = new TrianglePoint[gridRows, gridColumns];
        for (int x = 0; x < gridRows; x++)
        {
            for (int y = 0; y < gridColumns; y++)
            {
                bool isBorder = x == 0 || x == gridRows - 1 || y == 0 || y == gridColumns - 1;
                ret[x, y] = new TrianglePoint(x, y, isBorder);
            }
        }
        return ret;
    }

    private void PopulateTriangles(Dictionary<string, Edge> baseEdges, TrianglePoint[,] points)
    {
        for (int x = 0; x < gridRows - 1; x++)
        {
            for (int y = 0; y < gridColumns - 1; y++)
            {
                TrianglePoint pointA = points[x, y];
                TrianglePoint pointB = points[x, y + 1];
                TrianglePoint pointC = points[x + 1, y];
                TrianglePoint pointD = points[x + 1, y + 1];
                Edge edgeAB = baseEdges[Edge.GetKey(pointA, pointB)];
                Edge edgeAC = baseEdges[Edge.GetKey(pointA, pointC)];
                Edge edgeBC = baseEdges[Edge.GetKey(pointB, pointC)];
                Edge edgeCD = baseEdges[Edge.GetKey(pointC, pointD)];
                Edge edgeBD = baseEdges[Edge.GetKey(pointB, pointD)];

                Triangle triangleA = new Triangle(edgeAB, edgeBC, edgeAC);
                Triangle triangleB = new Triangle(edgeBC, edgeCD, edgeBD);

                edgeAB.Triangles[0] = triangleA;
                edgeAC.Triangles[0] = triangleA;
                edgeBC.Triangles[0] = triangleA;
                edgeBD.Triangles[1] = triangleB;
                edgeBC.Triangles[1] = triangleB;
                edgeCD.Triangles[1] = triangleB;
            }
        }
    }

    private IEnumerable<Edge> DestroyConnections(IEnumerable<Edge> connections)
    {
        IEnumerable<Edge> interiorEdges = connections.Where(item => item.Triangles.All(tri => tri != null));
        HashSet<Edge> edgesThatCanBeDestroyed = new HashSet<Edge>(interiorEdges);
        while (edgesThatCanBeDestroyed.Any())
        {
            int indexToDelete = Mathf.FloorToInt(edgesThatCanBeDestroyed.Count * UnityEngine.Random.value);
            Edge edgeToDelete = edgesThatCanBeDestroyed.ElementAt(indexToDelete);
            edgeToDelete.Delete = true;
            edgesThatCanBeDestroyed.Remove(edgeToDelete);
            IEnumerable<Edge> edgesThatCantBeDestroyed = edgeToDelete.Triangles.SelectMany(item => item.Edges);
            foreach (var item in edgesThatCantBeDestroyed)
            {
                edgesThatCanBeDestroyed.Remove(item);
            }
        }
        return connections.Where(item => !item.Delete).ToArray();
    }

    private IEnumerable<Edge> CreateConnectins(TrianglePoint[,] points)
    {
        Dictionary<string, Edge> baseEdges = new Dictionary<string, Edge>();
        for (int x = 0; x < gridRows - 1; x++)
        {
            for (int y = 0; y < gridColumns - 1; y++)
            {
                TrianglePoint pointA = points[x, y];
                TrianglePoint pointB = points[x, y + 1];
                TrianglePoint pointC = points[x + 1, y];
                TrianglePoint pointD = points[x + 1, y + 1];

                Edge edgeAB = new Edge(pointA, pointB);
                Edge edgeAC = new Edge(pointA, pointC);
                Edge edgeBC = new Edge(pointB, pointC);
                baseEdges.Add(edgeAB.Key, edgeAB);
                baseEdges.Add(edgeAC.Key, edgeAC);
                baseEdges.Add(edgeBC.Key, edgeBC);

                if (x == gridRows - 2)
                {
                    Edge edgeCD = new Edge(pointC, pointD);
                    baseEdges.Add(edgeCD.Key, edgeCD);
                }
                if (y == gridColumns - 2)
                {
                    Edge edgeBD = new Edge(pointB, pointD);
                    baseEdges.Add(edgeBD.Key, edgeBD);
                }
            }
        }
        PopulateTriangles(baseEdges, points);
        return baseEdges.Values;
    }
}

public class TrianglePoint
{
    private static Vector2 xUnitOffset = new Vector2(1, -1.73f).normalized;

    public int GridX { get; }
    public int GridY { get; }
    public float PosX { get; }
    public float PosY { get; }
    public bool IsBorder { get; }

    public TrianglePoint(int gridX, int gridY, bool isBorder)
    {
        GridX = gridX;
        GridY = gridY;
        PosY = xUnitOffset.y * gridY;
        PosX = gridX + (xUnitOffset.x * gridY);
        IsBorder = isBorder;
    }
}

public class Edge
{
    public TrianglePoint PointA { get; }
    public TrianglePoint PointB { get; }

    public string Key { get; }

    public Edge(TrianglePoint pointA, TrianglePoint pointB)
    {
        PointA = pointA;
        PointB = pointB;
        Key = GetKey(pointA, pointB);
    }

    public Triangle[] Triangles { get; } = new Triangle[2];

    public bool IsBorderEdge { get { return Triangles.All(item => item != null); } }

    public bool Delete { get; set; }

    public static string GetKey(TrianglePoint a, TrianglePoint b)
    {
        return "X:" + a.GridX + "Y:" + a.GridY + "to X:" + b.GridX + "Y:" + b.GridY;
    }
}

public class Triangle
{
    public Edge[] Edges { get; }

    public Triangle(Edge a, Edge b, Edge c)
    {
        Edges = new Edge[3] { a, b, c };
    }
}

class EasedGrid
{
    public IEnumerable<EasedPoint> EasedPoints { get; }

    public IEnumerable<EasedEdge> EasedEdges { get; }

    public EasedGrid(BaseGrid baseGrid)
    {
        Dictionary<TrianglePoint, EasedPoint> pointTable = CreatePointTable(baseGrid.Points);
        EasedPoints = GetPopulatedPoints(pointTable, baseGrid.CulledConnections);
        EasedEdges = baseGrid.CulledConnections.Select(item => 
            new EasedEdge(pointTable[item.PointA], pointTable[item.PointB], item.IsBorderEdge)
            ).ToArray();
    }

    public void DoEase(float weight)
    {
        foreach (EasedPoint point in EasedPoints.Where(item => !item.IsBorder))
        {
            Vector2 sumPos = Vector2.zero;
            foreach (EasedPoint connection in point.ConnectedPoints)
            {
                sumPos += connection.CurrentPos;
            }
            sumPos /= point.ConnectedPoints.Count;
            point.CurrentPos = Vector2.Lerp(point.BasePos, sumPos, weight);
        }
    }

    private IEnumerable<EasedPoint> GetPopulatedPoints(
        Dictionary<TrianglePoint, EasedPoint> pointTable, 
        IEnumerable<Edge> culledConnections)
    {
        foreach (Edge edge in culledConnections)
        {
            EasedPoint pointA = pointTable[edge.PointA];
            EasedPoint pointB = pointTable[edge.PointB];
            pointA.ConnectedPoints.Add(pointB);
            pointB.ConnectedPoints.Add(pointA);
        }
        return pointTable.Values;
    }

    private Dictionary<TrianglePoint, EasedPoint> CreatePointTable(TrianglePoint[,] points)
    {
        Dictionary<TrianglePoint, EasedPoint> ret = new Dictionary<TrianglePoint, EasedPoint>();
        foreach (TrianglePoint item in points)
        {
            ret.Add(item, new EasedPoint(new Vector2(item.PosX, item.PosY), item.IsBorder));
        }
        return ret;
    }
}

class EasedEdge
{
    public EasedPoint PointA { get; }
    public EasedPoint PointB { get; }
    public bool IsBorder { get; }

    public EasedEdge(EasedPoint pointA, EasedPoint pointB, bool isBorder)
    {
        PointA = pointA;
        PointB = pointB;
        IsBorder = isBorder;
    }
}

class EasedPoint
{
    public Vector2 BasePos { get; }
    public Vector2 CurrentPos { get; set; }

    public bool IsBorder { get; }

    public List<EasedPoint> ConnectedPoints { get; } = new List<EasedPoint>();

    public EasedPoint(Vector2 basePos, bool isBorder)
    {
        BasePos = basePos;
        CurrentPos = basePos;
        IsBorder = isBorder;
    }
}