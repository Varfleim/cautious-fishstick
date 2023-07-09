
using System.Collections.Generic;

using UnityEngine.UI;

using SandOcean.Diplomacy;
using SandOcean.AEO.RAEO;

namespace SandOcean.UI
{
    public class UIRegionOSbPOrganizationsTab : UIOSbPTab
    {
        public VerticalLayoutGroup scrollContent;
        public List<UIORAEOBriefInfoPanel> panelsList = new();

        public UIORAEOBriefInfoPanel oLAEOBriefInfoPanelPrefab;

        public void ClearOLAEOPanels()
        {
            foreach (UIORAEOBriefInfoPanel briefInfoPanel in scrollContent.GetComponentsInChildren<UIORAEOBriefInfoPanel>())
            {
                Destroy(briefInfoPanel.gameObject);
            }

            panelsList.Clear();
        }

        public UIORAEOBriefInfoPanel InstantiateORAEOBriefInfoPanel(
            ref COrganization organization)
        {
            UIORAEOBriefInfoPanel briefInfoPanel = Instantiate(oLAEOBriefInfoPanelPrefab);

            briefInfoPanel.organizationName.text = organization.selfName;

            briefInfoPanel.transform.SetParent(scrollContent.transform);

            panelsList.Add(briefInfoPanel);

            return briefInfoPanel;
        }
    }
}