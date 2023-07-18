
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
        None,
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

        public Transform mapCamera;
        public Transform swiwel;
        public Transform stick;
        public UnityEngine.Camera camera;

        public float movementSpeedMinZoom;
        public float movementSpeedMaxZoom;

        public float rotationSpeed;
        public float rotationAngle;
        public float minAngleX;
        public float maxAngleX;

        public float zoom;
        public float stickMinZoom;
        public float stickMaxZoom;
        public float swiwelMinZoom;
        public float swiwelMaxZoom;

        public int currentCenterColumnIndex = -1;


        public EcsPackedEntity playerPE;
        public EcsPackedEntity playerOrganizationPE;

        public EcsPackedEntity activeShipGroupPE;

        public bool isDrag;
        public HexDirection dragDirection;
        public EcsPackedEntity previousRegionPE;

        public EcsPackedEntity searchFromRegion;
        public EcsPackedEntity searchToRegion;
        public HexRegionPriorityQueue searchFrontier;
        public int searchFrontierPhase;

        public EcsPackedEntity currentPathFrom; 
        public EcsPackedEntity currentPathTo;
        public bool currentPathExists;

        public EcsPackedEntity currentRegionPE;
    }
}