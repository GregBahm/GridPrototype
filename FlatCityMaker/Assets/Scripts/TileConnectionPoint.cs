using System;

namespace TileDefinition
{
    [Serializable]
    public class TileConnectionPoint
    {
        public TileConnectionType Type;
        public bool ImposesConnection;

        public TileConnectionPoint(TileConnectionType type, bool imposesConnection)
        {
            Type = type;
            ImposesConnection = imposesConnection;
        }
    }
}