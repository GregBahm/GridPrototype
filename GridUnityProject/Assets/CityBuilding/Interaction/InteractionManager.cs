using Interaction;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [SerializeField]
    private float dragStartDistance = 2;

    private CameraInteraction cameraInteraction;
    private CityBuildingMain gameMain;
    private ExteriorsInteractionManager exteriorsInteractor;
    private InteriorsInteractionManager interiorsInteractor;
    private FoundationInteractionManager foundationInteractor;

    private DragDetector leftDragDetector;
    private DragDetector rightDragDetector;

    public bool GroundModificationMode;

    public Toggle ExteriorsTabButton;
    public Toggle InteriorsTabButton;
    public Toggle FoundationTabButton;
    public GameObject ExteriorsTab;
    public GameObject InteriorsTab;
    public GameObject FoundationTab;

    public Button UndoButton;

    public UiTab SelectedTab;
    public enum UiTab
    {
        Exteriors,
        Interiors,
        Foundation
    }

    private void Start()
    {
        gameMain = GetComponent<CityBuildingMain>();
        cameraInteraction = GetComponent<CameraInteraction>();
        exteriorsInteractor = GetComponent<ExteriorsInteractionManager>();
        interiorsInteractor = GetComponent<InteriorsInteractionManager>();
        foundationInteractor = GetComponent<FoundationInteractionManager>();
        leftDragDetector = new DragDetector(dragStartDistance);
        rightDragDetector = new DragDetector(dragStartDistance);
    }

    private void Update()
    {
        bool wasDragging = leftDragDetector.IsDragging || rightDragDetector.IsDragging;
        ManageTabs();
        UndoButton.interactable = gameMain.UndoManager.CanUndo;
        UndoButton.gameObject.SetActive(gameMain.UndoManager.CanUndo);

        if(UnityEngine.EventSystems.EventSystem.current.currentSelectedGameObject != null)
        {
            return;
        }
        HandleOrbit();
        HandlePan();
        cameraInteraction.HandleMouseScrollwheel();
        switch (SelectedTab)
        {
            case UiTab.Exteriors:
                exteriorsInteractor.ProceedWithUpdate(wasDragging);
                break;
            case UiTab.Interiors:
                interiorsInteractor.ProceedWithUpdate();
                break;
            case UiTab.Foundation:
            default:
                foundationInteractor.ProceedWithUpdate();
                break;
        }
    }


    public void SetTabToExteriors()
    {
        if (ExteriorsTabButton.isOn)
            SelectedTab = UiTab.Exteriors;
    }
    public void SetTabToInteriors()
    {
        if (InteriorsTabButton.isOn)
            SelectedTab = UiTab.Interiors;
    }
    public void SetTabToFoundation()
    {
        if (FoundationTabButton.isOn)
            SelectedTab = UiTab.Foundation;
    }

    private void ManageTabs()
    {
        ExteriorsTab.SetActive(SelectedTab == UiTab.Exteriors);
        InteriorsTab.SetActive(SelectedTab == UiTab.Interiors);
        FoundationTab.SetActive(SelectedTab == UiTab.Foundation);
    }

    public void Undo()
    {
        gameMain.UndoManager.Undo();
    }
    private void HandlePan()
    {
        if (Input.GetMouseButton(1))
        {
            if (rightDragDetector.IsDragging)
            {
                cameraInteraction.ContinuePan();
            }
            else
            {
                if (Input.GetMouseButtonDown(1))
                {
                    rightDragDetector.DragStartPos = Input.mousePosition;
                    cameraInteraction.StartPan();
                }
                else
                {
                    rightDragDetector.UpdateIsDragging();
                }
            }
        }
        else
        {
            rightDragDetector.IsDragging = false;
        }
    }

    private void HandleOrbit()
    {
        if (Input.GetMouseButton(0))
        {
            if(leftDragDetector.IsDragging)
            {
                cameraInteraction.ContinueOrbit();
            }
            else
            {
                if (Input.GetMouseButtonDown(0))
                {
                    leftDragDetector.DragStartPos = Input.mousePosition;
                    cameraInteraction.StartOrbit();
                }
                else
                {
                    leftDragDetector.UpdateIsDragging();
                }
            }
        }
        else
        {
            leftDragDetector.IsDragging = false;
        }
    }
    private class DragDetector
    {
        public Vector3 DragStartPos { get; set; }
        public bool IsDragging { get; set; }
        private readonly float dragStartDistance;

        public DragDetector(float dragStartDistance)
        {
            this.dragStartDistance = dragStartDistance;
        }

        public void UpdateIsDragging()
        {
            IsDragging = (DragStartPos - Input.mousePosition).magnitude > dragStartDistance;
        }
    }
}
