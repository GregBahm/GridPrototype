﻿using GameGrid;
using System.Collections.Generic;
using UnityEngine;

namespace MeshMaking
{
    class MeshBuilderTriangle
    {
        public IMeshBuilderPoint PointA { get; }
        public IMeshBuilderPoint PointB { get; }
        public IMeshBuilderPoint PointC { get; }
        public Vector3 LookTarget { get; }

        public IEnumerable<IMeshBuilderPoint> Points
        {
            get
            {
                yield return PointA;
                yield return PointB;
                yield return PointC;
            }
        }

        public VoxelCell TargetCell { get; }
        public VoxelCell SourceCell { get; }

        public MeshBuilderTriangle(VoxelCell targetCell,
            VoxelCell sourceCell,
            IMeshBuilderPoint pointA,
            IMeshBuilderPoint pointB,
            IMeshBuilderPoint pointC,
            Vector3 lookTarget)
        {
            TargetCell = targetCell;
            SourceCell = sourceCell;
            PointA = pointA;
            bool reorderVerts = GetShouldReorderVerts(lookTarget, pointA.Position, pointB.Position, pointC.Position);
            PointB = reorderVerts ? pointB : pointC;
            PointC = reorderVerts ? pointC : pointB;
            LookTarget = lookTarget;
        }
            public MeshBuilderTriangle(VoxelCell targetCell,
            VoxelCell sourceCell,
            IMeshBuilderPoint pointA,
            IMeshBuilderPoint pointB,
            IMeshBuilderPoint pointC)
            :this(targetCell, sourceCell, pointA, pointB, pointC, targetCell.CellPosition - pointA.Position)
        { }

        private bool GetShouldReorderVerts(Vector3 lookTarget, Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            Vector3 crossVector = Vector3.Cross(pointA - pointB, pointA - pointC);
            return Vector3.Dot(lookTarget, crossVector) > 0;
        }
    }
}