
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.GameWindow;
using SandOcean.UI.GameWindow.Object;
using SandOcean.UI.GameWindow.Object.Events;
using SandOcean.UI.GameWindow.Object.FleetManager;

namespace SandOcean.UI
{
    public class SUITickUpdate : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //События игры
        readonly EcsPoolInject<RGameObjectPanelAction> gameObjectPanelActionRequestPool = default;

        //Данные
        readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //Если активно окно игры
            if (eUI.Value.activeMainWindowType == MainWindowType.Game)
            {
                //Проверяем, требуется ли обновление в окне игры
                RefreshCheckGame();
            }
        }

        #region Game
        void RefreshCheckGame()
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Если активна панель объекта
            if (gameWindow.activeMainPanelType == MainPanelType.Object)
            {
                //Проверем, требуется ли обновление в панели объекта
                RefreshCheckGameObject();
            }
        }

        #region GameObject
        void RefreshCheckGameObject()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Если активна подпанель менеджера флотов
            if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
            {
                //Проверяем, требуется ли обновление в подпанели менеджера флотов
                RefreshCheckGameObjectFleetManager();
            }
            //Если активна подпанель организации
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
            {
                //Проверяем, требуется ли обновление в подпанели организации
                RefreshCheckGameObjectOrganization();
            }
            //Иначе, если активна подпанель региона
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
            {
                //Проверяем, требуется ли обновление в подпанели региона
                RefreshCheckGameObjectRegion();
            }
            //Иначе, если активна подпанель ORAEO
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
            {
                //Проверяем, требуется ли обновление в подпанели ORAEO
                RefreshCheckGameObjectORAEO();
            }
            //Иначе, если активна подпанель сооружения
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Building)
            {
                //Проверяем, требуется ли обновление в подпанели сооружения
                RefreshCheckGameObjectBuilding();
            }
        }

        void RefreshCheckGameObjectFleetManager()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //Если активна вкладка флотов
            if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.fleetsTab.selfTabButton)
            {
                //Запрашиваем обновление вкладки флотов
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.FleetManagerFleets,
                    objectPanel.activeObjectPE);
            }
            //Иначе, если активна вкладка шаблонов оперативных групп
            else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.taskForceTemplatesTab.selfTabButton)
            {
                //Берём вкладку шаблонов групп
                UITFTemplatesTab taskForceTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

                //Если активна подвкладка списка шаблонов групп
                if (taskForceTemplatesTab.listSubtab.isActiveAndEnabled == true)
                {
                    //Запрашиваем обновление подвкладки списка шаблонов групп
                    GameObjectPanelControlRequest(
                        ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesList,
                        objectPanel.activeObjectPE);
                }
                //Иначе, если активна подвкладка дизайнера шаблонов групп
                else if (taskForceTemplatesTab.designerSubtab.isActiveAndEnabled == true)
                {
                    //Запрашиваем обновление подвкладки дизайнера шаблонов групп
                    GameObjectPanelControlRequest(
                        ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner,
                        objectPanel.activeObjectPE);
                }
            }
        }

        void RefreshCheckGameObjectOrganization()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель организации
            UIOrganizationSubpanel organizationSubpanel = objectPanel.organizationSubpanel;

            //Если активна обзорная вкладка
            if (organizationSubpanel.tabGroup.selectedTab == organizationSubpanel.overviewTab.selfTabButton)
            {
                //Запрашиваем обновление обзорной вкладки
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.OrganizationOverview,
                    objectPanel.activeObjectPE);
            }
        }

        void RefreshCheckGameObjectRegion()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель региона
            UIRegionSubpanel islandSubpanel = objectPanel.regionSubpanel;

            //Если активна обзорная вкладка
            if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.overviewTab.selfTabButton)
            {
                //Запрашиваем обновление обзорной вкладки
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.RegionOverview,
                    objectPanel.activeObjectPE);
            }
            //Иначе, если активна вкладка организаций
            else if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.oRAEOsTab.selfTabButton)
            {
                //Запрашиваем обновление вкладки организаций
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.RegionOrganizations,
                    objectPanel.activeObjectPE);
            }
        }

        void RefreshCheckGameObjectORAEO()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель ORAEO
            UIORAEOSubpanel oRAEOSubpanel = objectPanel.oRAEOSubpanel;

            //Если активна обзорная вкладка
            if (oRAEOSubpanel.tabGroup.selectedTab == oRAEOSubpanel.overviewTab.selfTabButton)
            {
                //Запрашиваем обновление обзорной вкладки
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.ORAEOOverview,
                    objectPanel.activeObjectPE);
            }
            //Иначе, если активна вкладка сооружений
            else if (oRAEOSubpanel.tabGroup.selectedTab == oRAEOSubpanel.buildingsTab.selfTabButton)
            {
                //Запрашиваем обновление вкладки сооружений
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.ORAEOBuildings,
                    objectPanel.activeObjectPE);
            }
        }

        void RefreshCheckGameObjectBuilding()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель сооружения
            UIBuildingSubpanel buildingSubpanel = objectPanel.buildingSubpanel;

            //Если активна обзорная вкладка
            if (buildingSubpanel.tabGroup.selectedTab == buildingSubpanel.overviewTab.selfTabButton)
            {
                //Запрашиваем обновление обзорной вкладки
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.BuildingOverview,
                    objectPanel.activeObjectPE);
            }
        }

        void GameObjectPanelControlRequest(
           ObjectPanelActionRequestType requestType,
           EcsPackedEntity objectPE,
           bool isRefresh = true)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия панели объекта
            int requestEntity = world.Value.NewEntity();
            ref RGameObjectPanelAction requestComp = ref gameObjectPanelActionRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                requestType,
                objectPE,
                isRefresh);
        }
        #endregion
        #endregion
    }
}