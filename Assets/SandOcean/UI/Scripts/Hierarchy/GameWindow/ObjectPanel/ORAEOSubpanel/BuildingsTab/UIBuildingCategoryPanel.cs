
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace SandOcean.UI.GameWindow.Object.ORAEO.Buildings
{
    public class UIBuildingCategoryPanel : MonoBehaviour
    {
        public UIBuildingsTab parentBuildingsTab;

        public List<UIBuildingSummaryPanel> buildingSummaryPanels = new();
        public FlexibleGridLayout layoutGroup;
    }
}