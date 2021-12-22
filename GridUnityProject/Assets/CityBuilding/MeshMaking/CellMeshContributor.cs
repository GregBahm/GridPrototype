using GameGrid;
using System.Collections.Generic;
using System.Linq;

namespace MeshMaking
{
    internal class CellMeshContributor : IMeshContributor
    {
        private readonly VoxelCell cell;
        public IEnumerable<IMeshBuilderPoint> Points { get; }
        public IEnumerable<MeshBuilderTriangle> Triangles { get; }

        public CellMeshContributor(VoxelCell cell)
        {
            this.cell = cell;

            List<IMeshContributor> subContributors = new List<IMeshContributor>();
            if (GetDoesHaveBottom())
            {
                HorizontalMeshContributor groundContributor = new HorizontalMeshContributor(cell, false);
                subContributors.Add(groundContributor);
            }
            if (GetDoesHaveTop())
            {
                HorizontalMeshContributor groundContributor = new HorizontalMeshContributor(cell, true);
                subContributors.Add(groundContributor);
            }
            VerticalMeshContributor[] sidesToFill = GetSidesToFill().ToArray();
            subContributors.AddRange(sidesToFill);
            Points = subContributors.SelectMany(item => item.Points).ToArray();
            Triangles = subContributors.SelectMany(item => item.Triangles).ToArray();
        }

        private IEnumerable<VerticalMeshContributor> GetSidesToFill()
        {
            foreach (GroundEdge edge in cell.GroundPoint.Edges)
            {
                VoxelCell connectedCell = edge.GetOtherPoint(cell.GroundPoint).Voxels[cell.Height];
                yield return new VerticalMeshContributor(cell, edge, connectedCell);
            }
        }

        private bool GetDoesHaveTop()
        {
            return (cell.Height == MainGrid.VoxelHeight - 1) || !cell.GroundPoint.Voxels[cell.Height + 1].IsFilled;
        }

        private bool GetDoesHaveBottom()
        {
            return cell.Height != 0 && !cell.GroundPoint.Voxels[cell.Height - 1].IsFilled;
        }
    }
}