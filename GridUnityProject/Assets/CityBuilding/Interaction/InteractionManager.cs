using Interaction;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class InteractionManager : MonoBehaviour
{
    [SerializeField]
    private float dragStartDistance = 1;
    [SerializeField]
    private Toggle ExteriorsTabButton;
    [SerializeField]
    private Toggle InteriorsTabButton;
    [SerializeField]
    private GameObject ExteriorsTab;
    [SerializeField]
    private GameObject InteriorsTab;
    [SerializeField]
    private GameObject FoundationTab;
    [SerializeField]
    private Button UndoButton;

    public UiTab SelectedTab;

    private CameraInteraction cameraInteraction;
    private CityBuildingMain gameMain;
    private ExteriorsInteractionManager exteriorsInteractor;
    private InteriorsInteractionManager interiorsInteractor;

    private DragDetector leftDragDetector;
    private DragDetector rightDragDetector;

    private TabComponentGroup[] tabComponents;

    public enum UiTab
    {
        Exteriors,
        Interiors
    }

    private static readonly Plane groundPlane = new Plane(Vector3.up, 0);

    private void Start()
    {
        gameMain = GetComponent<CityBuildingMain>();
        cameraInteraction = GetComponent<CameraInteraction>();
        exteriorsInteractor = GetComponent<ExteriorsInteractionManager>();
        interiorsInteractor = GetComponent<InteriorsInteractionManager>();
        leftDragDetector = new DragDetector(dragStartDistance);
        rightDragDetector = new DragDetector(dragStartDistance);
        tabComponents = GetTabComponents();
        SetInitialTab();
    }

    private TabComponentGroup[] GetTabComponents()
    {
        TabComponentGroup interiorGroup = new TabComponentGroup(this, UiTab.Interiors, InteriorsTabButton, InteriorsTab);
        TabComponentGroup exteriorGroup = new TabComponentGroup(this, UiTab.Exteriors, ExteriorsTabButton, ExteriorsTab);
        return new TabComponentGroup[] { interiorGroup, exteriorGroup };
    }

    private void SetInitialTab()
    {
        ExteriorsTabButton.isOn = SelectedTab == UiTab.Exteriors;
        InteriorsTabButton.isOn = SelectedTab == UiTab.Interiors;
    }

    private void Update()
    {
        bool wasDragging = leftDragDetector.IsDragging || rightDragDetector.IsDragging;
        ManageTabs();
        UndoButton.interactable = gameMain.UndoManager.CanUndo;
        UndoButton.gameObject.SetActive(gameMain.UndoManager.CanUndo);
        UpdateCursorHighlight();
        bool uiHovered = UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

        if(!uiHovered)
        {
            HandleOrbit();
            HandlePan();
            cameraInteraction.HandleMouseScrollwheel();
        }
        exteriorsInteractor.ProceedWithUpdate(wasDragging, uiHovered);
        interiorsInteractor.ProceedWithUpdate(wasDragging, uiHovered);
    }

    private void ManageTabs()
    {
        ExteriorsTab.SetActive(SelectedTab == UiTab.Exteriors);
        InteriorsTab.SetActive(SelectedTab == UiTab.Interiors);
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

    private void UpdateCursorHighlight()
    {
        Vector3 cursorPos = GetGroundPositionAtScreenpoint(Input.mousePosition);
        Shader.SetGlobalVector("_DistToCursor", cursorPos);
    }

    public static Vector3 GetGroundPositionAtScreenpoint(Vector3 screenPoint)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        float enter;
        groundPlane.Raycast(ray, out enter);
        return ray.GetPoint(enter);
    }

    private void UpdateTabsContent()
    {
        foreach (var item in tabComponents)
        {
            item.UpdateTabContent();
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

    private class TabComponentGroup
    {
        private readonly InteractionManager interactionManager;
        private readonly UiTab tabType;
        private readonly Toggle toggleButton;
        private readonly GameObject tabContent;

        public TabComponentGroup(
            InteractionManager interactionManager,
            UiTab tabType, 
            Toggle toggleButton, 
            GameObject tabContent)
        {
            this.interactionManager = interactionManager;
            this.tabType = tabType;
            this.toggleButton = toggleButton;
            this.tabContent = tabContent;

            this.toggleButton.onValueChanged.AddListener(OnToggled);
        }
        private void OnToggled(bool val)
        {
            if (val)
            {
                interactionManager.SelectedTab = tabType;
                interactionManager.UpdateTabsContent();
            }
        }

        public void UpdateTabContent()
        {
            tabContent.SetActive(interactionManager.SelectedTab == tabType);
        }
    }
}
