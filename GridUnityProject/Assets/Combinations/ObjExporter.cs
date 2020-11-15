using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class ObjExporter
{
    public static void GameObjectToFile(GameObject obj, string path)
    {
        string data = MeshToString(obj);
        File.WriteAllText(path, data);
    }

    public static string MeshToString(GameObject obj)
    {
        StringBuilder ret = new StringBuilder();
        IEnumerable<MeshFilter> filters = obj.GetComponentsInChildren<MeshFilter>();
        //MeshFilter[] filters = new MeshFilter[] { obj.GetComponentInChildren<MeshFilter>() };
        ret.Append("g ").Append(obj.name).Append("\n");
        foreach (MeshFilter filter in filters)
        {
            foreach (Vector3 vert in filter.mesh.vertices)
            {
                Vector3 worldPoint = filter.transform.TransformPoint(vert);
                ret.Append(string.Format("v {0} {1} {2}\n", worldPoint.x, worldPoint.y, worldPoint.z));
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
}