
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
            //Заносим обзорную панель в список кэшированных
            cachedFleetPanels.Add(fleetDisplayedSummaryPanel.fleetSummaryPanel);

            //Удаляем обзорную панель из списка активных
            activeFleetPanels.Remove(fleetDisplayedSummaryPanel.fleetSummaryPanel);

            //Скрываем обзорную панель, выключаем переключатель и обнуляем родительский объект
            fleetDisplayedSummaryPanel.fleetSummaryPanel.gameObject.SetActive(false);
            fleetDisplayedSummaryPanel.fleetSummaryPanel.toggle.isOn = false;
            fleetDisplayedSummaryPanel.fleetSummaryPanel.transform.SetParent(null);
            fleetDisplayedSummaryPanel.fleetSummaryPanel.parentFleetsTab = null;

            //Удаляем ссылку на обзорную панель
            fleetDisplayedSummaryPanel.fleetSummaryPanel = null;
        }

        public void CacheTaskForceSummaryPanel(
            ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel)
        {
            //Берём обзорную панель флота-владельца
            UIFleetSummaryPanel fleetSummaryPanel = taskForceDisplayedSummaryPanel.taskForceSummaryPanel.parentFleetSummaryPanel;

            //Заносим обзорную панель в список кэшированнных
            cachedTaskForcePanels.Add(taskForceDisplayedSummaryPanel.taskForceSummaryPanel);

            //Удаляем обзорную панель из списка активных
            fleetSummaryPanel.taskForceSummaryPanels.Remove(taskForceDisplayedSummaryPanel.taskForceSummaryPanel);

            //Скрываем обзорную панель, выключаем переключатель и обнуляем родительский объект
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel.gameObject.SetActive(false);
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel.toggle.isOn = false;
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel.transform.SetParent(null);

            //Удаляем ссылку на обзорную панель
            taskForceDisplayedSummaryPanel.taskForceSummaryPanel = null;
        }

        public UIFleetSummaryPanel InstantiateFleetSummaryPanel(
            ref CFleet fleet, ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel)
        {
            //Создаём пустую переменную для панели
            UIFleetSummaryPanel fleetSummaryPanel;

            //Если список кэшированных панелей не пуст, то берём кэшированную
            if (cachedFleetPanels.Count > 0)
            {
                //Берём последнюю панель в списке и удаляем её из списка
                fleetSummaryPanel = cachedFleetPanels[cachedFleetPanels.Count - 1];
                cachedFleetPanels.RemoveAt(cachedFleetPanels.Count - 1);
            }
            //Иначе
            else
            {
                //Создаём новую панель
                fleetSummaryPanel = Instantiate(fleetSummaryPanelPrefab);
            }

            //Заполняем данные панели
            //fleetOverviewPanel.fleetName.text = fleet.ownedTaskForcePEs.Count.ToString();
            fleetSummaryPanel.selfPE = fleet.selfPE;

            //Отображаем панель и присоединяем ко вкладке
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
            //Создаём пустую переменную для панели
            UITaskForceSummaryPanel taskForceSummaryPanel;

            //Если список кэшированных панелей не пуст, то берём кэшированную
            if (cachedTaskForcePanels.Count > 0)
            {
                //Берём последнюю панель в списке и удаляем её из списка
                taskForceSummaryPanel = cachedTaskForcePanels[cachedTaskForcePanels.Count - 1];
                cachedTaskForcePanels.RemoveAt(cachedTaskForcePanels.Count - 1);
            }
            //Иначе
            else
            {
                //Создаём новую панель
                taskForceSummaryPanel = Instantiate(taskForceSummaryPanelPrefab);
            }

            //Заполняем данные панели
            //taskForceOverviewPanel.taskForceName.text = taskForce.rand.ToString();
            taskForceSummaryPanel.selfPE = taskForce.selfPE;

            //Если шаблон группы не пуст, отображаем его
            if (taskForce.template != null)
            {
                taskForceSummaryPanel.templateSummaryPanel.text = taskForce.template.selfName;
            }
            //Иначе отображаем пустой шаблон
            else
            {
                taskForceSummaryPanel.templateSummaryPanel.text = "";
            }

            //Отображаем панель и присоединяем ко вкладке
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
            //Заполняем данные панели

            //Если шаблон группы не пуст, отображаем его
            if (taskForce.template != null)
            {
                taskForceSummaryPanel.templateSummaryPanel.text = taskForce.template.selfName;
            }
            //Иначе отображаем пустой шаблон
            else
            {
                taskForceSummaryPanel.templateSummaryPanel.text = "";
            }

            //Скрываем список шаблонов групп
            HideTFTemplatesChangingList();
        }

        public void ShowTFTemplatesCreatingList()
        {
            //Если список для создания группы не активен
            if (templatesCreatingList.isActiveAndEnabled == false)
            {
                //Скрываем список для смены шаблона
                HideTFTemplatesChangingList();

                //Отображаем его
                templatesCreatingList.gameObject.SetActive(true);
            }
            //Иначе
            else
            {
                //Скрываем его
                templatesCreatingList.gameObject.SetActive(false);
            }
        }

        public void HideTFTemplatesCreatingList()
        {
            //Скрываем список для создания группы
            templatesCreatingList.gameObject.SetActive(false);
        }

        public void ShowTFTemplatesChangingList(
            UITaskForceSummaryPanel taskForceSummaryPanel)
        {
            //Скрываем список для создания группы
            HideTFTemplatesCreatingList();

            //Скрываем список для смены шаблона
            HideTFTemplatesChangingList();

            //Отображаем список и присоединяем к обзорной панели группы
            templatesChangingList.gameObject.SetActive(true);
            templatesChangingList.transform.SetParent(taskForceSummaryPanel.templateLayoutGroup.transform);

            //Указываем группу как группу со сменяемым шаблоном
            templateChangingTaskForcePanel = taskForceSummaryPanel;
        }

        public void HideTFTemplatesChangingList()
        {
            //Если была группа со сменяемым шаблоном
            if (templateChangingTaskForcePanel != null)
            {
                //Удаляем ссылку на группу со сменяемым шаблоном
                templateChangingTaskForcePanel = null;
            }

            //Скрываем список для смены шаблона, обнуляем родительский объект
            templatesChangingList.gameObject.SetActive(false);
            templatesChangingList.transform.SetParent(null);
        }

        public void CacheTFTemplateSummaryPanel(
            ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel)
        {
            //Заносим обзорную панель в список кэшированных
            cachedTemplatePanels.Add(templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel);

            //Удаляем обзорную панель из списка активных
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel.parentList.templatePanels.Remove(templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel);

            //Скрываем обзорную панель и обнуляем родительский объект
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel.gameObject.SetActive(false);
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel.transform.SetParent(null);

            //Удаляем ссылку на обзорную панель
            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel = null;
        }

        public void InstantiateTFTemplateSummaryPanel(
            DTFTemplate template, ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel,
            UITFTemplateSummaryPanelsList templatesList)
        {
            //Создаём пустую переменную для панели
            UITFTemplateSummaryPanel templateSummaryPanel;

            //Если список кэшированных панелей не пуст, то берём кэшированную
            if (cachedTemplatePanels.Count < 0)
            {
                //Берём последнюю панель в списке и удаляем её из списка
                templateSummaryPanel = cachedTemplatePanels[cachedTemplatePanels.Count - 1];
                cachedTemplatePanels.RemoveAt(cachedTemplatePanels.Count - 1);
            }
            //Иначе
            else
            {
                //Создаём новую панель
                templateSummaryPanel = Instantiate(templateSummaryPanelPrefab);
            }

            //Заполняем данные панели
            templateSummaryPanel.selfName.text = template.selfName;
            templateSummaryPanel.template = template;

            //Отображаем панель и присоединяем к списку
            templateSummaryPanel.gameObject.SetActive(true);
            templateSummaryPanel.transform.SetParent(templatesList.templateLayoutGroup.transform);
            templateSummaryPanel.parentList = templatesList;
            templatesList.templatePanels.Add(templateSummaryPanel);

            templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel = templateSummaryPanel;
        }
    }
}