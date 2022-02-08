using System;
using UnityEngine;
using UnityEngine.UI;

namespace Interaction
{
    public class InteriorsInteractionManager : MonoBehaviour
    {
        [SerializeField]
        private Toggle addButton;

        private Interior selectedInterior;

        public void ProceedWithUpdate(bool wasDragging, bool uiHovered)
        {
            if(!wasDragging && !uiHovered)
            {
                if(addButton.isOn)
                {
                    selectedInterior = null;
                    HandleAddRoom();
                }
                else
                {
                    HandleSelectRoom();
                }
            }
        }

        private void HandleAddRoom()
        {
            // In this case, filled designation cells without interiors can be clicked on to start a new room.
            // Right clicking cells with interiors clears the interior
        }

        private void HandleSelectRoom()
        {
            // In this case, hovering an interior highlights it, and clicking it makes it selected
            // So once they select a room, they can edit it. The "+" button becomes a "check" for done
            // Once they're done editing, they can click the check, which sets selectedInterior back to null
        }
    }
}