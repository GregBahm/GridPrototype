﻿using GameGrid;
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System.Collections.ObjectModel;

namespace MeshMaking
{
    public class MeshHitTarget
    {
        public VoxelCell TargetCell { get; }
        public VoxelCell SourceCell { get; }
        public Vector3[] FaceVerts { get; }
        public Vector3 Center { get; }

        internal MeshHitTarget(VoxelCell targetCell, VoxelCell sourceCell, IEnumerable<MeshBuilderTriangle> tris)
        {
            TargetCell = targetCell;
            SourceCell = sourceCell;
            Vector3[] unsortedFaceVerts = GetUnsortedFaceVerts(tris).ToArray();
            Center = Average(unsortedFaceVerts);
            FaceVerts = GetSortedFaceVerts(tris, unsortedFaceVerts).ToArray();
        }

        private IEnumerable<Vector3> GetSortedFaceVerts(IEnumerable<MeshBuilderTriangle> tris, Vector3[] unsortedFaceVerts)
        {
            Vector3 facingAngle = Average(tris.Select(item => item.LookTarget).ToArray()).normalized;
            Vector3 firstPoint = GetProjectedPoint(facingAngle, unsortedFaceVerts.First());
            return unsortedFaceVerts.OrderBy(item => GetAngle(item, facingAngle, firstPoint));
        }

        private object GetAngle(Vector3 pointPos, Vector3 facingAngle, Vector3 firstPoint)
        {
            Vector3 projectedPoint = GetProjectedPoint(facingAngle, pointPos);
            return Vector3.SignedAngle(firstPoint, projectedPoint, facingAngle);
        }

        private Vector3 GetProjectedPoint(Vector3 facingAngle, Vector3 pointPos)
        {
            Vector3 toPoint = pointPos - Center;
            return Vector3.ProjectOnPlane(toPoint, facingAngle);
        }

        private Vector3 Average(Vector3[] vert)
        {
            Vector3 ret = Vector3.zero;
            foreach (Vector3 vector in vert)
            {
                ret += vector;
            }
            return ret / vert.Length;
        }

        private IEnumerable<Vector3> GetUnsortedFaceVerts(IEnumerable<MeshBuilderTriangle> tris)
        {
            HashSet<string> vertKeys = new HashSet<string>();
            foreach (var item in tris)
            {
                foreach (IMeshBuilderPoint point in item.Points.Where(point => !point.IsCellPoint))
                {
                    if(!vertKeys.Contains(point.Key))
                    {
                        yield return point.Position;
                        vertKeys.Add(point.Key);
                    }
                }
            }
        }
    }
}