
using UnityEngine;

using TMPro;

using SandOcean.Designer.Workshop;
using SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;

namespace SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates
{
    public class UIDesignerSubtab : MonoBehaviour
    {
        public TMP_InputField tFTemplateName;
        public DTFTemplate template;

        public UIBattleGroupPanel longRangeGroup;
        public UIBattleGroupPanel mediumRangeGroup;
        public UIBattleGroupPanel shortRangeGroup;

        public UICurrentShipTypePanel currentShipTypePanelPrefab;
        public UIAvailableShipTypePanel availableShipTypePanelPrefab;

        public bool isBattleGroupsDisplayed = false;

        public void ClearBattleGroups()
        {
            //������� ������ ������
            ClearBattleGroup(shortRangeGroup);
        }

        void ClearBattleGroup(
            UIBattleGroupPanel battleGroupPanel)
        {
            //��� ������� ���� ������� � ������ ���������
            for (int a = 0; a < battleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //������� ������
                Destroy(battleGroupPanel.currentShipTypePanels[a]);
            }

            //��� ������� ���� ������� � ������ ���������
            for (int a = 0; a < battleGroupPanel.availableShipTypePanels.Count; a++)
            {
                //������� ������
                Destroy(battleGroupPanel.currentShipTypePanels[a]);
            }
        }


        public void InstantiateShipTypePanels(
            DShipType shipType)
        {
            //���� ��� ������� ��������� � ������ ����� ���������
            if (shipType.BattleGroup == TaskForceBattleGroup.ShortRange)
            {
                //������ ������ ���� � ������ ����� ���������
                InstantiateShipTypePanels(
                    shipType,
                    shortRangeGroup);
            }
            //�����, ���� ��� ������� ��������� � ������ ������� ���������
            else if (shipType.BattleGroup == TaskForceBattleGroup.MediumRange)
            {
                //������ ������ ���� � ������ ������� ���������
                InstantiateShipTypePanels(
                    shipType,
                    mediumRangeGroup);
            }
            //�����, ���� ��� ������� ��������� � ������ ������� ���������
            else if (shipType.BattleGroup == TaskForceBattleGroup.LongRange)
            {
                //������ ������ ���� � ������ ������� ���������
                InstantiateShipTypePanels(
                    shipType,
                    longRangeGroup);
            }
        }

        void InstantiateShipTypePanels(
            DShipType shipType,
            UIBattleGroupPanel battleGroupPanel)
        {
            //������ ������ ��� ���� ������� � ������ ���������
            UICurrentShipTypePanel currentShipTypePanel = Instantiate(currentShipTypePanelPrefab);

            //��������� ������ ������
            currentShipTypePanel.shipType = shipType;

            currentShipTypePanel.shipTypeName.text = shipType.ObjectName;

            //�������� ������, ������������ � ������ � ������� � ������
            currentShipTypePanel.gameObject.SetActive(false);
            currentShipTypePanel.transform.SetParent(battleGroupPanel.currentShipTypesLayoutGroup.transform);
            currentShipTypePanel.parentTaskForceTemplateDesignerSubtab = this;
            battleGroupPanel.currentShipTypePanels.Add(currentShipTypePanel);


            //������ ������ ��� ���� ������� � ������ ���������
            UIAvailableShipTypePanel availableShipTypePanel = Instantiate(availableShipTypePanelPrefab);

            //��������� ������ ������
            availableShipTypePanel.shipType = shipType;

            availableShipTypePanel.shipTypeName.text = shipType.ObjectName;

            //�������� ������, ������������ � ������ � ������� � ������
            availableShipTypePanel.gameObject.SetActive(false);
            availableShipTypePanel.transform.SetParent(battleGroupPanel.availableShipTypesLayoutGroup.transform);
            availableShipTypePanel.parentTaskForceTemplateDesignerSubtab = this;
            battleGroupPanel.availableShipTypePanels.Add(availableShipTypePanel);


            //��� ������� ������ ���� �� �����
            currentShipTypePanel.siblingAvailableShipTypePanel = availableShipTypePanel;
            availableShipTypePanel.siblingCurrentShipTypePanel = currentShipTypePanel;
        }

        public void ClearTaskForceTemplate()
        {
            //������� ������ ������
            ClearTaskForceBattleGroup(shortRangeGroup);
        }

        void ClearTaskForceBattleGroup(
            UIBattleGroupPanel battleGroupPanel)
        {
            //��� ������� ���� ������� � ������ ���������
            for (int a = 0; a < battleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //�������� ������ 
                HideCurrentShipType(battleGroupPanel.currentShipTypePanels[a]);
            }

            //��������� ������ ���������� ��������� ����� ��������
            battleGroupPanel.addShipTypesButton.panelToggle.isOn = false;

            //��� ������� ���� ������� � ������ ���������
            for (int a = 0; a < battleGroupPanel.availableShipTypePanels.Count; a++)
            {
                //���������� ������
                ShowAvailableShipType(battleGroupPanel.availableShipTypePanels[a]);
            }
        }

        public void AddAvailableShipType(
            DShipType shipType,
            UIBattleGroupPanel battleGroupPanel,
            int startCount = 1)
        {
            //��� ������� ���� ������� � ������ ���������
            for (int a = 0; a < battleGroupPanel.availableShipTypePanels.Count; a++)
            {
                //���� ��� ������������� ������������
                if (battleGroupPanel.availableShipTypePanels[a].shipType == shipType)
                {
                    //��������� ��������� ��� �������
                    AddAvailableShipType(
                        battleGroupPanel.availableShipTypePanels[a],
                        startCount);
                }
            }
        }

        public void AddAvailableShipType(
            UIAvailableShipTypePanel availableShipTypePanel,
            int startCount = 1)
        {
            //�������� ������ ���������� ���� �������
            HideAvailableShipType(availableShipTypePanel);

            //���������� ������ ���������� ���� �������
            ShowCurrentShipType(availableShipTypePanel.siblingCurrentShipTypePanel);

            //�������� ���������� �������� ������� ����
            availableShipTypePanel.siblingCurrentShipTypePanel.CurrentShipCountValue = startCount;
        }

        public void RemoveCurrentShipType(
            DShipType shipType,
            UIBattleGroupPanel battleGroupPanel)
        {
            //��� ������� ���� ������� � ������ ���������
            for (int a = 0; a < battleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //���� ��� ������������� ������������
                if (battleGroupPanel.currentShipTypePanels[a].shipType == shipType)
                {
                    //������� ��������� ��� �������
                    RemoveCurrentShipType(battleGroupPanel.currentShipTypePanels[a]);
                }
            }
        }

        public void RemoveCurrentShipType(
            UICurrentShipTypePanel currentShipTypePanel)
        {
            //�������� ������ ���������� ���� �������
            HideCurrentShipType(currentShipTypePanel);

            //���������� ������ ���������� ���� �������
            ShowAvailableShipType(currentShipTypePanel.siblingAvailableShipTypePanel);
        }


        void ShowCurrentShipType(
            UICurrentShipTypePanel currentShipTypePanel)
        {
            //������������� ���������� �������� �� 1
            currentShipTypePanel.CurrentShipCountValue = 1;

            //���������� ������
            currentShipTypePanel.gameObject.SetActive(true);
        }

        void HideCurrentShipType(
            UICurrentShipTypePanel currentShipTypePanel)
        {
            //�������� ���������� ��������
            currentShipTypePanel.CurrentShipCountValue = 0;

            //�������� ������
            currentShipTypePanel.gameObject.SetActive(false);
        }

        void ShowAvailableShipType(
            UIAvailableShipTypePanel availableShipTypePanel)
        {
            //���������� ������
            availableShipTypePanel.gameObject.SetActive(true);
        }

        void HideAvailableShipType(
            UIAvailableShipTypePanel availableShipTypePanel)
        {
            //�������� ������
            availableShipTypePanel.gameObject.SetActive(false);
        }
    }
}