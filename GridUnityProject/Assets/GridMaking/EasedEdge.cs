namespace GridMaking
{
    public class EasedEdge
    {
        public EasedPoint PointA { get; }
        public EasedPoint PointB { get; }

        public EasedEdge(EasedPoint pointA, EasedPoint pointB)
        {
            PointA = pointA;
            PointB = pointB;
        }
    }
}