using System.Linq;
using TileDefinition;
using UnityEngine;
using UnityEngine.UI;

public class TileVisualBehavior : MonoBehaviour
{
    private Image image;
    private static Vector3 FlippedCoords { get; } = new Vector3(-1, 1, 1); 

    public GridCell Model { get; set; }

    private void Start()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        if(Model != null)
        {
            if(Model.FilledWith != null)
            {
                image.sprite = Model.FilledWith.Sprite;
                image.transform.localScale = Model.FilledWith.HorizontallyFlipped ? Vector3.one : FlippedCoords;
            }
            else
            {
                image.sprite = null;
            }
        }
    }
}

