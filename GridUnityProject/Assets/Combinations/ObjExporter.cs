using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System;

public class ObjExporter
{
    public static void GameObjectToFile(GameObject obj, int objId, string path)
    {
        string data = MeshToString(obj, objId);
        File.WriteAllText(path, data);
    }

    public static string MeshToString(GameObject obj, int objId)
    {
        StringBuilder ret = new StringBuilder();
        IEnumerable<MeshFilter> filters = obj.GetComponentsInChildren<MeshFilter>();
        ret.Append("g ").Append("BaseObj" + objId).Append("\n");
        foreach (MeshFilter filter in filters)
        {
            foreach (Vector3 vert in filter.mesh.vertices)
            {
                Vector3 transformedPoint = GetTransformedPoint(vert, filter.transform, obj.transform.position);
                ret.Append(string.Format("v {0} {1} {2}\n", transformedPoint.x, transformedPoint.y, transformedPoint.z));
            }
        }
        ret.Append("\n");

        int mainOffset = 1;
        foreach (MeshFilter filter in filters)
        {
            int[] triangles = filter.mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int a = triangles[i] + mainOffset;
                int b = triangles[i + 1] + mainOffset;
                int c = triangles[i + 2] + mainOffset;
                ret.Append(string.Format("f {0} {1} {2}\n",
                    a, b, c));
            }
            mainOffset += filter.mesh.vertices.Length;
        }
        return ret.ToString();
    }

    private static Vector3 GetTransformedPoint(Vector3 vert, Transform objTransform, Vector3 rootOffset)
    {
        Vector3 ret = objTransform.TransformPoint(vert);
        ret -= rootOffset;
        ret = ret * 2;
        return ret;
    }
}