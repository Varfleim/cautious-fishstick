
using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace SandOcean.UI.DesignerWindow
{
    public abstract class UIContentObjectBriefInfoPanel : MonoBehaviour
    {
        public int contentSetIndex;
        public int objectIndex;

        public TextMeshProUGUI objectName;

        public Toggle panelToggle;
    }
}