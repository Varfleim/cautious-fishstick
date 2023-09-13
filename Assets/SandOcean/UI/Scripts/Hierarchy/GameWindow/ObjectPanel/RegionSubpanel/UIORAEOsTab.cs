
using System.Collections.Generic;

using UnityEngine.UI;

using SandOcean.Organization;
using SandOcean.UI.GameWindow.Object.Region.ORAEOs;

namespace SandOcean.UI.GameWindow.Object.Region
{
    public class UIORAEOsTab : UIAObjectSubpanelTab
    {
        public VerticalLayoutGroup scrollContent;
        public List<UIORAEOSummaryPanel> panelsList = new();

        public UIORAEOSummaryPanel oRAEOSummaryPanelPrefab;

        public void ClearORAEOSummaryPanels()
        {
            foreach (UIORAEOSummaryPanel briefInfoPanel in scrollContent.GetComponentsInChildren<UIORAEOSummaryPanel>())
            {
                Destroy(briefInfoPanel.gameObject);
            }

            panelsList.Clear();
        }

        public UIORAEOSummaryPanel InstantiateORAEOSummaryPanel(
            ref COrganization organization)
        {
            UIORAEOSummaryPanel briefInfoPanel = Instantiate(oRAEOSummaryPanelPrefab);

            briefInfoPanel.organizationName.text = organization.selfName;

            briefInfoPanel.transform.SetParent(scrollContent.transform);

            panelsList.Add(briefInfoPanel);

            return briefInfoPanel;
        }
    }
}