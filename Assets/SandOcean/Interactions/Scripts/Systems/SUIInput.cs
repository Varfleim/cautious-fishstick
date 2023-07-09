
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Unity.Ugui;

using SandOcean.UI.Events;
using SandOcean.Player;
using SandOcean.Diplomacy;
using SandOcean.Map;
using SandOcean.AEO.RAEO;
using SandOcean.Designer;
using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;
using SandOcean.Ship;
using SandOcean.Ship.Moving;
using SandOcean.Map.Events;

namespace SandOcean.UI
{
    public class SUIInput : IEcsInitSystem, IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;
        EcsWorld uguiSOWorld;
        EcsWorld uguiUIWorld;


        //Игроки
        //readonly EcsPoolInject<CPlayer> playerPool = default;

        //Объекты карты
        readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //Карта
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        //Корабли
        readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

        //readonly EcsPoolInject<CSGMoving> sGMovingPool = default;


        //События главного меню
        readonly EcsPoolInject<EMainMenuAction> mainMenuActionEventPool = default;

        //События меню новой игры
        readonly EcsPoolInject<ENewGameMenuAction> newGameMenuActionEventPool = default;

        //События меню загрузки

        //События мастерской
        readonly EcsPoolInject<EWorkshopAction> workshopActionEventPool = default;

        //События дизайнера
        readonly EcsPoolInject<EDesignerAction> designerActionEventPool = default;

        //События дизайнера кораблей
        readonly EcsPoolInject<EDesignerShipClassAction> designerShipClassActionEventPool = default;
        //События дизайнера компонентов
        readonly EcsPoolInject<EDesignerComponentAction> designerComponentActionEventPool = default;

        //События игры
        readonly EcsPoolInject<EGameAction> gameActionEventPool = default;

        readonly EcsPoolInject<EGameOpenDesigner> gameOpenDesignerEventPool = default;

        readonly EcsPoolInject<EGameDisplayObjectPanel> gameDisplayObjectPanelEventPool = default;

        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //События административно-экономических объектов
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        //События карты
        readonly EcsPoolInject<SRMapChunkRefresh> mapChunkRefreshSelfRequestPool = default;

        //Общие события
        EcsFilter clickEventSpaceFilter;
        EcsPool<EcsUguiClickEvent> clickEventSpacePool;
        
        EcsFilter clickEventUIFilter;
        EcsPool<EcsUguiClickEvent> clickEventUIPool;

        EcsFilter dropdownEventUIFilter;
        EcsPool<EcsUguiTmpDropdownChangeEvent> dropdownEventUIPool;

        EcsFilter sliderEventUIFilter;
        EcsPool<EcsUguiSliderChangeEvent> sliderEventUIPool;

        readonly EcsPoolInject<EQuitGame> quitGameEventPool = default;

        //Данные
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<ContentData> contentData = default;
        //readonly EcsCustomInject<DesignerData> designerData = default;
        readonly EcsCustomInject<SpaceGenerationData> spaceGenerationData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<RuntimeData> runtimeData = default;


        readonly EcsCustomInject<EUI> eUI = default;

        public void Init(IEcsSystems systems)
        {
            uguiSOWorld = systems.GetWorld("uguiSpaceEventsWorld");

            clickEventSpacePool = uguiSOWorld.GetPool<EcsUguiClickEvent>();
            clickEventSpaceFilter = uguiSOWorld.Filter<EcsUguiClickEvent>().End();

            uguiUIWorld = systems.GetWorld("uguiUIEventsWorld");

            clickEventUIPool = uguiUIWorld.GetPool<EcsUguiClickEvent>();
            clickEventUIFilter = uguiUIWorld.Filter<EcsUguiClickEvent>().End();

            dropdownEventUIPool = uguiUIWorld.GetPool<EcsUguiTmpDropdownChangeEvent>();
            dropdownEventUIFilter = uguiUIWorld.Filter<EcsUguiTmpDropdownChangeEvent>().End();

            sliderEventUIPool = uguiUIWorld.GetPool<EcsUguiSliderChangeEvent>();
            sliderEventUIFilter = uguiUIWorld.Filter<EcsUguiSliderChangeEvent>().End();

            spaceGenerationData.Value.terrainMaterial.EnableKeyword("GRID_ON");
        }

        public void Run(IEcsSystems systems)
        {
            //Для каждого события клика по интерфейсу
            foreach (int clickEventUIEntity in clickEventUIFilter)
            {
                //Берём компонент события
                ref EcsUguiClickEvent clickEvent = ref clickEventUIPool.Get(clickEventUIEntity);

                Debug.LogWarning(
                    "Click! "
                    + clickEvent.WidgetName);

                //Если активно окно игры
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.Game)
                {
                    //Берём ссылку на окно игры
                    UIGameWindow gameWindow
                        = eUI.Value.gameWindow;

                    //Берём компонент фракции игрока
                    inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerFactionEntity);
                    ref COrganization playerFaction
                        = ref organizationPool.Value.Get(playerFactionEntity);

                    //Если событие запрашивает открытие дизайнера кораблей
                    if (clickEvent.WidgetName
                        == "OpenShipClassDesigner")
                    {
                        //Запрашиваем открытие дизайнера кораблей
                        GameOpenDesignerEvent(
                            DesignerType.ShipClass,
                            playerFaction.contentSetIndex);
                    }
                    //Иначе, если событие запрашивает открытие дизайнера двигателей
                    else if (clickEvent.WidgetName
                        == "OpenEngineDesigner")
                    {
                        //Запрашиваем открытие дизайнера двигателей
                        GameOpenDesignerEvent(
                            DesignerType.ComponentEngine,
                            playerFaction.contentSetIndex);
                    }
                    //Иначе, если событие запрашивает открытие дизайнера реакторов
                    else if (clickEvent.WidgetName
                        == "OpenReactorDesigner")
                    {
                        //Запрашиваем открытие дизайнера реакторов
                        GameOpenDesignerEvent(
                            DesignerType.ComponentReactor,
                            playerFaction.contentSetIndex);
                    }
                    //Иначе, если событие запрашивает открытие дизайнера топливных баков
                    else if (clickEvent.WidgetName
                        == "OpenFuelTankDesigner")
                    {
                        //Запрашиваем открытие дизайнера топливных баков
                        GameOpenDesignerEvent(
                            DesignerType.ComponentHoldFuelTank,
                            playerFaction.contentSetIndex);
                    }
                    //Иначе, если событие запрашивает открытие дизайнера оборудования для твёрдой добычи
                    else if (clickEvent.WidgetName
                        == "OpenExtractionEquipmentSolidDesigner")
                    {
                        //Запрашиваем открытие дизайнера оборудования для твёрдой добычи
                        GameOpenDesignerEvent(
                            DesignerType.ComponentExtractionEquipmentSolid,
                            playerFaction.contentSetIndex);
                    }
                    //Иначе, если событие запрашивает открытие дизайнера энергетических орудий
                    else if (clickEvent.WidgetName
                        == "OpenEnergyGunDesigner")
                    {
                        //Запрашиваем открытие дизайнера энергетических орудий
                        GameOpenDesignerEvent(
                            DesignerType.ComponentGunEnergy,
                            playerFaction.contentSetIndex);
                    }

                    //Если активна панель объекта
                    if (gameWindow.activeMainPanelType == MainPanelType.Object)
                    {
                        //Берём панель объекта
                        UIObjectPanel objectPanel = gameWindow.objectPanel;

                        //Если активна подпанель организации
                        if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
                        {

                        }
                        //Иначе, если активна подпанель региона
                        else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                        {
                            //Берём подпанель региона
                            UIRegionObjectSubpanel regionSubpanel = objectPanel.regionObjectSubpanel;

                            //Если нажата кнопка обзорной вкладки
                            if (clickEvent.WidgetName == "RegionOverviewTab")
                            {
                                //Запрашиваем отображение обзорной вкладки
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.RegionOverview,
                                    objectPanel.activeObjectPE);
                            }
                            //Иначе, если нажата кнопка вкладки организаций
                            else if (clickEvent.WidgetName == "RegionOrganizationsTab")
                            {
                                //Запрашиваем отображение вкладки организаций
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.RegionOrganizations,
                                    objectPanel.activeObjectPE);
                            }

                            //Иначе, если активна обзорная вкладка
                            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.overviewTab.selfTabButton)
                            {
                                //Если нажата кнопка колонизации
                                if (clickEvent.WidgetName == "RAEOColonize")
                                {
                                    //Берём компонент организации игрока
                                    inputData.Value.playerOrganizationPE.Unpack(world.Value, out int organizationEntity);
                                    ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                                    //Берём компонент RAEO
                                    objectPanel.activeObjectPE.Unpack(world.Value, out int rAEOEntity);
                                    ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                                    //Берём компонент ExORAEO данной организации
                                    rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                                    ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);

                                    //Назначаем сущности ORAEO самозапрос действия
                                    ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Add(oRAEOEntity);

                                    //Заполняем данные самозапроса
                                    oRAEOActionSR = new(ORAEOActionType.Colonization);
                                }
                            }
                            //Иначе, если активна вкладка организаций
                            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.organizationsTab.selfTabButton)
                            {
                                //Если источник события имееет компонент ORAEOBriefInfoPanel
                                if (clickEvent.Sender.TryGetComponent(out UIORAEOBriefInfoPanel briefInfoPanel))
                                {
                                    //Запрашиваем отображение подпанели ORAEO
                                    GameDisplayObjectPanelEvent(
                                        DisplayObjectPanelEventType.ORAEO,
                                        briefInfoPanel.organizationRAEOPE);
                                }
                            }
                        }
                        //Иначе, если активна подпанель ORAEO
                        else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
                        {
                            //Берём подпанель ORAEO
                            UIORAEOObjectSubpanel oRAEOSubpanel = objectPanel.oRAEOObjectSubpanel;

                            //Если нажата кнопка обзорной вкладки
                            if (clickEvent.WidgetName == "ORAEOOverviewTab")
                            {
                                //Запрашиваем отображение обзорной вкладки
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.ORAEOOverview,
                                    objectPanel.activeObjectPE);
                            }
                        }
                    }
                }
                //Если активно окно главного меню
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.MainMenu)
                {
                    //Если событие запрашивает открытие меню новой игры
                    if (clickEvent.WidgetName
                        == "OpenNewGameMenu")
                    {
                        //Создаём событие, запрашивающее открытие окна новой игры
                        MainMenuActionEvent(
                            MainMenuActionType.OpenNewGameMenu);
                    }
                    //Иначе, если событие запрашивает открытие меню загрузки игры
                    else if (clickEvent.WidgetName
                        == "OpenLoadGameMenu")
                    {
                        //Создаём событие, запрашивающее открытие окна загрузки игры
                        MainMenuActionEvent(
                            MainMenuActionType.OpenLoadGameMenu);
                    }
                    //Иначе, если событие запрашивает открытие окна мастерской
                    else if (clickEvent.WidgetName
                        == "OpenWorkshop")
                    {
                        //Создаём событие, запрашивающее открытие окна мастерской
                        MainMenuActionEvent(
                            MainMenuActionType.OpenWorkshop);
                    }
                    //Иначе, если событие запрашивает открытие окна главных настроек
                    else if (clickEvent.WidgetName
                        == "OpenSettings")
                    {
                        //Создаём событие, запрашивающее открытие окна главных настроек
                        MainMenuActionEvent(
                            MainMenuActionType.OpenMainSettings);
                    }
                    //Иначе, если событие запрашивает выход из игры
                    else if (clickEvent.WidgetName
                        == "QuitGame")
                    {
                        //Создаём событие, запрашивающее выход из игры
                        QuitGameEvent();
                    }
                }
                //Если активно меню новой игры
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {
                    //Берём ссылку на окно меню новой игры
                    UINewGameMenuWindow newGameMenuWindow
                        = eUI.Value.newGameMenuWindow;

                    //Если событие запрашивает начало новой игры
                    if (clickEvent.WidgetName
                        == "StartNewGame")
                    {
                        //Создаём событие, запрашивающее начало новой игры
                        NewGameMenuActionEvent(
                            NewGameMenuActionType.StartNewGame);
                    }
                }
                //Если активна мастерская
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Workshop)
                {
                    //Берём ссылку на окно мастерской
                    UIWorkshopWindow workshopWindow
                        = eUI.Value.workshopWindow;

                    //Если название события отсутствует
                    if (clickEvent.WidgetName
                        == "")
                    {
                        //Пытаемся получить панель набора контента
                        if (clickEvent.Sender.TryGetComponent(
                            out UIWorkshopContentSetPanel workshopContentSetPanel))
                        {
                            //Запрашиваем отображение выбранного набора контента
                            WorkshopActionEvent(
                                WorkshopActionType.DisplayContentSet,
                                workshopContentSetPanel.contentSetIndex);
                        }
                    }
                    //Иначе, если событие запрашивает открытие дизайнера контента
                    else if (clickEvent.WidgetName
                        == "WorkshopOpenDesigner")
                    {
                        //Берём активный переключатель в списке контента
                        Toggle activeToggle
                            = workshopWindow.contentInfoToggleGroup.GetFirstActiveToggle();

                        //Если он не пуст
                        if (activeToggle != null)
                        {
                            //Пытаемся получить панель выбранного вида контента
                            if (activeToggle.TryGetComponent(
                                out UIWorkshopContentInfoPanel workshopContentInfoPanel))
                            {
                                //Запрашиваем отображение выбранного дизайнера в текущем наборе контента
                                WorkshopActionEvent(
                                    WorkshopActionType.OpenDesigner,
                                    workshopWindow.currentContentSetIndex,
                                    workshopContentInfoPanel.designerType);
                            }
                        }
                    }
                }
                //Если активен дизайнер
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Designer)
                {
                    //Берём ссылку на окно дизайнера
                    UIDesignerWindow designerWindow
                        = eUI.Value.designerWindow;

                    //Если событие запрашивает сохранение объекта контента из текущего набора контента
                    if (clickEvent.WidgetName
                        == "SaveCurrentContentSetPanel")
                    {
                        //Если сохранение возможно
                        if (DesignerContentSavePossible()
                            == true)
                        {
                            //Запрашиваем сохранение объекта контента
                            DesignerActionEvent(
                                DesignerActionType.SaveContentObject,
                                true);
                        }
                    }
                    //Иначе, если событие запрашивает загрузку объекта контента из текущего набора контента
                    else if (clickEvent.WidgetName
                        == "LoadCurrentContentSetPanel")
                    {
                        //Берём активный переключатель
                        Toggle activeToggle
                            = designerWindow.currentContentSetList.toggleGroup.GetFirstActiveToggle();

                        //Если он не пуст
                        if (activeToggle
                            != null)
                        {
                            //Пытаемся получить обзорную панель выбранного объекта
                            if (activeToggle.TryGetComponent(
                                out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                            {
                                //Запрашиваем загрузку объекта контента
                                DesignerActionEvent(
                                    DesignerActionType.LoadContentSetObject,
                                    true,
                                    contentObjectBriefInfoPanel.contentSetIndex,
                                    contentObjectBriefInfoPanel.objectIndex);
                            }
                        }
                    }
                    //Иначе, если событие запрашивает загрузку объекта контента из прочего набора контента
                    else if (clickEvent.WidgetName
                        == "LoadOtherContentSetPanel")
                    {
                        //Берём активный переключатель
                        Toggle activeToggle
                            = designerWindow.otherContentSetsList.toggleGroup.GetFirstActiveToggle();

                        //Если он не пуст
                        if (activeToggle
                            != null)
                        {
                            //Пытаемся получить обзорную панель выбранного объекта
                            if (activeToggle.TryGetComponent(
                                out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                            {
                                //Запрашиваем загрузку объекта контента
                                DesignerActionEvent(
                                    DesignerActionType.LoadContentSetObject,
                                    false,
                                    contentObjectBriefInfoPanel.contentSetIndex,
                                    contentObjectBriefInfoPanel.objectIndex);
                            }
                        }
                    }
                    //Иначе, если событие запрашивает удаление объекта контента из текущего набора контента
                    else if (clickEvent.WidgetName
                        == "DeleteCurrentContentSetPanel")
                    {
                        //Берём активный переключатель
                        Toggle activeToggle
                            = designerWindow.currentContentSetList.toggleGroup.GetFirstActiveToggle();

                        //Если он не пуст
                        if (activeToggle != null)
                        {
                            //Пытаемся получить обзорную панель выбранного объекта
                            if (activeToggle.TryGetComponent(
                                out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                            {
                                //Если удаление возможно
                                if (DesignerContentDeletePossible(
                                    contentObjectBriefInfoPanel))
                                {
                                    //Запрашиваем удаление объекта контента
                                    DesignerActionEvent(
                                        DesignerActionType.DeleteContentSetObject,
                                        true,
                                        contentObjectBriefInfoPanel.contentSetIndex,
                                        contentObjectBriefInfoPanel.objectIndex);
                                }
                            }
                        }
                    }
                    //Иначе, если событие запрашивает отображение панели прочих наборов контента
                    else if (clickEvent.WidgetName
                        == "DisplayOtherContentSets")
                    {
                        //Запрашиваем отображение панели прочих наборов контента
                        DesignerActionEvent(
                            DesignerActionType.DisplayContentSetPanel,
                            false);
                    }
                    //Иначе, если событие запрашивает сокрытие панели прочих наборов контента
                    else if (clickEvent.WidgetName
                        == "HideOtherContentSets")
                    {
                        //Запрашиваем сокрытие панели прочих наборов контента
                        DesignerActionEvent(
                            DesignerActionType.HideContentSetPanel,
                            false);
                    }
                    //Иначе, если событие запрашивает отображение панели текущего набора контента
                    else if (clickEvent.WidgetName
                        == "DisplayCurrentContentSet")
                    {
                        //Запрашиваем отображение панели текущего набора контента
                        DesignerActionEvent(
                            DesignerActionType.DisplayContentSetPanel,
                            true);
                    }
                    //Иначе, если событие запрашивает сокрытие панели прочих наборов контента
                    else if (clickEvent.WidgetName
                        == "HideCurrentContentSet")
                    {
                        //Запрашиваем сокрытие панели текущего набора контента
                        DesignerActionEvent(
                            DesignerActionType.HideContentSetPanel,
                            true);
                    }
                    //Иначе, если активен дизайнер кораблей
                    else if (designerWindow.designerType
                        == DesignerType.ShipClass)
                    {
                        //Берём ссылку на окно дизайнера кораблей
                        UIShipDesignerWindow shipClassDesignerWindow
                            = eUI.Value.designerWindow.shipDesigner;

                        //Если событие не имеет подписи
                        if (clickEvent.WidgetName
                            == "")
                        {
                            //Берём активный переключатель из списка доступных компонентов
                            Toggle activeToggleAvailableComponent
                                = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                            //Берём активный переключатель из списка установленных компонентов
                            Toggle activeToggleInstalledComponent
                                = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                            //Если переключатель в списке доступных компонентов не пуст
                            if (activeToggleAvailableComponent
                                != null
                                //И является родительским объектом события
                                && activeToggleAvailableComponent.gameObject
                                == clickEvent.Sender)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleAvailableComponent.TryGetComponent(
                                    out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                                {
                                    //Запрашиваем отображение подробной информации о компоненте
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                        shipClassDesignerWindow.currentAvailableComponentsType,
                                        contentObjectBriefInfoPanel.contentSetIndex,
                                        contentObjectBriefInfoPanel.objectIndex);

                                    //Если переключатель в списке установленных компонентов активен
                                    if (activeToggleInstalledComponent
                                        != null)
                                    {
                                        activeToggleInstalledComponent.isOn
                                            = false;
                                    }
                                }
                            }
                            //Иначе, если переключатель в списке установленных компонентов не пуст
                            else if (activeToggleInstalledComponent
                                != null
                                //И является родительским объектом события
                                && activeToggleInstalledComponent.gameObject
                                == clickEvent.Sender)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleInstalledComponent.TryGetComponent(
                                    out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                                {
                                    //Запрашиваем отображение подробной информации о компоненте
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                        componentBriefInfoPanel.componentType,
                                        componentBriefInfoPanel.contentSetIndex,
                                        componentBriefInfoPanel.componentIndex);

                                    //Если переключатель в списке доступных компонентов активен
                                    if (activeToggleAvailableComponent
                                        != null)
                                    {
                                        activeToggleAvailableComponent.isOn
                                            = false;
                                    }
                                }
                            }
                            //Иначе, если родительский объект события имеет компонент Toggle
                            else if (clickEvent.Sender.TryGetComponent(
                                out Toggle eventSenderToggle))
                            {
                                //Если родительской ToggleGroup является список доступных компонентов
                                if (eventSenderToggle.group
                                    == shipClassDesignerWindow.availableComponentsListToggleGroup
                                    //ИЛИ если родительской ToggleGroup является список установленных компонентов
                                    || eventSenderToggle.group
                                    == shipClassDesignerWindow.installedComponentsListToggleGroup)
                                {
                                    //Запрашиваем сокрытие подробной информации о компоненте
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.HideComponentDetailedInfo);
                                }
                            }
                        }
                        //Если событие запрашивает добавление выбранного компонента в редактируемый корабль
                        if (clickEvent.WidgetName
                            == "AddComponentToShipClass")
                        {
                            //Берём активный переключатель из списка доступных компонентов
                            Toggle activeToggleAvailableComponent
                                = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                            //Если переключатель не пуст
                            if (activeToggleAvailableComponent
                                != null)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleAvailableComponent.TryGetComponent(
                                    out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                                {
                                    //Создаём событие добавления компонента
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.AddComponentToClass,
                                        shipClassDesignerWindow.currentAvailableComponentsType,
                                        contentObjectBriefInfoPanel.contentSetIndex,
                                        contentObjectBriefInfoPanel.objectIndex,
                                        1);
                                }
                            }
                            //Иначе 
                            else
                            {
                                //Берём активный переключатель из списка установленных компонентов
                                Toggle activeToggleInstalledComponent
                                    = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                                //Если переключатель не пуст
                                if (activeToggleInstalledComponent
                                    != null)
                                {
                                    //Пытаемся получить панель выбранного компонента
                                    if (activeToggleInstalledComponent.TryGetComponent(
                                        out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                                    {
                                        //Создаём событие добавления компонента
                                        DesignerShipClassActionEvent(
                                            DesignerShipClassActionType.AddComponentToClass,
                                            installedComponentBriefInfoPanel.componentType,
                                            installedComponentBriefInfoPanel.contentSetIndex,
                                            installedComponentBriefInfoPanel.componentIndex,
                                            1);
                                    }
                                }
                            }
                        }
                        //Иначе, если событие запрашивает удаление выбранного компонента из редактируемого корабля
                        else if (clickEvent.WidgetName
                            == "DeleteComponentFromShipClass")
                        {
                            //Берём активный переключатель из списка установленных компонентов
                            Toggle activeToggleInstalledComponent
                                = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                            //Если переключатель не пуст
                            if (activeToggleInstalledComponent
                                != null)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleInstalledComponent.TryGetComponent(
                                    out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                                {
                                    //Создаём событие удаления компонента
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.DeleteComponentFromClass,
                                        componentBriefInfoPanel.componentType,
                                        componentBriefInfoPanel.contentSetIndex,
                                        componentBriefInfoPanel.componentIndex,
                                        1);
                                }
                            }
                        }
                    }
                    //Иначе, если активен дизайнер двигателей
                    else if (designerWindow.designerType
                        == DesignerType.ComponentEngine)
                    {

                    }
                    //Иначе, если активен дизайнер реакторов
                    else if (designerWindow.designerType
                        == DesignerType.ComponentReactor)
                    {

                    }
                    //Иначе, если активен дизайнер топливных баков
                    else if (designerWindow.designerType
                        == DesignerType.ComponentHoldFuelTank)
                    {

                    }
                    //Иначе, если активен дизайнер оборудования для твёрдой добычи
                    else if (designerWindow.designerType
                        == DesignerType.ComponentExtractionEquipmentSolid)
                    {

                    }
                    //Иначе, если активен дизайнер энергетических орудий
                    else if (designerWindow.designerType
                        == DesignerType.ComponentGunEnergy)
                    {

                    }
                }

                uguiUIWorld.DelEntity(clickEventUIEntity);
            }

            //Для каждого события изменения значения выпадающего списка
            foreach (int dropdownEventUIEntity in dropdownEventUIFilter)
            {
                //Берём компонент события
                ref EcsUguiTmpDropdownChangeEvent dropdownEvent = ref dropdownEventUIPool.Get(dropdownEventUIEntity);

                Debug.LogWarning(
                    "Dropdown change! " + dropdownEvent.WidgetName
                    + " ! " + dropdownEvent.Value);

                //Если активен дизайнер
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.Designer)
                {
                    //Берём ссылку на окно дизайнера
                    UIDesignerWindow designerWindow
                        = eUI.Value.designerWindow;

                    //Если событие запрашивает смену набора контента на панели прочих наборов контента
                    if (dropdownEvent.WidgetName
                        == "ChangeOtherContentSet")
                    {
                        //Определяем истинный индекс набора контента
                        int contentSetIndex
                            = -1;

                        //Если индекс выпадающего списка меньше, чем индекс текущего набора контента
                        if (dropdownEvent.Value
                            < designerWindow.currentContentSetIndex)
                        {
                            //То настоящий индекс соответствует индексу из списка
                            contentSetIndex
                                = dropdownEvent.Value;
                        }
                        //Иначе
                        else
                        {
                            //Настоящий индекс больше на 1
                            contentSetIndex
                                = dropdownEvent.Value + 1;
                        }

                        //Запрашиваем смену набора контента на панели прочих наборов контента
                        DesignerActionEvent(
                            DesignerActionType.DisplayContentSetPanelList,
                            false,
                            contentSetIndex);
                    }
                    //Иначе, если активен дизайнер кораблей
                    else if (designerWindow.designerType
                        == DesignerType.ShipClass)
                    {
                        //Берём ссылку на окно дизайнера кораблей
                        UIShipDesignerWindow shipDesignerWindow
                            = designerWindow.shipDesigner;

                        //Если событие запрашивает смену типа доступных компонентов
                        if (dropdownEvent.WidgetName
                            == "ChangeAvailableComponentsType")
                        {
                            //Запрашиваем изменение типа доступных компонентов
                            DesignerShipClassActionEvent(
                                DesignerShipClassActionType.ChangeAvailableComponentsType,
                                (ShipComponentType)shipDesignerWindow.availableComponentTypeDropdown.value);
                        }
                    }
                    //Иначе, если активен дизайнер двигателей
                    else if (designerWindow.designerType
                        == DesignerType.ComponentEngine)
                    {
                        //Берём ссылку на окно дизайнера двигателей
                        UIEngineDesignerWindow engineDesignerWindow
                            = designerWindow.engineDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей мощность двигателя на единицу размера
                        if (dropdownEvent.WidgetName
                            == "ChangeEnginePowerPerSizeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере двигателей
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер реакторов
                    else if (designerWindow.designerType
                        == DesignerType.ComponentReactor)
                    {
                        //Берём ссылку на окно дизайнера реакторов
                        UIReactorDesignerWindow reactorDesignerWindow
                            = designerWindow.reactorDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей энергию реактора на единицу размера
                        if (dropdownEvent.WidgetName
                            == "ChangeReactorEnergyPerSizeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере реакторов
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер топливных баков
                    else if (designerWindow.designerType
                        == DesignerType.ComponentHoldFuelTank)
                    {
                        //Берём ссылку на окно дизайнера реакторов
                        UIFuelTankDesignerWindow fuelTankDesignerWindow
                            = designerWindow.fuelTankDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей сжатие топливного бака
                        if (dropdownEvent.WidgetName
                            == "ChangeFuelTankCompressionTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере топливных баков
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер оборудования для твёрдой добычи
                    else if (designerWindow.designerType
                        == DesignerType.ComponentExtractionEquipmentSolid)
                    {
                        //Берём ссылку на окно дизайнера добывающего оборудования
                        UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                            = designerWindow.extractionEquipmentDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей скорость на единицу размера
                        if (dropdownEvent.WidgetName
                            == "ChangeExtractionEquipmentSolidSpeedPerSizeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере оборудования для твёрдой добычи
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер энергетических орудий
                    else if (designerWindow.designerType
                        == DesignerType.ComponentGunEnergy)
                    {
                        //Берём ссылку на окно дизайнера энергетических орудий
                        UIGunEnergyDesignerWindow energyGunDesignerWindow
                            = designerWindow.energyGunDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей перезарядку
                        if (dropdownEvent.WidgetName
                            == "ChangeEnergyGunRechargeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере энергетических орудий
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                dropdownEvent.Value);
                        }
                    }
                }

                uguiUIWorld.DelEntity(dropdownEventUIEntity);
            }

            //Для каждого события изменения значения ползунка
            foreach (int sliderEventUIEntity
                in sliderEventUIFilter)
            {
                //Берём компонент события
                ref EcsUguiSliderChangeEvent
                    sliderEvent
                    = ref sliderEventUIPool.Get(sliderEventUIEntity);

                Debug.LogWarning(
                    "Slider change! " + sliderEvent.WidgetName
                    + " ! " + sliderEvent.Value);

                //Если активно меню новой игры
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {

                }
            }

            //Для каждого события клика по космическому объекту
            foreach (int clickEventSpaceEntity in clickEventSpaceFilter)
            {
                //Берём компонент события
                ref EcsUguiClickEvent clickEvent = ref clickEventSpacePool.Get(clickEventSpaceEntity);

                //Отмечаем, что объект клика ещё не найден
                bool clickEventComplete = false;

                Debug.LogWarning(
                    "Click! "
                    + clickEvent.WidgetName);

                //Если активен режим карты галактики
                /*if (inputData.Value.activeMapMode == MapMode.Galaxy)
                {
                    //Для каждого SO в фильтре галактического режима
                    foreach (int spaceObjectEntity in galaxySpaceObjectFilter.Value)
                    {
                        //Берём компонент SO
                        ref CSpaceObject spaceObject = ref spaceObjectPool.Value.Get(spaceObjectEntity);

                        //Если текущий SO - искомый
                        if (spaceObject.Transform == clickEvent.Sender.transform.parent)
                        {
                            //Запрашиваем смену режима карты на режим сектора
                            MapChangeModeEvent(
                                ChangeMapModeRequestType.Sector,
                                spaceObject.selfPE);
                        }
                    }
                }*/

                uguiSOWorld.DelEntity(clickEventSpaceEntity);
            }


            //Ввод перемещения камеры
            InputCameraMoving();

            //Ввод приближения камеры
            InputCameraZoom();

            //Если активен режим карты планетарной системы
            /*if (inputData.Value.activeMapMode
                == MapMode.PlanetSystem)
            {
                //Ввод поворота камеры
                InputCameraRotating();

                //Ввод изменения масштаба камеры
                MapScaleChangingInput();
            }*/

            //Прочий ввод
            InputOther();

            //Ввод мыши
            MouseInput();

            if (Input.GetMouseButton(0)
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
            {
                HandleInput2();
            }
            else
            {
                //Очищаем предыдущую ячейку
                inputData.Value.previousRegionPE = new();
            }
        }

        //Главное меню
        void MainMenuActionEvent(
            MainMenuActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент события действия в главном меню
            int eventEntity = world.Value.NewEntity();
            ref EMainMenuAction mainMenuActionEvent
                = ref mainMenuActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в главном меню запрашивается
            mainMenuActionEvent.actionType 
                = actionType;
        }

        //Меню новой игры
        void NewGameMenuActionEvent(
            NewGameMenuActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент события действия в меню новой игры
            int eventEntity = world.Value.NewEntity();
            ref ENewGameMenuAction newGameMenuActionEvent
                = ref newGameMenuActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в меню новой игры запрашивается
            newGameMenuActionEvent.actionType
                = actionType;
        }

        //Меню загрузки
        //

        //Мастерская
        void WorkshopActionEvent(
            WorkshopActionType actionType,
            int contentSetIndex = -1,
            DesignerType designerType = DesignerType.None)
        {
            //Создаём новую сущность и назначаем ей компонент события действия в мастерской
            int eventEntity = world.Value.NewEntity();
            ref EWorkshopAction workshopActionEvent
                = ref workshopActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в мастерской запрашивается
            workshopActionEvent.actionType
                = actionType;

            //Указываем индекс набора контента, который требуется отобразить
            workshopActionEvent.contentSetIndex
                = contentSetIndex;

            //Указываем, какой тип дизайнера требуется открыть
            workshopActionEvent.designerType
                = designerType;
        }

        //Дизайнер
        void DesignerActionEvent(
            DesignerActionType actionType,
            bool isCurrentContentSet = true,
            int contentSetIndex = -1,
            int objectIndex = -1)
        {
            //Создаёнм новую сущность и назначаем ей компонент события действия в дизайнере
            int eventEntity = world.Value.NewEntity();
            ref EDesignerAction designerActionEvent
                = ref designerActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в дизайнере запрашивается
            designerActionEvent.actionType
                = actionType;

            //Указываем, с текущим ли набором контента требуется совершить действие
            designerActionEvent.isCurrentContentSet
                = isCurrentContentSet;

            //Указываем индекс набора контента
            designerActionEvent.contentSetIndex
                = contentSetIndex;

            //Указываем индекс объекта
            designerActionEvent.objectIndex
                = objectIndex;
        }

        bool DesignerContentSavePossible()
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если графа названия объекта не пуста
            if (designerWindow.currentContentSetList.objectName.text
                != "")
            {
                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Для каждой обзорной панели в списке текущего набора контента
                    for (int a = 0; a < designerWindow.currentContentSetList.panelsList.Count; a++)
                    {
                        //Пытаемся получить панель
                        if (designerWindow.currentContentSetList.panelsList[a].TryGetComponent(
                            out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                        {
                            //Если название объекта совпадает с названием в графе
                            if (contentObjectBriefInfoPanel.objectName.text
                                == designerWindow.currentContentSetList.objectName.text)
                            {
                                //Сохранение невозможно
                                return false;
                            }
                        }
                    }
                }

                //Если активен дизайнер двигателей
                if (designerWindow.designerType
                    == DesignerType.ComponentEngine)
                {
                    //Берём ссылку на окно дизайнера двигателей
                    UIEngineDesignerWindow engineDesignerWindow
                        = designerWindow.engineDesigner;

                    //Если в дизайнере двигателей указана основная технология каждого типа
                    if (engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //Сохранение возможно
                        return true;
                    }
                    //Иначе
                    else
                    {
                        //Сохранение невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер реакторов
                else if(designerWindow.designerType
                    == DesignerType.ComponentReactor)
                {
                    //Берём ссылку на окно дизайнера реакторов
                    UIReactorDesignerWindow reactorDesignerWindow
                        = designerWindow.reactorDesigner;

                    //Если в дизайнере реакторов указана основная технология каждого типа
                    if (reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //Сохранение возможно
                        return true;
                    }
                    //Иначе
                    else
                    {
                        //Сохранение невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер топливных баков
                else if (designerWindow.designerType
                    == DesignerType.ComponentHoldFuelTank)
                {
                    //Берём ссылку на окно дизайнера топливных баков
                    UIFuelTankDesignerWindow fuelTankDesignerWindow
                        = designerWindow.fuelTankDesigner;

                    //Если в дизайнере топливных баков указана основная технология каждого типа
                    if (fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //Сохранение возможно
                        return true;
                    }
                    //Иначе
                    else
                    {
                        //Сохранение невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер оборудования для твёрдой добычи
                else if (designerWindow.designerType
                    == DesignerType.ComponentExtractionEquipmentSolid)
                {
                    //Берём ссылку на окно дизайнера добывающего оборудования
                    UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                        = designerWindow.extractionEquipmentDesigner;

                    //Если в дизайнере добывающего оборудования указана основная технология каждого типа
                    if (extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //Сохранение возможно
                        return true;
                    }
                    //Иначе
                    else
                    {
                        //Сохранение невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер энергетических орудий
                else if (designerWindow.designerType
                    == DesignerType.ComponentGunEnergy)
                {
                    //Берём ссылку на окно дизайнера энергетических орудий
                    UIGunEnergyDesignerWindow energyGunDesignerWindow
                        = designerWindow.energyGunDesigner;

                    //Если в дизайнере энергетических орудий указана основная технология каждого типа
                    if (energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //Сохранение возможно
                        return true;
                    }
                    //Иначе
                    else
                    {
                        //Сохранение невозможно
                        return false;
                    }
                }

                //Иначе
                else
                {
                    //Сохранение возможно
                    return true;
                }
            }
            //Иначе
            else
            {
                //Сохранение невозможно
                return false;
            }
        }

        bool DesignerContentDeletePossible(
            UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если активен внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == true)
            {
                //Если активен дизайнер двигателей
                if (designerWindow.designerType
                    == DesignerType.ComponentEngine)
                {
                    //Берём ссылку на двигатель, который требуется удалить
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .engines[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на двигатель классов кораблей пуст
                    if (engine.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер реакторов
                else if (designerWindow.designerType
                    == DesignerType.ComponentReactor)
                {
                    //Берём ссылку на реактор, который требуется удалить
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .reactors[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на реактор классов кораблей пуст
                    if (reactor.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер топливных баков
                else if (designerWindow.designerType
                    == DesignerType.ComponentHoldFuelTank)
                {
                    //Берём ссылку на топливный бак, который требуется удалить
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .fuelTanks[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на топливный бак классов кораблей пуст
                    if (fuelTank.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер оборудования для твёрдой добычи
                else if (designerWindow.designerType
                    == DesignerType.ComponentExtractionEquipmentSolid)
                {
                    //Берём ссылку на оборудование для твёрдой добычи, которое требуется удалить
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .solidExtractionEquipments[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на оборудование для твёрдой добычи классов кораблей пуст
                    if (extractionEquipmentSolid.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер энергетических орудий
                else if (designerWindow.designerType
                    == DesignerType.ComponentGunEnergy)
                {
                    //Берём ссылку на энергетическое орудие, которое требуется удалить
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .energyGuns[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на энергетическое орудие классов кораблей пуст
                    if (energyGun.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }

                //Удаление невозможно
                return false;
            }
            //Иначе
            else
            {
                //Если активен дизайнер двигателей
                if (designerWindow.designerType
                    == DesignerType.ComponentEngine)
                {
                    //Берём ссылку на двигатель, который требуется удалить
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .engines[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на двигатель классов кораблей пуст
                    if (engine.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер реакторов
                else if (designerWindow.designerType
                    == DesignerType.ComponentReactor)
                {
                    //Берём ссылку на реактор, который требуется удалить
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .reactors[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на реактор классов кораблей пуст
                    if (reactor.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер топливных баков
                else if (designerWindow.designerType
                    == DesignerType.ComponentHoldFuelTank)
                {
                    //Берём ссылку на топливный бак, который требуется удалить
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .fuelTanks[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на топливный бак классов кораблей пуст
                    if (fuelTank.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер оборудования для твёрдой добычи
                else if (designerWindow.designerType
                    == DesignerType.ComponentExtractionEquipmentSolid)
                {
                    //Берём ссылку на оборудование для твёрдой добычи, которое требуется удалить
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .solidExtractionEquipments[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на оборудование для твёрдой добычи классов кораблей пуст
                    if (extractionEquipmentSolid.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }
                //Иначе, если активен дизайнер энергетических орудий
                else if (designerWindow.designerType
                    == DesignerType.ComponentGunEnergy)
                {
                    //Берём ссылку на энергетическое орудие, которое требуется удалить
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .energyGuns[contentObjectBriefInfoPanel.objectIndex];

                    //Если список ссылающихся на энергетическое орудие классов кораблей пуст
                    if (energyGun.ShipClasses.Count
                        > 0)
                    {
                        //Удаление невозможно
                        return false;
                    }
                }

                //Удаление невозможно
                return false;
            }
        }

        //Дизайнер кораблей
        void DesignerShipClassActionEvent(
            DesignerShipClassActionType actionType,
            ShipComponentType componentType = ShipComponentType.None,
            int contentSetIndex = -1,
            int modelIndex = -1,
            int numberOfComponents = -1)
        {
            //Создаём новую сущность и назначаем ей компонент события обработки панели обзора компонента
            int eventEntity = world.Value.NewEntity();
            ref EDesignerShipClassAction designerShipClassActionEvent
                = ref designerShipClassActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в дизайнере кораблей запрашивается
            designerShipClassActionEvent.actionType
                = actionType;


            //Указываем тип компонента
            designerShipClassActionEvent.componentType
                = componentType;


            //Указываем индекс набора контента
            designerShipClassActionEvent.contentSetIndex
                = contentSetIndex;

            //Указываем индекс модели
            designerShipClassActionEvent.modelIndex
                = modelIndex;


            //Указываем число компонентов, которое требуется установить/удалить
            designerShipClassActionEvent.numberOfComponents
                = numberOfComponents;
        }


        void DesignerComponentActionEvent(
            DesignerComponentActionType actionType,
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyIndex)
        {
            //Создаём новую сущность и назначаем ей компонент события смены основной технологии
            int eventEntity = world.Value.NewEntity();
            ref EDesignerComponentAction designerComponentActionEvent
                = ref designerComponentActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в дизайнере компонентов запрашивается
            designerComponentActionEvent.actionType
                = actionType;

            //Указываем тип модификатора
            designerComponentActionEvent.componentCoreModifierType
                = componentCoreModifierType;

            //Указываем индекс выбранной технологии
            designerComponentActionEvent.technologyDropdownIndex
                = technologyIndex;
        }

        //Игра
        void GameActionEvent(
            GameActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент события действия в игре
            int eventEntity = world.Value.NewEntity();
            ref EGameAction gameActionEvent = ref gameActionEventPool.Value.Add(eventEntity);

            //Указываем, какое действие в игре запрашивается
            gameActionEvent.actionType = actionType;
        }

        void GameOpenDesignerEvent(
            DesignerType designerType,
            int contentSetIndex)
        {
            //Создаём новую сущность и назначаем ей компонент события открытия дизайнера в игре
            int eventEntity = world.Value.NewEntity();
            ref EGameOpenDesigner gameOpenDesignerEvent = ref gameOpenDesignerEventPool.Value.Add(eventEntity);

            //Указываем, какой дизайнер требуется открыть
            gameOpenDesignerEvent.designerType = designerType;

            //Указываем, какой набор контента требуется открыть
            gameOpenDesignerEvent.contentSetIndex = contentSetIndex;
        }

        void GameDisplayObjectPanelEvent(
            DisplayObjectPanelEventType eventType,
            EcsPackedEntity objectPE,
            bool isRefresh = false)
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

        void MapChangeModeEvent(
            ChangeMapModeRequestType requestType,
            EcsPackedEntity objectPE)
        {
            //Создаём новую сущность и назначаем ей компонент запроса смены режима карты
            int changeMapModeRequestEntity = world.Value.NewEntity();
            ref RChangeMapMode changeMapModeRequest = ref changeMapModeRequestPool.Value.Add(changeMapModeRequestEntity);

            //Указываем требуемый режим карты
            changeMapModeRequest.requestType = requestType;

            changeMapModeRequest.mapMode = MapMode.Distance;

            //Указываем, карта какого объекта открывается
            changeMapModeRequest.activeObject = objectPE;
        }


        void InputCameraMoving()
        {
            //Если нажата клавиша движения вперёд
            if (Input.GetKey(KeyCode.W))
            {
                //Рассчитываем перемещение камеры вперёд
                inputData.Value.cameraFocusMoving
                    //Умножаем вектор направления "вверх"
                    += (-inputData.Value.rotationObjectZ.transform.forward
                    //На скорость движения камеры
                    * inputData.Value.movementSpeed
                    //И на время кадра
                    * UnityEngine.Time.deltaTime);

            }
            //Иначе, если нажата клавиша движения назад
            else if (Input.GetKey(KeyCode.S))
            {
                //Рассчитываем перемещение камеры назад
                inputData.Value.cameraFocusMoving
                    //Умножаем вектор направления "вверх"
                    += (inputData.Value.rotationObjectZ.transform.forward
                    //На скорость движения камеры
                    * inputData.Value.movementSpeed
                    //И на время кадра
                    * UnityEngine.Time.deltaTime);
            }

            //Если нажата клавиша движения влево
            if (Input.GetKey(KeyCode.A))
            {
                //Рассчитываем перемещение камеры влево
                inputData.Value.cameraFocusMoving
                    //Умножаем вектор направления "вправо"
                    += (inputData.Value.rotationObjectZ.transform.right
                    //На скорость движения камеры
                    * inputData.Value.movementSpeed
                    //И на время кадра
                    * UnityEngine.Time.deltaTime);
            }
            //Иначе, если нажата клавиша движения вправо
            else if (Input.GetKey(KeyCode.D))
            {
                //Рассчитываем перемещение камеры вправо
                inputData.Value.cameraFocusMoving
                    //Умножаем вектор направления "вправо"
                    += (-inputData.Value.rotationObjectZ.transform.right
                    //На скорость движения камеры
                    * inputData.Value.movementSpeed
                    //И на время кадра
                    * UnityEngine.Time.deltaTime);
            }
        }

        void InputCameraRotating()
        {
            //Если нажата клавиша поворота вправо
            if (Input.GetKey(KeyCode.E))
            {
                //Рассчитываем поворот камеры вправо
                inputData.Value.rotationAnglesZ
                    //Умножаем скорость поворота камеры
                    += -inputData.Value.rotationSpeed
                    //На время кадра
                    * UnityEngine.Time.deltaTime;
            }
            //Иначе, если нажата клавиша поворота влево
            else if (Input.GetKey(KeyCode.Q))
            {
                //Рассчитываем поворот камеры влево
                inputData.Value.rotationAnglesZ
                    //Умножаем скорость поворота камеры
                    += inputData.Value.rotationSpeed
                    //На время кадра
                    * UnityEngine.Time.deltaTime;
            }

            //Если нажата клавиша поворота камеры вверх
            if (Input.GetKey(KeyCode.X))
            {
                //Рассчитываем поворот камеры вверх
                inputData.Value.rotationAnglesX
                    //Умножаем скорость поворота камеры
                    += inputData.Value.rotationSpeed
                    //На время кадра
                    * UnityEngine.Time.deltaTime;
            }
            //Иначе, если нажата клавиша поворота камеры вниз
            else if (Input.GetKey(KeyCode.Z))
            {
                //Рассчитываем поворот камеры вниз
                inputData.Value.rotationAnglesX
                    //Умножаем скорость поворота камеры
                    += -inputData.Value.rotationSpeed
                    //На время кадра
                    * UnityEngine.Time.deltaTime;
            }
        }

        void InputCameraZoom()
        {
            //Если есть вращение колёсика мыши
            if (Input.mouseScrollDelta.y
                != 0)
            {
                //Рассчитываем приближение камеры
                inputData.Value.zoomAmount
                    //Умножаем вращение колёсика мыши
                    += Input.mouseScrollDelta.y
                    //На скорость приближения камеры
                    * inputData.Value.zoomSpeed;
            }
        }

        void InputOther()
        {
            //Пауза тиков
            if (Input.GetKeyUp(KeyCode.Space))
            {
                //Если игра активна
                if (runtimeData.Value.isGameActive
                    == true)
                {
                    //Запрашиваем включение паузы
                    GameActionEvent(
                        GameActionType.PauseOn);
                }
                //Иначе
                else
                {
                    //Запрашиваем выключение паузы
                    GameActionEvent(
                        GameActionType.PauseOff);
                }
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                //Если активно окно игры
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.Game)
                {
                    //Создаём событие, запрашивающее выход из игры
                    QuitGameEvent();
                }
                //Иначе, если активно окно дизайнера
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Designer)
                {
                    //Если активен внутриигровой дизайнер
                    if (eUI.Value.designerWindow.isInGameDesigner
                        == true)
                    {
                        //Запрашиваем открытие окна игры
                        DesignerActionEvent(
                            DesignerActionType.OpenGame);
                    }
                    //Иначе
                    else
                    {
                        //Запрашиваем открытие окна мастерской
                        DesignerActionEvent(
                            DesignerActionType.OpenWorkshop);
                    }
                }
                //Иначе, если активно окно мастерской
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Workshop)
                {
                    //Запрашиваем открытие окна главного меню
                    WorkshopActionEvent(
                        WorkshopActionType.OpenMainMenu);
                }
                //Иначе, если активно окно меню новой игры
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {
                    //Запрашиваем открытие окна главного меню
                    NewGameMenuActionEvent(
                        NewGameMenuActionType.OpenMainMenu);
                }
            }

            if (Input.GetKeyUp(KeyCode.Y))
            {
                Debug.LogWarning("Y!");

                //Запрашиваем смену режима карты на режим расстояний
                MapChangeModeEvent(
                    ChangeMapModeRequestType.Distance,
                    new EcsPackedEntity());
            }
        }

        void MouseInput()
        {
            //Берём луч из положения мыши
            Ray inputRay = inputData.Value.camera.ScreenPointToRay(Input.mousePosition);

            //Если луч касается объекта
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                //Пытаемся получить компонент GO MO
                if (hit.transform.gameObject.TryGetComponent(
                    out GOMapObject gOMapObject))
                {
                    //Берём компонент родительского MO
                    gOMapObject.mapObjectPE.Unpack(world.Value, out int mapObjectEntity);
                    ref CMapObject mapObject = ref mapObjectPool.Value.Get(mapObjectEntity);

                    //Если тип MO - группа кораблей
                    if (mapObject.objectType == MapObjectType.ShipGroup)
                    {
                        //Берём компонент группы кораблей
                        ref CShipGroup shipGroup = ref shipGroupPool.Value.Get(mapObjectEntity);

                        //Если это активная группа кораблей
                        if (inputData.Value.activeShipGroupPE.EqualsTo(mapObject.selfPE) == true)
                        {
                            //Делаем её неактивной
                            inputData.Value.activeShipGroupPE = new();
                        }
                        //Иначе
                        else
                        {
                            //Делаем её активной
                            inputData.Value.activeShipGroupPE = mapObject.selfPE;
                        }
                    }
                    //Иначе, если тип MO - регион
                    /*else if (mapObject.objectType == MapObjectType.Island)
                    {
                        //Берём компонент региона
                        ref CIsland island = ref islandPool.Value.Get(mapObjectEntity);

                        //Если нажата ЛКМ
                        if (Input.GetMouseButtonDown(0))
                        {
                            //Запрашиваем отображение подпанели объекта острова
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.Island,
                                island.selfPE);
                        }

                        //Если есть активная группа кораблей
                        if (inputData.Value.activeShipGroupPE.Unpack(world.Value, out int activeShipGroupEntity))
                        {
                            //Берём компонент группы кораблей
                            ref CShipGroup activeShipGroup = ref shipGroupPool.Value.Get(activeShipGroupEntity);

                            //Если группа кораблей находтися в режиме ожидания
                            if (activeShipGroup.movingMode == Ship.ShipGroupMovingMode.Idle)
                            {
                                //Назначаем сущности компонент движения
                                ref CSGMoving activeSGMoving = ref sGMovingPool.Value.Add(activeShipGroupEntity);

                                //Заполняем основные данные компонента движения
                                activeSGMoving = new(0);

                                //Переводим группу кораблей в режим движения
                                activeShipGroup.movingMode = Ship.ShipGroupMovingMode.Moving;

                                //Указываем целевой объект как первую точку пути
                                activeSGMoving.pathPoints.AddLast(new DShipGroupPathPoint(
                                    new(),
                                    island.selfPE,
                                    MovementTargetType.RAEO,
                                    MovementType.Pathfinding,
                                    DestinationPointRegion.None,
                                    DestinationPointTask.Landing));
                            }
                        }
                    }*/

                    Debug.LogWarning(mapObject.objectType);
                }
            }
        }

        void HandleInput2()
        {
            Ray inputRay
                = inputData.Value.camera.ScreenPointToRay(
                    Input.mousePosition);

            RaycastHit hit;

            if (Physics.Raycast(
                inputRay,
                out hit))
            {
                //Берём компонент региона
                GetRegionPE(hit.point).Unpack(world.Value, out int regionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(regionEntity);

                //Если текущий регион не совпадает с предыдущим
                if (inputData.Value.previousRegionPE.EqualsTo(currentRegion.selfPE) == false)
                {
                    //Проверяем перетаскивание в текущий регион
                    ValidateDrag(ref currentRegion);
                }
                //Иначе
                else
                {
                    //Отмечаем, что перетаскивание неактивно
                    inputData.Value.isDrag = false;
                }

                //Если режим редактирования активен
                if (false)
                {
                    //Редактируем регион
                    RegionsEdit(ref currentRegion);
                }
                //Иначе, если нажата кнопка
                else if (Input.GetKey(KeyCode.LeftShift)
                    //И конечный регион не совпадает с текущей
                    && inputData.Value.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                {
                    //Если стартовый регион не совпадает с текущей
                    if (inputData.Value.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //Если стартовый регион задана
                        if (inputData.Value.searchFromRegion.Unpack(world.Value, out int fromRegionEntity))
                        {
                            //Берём компонент стартового региона
                            ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                            //Отключаем подсветку стартового региона
                            fromRegion.DisableHighlight();
                        }

                        //Обновляем стартовый регион
                        inputData.Value.searchFromRegion = currentRegion.selfPE;
                        currentRegion.EnableHighlight(Color.blue);

                        //Если конечный регион задан
                        if (inputData.Value.searchToRegion.Unpack(world.Value, out int toRegionEntity))
                        {
                            //Берём компонент конечного региона
                            ref CHexRegion toRegion = ref regionPool.Value.Get(toRegionEntity);

                            //Ищем путь
                            FindPath(ref currentRegion, ref toRegion);
                        }
                    }
                }
                //Иначе, если стартовый регион задана
                else if (inputData.Value.searchFromRegion.Unpack(world.Value, out int fromRegionEntity)
                    //И стартовый регион не совпадает с конечным
                    && inputData.Value.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                {
                    //Если конечный регион не совпадает с текущим
                    if (inputData.Value.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //Сохраняем конечный регион
                        inputData.Value.searchToRegion = currentRegion.selfPE;

                        //Берём компонент стартового региона
                        ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                        FindPath(ref fromRegion, ref currentRegion);
                    }
                }
                //Иначе
                else
                {
                    //Запрашиваем отображение подпанели объекта региона
                    GameDisplayObjectPanelEvent(
                        DisplayObjectPanelEventType.Region,
                        currentRegion.selfPE);
                }

                //Если какой-либо корабль активен
                /*if (inputData.Value.activeShipPE.Unpack(world.Value, out int activeShipEntity))
                {
                    //Берём компонент корабля
                    ref CShip activeShip
                        = ref shipPool.Value.Get(activeShipEntity);

                    //Если корабль находится в режиме ожидания
                    if (activeShip.shipMode
                        == ShipMode.Idle)
                    {
                        //Переводим корабль в режим поиска пути
                        activeShip.shipMode
                            = ShipMode.Pathfinding;

                        //Указываем целевой объект
                        activeShip.targetPE
                            = currentCell.selfPE;

                        //Указываем тип целевого объекта
                        activeShip.targetType
                            = MovementTargetType.Cell;
                    }
                }*/

                //Сохраняем регион как предыдущий для следующего кадра
                inputData.Value.previousRegionPE = currentRegion.selfPE;
            }
            else
            {
                //Очищаем предыдущий регион
                inputData.Value.previousRegionPE = new();
            }
        }

        EcsPackedEntity GetRegionPE(
            Vector3 position)
        {
            //Определяем позицию клика
            position = sceneData.Value.coreObject.transform.InverseTransformPoint(position);

            //Вычисляем координаты региона
            DHexCoordinates coordinates = DHexCoordinates.FromPosition(position);

            //Определяем индекс региона
            int index = coordinates.X + coordinates.Z * spaceGenerationData.Value.regionCountX + coordinates.Z / 2;

            //Возвращаем PE региона по этому индексу
            return spaceGenerationData.Value.regionPEs[index];
            //return spaceGenerationData.Value.cells[coordinates];
        }

        EcsPackedEntity GetRegionPE(
            DHexCoordinates coordinates)
        {
            int z = coordinates.Z;

            //Если координата выходит за границы карты
            if (z < 0 || z >= spaceGenerationData.Value.regionCountZ)
            {
                return new();
            }

            int x = coordinates.X + z / 2;

            //Если координата выходит за границы карты
            if (x < 0 || x >= spaceGenerationData.Value.regionCountX)
            {
                return new();
            }

            return spaceGenerationData.Value.regionPEs[x + z * spaceGenerationData.Value.regionCountX];
        }

        void RegionsEdit(
            ref CHexRegion centerRegion)
        {
            //Берём координаты центрального региона
            int centerX = centerRegion.coordinates.X;
            int centerZ = centerRegion.coordinates.Z;

            //Случайно определяем размер кисти
            int brushSize = 0;

            //Для каждого региона в нижней половине кисти
            for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
            {
                for (int x = centerX - r; x <= centerX + brushSize; x++)
                {
                    //Если существует регион с такими координатами
                    if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int regionEntity))
                    {
                        //Берём компонент региона
                        ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                        //Редактируем регион
                        RegionEdit(ref region);
                    }
                }
            }
            //Для каждого региона в верхней половине кисти
            for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
            {
                for (int x = centerX - brushSize; x <= centerX + r; x++)
                {
                    //Если существует регион с такими координатами
                    if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int regionEntity))
                    {
                        //Берём компонент региона
                        ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                        //Редактируем регион
                        RegionEdit(ref region);
                    }
                }
            }
        }

        void RegionEdit(
            ref CHexRegion region)
        {


            if (eUI.Value.gameWindow.activeTerrainTypeIndex >= 0)
            {
                region.TerrainTypeIndex = eUI.Value.gameWindow.activeTerrainTypeIndex;
            }
            if (eUI.Value.gameWindow.applyElevationLevel == true)
            {
                region.Elevation = eUI.Value.gameWindow.activeElevationLevel;
            }
            if (eUI.Value.gameWindow.applyWaterLevel == true)
            {
                region.WaterLevel = eUI.Value.gameWindow.activeWaterLevel;
            }
            if (eUI.Value.gameWindow.applySpecialIndex == true)
            {
                region.SpecialIndex = eUI.Value.gameWindow.activeSpecialIndex;
            }
            if (eUI.Value.gameWindow.applyUrbanLevel == true)
            {
                region.UrbanLevel = eUI.Value.gameWindow.activeUrbanLevel;
            }
            if (eUI.Value.gameWindow.applyFarmLevel == true)
            {
                region.FarmLevel = eUI.Value.gameWindow.activeFarmLevel;
            }
            if (eUI.Value.gameWindow.applyPlantLevel == true)
            {
                region.PlantLevel = eUI.Value.gameWindow.activePlantLevel;
            }
            //Если активен режим удаления рек
            if (eUI.Value.gameWindow.riverMode == OptionalToggle.No)
            {
                //Удаляем реку
                RegionRemoveRiver(
                    ref region);
            }
            //Если активен режим удаления дорог
            if (eUI.Value.gameWindow.roadMode == OptionalToggle.No)
            {
                //Удаляем дороги
                RegionRemoveRoads(
                    ref region);
            }
            //Если режим стен не неактивен
            if (eUI.Value.gameWindow.walledMode != OptionalToggle.Ignore)
            {
                //Устанавливаем стены согласно переключателю
                region.Walled = eUI.Value.gameWindow.walledMode == OptionalToggle.Yes;
            }
            //Если перетаскивание активно
            if (inputData.Value.isDrag == true)
            {
                //Если предыдущий регион существует
                if (region.GetNeighbour(inputData.Value.dragDirection.Opposite()).Unpack(world.Value, out int previousRegionEntity))
                {
                    //Берём компонент предыдущего региона
                    ref CHexRegion previousRegion
                        = ref regionPool.Value.Get(previousRegionEntity);

                    //Если активен режим создания рек
                    if (eUI.Value.gameWindow.riverMode == OptionalToggle.Yes)
                    {
                        //Создаём реку
                        RegionSetOutgoingRiver(
                            ref previousRegion,
                            inputData.Value.dragDirection);
                    }
                    //Иначе, если активен режим создания дорог
                    else if ( eUI.Value.gameWindow.roadMode == OptionalToggle.Yes)
                    {
                        //Создаём дорогу
                        RegionAddRoad(
                            ref previousRegion,
                            inputData.Value.dragDirection);
                    }
                }
            }

            //Запрашиваем триангуляцию чанка
            ChunkRefreshSelfRequest(
                ref region);
        }

        void ValidateDrag(
            ref CHexRegion region)
        {
            //Берём компонент предыдущего региона
            if (inputData.Value.previousRegionPE.Unpack(world.Value, out int previousRegionEntity))
            {
                ref CHexRegion previousRegion
                    = ref regionPool.Value.Get(previousRegionEntity);

                //Для каждого PE соседа
                for (inputData.Value.dragDirection = HexDirection.NE; inputData.Value.dragDirection <= HexDirection.NW; inputData.Value.dragDirection++)
                {
                    //Если сосед - это текущий регион
                    if (previousRegion.GetNeighbour(inputData.Value.dragDirection).EqualsTo(region.selfPE) == true)
                    {
                        //Отмечаем, что перетаскивание активно
                        inputData.Value.isDrag = true;

                        //Выходим из функции
                        return;
                    }
                }
            }

            //Отмечаем, что перетаскивание неактивно
            inputData.Value.isDrag = false;
        }

        void RegionSetOutgoingRiver(
            ref CHexRegion region,
            HexDirection direction)
        {
            //Если река уже существует
            if (region.HasOutgoingRiver == true
                && region.OutgoingRiver == direction)
            {
                //Выходим из функции
                return;
            }

            //Если сосед с этого направления существует
            if (region.GetNeighbour(direction).Unpack(world.Value, out int neighbourRegionEntity))
            {
                //Берём компонент соседа
                ref CHexRegion neighbourRegion
                    = ref regionPool.Value.Get(neighbourRegionEntity);

                //Если высота соседа больше
                if (region.IsValidRiverDestination(ref neighbourRegion) == false)
                {
                    //Выходим из функции
                    return;
                }

                //Удаляем исходяшую реку
                RegionRemoveOutgoingRiver(
                    ref region);
                //Если направление занято входящей рекой
                if (region.HasIncomingRiver == true
                    && region.IncomingRiver == direction)
                {
                    //Удаляем входящую реку
                    RegionRemoveIncomingRiver(
                        ref region);
                }

                //Создаём исходящую реку
                region.HasOutgoingRiver = true;
                region.OutgoingRiver = direction;
                region.SpecialIndex = 0;

                //Создаём входящую реку
                RegionRemoveIncomingRiver(
                    ref neighbourRegion);
                neighbourRegion.HasIncomingRiver = true;
                neighbourRegion.IncomingRiver = direction.Opposite();
                neighbourRegion.SpecialIndex = 0;

                //Удаляем дорогу
                RegionSetRoad(
                    ref region,
                    (int)direction,
                    false);
            }
        }

        void RegionRemoveOutgoingRiver(
            ref CHexRegion region)
        {
            //Если регион имеет исходящую реку
            if (region.HasOutgoingRiver == true)
            {
                //Удаляем исходящую реку
                region.HasOutgoingRiver = false;

                //Берём компонент соседа, куда идёт река
                region.GetNeighbour(region.OutgoingRiver).Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion
                    = ref regionPool.Value.Get(neighbourRegionEntity);

                //Удаляем входящую реку
                neighbourRegion.HasIncomingRiver = false;

                //Запрашиваем триангуляцию чанка
                ChunkRefreshSelfRequest(
                    ref region);
                //Если соседний регион относится к другому чанку
                if (region.parentChunkPE.EqualsTo(in neighbourRegion.parentChunkPE) == false)
                {
                    //Запрашиваем триангуляцию чанка
                    ChunkRefreshSelfRequest(
                        ref neighbourRegion);
                }
            }
        }

        void RegionRemoveIncomingRiver(
            ref CHexRegion region)
        {
            //Если регион имеет исходящую реку
            if (region.HasIncomingRiver == true)
            {
                //Удаляем исходящую реку
                region.HasIncomingRiver = false;

                //Берём компонент соседа, куда идёт река
                region.GetNeighbour(region.IncomingRiver).Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion
                    = ref regionPool.Value.Get(neighbourRegionEntity);

                //Удаляем входящую реку
                neighbourRegion.HasOutgoingRiver = false;

                //Запрашиваем триангуляцию чанка
                ChunkRefreshSelfRequest(
                    ref region);
                //Если соседний регион относится к другому чанку
                if (region.parentChunkPE.EqualsTo(in neighbourRegion.parentChunkPE) == false)
                {
                    //Запрашиваем триангуляцию чанка
                    ChunkRefreshSelfRequest(
                        ref neighbourRegion);
                }
            }
        }

        void RegionRemoveRiver(
            ref CHexRegion region)
        {
            //Удаляем исходящую и входящую реки
            RegionRemoveOutgoingRiver(
                ref region);
            RegionRemoveIncomingRiver(
                ref region);
        }

        void RegionAddRoad(
            ref CHexRegion region,
            HexDirection direction)
        {
            //Если дорога в данном направлении отсутствует
            if (region.roads[(int)direction] == false
                && region.IsSpecial == false
                //И реки через данное ребро не существует
                && region.HasRiverThroughEdge(direction) == false)
            {
                //Создаём дорогу
                RegionSetRoad(
                    ref region,
                    (int)direction,
                    true);
            }
        }

        void RegionRemoveRoads(
            ref CHexRegion region)
        {
            //Для каждой возможой дороги в регионе
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //Если в данном направлении есть дорога
                if (region.roads[a] == true)
                {
                    //Удаляем дорогу
                    RegionSetRoad(
                        ref region,
                        a,
                        false);
                }
            }
        }

        void RegionSetRoad(
            ref CHexRegion region,
            int index,
            bool state)
        {
            //Задаём дороге нужное состояние
            region.roads[index] = state;

            //Берём компонент соседа с данного направления
            region.neighbourRegionPEs[index].Unpack(world.Value, out int neighbourRegionEntity);
            ref CHexRegion neighbourRegion
                = ref regionPool.Value.Get(neighbourRegionEntity);

            //Задаём дороге нужное состояние
            neighbourRegion.roads[(int)((HexDirection)index).Opposite()] = state;

            //Запрашиваем триангуляцию чанка
            ChunkRefreshSelfRequest(
                ref region);
            //Если соседний регион относится к другому чанку
            if (region.parentChunkPE.EqualsTo(in neighbourRegion.parentChunkPE) == false)
            {
                //Запрашиваем триангуляцию чанка
                ChunkRefreshSelfRequest(
                    ref neighbourRegion);
            }
        }

        void ChunkRefreshSelfRequest(
            ref CHexRegion region)
        {
            //Берём компонент родительского чанка региона
            region.parentChunkPE.Unpack(world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            //Если ещё не существует самозапроса обновления чанка
            if (mapChunkRefreshSelfRequestPool.Value.Has(parentChunkEntity) == false)
            {
                //Назначаем сущности самозапрос обновления чанка
                mapChunkRefreshSelfRequestPool.Value.Add(parentChunkEntity);
            }

            //Для каждого соседа региона
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //Если сосед существует 
                if (region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //Берём компонент соседа
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Берём сущность родительского чанка региона
                    neighbourRegion.parentChunkPE.Unpack(world.Value, out int neighbourParentChunkEntity);

                    //Если ещё не существует самозапроса обновления чанка
                    if (mapChunkRefreshSelfRequestPool.Value.Has(neighbourParentChunkEntity) == false)
                    {
                        //Назначаем сущности самозапрос обновления чанка
                        mapChunkRefreshSelfRequestPool.Value.Add(neighbourParentChunkEntity);
                    }
                }
            }
        }

        void QuitGameEvent()
        {
            //Создаём новую сущность и назначаем ей компонент события выхода из игры
            int eventEntity = world.Value.NewEntity();
            ref EQuitGame quitGameEvent
                = ref quitGameEventPool.Value.Add(eventEntity);
        }

        void FindPath(
            ref CHexRegion fromCell, ref CHexRegion toCell)
        {
            if (inputData.Value.searchFrontier == null)
            {
                inputData.Value.searchFrontier = new();
            }
            else
            {
                inputData.Value.searchFrontier.Clear();
            }

            foreach (int cellEntity
                in regionFilter.Value)
            {
                //Берём компонент ячейки
                ref CHexRegion currentCell
                    = ref regionPool.Value.Get(cellEntity);

                //Очищаем ячейку
                currentCell.SearchPhase = 0;
                currentCell.SetLabel(null);
                currentCell.DisableHighlight();
            }

            //Выводим стартовую ячейку за границу поиска
            fromCell.SearchPhase = 2;
            fromCell.Distance = 0;
            fromCell.EnableHighlight(
                Color.blue);

            //Берём сущность искомой начальной ячейки
            fromCell.selfPE.Unpack(world.Value, out int fromCellEntity);

            //Заносим её первой в очередь
            inputData.Value.searchFrontier.Enqueue(
                fromCell.selfPE,
                0);

            while (inputData.Value.searchFrontier.Count > 0)
            {
                //Берём компонент текущей ячейки
                inputData.Value.searchFrontier.Dequeue().cellPE.Unpack(world.Value, out int currentCellEntity);
                ref CHexRegion currentCell
                    = ref regionPool.Value.Get(currentCellEntity);

                //Увеличиваем фазу поиска ячейки
                currentCell.SearchPhase += 1;

                //Если текущая ячейка совпадает с искомой
                if (currentCell.selfPE.EqualsTo(toCell.selfPE) == true)
                {
                    //Берём ссылку на предыдущую ячейку
                    //currentCell.PathFromPE.Unpack(world.Value, out int pathFromCellEntity);
                    //ref CHexCell pathFromCell = ref cellPool.Value.Get(pathFromCellEntity);

                    //Пока предыдущая ячейка - не стартовая ячейка
                    while (currentCell.selfPE.EqualsTo(fromCell.selfPE) == false)
                    {
                        currentCell.SetLabel(currentCell.Distance.ToString());

                        currentCell.EnableHighlight(
                            Color.white);

                        //Берём компонент предыдущей ячейки
                        currentCell.PathFromPE.Unpack(world.Value, out currentCellEntity);
                        currentCell = ref regionPool.Value.Get(currentCellEntity);
                    }

                    toCell.EnableHighlight(
                        Color.red);

                    //Выходим из цикла
                    break;
                }

                //Для каждой соседней ячейки
                for(HexDirection a = HexDirection.NE; a <= HexDirection.NW; a++)
                {
                    //Если ячейка существует
                    if (currentCell.GetNeighbour(a).Unpack(world.Value, out int neighbourCellEntity))
                    {
                        //Берём компонент соседней ячейки
                        ref CHexRegion neighbourCell
                            = ref regionPool.Value.Get(neighbourCellEntity);

                        //Если ячейка находится за границей поиска
                        if (neighbourCell.SearchPhase > 2)
                        {
                            //Переходим к следующей
                            continue;
                        }

                        //Определяем тип ребра
                        HexEdgeType edgeType = SpaceGenerationData.GetEdgeType(currentCell.Elevation, neighbourCell.Elevation);

                        //Если ячейка находится под водой
                        if (neighbourCell.IsUnderwater == true
                            //Или тип ребра - обрыв
                            || edgeType == HexEdgeType.Cliff
                            //Или дорогу преграждает стена
                            || currentCell.Walled != neighbourCell.Walled)
                        {
                            //Переходим к следующей ячейке
                            continue;
                        }

                        int distance = currentCell.Distance;

                        if (currentCell.HasRoadThroughEdge(a))
                        {
                            distance += 1;
                        }
                        else
                        {
                            distance += edgeType == HexEdgeType.Flat ? 5 : 10;

                            distance += neighbourCell.UrbanLevel + neighbourCell.FarmLevel + neighbourCell.PlantLevel;
                        }

                        //Если расстояние до соседа ещё не было указано
                        if (neighbourCell.SearchPhase < 2)
                        {
                            //Выводим ячейку за границу поиска
                            neighbourCell.SearchPhase = 2;

                            //Указываем расстояние до соседа
                            neighbourCell.Distance = distance;

                            neighbourCell.PathFromPE = currentCell.selfPE;

                            neighbourCell.SearchHeuristic = neighbourCell.coordinates.DistanceTo(
                                toCell.coordinates);

                            //Заносим его сущность в очередь
                            inputData.Value.searchFrontier.Enqueue(
                                neighbourCell.selfPE,
                                neighbourCell.SearchPriority);
                        }
                        //Иначе, если расстояние до соседа больше текущего
                        else if(distance < neighbourCell.Distance)
                        {
                            //Сохраняем приоритет
                            int oldPriority = neighbourCell.SearchPriority;

                            //Обновляем расстояние до него
                            neighbourCell.Distance = distance;

                            neighbourCell.PathFromPE = currentCell.selfPE;

                            inputData.Value.searchFrontier.Change(
                                neighbourCell.selfPE, 
                                oldPriority,
                                neighbourCell.SearchPriority);
                        }
                    }
                }
            }
        }
    }
}