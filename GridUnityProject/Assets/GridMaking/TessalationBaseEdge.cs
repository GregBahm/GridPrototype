namespace GridMaking
{
    class TessalationBaseEdge
    {
        public TessalationPoint PointA { get; }
        public TessalationPoint PointB { get; }

        public TessalationPoint SubPoint { get; }

        public TessalationEdge SubEdgeA { get; }
        public TessalationEdge SubEdgeB { get; }

        public TessalationBaseEdge(TessalationPoint pointA, TessalationPoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
            SubPoint = new TessalationPoint((pointA.Pos + pointB.Pos) / 2, pointA.IsBorder && pointB.IsBorder);
            SubEdgeA = new TessalationEdge(PointA, SubPoint);
            SubEdgeB = new TessalationEdge(SubPoint, pointB);
        }
    }
}