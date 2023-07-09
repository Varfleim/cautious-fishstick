
using UnityEngine;

using TMPro;

using Leopotam.EcsLite;

namespace SandOcean.UI
{
    public enum ObjectSubpanelType : byte
    {
        None,
        Organization,
        Region,
        ORAEO
    }

    public class UIObjectPanel : MonoBehaviour
    {
        public TextMeshProUGUI objectName;

        public ObjectSubpanelType activeObjectSubpanelType = ObjectSubpanelType.None;
        public UIObjectSubpanel activeObjectSubpanel;
        public EcsPackedEntity activeObjectPE;

        public UIOrganizationObjectSubpanel organizationObjectSubpanel;

        public UIRegionObjectSubpanel regionObjectSubpanel;

        public UIORAEOObjectSubpanel oRAEOObjectSubpanel;
    }
}