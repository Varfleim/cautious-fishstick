
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
            //Очищаем боевые группы
            ClearBattleGroup(shortRangeGroup);
        }

        void ClearBattleGroup(
            UIBattleGroupPanel battleGroupPanel)
        {
            //Для каждого типа корабля в списке имеющихся
            for (int a = 0; a < battleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //Удаляем панель
                Destroy(battleGroupPanel.currentShipTypePanels[a]);
            }

            //Для каждого типа корабля в списке доступных
            for (int a = 0; a < battleGroupPanel.availableShipTypePanels.Count; a++)
            {
                //Удаляем панель
                Destroy(battleGroupPanel.currentShipTypePanels[a]);
            }
        }


        public void InstantiateShipTypePanels(
            DShipType shipType)
        {
            //Если тип корабля относится к группе малой дальности
            if (shipType.BattleGroup == TaskForceBattleGroup.ShortRange)
            {
                //Создаём панели типа в группе малой дальности
                InstantiateShipTypePanels(
                    shipType,
                    shortRangeGroup);
            }
            //Иначе, если тип корабля относится к группе средней дальности
            else if (shipType.BattleGroup == TaskForceBattleGroup.MediumRange)
            {
                //Создаём панели типа в группе средней дальности
                InstantiateShipTypePanels(
                    shipType,
                    mediumRangeGroup);
            }
            //Иначе, если тип корабля относится к группе большой дальности
            else if (shipType.BattleGroup == TaskForceBattleGroup.LongRange)
            {
                //Создаём панели типа в группе большой дальности
                InstantiateShipTypePanels(
                    shipType,
                    longRangeGroup);
            }
        }

        void InstantiateShipTypePanels(
            DShipType shipType,
            UIBattleGroupPanel battleGroupPanel)
        {
            //Создаём панель для типа корабля в списке имеющихся
            UICurrentShipTypePanel currentShipTypePanel = Instantiate(currentShipTypePanelPrefab);

            //Заполняем данные панели
            currentShipTypePanel.shipType = shipType;

            currentShipTypePanel.shipTypeName.text = shipType.ObjectName;

            //Скрываем панель, присоединяем к панели и заносим в список
            currentShipTypePanel.gameObject.SetActive(false);
            currentShipTypePanel.transform.SetParent(battleGroupPanel.currentShipTypesLayoutGroup.transform);
            currentShipTypePanel.parentTaskForceTemplateDesignerSubtab = this;
            battleGroupPanel.currentShipTypePanels.Add(currentShipTypePanel);


            //Создаём панель для типа корабля в списке доступных
            UIAvailableShipTypePanel availableShipTypePanel = Instantiate(availableShipTypePanelPrefab);

            //Заполняем данные панели
            availableShipTypePanel.shipType = shipType;

            availableShipTypePanel.shipTypeName.text = shipType.ObjectName;

            //Скрываем панель, присоединяем к панели и заносим в список
            availableShipTypePanel.gameObject.SetActive(false);
            availableShipTypePanel.transform.SetParent(battleGroupPanel.availableShipTypesLayoutGroup.transform);
            availableShipTypePanel.parentTaskForceTemplateDesignerSubtab = this;
            battleGroupPanel.availableShipTypePanels.Add(availableShipTypePanel);


            //Даём панелям ссылки друг на друга
            currentShipTypePanel.siblingAvailableShipTypePanel = availableShipTypePanel;
            availableShipTypePanel.siblingCurrentShipTypePanel = currentShipTypePanel;
        }

        public void ClearTaskForceTemplate()
        {
            //Очищаем боевые группы
            ClearTaskForceBattleGroup(shortRangeGroup);
        }

        void ClearTaskForceBattleGroup(
            UIBattleGroupPanel battleGroupPanel)
        {
            //Для каждого типа корабля в списке имеющихся
            for (int a = 0; a < battleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //Скрываем панель 
                HideCurrentShipType(battleGroupPanel.currentShipTypePanels[a]);
            }

            //Выключаем кнопку добавления доступных типов кораблей
            battleGroupPanel.addShipTypesButton.panelToggle.isOn = false;

            //Для каждого типа корабля в списке доступных
            for (int a = 0; a < battleGroupPanel.availableShipTypePanels.Count; a++)
            {
                //Отображаем панель
                ShowAvailableShipType(battleGroupPanel.availableShipTypePanels[a]);
            }
        }

        public void AddAvailableShipType(
            DShipType shipType,
            UIBattleGroupPanel battleGroupPanel,
            int startCount = 1)
        {
            //Для каждого типа корабля в списке доступных
            for (int a = 0; a < battleGroupPanel.availableShipTypePanels.Count; a++)
            {
                //Если тип соответствует запрошенному
                if (battleGroupPanel.availableShipTypePanels[a].shipType == shipType)
                {
                    //Добавляем доступный тип корабля
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
            //Скрываем панель доступного типа корабля
            HideAvailableShipType(availableShipTypePanel);

            //Отображаем панель имеющегося типа корабля
            ShowCurrentShipType(availableShipTypePanel.siblingCurrentShipTypePanel);

            //Изменяем количество кораблей данного типа
            availableShipTypePanel.siblingCurrentShipTypePanel.CurrentShipCountValue = startCount;
        }

        public void RemoveCurrentShipType(
            DShipType shipType,
            UIBattleGroupPanel battleGroupPanel)
        {
            //Для каждого типа корабля в списке имеющихся
            for (int a = 0; a < battleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //Если тип соответствует запрошенному
                if (battleGroupPanel.currentShipTypePanels[a].shipType == shipType)
                {
                    //Удаляем имеющийся тип корабля
                    RemoveCurrentShipType(battleGroupPanel.currentShipTypePanels[a]);
                }
            }
        }

        public void RemoveCurrentShipType(
            UICurrentShipTypePanel currentShipTypePanel)
        {
            //Скрываем панель имеющегося типа корабля
            HideCurrentShipType(currentShipTypePanel);

            //Отображаем панель доступного типа корабля
            ShowAvailableShipType(currentShipTypePanel.siblingAvailableShipTypePanel);
        }


        void ShowCurrentShipType(
            UICurrentShipTypePanel currentShipTypePanel)
        {
            //Устанавливаем количество кораблей на 1
            currentShipTypePanel.CurrentShipCountValue = 1;

            //Отображаем панель
            currentShipTypePanel.gameObject.SetActive(true);
        }

        void HideCurrentShipType(
            UICurrentShipTypePanel currentShipTypePanel)
        {
            //Обнуляем количество кораблей
            currentShipTypePanel.CurrentShipCountValue = 0;

            //Скрываем панель
            currentShipTypePanel.gameObject.SetActive(false);
        }

        void ShowAvailableShipType(
            UIAvailableShipTypePanel availableShipTypePanel)
        {
            //Отображаем панель
            availableShipTypePanel.gameObject.SetActive(true);
        }

        void HideAvailableShipType(
            UIAvailableShipTypePanel availableShipTypePanel)
        {
            //Скрываем панель
            availableShipTypePanel.gameObject.SetActive(false);
        }
    }
}