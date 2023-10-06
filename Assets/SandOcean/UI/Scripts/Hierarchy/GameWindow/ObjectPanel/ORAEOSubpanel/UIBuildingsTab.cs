
using System.Collections.Generic;

using UnityEngine.UI;

using SandOcean.UI.GameWindow.Object.ORAEO.Buildings;
using SandOcean.Economy.Building;

namespace SandOcean.UI.GameWindow.Object.ORAEO
{
    public class UIBuildingsTab : UIAObjectSubpanelTab
    {
        public UIBuildingCategoryPanel testCategory;

        List<UIBuildingSummaryPanel> cachedBuildingPanels = new();

        public UIBuildingSummaryPanel buildingSummaryPanelPrefab;

        public void CacheBuildingSummaryPanel(
            ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel)
        {
            //���� ������������ ��������� ����������
            UIBuildingCategoryPanel buildingCategory = buildingDisplayedSummaryPanel.buildingSummaryPanel.parentBuildingCategoryPanel;

            //������� �������� ������ � ������ ������������
            cachedBuildingPanels.Add(buildingDisplayedSummaryPanel.buildingSummaryPanel);

            //������� �������� ������ �� ������ ��������
            buildingCategory.buildingSummaryPanels.Remove(buildingDisplayedSummaryPanel.buildingSummaryPanel);

            //�������� �������� ������ � �������� ������������ ������
            buildingDisplayedSummaryPanel.buildingSummaryPanel.gameObject.SetActive(false);
            buildingDisplayedSummaryPanel.buildingSummaryPanel.transform.SetParent(null);

            //������� ������ �� �������� ������
            buildingDisplayedSummaryPanel.buildingSummaryPanel = null;
        }

        public void InstantiateBuildingSummaryPanel(
            ref CBuilding building, ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel)
        {
            //������ ������ ���������� ��� ������
            UIBuildingSummaryPanel buildingSummaryPanel;

            //���� ������ ������������ ������� �� ����, �� ���� �������������
            if (cachedBuildingPanels.Count > 0)
            {
                //���� ��������� ������ � ������ � ������� � �� ������
                buildingSummaryPanel = cachedBuildingPanels[cachedBuildingPanels.Count - 1];
                cachedBuildingPanels.RemoveAt(cachedBuildingPanels.Count - 1);
            }
            //�����
            else
            {
                //������ ����� ������
                buildingSummaryPanel = Instantiate(buildingSummaryPanelPrefab);
            }

            //��������� ������ ������
            buildingSummaryPanel.selfName.text = building.buildingType.ObjectName;
            buildingSummaryPanel.selfPE = building.selfPE;

            //���������� ������ � ������������ � �� �������
            buildingSummaryPanel.gameObject.SetActive(true);
            //����
            //���� ���������� ��������� � �������� ���������
            if (building.buildingType.BuildingCategory == BuildingCategory.Test)
            {
                buildingSummaryPanel.transform.SetParent(testCategory.layoutGroup.transform);
                buildingSummaryPanel.parentBuildingCategoryPanel = testCategory;
            }
            //����

            buildingDisplayedSummaryPanel.buildingSummaryPanel = buildingSummaryPanel;

            testCategory.buildingSummaryPanels.Add(buildingSummaryPanel);
        }

        public void RefreshBuildingSummaryPanel(
            ref CBuilding building, UIBuildingSummaryPanel buildingSummaryPanel)
        {
            //��������� ������ ������


        }
    }
}