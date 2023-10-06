
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
            //Берём родительскую категорию сооружения
            UIBuildingCategoryPanel buildingCategory = buildingDisplayedSummaryPanel.buildingSummaryPanel.parentBuildingCategoryPanel;

            //Заносим обзорную панель в список кэшированных
            cachedBuildingPanels.Add(buildingDisplayedSummaryPanel.buildingSummaryPanel);

            //Удаляем обзорную панель из списка активных
            buildingCategory.buildingSummaryPanels.Remove(buildingDisplayedSummaryPanel.buildingSummaryPanel);

            //Скрываем обзорную панель и обнуляем родительский объект
            buildingDisplayedSummaryPanel.buildingSummaryPanel.gameObject.SetActive(false);
            buildingDisplayedSummaryPanel.buildingSummaryPanel.transform.SetParent(null);

            //Удаляем ссылку на обзорную панель
            buildingDisplayedSummaryPanel.buildingSummaryPanel = null;
        }

        public void InstantiateBuildingSummaryPanel(
            ref CBuilding building, ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel)
        {
            //Создаём пустую переменную для панели
            UIBuildingSummaryPanel buildingSummaryPanel;

            //Если список кэшированных панелей не пуст, то берём кэшированнную
            if (cachedBuildingPanels.Count > 0)
            {
                //берём последнюю панель в списке и удаляем её из списка
                buildingSummaryPanel = cachedBuildingPanels[cachedBuildingPanels.Count - 1];
                cachedBuildingPanels.RemoveAt(cachedBuildingPanels.Count - 1);
            }
            //Иначе
            else
            {
                //Создаём новую панель
                buildingSummaryPanel = Instantiate(buildingSummaryPanelPrefab);
            }

            //Заполняем данные панели
            buildingSummaryPanel.selfName.text = building.buildingType.ObjectName;
            buildingSummaryPanel.selfPE = building.selfPE;

            //Отображаем панель и присоединяем её ко вкладке
            buildingSummaryPanel.gameObject.SetActive(true);
            //ТЕСТ
            //Если сооружение относится к тестовой категории
            if (building.buildingType.BuildingCategory == BuildingCategory.Test)
            {
                buildingSummaryPanel.transform.SetParent(testCategory.layoutGroup.transform);
                buildingSummaryPanel.parentBuildingCategoryPanel = testCategory;
            }
            //ТЕСТ

            buildingDisplayedSummaryPanel.buildingSummaryPanel = buildingSummaryPanel;

            testCategory.buildingSummaryPanels.Add(buildingSummaryPanel);
        }

        public void RefreshBuildingSummaryPanel(
            ref CBuilding building, UIBuildingSummaryPanel buildingSummaryPanel)
        {
            //Заполняем данные панели


        }
    }
}