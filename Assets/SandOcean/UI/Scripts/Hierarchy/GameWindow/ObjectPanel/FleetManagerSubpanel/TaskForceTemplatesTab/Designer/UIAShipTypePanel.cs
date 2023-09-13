
using UnityEngine;

using TMPro;
using SandOcean.Warfare.Ship;

namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer
{
    public abstract class UIAShipTypePanel : MonoBehaviour
    {
        public UIDesignerSubtab parentTaskForceTemplateDesignerSubtab;

        public DShipType shipType;

        public TextMeshProUGUI shipTypeName;
    }
}