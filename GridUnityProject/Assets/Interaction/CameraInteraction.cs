using System;
using UnityEngine;

namespace Interaction
{
    public class CameraInteraction : MonoBehaviour
    {
        [SerializeField]
        private float scrollSpeed;
        [SerializeField]
        private float minZoom;
        [SerializeField]
        private float maxZoom;

        private static readonly Plane orbitPlane = new Plane(Vector3.up, 0);
        private static readonly Vector3 screenCenter = new Vector3(.5f, .5f, 0);
        private Vector3 orbitScreenStart;
        private Vector3 orbitStartAngle;
        private Transform orbitPoint;

        private Transform panPoint;

        private void Start()
        {
            orbitPoint = new GameObject("Camera Orbit").transform;
            panPoint = new GameObject("Camera Pan").transform;
            //orbitPoint.parent = panPoint;
            StartOrbit();
        }

        public void HandleMouseScrollwheel()
        {
            float scaleFactor = 1f - (Input.mouseScrollDelta.y * scrollSpeed);
            float newPos = Camera.main.transform.localPosition.z * scaleFactor;
            newPos = Mathf.Clamp(newPos, minZoom, maxZoom);
            Camera.main.transform.localPosition = new Vector3(0, 0, newPos);
        }

        public void StartOrbit()
        {
            orbitScreenStart = Input.mousePosition;

            Camera.main.transform.SetParent(null, true);
            Vector3 screenPixelCenter = Camera.main.ViewportToScreenPoint(screenCenter);
            Vector3 centerPos = GetPlanePositionAtScreenpoint(screenPixelCenter);
            orbitPoint.transform.position = centerPos;
            orbitPoint.LookAt(Camera.main.transform, Vector3.up);
            Camera.main.transform.SetParent(orbitPoint, true);
            orbitStartAngle = orbitPoint.eulerAngles;
        }

        internal void ContinuePan()
        {
            Vector3 cursorPoint = GetPlanePositionAtScreenpoint(Input.mousePosition);
            orbitPoint.localPosition = cursorPoint - panPoint.position;
        }

        internal void StartPan()
        {
            orbitPoint.SetParent(null, true);
            panPoint.position = orbitPoint.position;
            orbitPoint.SetParent(panPoint, true);
        }

        public void ContinueOrbit()
        {
            Vector3 screenDelta = orbitScreenStart - Input.mousePosition;
            float xAngle = screenDelta.x * -.5f + orbitStartAngle.y;
            float yAngle = (screenDelta.y * -.5f) + orbitStartAngle.x;
            yAngle = Mathf.Clamp(yAngle, 280, 340);
            orbitPoint.rotation = Quaternion.Euler(yAngle, xAngle, 0);
        }

        public static Vector3 GetPlanePositionAtScreenpoint(Vector3 screenPoint)
        {
            Ray ray = Camera.main.ScreenPointToRay(screenPoint);
            float enter;
            orbitPlane.Raycast(ray, out enter);
            return ray.GetPoint(enter);
        }
    }
}