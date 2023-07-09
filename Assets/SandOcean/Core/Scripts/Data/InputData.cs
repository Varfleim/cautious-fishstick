
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

using SandOcean.UI.Camera;
using SandOcean.Map;

namespace SandOcean.UI
{
    public enum MapMode : byte
    {
        Default,
        Distance
    }

    public enum OptionalToggle : byte
    {
        Ignore,
        No,
        Yes
    }

    public class InputData : MonoBehaviour
    {
        public MapMode mapMode = MapMode.Default;

        public EcsPackedEntity cameraPE;

        public Transform panObject;
        public Transform rotationObjectX;
        public Transform rotationObjectZ;
        public UnityEngine.Camera camera;

        public float movementSpeed;

        public Vector3 dragStartPosition;
        public Vector3 dragCurrentPosition;


        public float rotationSpeed;

        public float rotationZ;
        public float rotationAnglesZ;

        public float rotationX;
        public float rotationAnglesX;
        public float minAngleX;
        public float maxAngleX;


        public float zoomSpeed;
        public float zoomAmount;
        public float maxZoom;
        public float minZoom;



        public Vector3 cameraFocusPosition = new Vector3(0f, 0f, 0f);
        public Vector3 cameraFocusMoving;

        public EcsPackedEntity playerPE;
        public EcsPackedEntity playerOrganizationPE;

        public EcsPackedEntity activeShipGroupPE;

        public bool isDrag;
        public HexDirection dragDirection;
        public EcsPackedEntity previousRegionPE;

        public EcsPackedEntity searchFromRegion;
        public EcsPackedEntity searchToRegion;
        public HexCellPriorityQueue searchFrontier;
    }
}