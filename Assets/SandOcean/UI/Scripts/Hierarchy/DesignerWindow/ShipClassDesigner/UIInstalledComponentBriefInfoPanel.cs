using UnityEngine;
using UnityEngine.UI;

using TMPro;

using SandOcean.Designer;

namespace SandOcean.UI.DesignerWindow.ShipClassDesigner
{
    public class UIInstalledComponentBriefInfoPanel : MonoBehaviour
    {
        public int contentSetIndex;
        public int componentIndex;
        public ShipComponentType componentType;

        public TextMeshProUGUI modelName;
        public TextMeshProUGUI componentTypeName;
        public TextMeshProUGUI componentNumber;
        public TextMeshProUGUI componentTotalSize;

        public Toggle panelToggle;
    }
}