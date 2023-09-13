
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Leopotam.EcsLite;

namespace SandOcean.UI.GameWindow.Object.FleetManager.Fleets
{
    public class UIFleetSummaryPanel : MonoBehaviour
    {
        public UIFleetsTab parentFleetsTab;

        public TextMeshProUGUI selfName;
        public EcsPackedEntity selfPE;

        public Toggle toggle;

        public List<UITaskForceSummaryPanel> taskForceSummaryPanels = new();
        public VerticalLayoutGroup taskForcesLayoutGroup;
    }
}