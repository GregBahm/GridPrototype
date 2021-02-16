namespace VisualsSolver
{
    public class VoxelDesignation
    {
        public bool UpLeftFilled { get; }
        public bool UpRightFilled { get; }
        public bool DownLeftFilled { get; }
        public bool DownRightFilled { get; }
        public string Key { get; }

        public VoxelDesignation(bool upLeft,
            bool upRight,
            bool downLeft,
            bool downRight)
        {
            UpLeftFilled = upLeft;
            UpRightFilled = upRight;
            DownLeftFilled = downLeft;
            DownRightFilled = downRight;
            Key = UpLeftFilled + " "
                + UpRightFilled + " "
                + DownLeftFilled + " "
                + DownRightFilled;
        }
    }
}