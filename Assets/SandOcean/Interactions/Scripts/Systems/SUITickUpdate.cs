
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;

namespace SandOcean.UI
{
    public class SUITickUpdate : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //События игры
        readonly EcsPoolInject<EGameDisplayObjectPanel> gameDisplayObjectPanelEventPool = default;

        //Данные
        readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //Если активно окно игры
            if (eUI.Value.activeMainWindowType == MainWindowType.Game)
            {
                //Берём окно игры
                UIGameWindow gameWindow = eUI.Value.gameWindow;

                //Если активна панель объекта
                if (gameWindow.activeMainPanelType == MainPanelType.Object)
                {
                    //Берём панель объекта
                    UIObjectPanel objectPanel = gameWindow.objectPanel;

                    //Если активна подпанель организации
                    if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
                    {
                        //Берём подпанель организации
                        UIOrganizationObjectSubpanel organizationSubpanel = objectPanel.organizationObjectSubpanel;

                        //Если активна обзорная вкладка
                        if (organizationSubpanel.tabGroup.selectedTab == organizationSubpanel.overviewTab.selfTabButton)
                        {
                            //Запрашиваем обновление обзорной вкладки
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.OrganizationOverview,
                                objectPanel.activeObjectPE);
                        }
                    }
                    //Иначе, если активна подпанель острова
                    else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                    {
                        //Берём подпанель острова
                        UIRegionObjectSubpanel islandSubpanel = objectPanel.regionObjectSubpanel;

                        //Если активна обзорная вкладка
                        if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.overviewTab.selfTabButton)
                        {
                            //Запрашиваем обновление обзорной вкладки
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.RegionOverview,
                                objectPanel.activeObjectPE);
                        }
                        //Иначе, если активна вкладка организаций
                        else if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.organizationsTab.selfTabButton)
                        {
                            //Запрашиваем обновление вкладки организаций
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.RegionOrganizations,
                                objectPanel.activeObjectPE);
                        }
                    }
                    //Иначе, если активна подпанель ORAEO
                    else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
                    {
                        //Берём подпанель ORAEO
                        UIORAEOObjectSubpanel oRAEOSubpanel = objectPanel.oRAEOObjectSubpanel;

                        //Если активна обзорная вкладка
                        if (oRAEOSubpanel.tabGroup.selectedTab == oRAEOSubpanel.overviewTab.selfTabButton)
                        {
                            //Запрашиваем обновление обзорной вкладки
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.ORAEOOverview,
                                objectPanel.activeObjectPE);
                        }
                    }
                }
            }
        }

        void GameDisplayObjectPanelEvent(
            DisplayObjectPanelEventType eventType,
            EcsPackedEntity objectPE,
            bool isRefresh = true)
        {
            //Создаём новую сущность и назначаем ей компонент события отображения панели объекта
            int eventEntity = world.Value.NewEntity();
            ref EGameDisplayObjectPanel gameDisplayObjectPanelEvent = ref gameDisplayObjectPanelEventPool.Value.Add(eventEntity);

            //Заполняем данные события
            gameDisplayObjectPanelEvent = new(
                eventType,
                objectPE,
                isRefresh);
        }
    }
}