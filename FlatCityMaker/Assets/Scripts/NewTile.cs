using UnityEngine;

[CreateAssetMenu(menuName = "TileDefinition/NewTile")]
public class NewTile  : ScriptableObject
{
    public TileDesignationType TopLeft;
    public TileDesignationType TopRight;
    public TileDesignationType BottomLeft;
    public TileDesignationType BottomRight;

    public bool DropsStrut;

    public Sprite Sprite;
    public Sprite SpriteReceivingStrut;
    

    public bool HorizontallyFlipped { get; set; }

    public bool GetIsAsymmetrical()
    {
        return TopLeft != TopRight
            || BottomLeft != BottomRight;
    }

    public NewTile GetHorizontallyFlipped()
    {
        NewTile ret = Instantiate(this);
        ret.TopLeft = TopRight;
        ret.TopRight = TopLeft;
        ret.BottomLeft = BottomRight;
        ret.BottomRight = BottomLeft;

        ret.HorizontallyFlipped = true;
        return ret;
    }

    public override string ToString()
    {
        return Sprite.name + (HorizontallyFlipped ? " (flipped)" : "");
    }
}
