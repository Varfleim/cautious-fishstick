
using UnityEngine;

using TMPro;

using Leopotam.EcsLite;
using SandOcean.UI.GameWindow.Object;

namespace SandOcean.UI.GameWindow
{
    public enum ObjectSubpanelType : byte
    {
        None,
        FleetManager,
        Organization,
        Region,
        ORAEO,
        Building
    }

    public class UIObjectPanel : MonoBehaviour
    {
        public TextMeshProUGUI objectName;

        public ObjectSubpanelType activeObjectSubpanelType = ObjectSubpanelType.None;
        public UIAObjectSubpanel activeObjectSubpanel;
        public EcsPackedEntity activeObjectPE;

        public UIFleetManagerSubpanel fleetManagerSubpanel;

        public UIOrganizationSubpanel organizationSubpanel;

        public UIRegionSubpanel regionSubpanel;

        public UIORAEOSubpanel oRAEOSubpanel;

        public UIBuildingSubpanel buildingSubpanel;
    }
}