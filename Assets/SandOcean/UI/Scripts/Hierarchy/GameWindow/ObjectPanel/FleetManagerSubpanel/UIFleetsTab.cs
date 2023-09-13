
using System.Collections.Generic;

using UnityEngine.UI;

using SandOcean.Warfare.Fleet;
using SandOcean.UI.GameWindow.Object.FleetManager.Fleets;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.UI.GameWindow.Object.FleetManager
{
    public class UIFleetsTab : UIAObjectSubpanelTab
    {
        public VerticalLayoutGroup layoutGroup;
        public ToggleGroup toggleGroup;

        List<UIFleetSummaryPanel> activeFleetPanels = new();
        List<UIFleetSummaryPanel> cachedFleetPanels = new();

        List<UITaskForceSummaryPanel> cachedTaskForcePanels = new();

        public UITFTemplateSummaryPanelsList templatesCreatingList;
        public UITFTemplateSummaryPanelsList templatesChangingList;
        public UITaskForceSummaryPanel templateChangingTaskForcePanel;

        List<UITFTemplateSummaryPanel> cachedTemplatePanels = new();

        public UIFleetSummaryPanel fleetSummaryPanelPrefab;
        public UITaskForceSummaryPanel taskForceSummaryPanelPrefab;
        public UITFTemplateSummaryPanel templateSummaryPanelPrefab;

        public void CacheFleetSummaryPanel(
            ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel)
        {
            //������� �������� ������ � ������ ������������
            cachedFleetPanels.Add(fleetDisplayedSummaryPanel.fleetSummaryPanel);

            //������� �������� ������ �� ������ ��������
            activeFleetPanels.Remove(fleetDisplayedSummaryPanel.fleetSummaryPanel);

            //�������� �������� ������, ��������� ������������� � �������� ������������ ������
            fleetDisplayedSummaryPanel.fleetSummaryPanel.gameObject.SetActive(false);
            fleetDisplayedSummaryPanel.fleetSummaryPanel.toggle.isOn = false;
            fleetDisplayedSummaryPanel.fleetSummaryPanel.transform.SetParent(null);
            fleetDisplayedSummaryPanel.fleetSummaryPanel.parentFleetsTab = null;

            //������� ������ �� �������� ������
            fleetDisplayedSummaryPanel.fleetSummaryPanel = null;
        }

        public void CacheTaskForceSummaryPanel(
            ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel)
        {
            //���� �������� ������ �����-���������
            UIFleetSummaryPanel fleetSummaryPanel = taskForceDisplayedSummaryPanel.taskForceSummaryPanel.parentFleetSummaryPanel;

            //������� �������� ������ � ������ �������������
            cachedTaskForcePanels.Add(taskForceDisplayedSummaryPanel.taskForceSummaryPanel);

            //������� �������� ������ �� ������ ��������
            fleetSummaryPanel.taskForceSummaryPanels.Remove(taskForceDisplayedSummaryPanel.taskForceSummaryPanel);

            //�������� �������� ������, ��������� ������������� � �������� ������������ ������
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel.gameObject.SetActive(false);
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel.toggle.isOn = false;
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel.transform.SetParent(null);

            //������� ������ �� �������� ������
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel = null;
        }

        public UIFleetSummaryPanel InstantiateFleetSummaryPanel(
            ref CFleet fleet, ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel)
        {
            //������ ������ ���������� ��� ������
            UIFleetSummaryPanel fleetSummaryPanel;

            //���� ������ ������������ ������� �� ����, �� ���� ������������
            if (cachedFleetPanels.Count > 0)
            {
                //���� ��������� ������ � ������ � ������� � �� ������
                fleetSummaryPanel = cachedFleetPanels[cachedFleetPanels.Count - 1];
                cachedFleetPanels.RemoveAt(cachedFleetPanels.Count - 1);
            }
            //�����
            else
            {
                //������ ����� ������
                fleetSummaryPanel = Instantiate(fleetSummaryPanelPrefab);
            }

            //��������� ������ ������
            //fleetOverviewPanel.fleetName.text = fleet.ownedTaskForcePEs.Count.ToString();
            fleetSummaryPanel.selfPE = fleet.selfPE;

            //���������� ������ � ������������ �� �������
            fleetSummaryPanel.gameObject.SetActive(true);
            fleetSummaryPanel.transform.SetParent(layoutGroup.transform);
            fleetSummaryPanel.toggle.group = toggleGroup;
            fleetSummaryPanel.parentFleetsTab = this;

            fleetDisplayedSummaryPanel.fleetSummaryPanel = fleetSummaryPanel;

            activeFleetPanels.Add(fleetSummaryPanel);

            return fleetSummaryPanel;
        }

        public UITaskForceSummaryPanel InstantiateTaskForceSummaryPanel(
            UIFleetSummaryPanel fleetSummaryPanel,
            ref CTaskForce taskForce, ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel)
        {
            //������ ������ ���������� ��� ������
            UITaskForceSummaryPanel taskForceSummaryPanel;

            //���� ������ ������������ ������� �� ����, �� ���� ������������
            if (cachedTaskForcePanels.Count > 0)
            {
                //���� ��������� ������ � ������ � ������� � �� ������
                taskForceSummaryPanel = cachedTaskForcePanels[cachedTaskForcePanels.Count - 1];
                cachedTaskForcePanels.RemoveAt(cachedTaskForcePanels.Count - 1);
            }
            //�����
            else
            {
                //������ ����� ������
                taskForceSummaryPanel = Instantiate(taskForceSummaryPanelPrefab);
            }

            //��������� ������ ������
            //taskForceOverviewPanel.taskForceName.text = taskForce.rand.ToString();
            taskForceSummaryPanel.selfPE = taskForce.selfPE;

            //���� ������ ������ �� ����, ���������� ���
            if (taskForce.template != null)
            {
                taskForceSummaryPanel.templateSummaryPanel.text = taskForce.template.selfName;
            }
            //����� ���������� ������ ������
            else
            {
                taskForceSummaryPanel.templateSummaryPanel.text = "";
            }

            //���������� ������ � ������������ �� �������
            taskForceSummaryPanel.gameObject.SetActive(true);
            taskForceSummaryPanel.parentFleetSummaryPanel = fleetSummaryPanel;
            taskForceSummaryPanel.transform.SetParent(fleetSummaryPanel.taskForcesLayoutGroup.transform);

            taskForceDisplayedSummaryPanel.taskForceSummaryPanel = taskForceSummaryPanel;

            fleetSummaryPanel.taskForceSummaryPanels.Add(taskForceSummaryPanel);

            return taskForceSummaryPanel;
        }

        public void RefreshTaskForceSummaryPanel(
            ref CTaskForce taskForce, UITaskForceSummaryPanel taskForceSummaryPanel)
        {
            //��������� ������ ������

            //���� ������ ������ �� ����, ���������� ���
            if (taskForce.template != null)
            {
                taskForceSummaryPanel.templateSummaryPanel.text = taskForce.template.selfName;
            }
            //����� ���������� ������ ������
            else
            {
                taskForceSummaryPanel.templateSummaryPanel.text = "";
            }

            //�������� ������ �������� �����
            HideTFTemplatesChangingList();
        }

        public void ShowTFTemplatesCreatingList()
        {
            //���� ������ ��� �������� ������ �� �������
            if (templatesCreatingList.isActiveAndEnabled == false)
            {
                //�������� ������ ��� ����� �������
                HideTFTemplatesChangingList();

                //���������� ���
                templatesCreatingList.gameObject.SetActive(true);
            }
            //�����
            else
            {
                //�������� ���
                templatesCreatingList.gameObject.SetActive(false);
            }
        }

        public void HideTFTemplatesCreatingList()
        {
            //�������� ������ ��� �������� ������
            templatesCreatingList.gameObject.SetActive(false);
        }

        public void ShowTFTemplatesChangingList(
            UITaskForceSummaryPanel taskForceSummaryPanel)
        {
            //�������� ������ ��� �������� ������
            HideTFTemplatesCreatingList();

            //�������� ������ ��� ����� �������
            HideTFTemplatesChangingList();

            //���������� ������ � ������������ � �������� ������ ������
            templatesChangingList.gameObject.SetActive(true);
            templatesChangingList.transform.SetParent(taskForceSummaryPanel.templateLayoutGroup.transform);

            //��������� ������ ��� ������ �� ��������� ��������
            templateChangingTaskForcePanel = taskForceSummaryPanel;
        }

        public void HideTFTemplatesChangingList()
        {
            //���� ���� ������ �� ��������� ��������
            if (templateChangingTaskForcePanel != null)
            {
                //������� ������ �� ������ �� ��������� ��������
                templateChangingTaskForcePanel = null;
            }

            //�������� ������ ��� ����� �������, �������� ������������ ������
            templatesChangingList.gameObject.SetActive(false);
            templatesChangingList.transform.SetParent(null);
        }

        public void CacheTFTemplateSummaryPanel(
            ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel)
        {
            //������� �������� ������ � ������ ������������
            cachedTemplatePanels.Add(templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel);

            //������� �������� ������ �� ������ ��������
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel.parentList.templatePanels.Remove(templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel);

            //�������� �������� ������ � �������� ������������ ������
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel.gameObject.SetActive(false);
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel.transform.SetParent(null);

            //������� ������ �� �������� ������
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel = null;
        }

        public void InstantiateTFTemplateSummaryPanel(
            DTFTemplate template, ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel,
            UITFTemplateSummaryPanelsList templatesList)
        {
            //������ ������ ���������� ��� ������
            UITFTemplateSummaryPanel templateSummaryPanel;

            //���� ������ ������������ ������� �� ����, �� ���� ������������
            if (cachedTemplatePanels.Count < 0)
            {
                //���� ��������� ������ � ������ � ������� � �� ������
                templateSummaryPanel = cachedTemplatePanels[cachedTemplatePanels.Count - 1];
                cachedTemplatePanels.RemoveAt(cachedTemplatePanels.Count - 1);
            }
            //�����
            else
            {
                //������ ����� ������
                templateSummaryPanel = Instantiate(templateSummaryPanelPrefab);
            }

            //��������� ������ ������
            templateSummaryPanel.selfName.text = template.selfName;
            templateSummaryPanel.template = template;

            //���������� ������ � ������������ � ������
            templateSummaryPanel.gameObject.SetActive(true);
            templateSummaryPanel.transform.SetParent(templatesList.templateLayoutGroup.transform);
            templateSummaryPanel.parentList = templatesList;
            templatesList.templatePanels.Add(templateSummaryPanel);

            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel = templateSummaryPanel;
        }
    }
}