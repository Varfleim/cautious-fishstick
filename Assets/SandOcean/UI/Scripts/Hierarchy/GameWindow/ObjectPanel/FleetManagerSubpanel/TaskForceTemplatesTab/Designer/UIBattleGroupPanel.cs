
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer
{
    public class UIBattleGroupPanel : MonoBehaviour
    {
        public VerticalLayoutGroup currentShipTypesLayoutGroup;
        public List<UICurrentShipTypePanel> currentShipTypePanels = new();

        public UIAddShipTypesButton addShipTypesButton;

        public VerticalLayoutGroup availableShipTypesLayoutGroup;
        public List<UIAvailableShipTypePanel> availableShipTypePanels = new();

        public void ShowNewShipTypes()
        {
            //≈сли список новых типов кораблей отображаетс€, скрываем его
            if (availableShipTypesLayoutGroup.isActiveAndEnabled == true)
            {
                availableShipTypesLayoutGroup.gameObject.SetActive(false);
            }
            //»наче отображаем его
            else
            {
                availableShipTypesLayoutGroup.gameObject.SetActive(true);
            }
        }
    }
}