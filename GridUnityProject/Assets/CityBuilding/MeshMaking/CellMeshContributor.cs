using GameGrid;
using System.Collections.Generic;
using System.Linq;

namespace MeshMaking
{
    internal class CellMeshContributor : IMeshContributor
    {
        private readonly DesignationCell cell;
        public IEnumerable<IMeshBuilderPoint> Points { get; }
        public IEnumerable<MeshBuilderTriangle> Triangles { get; }

        public CellMeshContributor(DesignationCell cell)
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
                DesignationCell connectedCell = edge.GetOtherPoint(cell.GroundPoint).DesignationCells[cell.Height];
                yield return new VerticalMeshContributor(cell, edge, connectedCell);
            }
        }

        private bool GetDoesHaveTop()
        {
            return (cell.Height == MainGrid.MaxHeight - 1) || !cell.GroundPoint.DesignationCells[cell.Height + 1].IsFilled;
        }

        private bool GetDoesHaveBottom()
        {
            return cell.Height != 0 && !cell.GroundPoint.DesignationCells[cell.Height - 1].IsFilled;
        }
    }
}