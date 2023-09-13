
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
            //Заносим обзорную панель в список кэшированных
            cachedTemplatePanels.Add(templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel);

            //Удаляем обзорную панель из списка активных
            activeTemplatePanels.Remove(templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel);

            //Скрываем обзорную панель и обнуляем родительский объект
            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel.gameObject.SetActive(false);
            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel.transform.SetParent(null);

            //Удаляем ссылку на обзорную панель
            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel = null;
        }

        public void InstantiateTFTemplateSummaryPanel(
            DTFTemplate template, ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel)
        {
            //Создаём пустую переменную для панели
            UITFTemplateSummaryPanel templateSummaryPanel;

            //Если список кэшированных панелей не пуст, то берём кэшированную
            if (cachedTemplatePanels.Count > 0)
            {
                //Берём последнюю панель в списке и удаляем её из списка
                templateSummaryPanel = cachedTemplatePanels[cachedTemplatePanels.Count - 1];
                cachedTemplatePanels.RemoveAt(cachedTemplatePanels.Count - 1);
            }
            //Иначе
            else
            {
                //Создаём новую панель
                templateSummaryPanel = Instantiate(tFTemplateSummaryPanelPrefab);
            }

            //Заполняем данные панели
            templateSummaryPanel.selfName.text = template.selfName;
            templateSummaryPanel.template = template;

            //Если существуют группы, созданные с данным шаблоном
            if (template.taskForces.Count > 0)
            {
                //Делаем кнопку удаления группы активной
                templateSummaryPanel.deleteButton.interactable = false;
            }
            //Иначе
            else
            {
                //Делаем кнопку удаления группы неактивной
                templateSummaryPanel.deleteButton.interactable = true;
            }

            //Отображаем панель и присоединяем ко вкладке
            templateSummaryPanel.gameObject.SetActive(true);
            templateSummaryPanel.transform.SetParent(layoutGroup.transform);

            templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel = templateSummaryPanel;

            activeTemplatePanels.Add(templateSummaryPanel);
        }
    }
}