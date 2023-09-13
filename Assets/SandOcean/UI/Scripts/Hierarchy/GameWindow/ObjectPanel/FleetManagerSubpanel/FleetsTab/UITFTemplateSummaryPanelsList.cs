
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using SandOcean.UI.GameWindow.Object.FleetManager.Fleets;

namespace SandOcean.UI.GameWindow
{
    public class UITFTemplateSummaryPanelsList : MonoBehaviour
    {
        public RectTransform selfRectTransform;

        public List<UITFTemplateSummaryPanel> templatePanels = new();
        public VerticalLayoutGroup templateLayoutGroup;
    }
}