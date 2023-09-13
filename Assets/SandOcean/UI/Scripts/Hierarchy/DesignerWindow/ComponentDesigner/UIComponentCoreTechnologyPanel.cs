
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

namespace SandOcean.UI.DesignerWindow
{
    public class UIComponentCoreTechnologyPanel : MonoBehaviour
    {
        public TextMeshProUGUI coreTechnologyTypeName;

        public TMP_Dropdown technologiesDropdown;

        public int currentTechnologyIndex;

        public TextMeshProUGUI currentParameterText;
        //public float currentParameterValue;
    }
}