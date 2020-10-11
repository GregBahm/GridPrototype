public interface IGridCell
{
    bool IsDirty { get; set; }
    bool DoesLeftConnectTo(ConnectionType type);
    bool DoesRightConnectTo(ConnectionType type);
    bool DoesUpConnectTo(ConnectionType type);
    bool DoesDownConnectTo(ConnectionType type);
}
