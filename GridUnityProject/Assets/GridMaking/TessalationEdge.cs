namespace GridMaking
{
    class TessalationEdge
    {
        public TessalationPoint PointA { get; }
        public TessalationPoint PointB { get; }

        public TessalationEdge(TessalationPoint pointA, TessalationPoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }
}