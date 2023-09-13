
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using SandOcean.Warfare.Fleet;
using SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.List;
using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates
{
    public class UIListSubtab : MonoBehaviour
    {
        public VerticalLayoutGroup layoutGroup;

        List<UITFTemplateSummaryPanel> activeTemplatePanels = new();
        List<UITFTemplateSummaryPanel> cachedTemplatePanels = new();

        public UITFTemplateSummaryPanel tFTemplateSummaryPanelPrefab;

        public void CacheTFTemplateSummaryPanel(
            ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel)
        {
            //������� �������� ������ � ������ ������������
            cachedTemplatePanels.Add(templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel);

            //������� �������� ������ �� ������ ��������
            activeTemplatePanels.Remove(templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel);

            //�������� �������� ������ � �������� ������������ ������
            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel.gameObject.SetActive(false);
            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel.transform.SetParent(null);

            //������� ������ �� �������� ������
            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel = null;
        }

        public void InstantiateTFTemplateSummaryPanel(
            DTFTemplate template, ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel)
        {
            //������ ������ ���������� ��� ������
            UITFTemplateSummaryPanel templateSummaryPanel;

            //���� ������ ������������ ������� �� ����, �� ���� ������������
            if (cachedTemplatePanels.Count > 0)
            {
                //���� ��������� ������ � ������ � ������� � �� ������
                templateSummaryPanel = cachedTemplatePanels[cachedTemplatePanels.Count - 1];
                cachedTemplatePanels.RemoveAt(cachedTemplatePanels.Count - 1);
            }
            //�����
            else
            {
                //������ ����� ������
                templateSummaryPanel = Instantiate(tFTemplateSummaryPanelPrefab);
            }

            //��������� ������ ������
            templateSummaryPanel.selfName.text = template.selfName;
            templateSummaryPanel.template = template;

            //���� ���������� ������, ��������� � ������ ��������
            if (template.taskForces.Count > 0)
            {
                //������ ������ �������� ������ ��������
                templateSummaryPanel.deleteButton.interactable = false;
            }
            //�����
            else
            {
                //������ ������ �������� ������ ����������
                templateSummaryPanel.deleteButton.interactable = true;
            }

            //���������� ������ � ������������ �� �������
            templateSummaryPanel.gameObject.SetActive(true);
            templateSummaryPanel.transform.SetParent(layoutGroup.transform);

            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel = templateSummaryPanel;

            activeTemplatePanels.Add(templateSummaryPanel);
        }
    }
}