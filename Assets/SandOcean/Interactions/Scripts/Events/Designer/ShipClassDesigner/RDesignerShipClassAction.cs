
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SandOcean.Designer;

namespace SandOcean.UI.Events
{
    public enum DesignerShipClassActionType : byte
    {
        None,
        AddComponentToClass,
        DeleteComponentFromClass,
        DisplayComponentDetailedInfo,
        HideComponentDetailedInfo,
        ChangeAvailableComponentsType
    }

    public struct RDesignerShipClassAction
    {
        public DesignerShipClassActionType actionType;

        public ShipComponentType componentType;

        public int contentSetIndex;
        public int modelIndex;

        public int numberOfComponents;
    }
}