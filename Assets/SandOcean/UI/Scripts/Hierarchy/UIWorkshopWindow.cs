using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using TMPro;

using SandOcean.Designer.Workshop;
using SandOcean.UI.WorkshopWindow;

namespace SandOcean.UI
{
    public class UIWorkshopWindow : MonoBehaviour
    {
        public VerticalLayoutGroup contentSetsLayoutGroup;
        public ToggleGroup contentSetsToggleGroup;

        public List<UIWorkshopContentSetPanel> contentSetPanels = new List<UIWorkshopContentSetPanel>();

        public UIWorkshopContentSetPanel contentSetPanelPrefab;


        public int currentContentSetIndex;
        public TextMeshProUGUI currentContentSetName;

        public TextMeshProUGUI currentContentSetGameVersion;
        public TextMeshProUGUI currentContentSetVersion;


        public ToggleGroup contentObjectCountToggleGroup;

        public UIWorkshopContentObjectCountPanel shipsInfoPanel;
        public UIWorkshopContentObjectCountPanel enginesInfoPanel;
        public UIWorkshopContentObjectCountPanel reactorsInfoPanel;
        public UIWorkshopContentObjectCountPanel fuelTanksInfoPanel;
        public UIWorkshopContentObjectCountPanel solidExtractionEquipmentsInfoPanel;
        public UIWorkshopContentObjectCountPanel energyGunsInfoPanel;
        public UIWorkshopContentObjectCountPanel shipPartsCountPanel;
        public UIWorkshopContentObjectCountPanel shipPartCoreTechnologiesCountPanel;
        public UIWorkshopContentObjectCountPanel shipPartDirectionsOfImprovementCountPanel;
        public UIWorkshopContentObjectCountPanel shipPartImprovementsCountPanel;

        public void ClearContentSetPanels()
        {
            //Для каждой дочерней панели набора контента
            for (int a = 0; a < contentSetPanels.Count; a++)
            {
                Destroy(contentSetPanels[a].gameObject);
            }

            contentSetPanels.Clear();
        }

        public UIWorkshopContentSetPanel InstantiateWorkshopContentSetPanel(
            int contentSetIndex,
            in WDContentSet contentSet)
        {
            UIWorkshopContentSetPanel workshopContentSetPanel = Instantiate(contentSetPanelPrefab);

            workshopContentSetPanel.contentSetName.text = contentSet.ContentSetName;

            workshopContentSetPanel.contentSetIndex = contentSetIndex;

            workshopContentSetPanel.panelToggle.group = contentSetsToggleGroup;
            contentSetPanels.Add(workshopContentSetPanel);
            workshopContentSetPanel.transform.SetParent(contentSetsLayoutGroup.transform);

            return workshopContentSetPanel;
        }
    }
}