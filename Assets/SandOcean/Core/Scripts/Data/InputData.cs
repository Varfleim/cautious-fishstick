
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

    public enum RefresUIType : byte
    {
        Refresh,
        Delete
    }

    public class InputData : MonoBehaviour
    {
        public MapMode mapMode = MapMode.Default;

        public static bool leftMouseButtonClick;
        public static bool leftMouseButtonPressed;
        public static bool leftMouseButtonRelease;

        public static bool rightMouseButtonClick;
        public static bool rightMouseButtonPressed;
        public static bool rightMouseButtonRelease;

        public static bool leftShiftKeyPressed;
        public static bool LMBAndLeftShift
        {
            get
            {
                return (leftMouseButtonClick || leftMouseButtonPressed) && leftShiftKeyPressed;
            }
        }
        public static bool RMBAndLeftShift
        {
            get
            {
                return (rightMouseButtonClick || rightMouseButtonPressed) && leftShiftKeyPressed;
            }
        }

        public EcsPackedEntity cameraPE;

        public Transform mapCamera;
        public Transform swiwel;
        public Transform stick;
        public UnityEngine.Camera camera;

        public float movementSpeedMinZoom;
        public float movementSpeedMaxZoom;

        public float rotationSpeed;
        public float rotationAngleY;
        public float rotationAngleX;
        public float minAngleX;
        public float maxAngleX;

        public float zoom;
        public float stickMinZoom;
        public float stickMaxZoom;
        public float swiwelMinZoom;
        public float swiwelMaxZoom;


        public EcsPackedEntity playerPE;
        public EcsPackedEntity playerOrganizationPE;


        public static bool isMouseOver;
        public static int lastHitRegionIndex;
        public static EcsPackedEntity lastHighlightedRegionPE;
        public static int lastHighlightedRegionIndex;

        public static EcsPackedEntity searchFromRegion;
        public static EcsPackedEntity searchToRegion;

        public static EcsPackedEntity activeFleetPE;

        public static List<EcsPackedEntity> activeTaskForcePEs = new();
    }
}