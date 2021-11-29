using System;
using UnityEngine;

namespace Interaction
{
    public class CameraInteraction : MonoBehaviour
    {
        [SerializeField]
        private float scrollSpeed = 0.1f;
        [SerializeField]
        private float minZoom = 10;
        [SerializeField]
        private float maxZoom = 100;
        [SerializeField]
        private float orbitSpeed = 0.5f;
        [SerializeField]
        private float panSpeed = 0.1f;

        private static readonly Vector3 screenCenter = new Vector3(.5f, .5f, 0);
        private Vector3 orbitScreenStart;
        private Vector3 orbitStartAngle;
        private Transform orbitPoint;

        private Vector3 panScreenStart;
        private Vector3 panCameraStart;

        private void Start()
        {
            orbitPoint = new GameObject("Camera Orbit").transform; 
            orbitPoint.LookAt(Camera.main.transform, Vector3.up);
            Camera.main.transform.SetParent(orbitPoint, true);
            StartOrbit();
        }

        public void HandleMouseScrollwheel()
        {
            Vector3 oldPos = Camera.main.transform.localPosition;
            float scaleFactor = 1f - (Input.mouseScrollDelta.y * scrollSpeed);
            float newPos = oldPos.z * scaleFactor;
            newPos = Mathf.Clamp(newPos, minZoom, maxZoom);

            Camera.main.transform.localPosition = new Vector3(oldPos.x, oldPos.y, newPos);
        }

        public void StartOrbit()
        {
            orbitScreenStart = Input.mousePosition;
            orbitStartAngle = orbitPoint.eulerAngles;
        }

        internal void ContinuePan()
        {
            Vector3 offset = Input.mousePosition - panScreenStart;
            offset *= new Vector2(panSpeed, -panSpeed);
            Camera.main.transform.localPosition = panCameraStart + offset;
        }

        internal void StartPan()
        {
            panScreenStart = Input.mousePosition;
            panCameraStart = Camera.main.transform.localPosition;
        }


        public void ContinueOrbit()
        {
            Vector3 screenDelta = orbitScreenStart - Input.mousePosition;
            float yaw = -screenDelta.x * orbitSpeed + orbitStartAngle.y;
            float startAngle = orbitStartAngle.x;
            if(startAngle < 180)
            {
                startAngle += 360;
            }
            float pitch = -screenDelta.y * orbitSpeed + startAngle;
            pitch = Mathf.Clamp(pitch, 280, 440);
            orbitPoint.rotation = Quaternion.Euler(pitch, yaw, 0);
        }
    }
}