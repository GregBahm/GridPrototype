using System.Collections;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

namespace TileDefinition
{
    [CreateAssetMenu(menuName = "TileDefinition/Tile")]
    public class Tile : ScriptableObject
    {
        public Sprite Sprite;
        public TileConnectionType Up;
        public TileConnectionType Down;
        public TileConnectionType Left;
        public TileConnectionType Right;
        public TileConnectionType UpLeft;
        public TileConnectionType UpRight;
        public TileConnectionType DownLeft;
        public TileConnectionType DownRight;

        public bool HorizontallyFlipped { get; set; }

        public bool GetIsAsymmetrical()
        {
            return Left != Right
                || UpLeft != UpRight
                || DownLeft != DownRight;
        }

        public Tile GetHorizontallyFlipped()
        {
            Tile ret = Instantiate(this);
            ret.Left = Right;
            ret.Right = Left;
            ret.UpLeft = UpRight;
            ret.UpRight = UpLeft;
            ret.DownLeft = DownRight;
            ret.DownRight = DownLeft;
            ret.HorizontallyFlipped = true;
            return ret;
        }

        public override string ToString()
        {
            return Sprite.name;
        }
    }
}