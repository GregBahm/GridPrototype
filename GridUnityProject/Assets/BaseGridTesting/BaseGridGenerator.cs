using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

public class BaseGridGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject pointMarker;

    private List<Vector2> points;

    [SerializeField]
    private float searchDistanceIncrement = .1f;
    [SerializeField]
    private float searchRotationIncrement = 1f;
    [SerializeField]
    private int axisOfSymetry = 4;

    private float searchDistance;
    private float searchRotation;

    private float searchRotationMax;

    [SerializeField]
    private bool Go;

    [SerializeField]
    private bool Save;

    [SerializeField]
    private bool Load;

    private void Start()
    {
        points = new List<Vector2>();
        searchRotationMax = 360f / axisOfSymetry;
    }

    private void Update()
    {
        if (Go)
            Iterate();
        if(Save)
        {
            Save = false;
            DoSave();
        }
    }

    private void DoSave()
    {
        BaseGridGeneratorSave theSave = new BaseGridGeneratorSave(points);
        string filePath = Application.dataPath + "/BaseGridTesting/gridSave.txt";
        string asJson = JsonUtility.ToJson(theSave);
        System.IO.File.WriteAllText(filePath, asJson);
    }

    private void Iterate()
    {
        searchRotation += searchRotationIncrement;
        if(searchRotation > searchRotationMax)
        {
            searchRotation = 0;
            searchDistance += searchDistanceIncrement;
        }

        Vector2 prospectivePoint = GetProspectivePoint();
        bool canPlace = GetCanPlace(prospectivePoint);
        if (canPlace)
        {
            PlacePoint(prospectivePoint);
        }
    }

    private void PlacePoint(Vector2 prospectivePoint)
    {
        for (int i = 0; i < axisOfSymetry; i++)
        {
            Vector2 point = Rotate(prospectivePoint, i * searchRotationMax);
            points.Add(point);
            GameObject obj = Instantiate(pointMarker);
            obj.transform.localPosition = new Vector3(point.x, 0, point.y);
        }
    }

    private bool GetCanPlace(Vector2 prospectivePoint)
    {
        foreach (Vector2 point in points)
        {
            float squareDist = (prospectivePoint - point).sqrMagnitude;
            if (squareDist < 1f)
                return false;
        }
        return true;
    }

    private Vector2 GetProspectivePoint()
    {
        Vector2 vect = new Vector2(searchDistance, 0);
        return Rotate(vect, searchRotation);
    }
    private static Vector2 Rotate(Vector2 vect, float degrees)
    {
        float radianAngle = Mathf.Deg2Rad * degrees;
        return new Vector2(
            vect.x * Mathf.Cos(radianAngle) - vect.y * Mathf.Sin(radianAngle),
            vect.x * Mathf.Sin(radianAngle) + vect.y * Mathf.Cos(radianAngle)
        );
    }
}

[Serializable]
public class BaseGridGeneratorSave
{
    [SerializeField]
    private List<Vector2> points;
    public List<Vector2> Points => points;

    public BaseGridGeneratorSave(List<Vector2> points)
    {
        this.points = points;
    }
}