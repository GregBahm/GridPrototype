using GameGrid;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Interaction
{
    public class ExpansionCursor : MonoBehaviour
    {
        private LineRenderer[] lineRenderers;

        [SerializeField]
        private GameObject lineSegmentBasis;
        [SerializeField]
        private int maxSegments = 256;

        [SerializeField]
        private CityBuildingMain builderMain;

        private void Start()
        {
            lineRenderers = new LineRenderer[maxSegments];
            for (int i = 0; i < maxSegments; i++)
            {
                GameObject newSeg = Instantiate(lineSegmentBasis);
                newSeg.name = "Segment " + i;
                newSeg.transform.parent = transform;
                lineRenderers[i] = newSeg.GetComponent<LineRenderer>();
            }
        }

        internal void PreviewExpansion(GridExpander expander)
        {
            Dictionary<int, Vector3> positions = expander.Points
                .ToDictionary(item => item.Index, item => new Vector3(item.Position.x, 0, item.Position.y));
            for (int i = 0; i < maxSegments; i++)
            {
                LineRenderer lineRenderer = lineRenderers[i];
                if (i < expander.Edges.Count)
                {
                    lineRenderer.gameObject.SetActive(true);

                    int startIndex = expander.Edges[i].PointAIndex;
                    int endIndex = expander.Edges[i].PointBIndex;

                    Vector3 start = GetPoint(startIndex, positions);
                    Vector3 end = GetPoint(endIndex, positions);
                    lineRenderer.SetPositions(new Vector3[] { start, end });
                }
                else
                {
                    lineRenderer.gameObject.SetActive(false);
                }
            }
        }

        private Vector3 GetPoint(int index, Dictionary<int, Vector3> newPositions)
        {
            if (newPositions.ContainsKey(index))
            {
                return newPositions[index];
            }
            Vector2 pos = builderMain.MainGrid.Points[index].Position;
            return new Vector3(pos.x, 0, pos.y);
        }
    }
}