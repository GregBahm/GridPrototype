using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VoxelVisuals;

public class AnchoringTester : MonoBehaviour
{
    public Transform Anchor00;
    public Transform Anchor01;
    public Transform Anchor02;
    public Transform Anchor10;
    public Transform Anchor11;
    public Transform Anchor12;
    public Transform Anchor20;
    public Transform Anchor21;
    public Transform Anchor22;

    public Transform MainA;
    public Transform MainB;
    public Transform MainC;
    public Transform MainD;

    public Material HouseAMat;
    public Material HouseBMat;
    public Material HouseCMat;
    public Material HouseDMat;

    private Group groupA;
    private Group groupB;
    private Group groupC;
    private Group groupD;

    private Group[] groups;

    private void Start()
    {
        groupA = new Group(MainA, HouseAMat, Anchor10, Anchor11, Anchor01, Anchor00, true);
        groupB = new Group(MainB, HouseBMat, Anchor12, Anchor11, Anchor01, Anchor02);
        groupC = new Group(MainC, HouseCMat, Anchor10, Anchor11, Anchor21, Anchor20);
        groupD = new Group(MainD, HouseDMat, Anchor12, Anchor11, Anchor21, Anchor22, true);
        groups = new Group[] { groupA, groupB, groupC, groupD };
    }

    private void Update()
    {
        foreach (Group group in groups)
        {
            group.Update();
        }
    }

    private class Group
    {
        private readonly Transform MainTransform;
        private readonly Material mat;
        private readonly Transform anchorA;
        private readonly Transform anchorB;
        private readonly Transform anchorC;
        private readonly Transform anchorD;

        public Vector3 ContentPosition
        {
            get
            {
                return (anchorA.position
                    + anchorB.position
                    + anchorC.position
                    + anchorD.position) / 4;
            }
        }

        private readonly bool flip;

        public Group(Transform mainTransform, Material mat, Transform anchorA, Transform anchorB, Transform anchorC, Transform anchorD, bool flip = false)
        {
            this.flip = flip;
            MainTransform = mainTransform;
            this.mat = mat;
            this.anchorA = anchorA;
            this.anchorB = anchorB;
            this.anchorC = anchorC;
            this.anchorD = anchorD;
        }
        private void SetAnchoring(GroundPointAnchor anchor, string letter)
        {
            Vector3 baseAnchorPosition = GetRelativeAnchorPosition(anchor);
            mat.SetVector("_" + letter + "Anchor", baseAnchorPosition);
            mat.SetVector("_" + letter + "Xnorm", anchor.XNormal);
            mat.SetVector("_" + letter + "Znorm", anchor.YNormal);
        }

        private Vector3 GetRelativeAnchorPosition(GroundPointAnchor anchor)
        {
            return new Vector3(anchor.AbsolutePosition.x - ContentPosition.x, 0, anchor.AbsolutePosition.y - ContentPosition.z);
        }

        internal void Update()
        {
            MainTransform.position = ContentPosition;
            mat.SetFloat("_Cull", flip ? 1 : 2);
            GroundPointAnchor a = GetAnchor(anchorA, anchorB, anchorD, 1, -1);
            SetAnchoring(a, "A");
            GroundPointAnchor b = GetAnchor(anchorB, anchorA, anchorC, -1, -1);
            SetAnchoring(b, "B");
            GroundPointAnchor c = GetAnchor(anchorC, anchorD, anchorB, 1, 1);
            SetAnchoring(c, "C");
            GroundPointAnchor d = GetAnchor(anchorD, anchorC, anchorA, -1, 1);
            SetAnchoring(d, "D");
        }

        private GroundPointAnchor GetAnchor(Transform anchor, Transform xAnchor, Transform zAnchor, float xDirection, float zDirection)
        {
            Vector3 xNorm = anchor.position - xAnchor.position;
            Vector3 zNorm = anchor.position - zAnchor.position;
            Debug.DrawLine(anchor.position, anchor.position + xNorm * .5f, Color.red);
            Debug.DrawLine(anchor.position, anchor.position + zNorm * .5f, Color.blue);
            Vector2 pos = new Vector2(anchor.position.x, anchor.position.z);
            Vector2 x = new Vector2(xNorm.x, xNorm.z).normalized * xDirection;
            Vector2 z = new Vector2(zNorm.x, zNorm.z).normalized * zDirection;
            return new GroundPointAnchor(pos, x, z);
        }
    }
}
