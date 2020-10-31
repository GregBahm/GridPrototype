using UnityEngine;

namespace TileDefinition
{
    [CreateAssetMenu(menuName = "TileDefinition/Tile")]
    public class Tile : ScriptableObject
    {
        public Sprite Sprite;
        public TileConnectionPoint TopSideLeft;
        public TileConnectionPoint TopSideRight;
        public TileConnectionPoint LeftSideUpper;
        public TileConnectionPoint LeftSideLower;
        public TileConnectionPoint RightSideUpper;
        public TileConnectionPoint RightSideLower;
        public TileConnectionPoint BottomSideLeft;
        public TileConnectionPoint BottomSideRight;

        public bool HorizontallyFlipped { get; set; }

        public bool GetIsAsymmetrical()
        {
            return TopSideLeft.Type != TopSideRight.Type
                || LeftSideUpper.Type != RightSideUpper.Type
                || LeftSideLower.Type != RightSideLower.Type
                || BottomSideLeft.Type != BottomSideRight.Type;
        }

        public Tile GetHorizontallyFlipped()
        {
            Tile ret = Instantiate(this);
            ret.TopSideLeft = TopSideRight;
            ret.LeftSideUpper = RightSideUpper;
            ret.LeftSideLower = RightSideLower;
            ret.BottomSideLeft = BottomSideRight;

            ret.TopSideRight = TopSideLeft;
            ret.RightSideUpper = LeftSideUpper;
            ret.RightSideLower = LeftSideLower;
            ret.BottomSideRight = BottomSideLeft;

            ret.HorizontallyFlipped = true;
            return ret;
        }

        public override string ToString()
        {
            return Sprite.name + (HorizontallyFlipped ? " (flipped)" : "");
        }
    }
}