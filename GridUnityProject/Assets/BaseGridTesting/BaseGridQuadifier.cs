using GameGrid;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEditor.SearchService;
using UnityEngine;
using static UnityEditor.PlayerSettings;

public class BaseGridQuadifier : MonoBehaviour
{
    [SerializeField]
    private GameObject pointMarker;
    [SerializeField]
    private GameObject progSegment;
    [SerializeField]
    private GameObject quadSegment;

    [SerializeField]
    private TextAsset SaveFile;

    private List<Point>[] points;

    private List<QuadSet> quadSets;

    [SerializeField]
    private int axisOfSymetry = 8;

    [SerializeField]
    private bool DoSave;

    private Point ProgressPointA;
    private Point ProgressPointB;
    private Point ProgressPointC;

    private Point ClosestPoint;

    private GameObject progSegA;
    private GameObject progSegB;
    private GameObject progSegC;
    private GameObject progSegD;

    void Start()
    {
        progSegA = Instantiate(progSegment);
        progSegB = Instantiate(progSegment);
        progSegC = Instantiate(progSegment);
        progSegD = Instantiate(progSegment);
        quadSets = new List<QuadSet>();
        DoLoad();
    }

    public void Save()
    {
        string filePath = Application.dataPath + "/BaseGridTesting/MainGrid.json";
        Debug.Log(filePath);
        GroundSaveState groundSaveState = GetGroundSaveState();
        string asJson = JsonUtility.ToJson(groundSaveState);
        System.IO.File.WriteAllText(filePath, asJson);
    }

    private void Update()
    {
        DrawInProgressQuad();
        if (Input.GetMouseButtonUp(0))
        {
            SelectNewPoint();
        }
        if (Input.GetMouseButtonUp(1))
        {
            UnselectPoint();
        }
        if (DoSave)
        {
            DoSave = false;
            Save();
        }
    }

    private void UnselectPoint()
    {
        if (ProgressPointC != null)
        {
            ProgressPointC = null;
            return;
        }
        if (ProgressPointB != null)
        {
            ProgressPointB = null;
            return;
        }
        if (ProgressPointA != null)
        {
            ProgressPointA = null;
            return;
        }
        QuadSet quadToKill = quadSets.Last();
        quadToKill.DeleteSegements();
        quadSets.Remove(quadToKill);
    }

    private void SelectNewPoint()
    {
        if (ProgressPointA == null)
        {
            ProgressPointA = ClosestPoint;
            return;
        }
        if (ProgressPointB == null)
        {
            ProgressPointB = ClosestPoint;
            return;
        }
        if (ProgressPointC == null)
        {
            ProgressPointC = ClosestPoint;
            return;
        }
        QuadSet set = new QuadSet(points, axisOfSymetry, ProgressPointA, ProgressPointB, ProgressPointC, ClosestPoint, quadSegment);
        quadSets.Add(set);
        ProgressPointA = null;
        ProgressPointB = null;
        ProgressPointC = null;
    }

    private void DrawInProgressQuad()
    {
        progSegA.SetActive(false);
        progSegB.SetActive(false);
        progSegC.SetActive(false);
        progSegD.SetActive(false);

        ClosestPoint = GetClosestPoint();
        Vector3 clostDrawPos = new Vector3(ClosestPoint.Pos.x, 0, ClosestPoint.Pos.y);
        if (ProgressPointA == null)
        {
            Vector2 baseMousePos = GetMousePos();
            Vector3 mousPos = new Vector3(baseMousePos.x, 0, baseMousePos.y);
            progSegA.SetActive(true);
            PlaceSegment(progSegA.transform, clostDrawPos, mousPos);
            return;
        }
        if (ProgressPointB == null)
        {
            Vector3 drawA = new Vector3(ProgressPointA.Pos.x, 0, ProgressPointA.Pos.y);
            Debug.DrawLine(drawA, clostDrawPos, Color.white);

            progSegA.SetActive(true);
            PlaceSegment(progSegA.transform, drawA, clostDrawPos);
            return;
        }
        if (ProgressPointC == null)
        {
            Vector3 drawA = new Vector3(ProgressPointA.Pos.x, 0, ProgressPointA.Pos.y);
            Vector3 drawB = new Vector3(ProgressPointB.Pos.x, 0, ProgressPointB.Pos.y);

            progSegA.SetActive(true);
            PlaceSegment(progSegA.transform, drawA, drawB);
            progSegB.SetActive(true);
            PlaceSegment(progSegB.transform, drawB, clostDrawPos);
            return;
        }
        else
        {
            Vector3 drawA = new Vector3(ProgressPointA.Pos.x, 0, ProgressPointA.Pos.y);
            Vector3 drawB = new Vector3(ProgressPointB.Pos.x, 0, ProgressPointB.Pos.y);
            Vector3 drawC = new Vector3(ProgressPointC.Pos.x, 0, ProgressPointC.Pos.y);


            progSegA.SetActive(true);
            PlaceSegment(progSegA.transform, drawA, drawB);
            progSegB.SetActive(true);
            PlaceSegment(progSegB.transform, drawB, drawC);
            progSegC.SetActive(true);
            PlaceSegment(progSegC.transform, drawC, clostDrawPos);
            progSegD.SetActive(true);
            PlaceSegment(progSegD.transform, drawA, clostDrawPos);
        }
    }
    public static void PlaceSegment(Transform quad, Vector3 start, Vector3 end)
    {
        quad.localPosition = (start + end) / 2;
        float length = (start - end).magnitude;
        quad.localScale = new Vector3(.1f, 1, length);

        float angle = Vector3.SignedAngle(Vector3.forward, start - end, Vector3.up);
        quad.localRotation = Quaternion.Euler(0, angle, 0);
    }

    private Vector2 GetMousePos()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        float hmm;
        if (plane.Raycast(ray, out hmm))
        {
            Vector3 ret = ray.GetPoint(hmm);
            return new Vector2(ret.x, ret.z);
        }
        return Vector2.zero;
    }

    private Point GetClosestPoint()
    {
        Vector2 pos = GetMousePos();
        Point closest = null;
        float minDist = float.MaxValue;
        foreach (Point point in points.SelectMany(item => item))
        {
            float dist = (pos - point.Pos).sqrMagnitude;
            if (dist < minDist)
            {
                closest = point;
                minDist = dist;
            }
        }
        return closest;
    }

    private void DoLoad()
    {
        string data = SaveFile.text;
        BaseGridGeneratorSave saveFile = JsonUtility.FromJson<BaseGridGeneratorSave>(data);
        points = new List<Point>[axisOfSymetry];
        for (int i = 0; i < axisOfSymetry; i++)
        {
            points[i] = new List<Point>
            {
                new Point(new Vector2(0, 0), i, 0)
            };
        }
        GameObject obj = Instantiate(pointMarker);
        obj.transform.localPosition = new Vector3(0, 0, 0);
        for (int i = 0; i < saveFile.Points.Count; i++)
        {
            Vector2 pos = saveFile.Points[i];
            int mod = i % axisOfSymetry;
            Point point = new Point(pos, mod, points[mod].Count);
            points[mod].Add(point);
            GameObject newObj = Instantiate(pointMarker);
            newObj.transform.localPosition = new Vector3(pos.x, 0, pos.y);
        }
    }

    private GroundSaveState GetGroundSaveState()
    {
        List<Quad> quads = quadSets.SelectMany(item => item.Quads).ToList();

        List<Point> allPoints = new HashSet<Point>(quads.SelectMany(item => item.Points)).ToList();

        Dictionary<Point, GroundPointBuilder> groundPoints = new Dictionary<Point, GroundPointBuilder>();
        for (int i = 0; i < allPoints.Count; i++)
        {
            Point point = allPoints[i];
            groundPoints.Add(point, new GroundPointBuilder(i, point.Pos));
        }

        GroundQuadBuilder[] quadBuilders = new GroundQuadBuilder[quads.Count];
        for (int i = 0; i < quads.Count; i++)
        {
            Quad quad = quads[i];
            int indexA = groundPoints[quad.PointA].Index;
            int indexB = groundPoints[quad.PointB].Index;
            int indexC = groundPoints[quad.PointC].Index;
            int indexD = groundPoints[quad.PointD].Index;
            quadBuilders[i] = new GroundQuadBuilder(indexA, indexB, indexC, indexD);
        }

        GroundPointBuilder[] pointBuilders = groundPoints.Values.ToArray();
        return new GroundSaveState(pointBuilders, quadBuilders);
    }
}

class Point
{
    public int SetIndex { get; }
    public int IndexInSet { get; }

    public Vector2 Pos { get; }

    public Point(Vector2 pos, int setIndex, int indexInSet)
    {
        Pos = pos;
        SetIndex = setIndex;
        IndexInSet = indexInSet;
    }
}

class QuadSet
{
    private readonly List<Quad> quads;
    public IEnumerable<Quad> Quads { get { return quads; } }

    public QuadSet(List<Point>[] points,
        int axiOfSymetry,
        Point pointA,
        Point pointB,
        Point pointC,
        Point pointD,
        GameObject quadSegment)
    {
        quads = new List<Quad>();
        for (int i = 0; i < axiOfSymetry; i++)
        {
            Point offsetA = GetOffsetPoint(pointA, i, points, axiOfSymetry);
            Point offsetB = GetOffsetPoint(pointB, i, points, axiOfSymetry);
            Point offsetC = GetOffsetPoint(pointC, i, points, axiOfSymetry);
            Point offsetD = GetOffsetPoint(pointD, i, points, axiOfSymetry);
            Quad quad = new Quad(offsetA, offsetB, offsetC, offsetD, quadSegment);
            quads.Add(quad);
        }
    }

    private Point GetOffsetPoint(Point basePoint, int offset, List<Point>[] points, int axiOfSymetry)
    {
        int setEquivalent = (basePoint.SetIndex + offset) % axiOfSymetry;
        return points[setEquivalent][basePoint.IndexInSet];
    }

    public void DeleteSegements()
    {
        foreach (var item in quads)
        {
            item.DeleteSegments();
        }
    }
}

class Quad
{
    public Point PointA { get; }
    public Point PointB { get; }
    public Point PointC { get; }
    public Point PointD { get; }

    public IEnumerable<Point> Points
    {
        get
        {
            yield return PointA;
            yield return PointB;
            yield return PointC;
            yield return PointD;
        }
    }

    private GameObject lineA;
    private GameObject lineB;
    private GameObject lineC;
    private GameObject lineD;

    public Quad(Point pointA, Point pointB, Point pointC, Point pointD, GameObject quadSegment)
    {
        PointA = pointA;
        PointB = pointB;
        PointC = pointC;
        PointD = pointD;
        Vector3 midPoint = (pointA.Pos + pointB.Pos + pointC.Pos + pointD.Pos) / 4;

        Vector2 cornerA = Vector2.Lerp(midPoint, PointA.Pos, .85f);
        Vector2 cornerB = Vector2.Lerp(midPoint, PointB.Pos, .85f);
        Vector2 cornerC = Vector2.Lerp(midPoint, PointC.Pos, .85f);
        Vector2 cornerD = Vector2.Lerp(midPoint, PointD.Pos, .85f);

        Vector3 drawCornerA = new Vector3(cornerA.x, 0, cornerA.y);
        Vector3 drawCornerB = new Vector3(cornerB.x, 0, cornerB.y);
        Vector3 drawCornerC = new Vector3(cornerC.x, 0, cornerC.y);
        Vector3 drawCornerD = new Vector3(cornerD.x, 0, cornerD.y);

        lineA = GameObject.Instantiate(quadSegment);
        lineB = GameObject.Instantiate(quadSegment);
        lineC = GameObject.Instantiate(quadSegment);
        lineD = GameObject.Instantiate(quadSegment);
        BaseGridQuadifier.PlaceSegment(lineA.transform, drawCornerA, drawCornerB);
        BaseGridQuadifier.PlaceSegment(lineB.transform, drawCornerB, drawCornerC);
        BaseGridQuadifier.PlaceSegment(lineC.transform, drawCornerC, drawCornerD);
        BaseGridQuadifier.PlaceSegment(lineD.transform, drawCornerD, drawCornerA);
    }

    public void DeleteSegments()
    {
        GameObject.Destroy(lineA);
        GameObject.Destroy(lineB);
        GameObject.Destroy(lineC);
        GameObject.Destroy(lineD);
    }
}