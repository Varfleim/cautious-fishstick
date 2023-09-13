
using UnityEngine;
using UnityEngine.UI;

using TMPro;

using Leopotam.EcsLite;

namespace SandOcean.UI.GameWindow.Object.FleetManager.Fleets
{
    public class UITaskForceSummaryPanel : MonoBehaviour
    {
        public UIFleetSummaryPanel parentFleetSummaryPanel;

        public TextMeshProUGUI selfName;
        public EcsPackedEntity selfPE;

        public Toggle toggle;

        public Button templateSummaryButton;
        public TextMeshProUGUI templateSummaryPanel;
        public int templateIndex;

        public VerticalLayoutGroup templateLayoutGroup;

        public void ShowTFTemplatesChangingList()
        {
            //≈сли измен€ема€ группа - не текуща€, запрашиваем отображение списка дл€ смены шаблона дл€ текущей группы
            if (parentFleetSummaryPanel.parentFleetsTab.templateChangingTaskForcePanel != this)
            {
                parentFleetSummaryPanel.parentFleetsTab.ShowTFTemplatesChangingList(this);
            }
            //»наче скрываем список дл€ смены шаблона
            else
            {
                parentFleetSummaryPanel.parentFleetsTab.HideTFTemplatesChangingList();
            }
        }
    }
}