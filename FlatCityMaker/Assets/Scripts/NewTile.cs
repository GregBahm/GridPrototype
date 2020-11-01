using UnityEngine;

[CreateAssetMenu(menuName = "TileDefinition/NewTile")]
public class NewTile  : ScriptableObject
{
    public bool TopLeftFilled;
    public bool TopRightFilled;
    public bool BottomLeftFilled;
    public bool BottomRightFilled;

    public bool DropsStrut;

    public Sprite Default;
    public Sprite DefaultReceivingStrut;
    
    public Sprite SlantedVariant;
    public Sprite SlantedReceivingStrut;

    public Sprite RoundedVariant;
    public Sprite RoundedReceivingStrut;


    public bool HorizontallyFlipped { get; set; }

    public bool GetIsAsymmetrical()
    {
        return TopLeftFilled != TopRightFilled
            || BottomLeftFilled != BottomRightFilled;
    }

    public NewTile GetHorizontallyFlipped()
    {
        NewTile ret = Instantiate(this);
        ret.TopLeftFilled = TopRightFilled;
        ret.TopRightFilled = TopLeftFilled;
        ret.BottomLeftFilled = BottomRightFilled;
        ret.BottomRightFilled = BottomLeftFilled;

        ret.HorizontallyFlipped = true;
        return ret;
    }

    public override string ToString()
    {
        return Default.name + (HorizontallyFlipped ? " (flipped)" : "");
    }
}

public class TileFill
{
    public Sprite Sprite { get; }
    public bool HorizontallyFlipped { get; }

    public TileFill(Sprite sprite, bool horizontallyFlipped)
    {
        Sprite = sprite;
        HorizontallyFlipped = horizontallyFlipped;
    }
}
