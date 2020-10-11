public class OffGridCell : IGridCell
{
    public bool IsDirty
    {
        get { return false; }
        set { }
    }

    public static IGridCell Instance { get; } = new OffGridCell();

    public bool DoesDownConnectTo(ConnectionType type)
    {
        return true;
    }

    public bool DoesLeftConnectTo(ConnectionType type)
    {
        return true;
    }

    public bool DoesRightConnectTo(ConnectionType type)
    {
        return true;
    }

    public bool DoesUpConnectTo(ConnectionType type)
    {
        return true;
    }
}
