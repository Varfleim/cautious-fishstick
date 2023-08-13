
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
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        //Корабли
        //readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

        //readonly EcsPoolInject<CSGMoving> sGMovingPool = default;


        //События главного меню
        readonly EcsPoolInject<RMainMenuAction> mainMenuActionRequestPool = default;

        //События меню новой игры
        readonly EcsPoolInject<RNewGameMenuAction> newGameMenuActionRequestPool = default;

        //События меню загрузки

        //События мастерской
        readonly EcsPoolInject<RWorkshopAction> workshopActionRequestPool = default;

        //События дизайнера
        readonly EcsPoolInject<RDesignerAction> designerActionRequestPool = default;

        //События дизайнера кораблей
        readonly EcsPoolInject<RDesignerShipClassAction> designerShipClassActionRequestPool = default;
        //События дизайнера компонентов
        readonly EcsPoolInject<RDesignerComponentAction> designerComponentActionRequestPool = default;

        //События игры
        readonly EcsPoolInject<RGameAction> gameActionRequestPool = default;

        readonly EcsPoolInject<RGameOpenDesigner> gameOpenDesignerRequestPool = default;

        readonly EcsPoolInject<RGameDisplayObjectPanel> gameDisplayObjectPanelRequestPool = default;

        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //События административно-экономических объектов
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        //События карты


        //Общие события
        EcsFilter clickEventSpaceFilter;
        EcsPool<EcsUguiClickEvent> clickEventSpacePool;
        
        EcsFilter clickEventUIFilter;
        EcsPool<EcsUguiClickEvent> clickEventUIPool;

        EcsFilter dropdownEventUIFilter;
        EcsPool<EcsUguiTmpDropdownChangeEvent> dropdownEventUIPool;

        EcsFilter sliderEventUIFilter;
        EcsPool<EcsUguiSliderChangeEvent> sliderEventUIPool;

        readonly EcsPoolInject<RQuitGame> quitGameRequestPool = default;


        //Данные
        readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<ContentData> contentData = default;
        //readonly EcsCustomInject<DesignerData> designerData = default;
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;
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
                if (eUI.Value.activeMainWindowType == MainWindowType.Game)
                {
                    //Берём ссылку на окно игры
                    UIGameWindow gameWindow = eUI.Value.gameWindow;

                    //Берём компонент организации игрока
                    inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
                    ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

                    //Если запрашивается открытие дизайнера кораблей
                    if (clickEvent.WidgetName == "OpenShipClassDesigner")
                    {
                        //Запрашиваем открытие дизайнера кораблей
                        GameOpenDesignerRequest(
                            DesignerType.ShipClass,
                            playerOrganization.contentSetIndex);
                    }
                    //Иначе, если запрашивается открытие дизайнера двигателей
                    else if (clickEvent.WidgetName == "OpenEngineDesigner")
                    {
                        //Запрашиваем открытие дизайнера двигателей
                        GameOpenDesignerRequest(
                            DesignerType.ComponentEngine,
                            playerOrganization.contentSetIndex);
                    }
                    //Иначе, если запрашивается открытие дизайнера реакторов
                    else if (clickEvent.WidgetName == "OpenReactorDesigner")
                    {
                        //Запрашиваем открытие дизайнера реакторов
                        GameOpenDesignerRequest(
                            DesignerType.ComponentReactor,
                            playerOrganization.contentSetIndex);
                    }
                    //Иначе, если запрашивается открытие дизайнера топливных баков
                    else if (clickEvent.WidgetName == "OpenFuelTankDesigner")
                    {
                        //Запрашиваем открытие дизайнера топливных баков
                        GameOpenDesignerRequest(
                            DesignerType.ComponentHoldFuelTank,
                            playerOrganization.contentSetIndex);
                    }
                    //Иначе, если запрашивается открытие дизайнера оборудования для твёрдой добычи
                    else if (clickEvent.WidgetName == "OpenExtractionEquipmentSolidDesigner")
                    {
                        //Запрашиваем открытие дизайнера оборудования для твёрдой добычи
                        GameOpenDesignerRequest(
                            DesignerType.ComponentExtractionEquipmentSolid,
                            playerOrganization.contentSetIndex);
                    }
                    //Иначе, если запрашивается открытие дизайнера энергетических орудий
                    else if (clickEvent.WidgetName == "OpenEnergyGunDesigner")
                    {
                        //Запрашиваем открытие дизайнера энергетических орудий
                        GameOpenDesignerRequest(
                            DesignerType.ComponentGunEnergy,
                            playerOrganization.contentSetIndex);
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
                                GameDisplayObjectPanelRequest(
                                    DisplayObjectPanelRequestType.RegionOverview,
                                    objectPanel.activeObjectPE);
                            }
                            //Иначе, если нажата кнопка вкладки организаций
                            else if (clickEvent.WidgetName == "RegionOrganizationsTab")
                            {
                                //Запрашиваем отображение вкладки организаций
                                GameDisplayObjectPanelRequest(
                                    DisplayObjectPanelRequestType.RegionOrganizations,
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
                                    GameDisplayObjectPanelRequest(
                                        DisplayObjectPanelRequestType.ORAEO,
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
                                GameDisplayObjectPanelRequest(
                                    DisplayObjectPanelRequestType.ORAEOOverview,
                                    objectPanel.activeObjectPE);
                            }
                        }
                    }
                }
                //Если активно окно главного меню
                if (eUI.Value.activeMainWindowType == MainWindowType.MainMenu)
                {
                    //Если запрашивается открытие меню новой игры
                    if (clickEvent.WidgetName == "OpenNewGameMenu")
                    {
                        //Создаём запрос открытия окна новой игры
                        MainMenuActionRequest(MainMenuActionType.OpenNewGameMenu);
                    }
                    //Иначе, если запрашивается открытие меню загрузки игры
                    else if (clickEvent.WidgetName == "OpenLoadGameMenu")
                    {
                        //Создаём запрос открытия окна загрузки игры
                        MainMenuActionRequest(MainMenuActionType.OpenLoadGameMenu);
                    }
                    //Иначе, если запрашивается открытие окна мастерской
                    else if (clickEvent.WidgetName == "OpenWorkshop")
                    {
                        //Создаём запроса открытия окна мастерской
                        MainMenuActionRequest(MainMenuActionType.OpenWorkshop);
                    }
                    //Иначе, если запрашивается открытие окна главных настроек
                    else if (clickEvent.WidgetName == "OpenSettings")
                    {
                        //Создаём запрос открытия окна главных настроек
                        MainMenuActionRequest(MainMenuActionType.OpenMainSettings);
                    }
                    //Иначе, если запрашивается выход из игры
                    else if (clickEvent.WidgetName == "QuitGame")
                    {
                        //Создаём запрос выхода из игры
                        QuitGameRequest();
                    }
                }
                //Если активно меню новой игры
                else if (eUI.Value.activeMainWindowType == MainWindowType.NewGameMenu)
                {
                    //Берём ссылку на окно меню новой игры
                    UINewGameMenuWindow newGameMenuWindow = eUI.Value.newGameMenuWindow;

                    //Если запрашивается начало новой игры
                    if (clickEvent.WidgetName == "StartNewGame")
                    {
                        //Создаём запрос начала новой игры
                        NewGameMenuActionRequest(NewGameMenuActionType.StartNewGame);
                    }
                }
                //Если активна мастерская
                else if (eUI.Value.activeMainWindowType == MainWindowType.Workshop)
                {
                    //Берём ссылку на окно мастерской
                    UIWorkshopWindow workshopWindow = eUI.Value.workshopWindow;

                    //Если название события отсутствует
                    if (clickEvent.WidgetName == "")
                    {
                        //Пытаемся получить панель набора контента
                        if (clickEvent.Sender.TryGetComponent(out UIWorkshopContentSetPanel workshopContentSetPanel))
                        {
                            //Запрашиваем отображение выбранного набора контента
                            WorkshopActionRequest(
                                WorkshopActionType.DisplayContentSet,
                                workshopContentSetPanel.contentSetIndex);
                        }
                    }
                    //Иначе, если событие запрашивает открытие дизайнера контента
                    else if (clickEvent.WidgetName == "WorkshopOpenDesigner")
                    {
                        //Берём активный переключатель в списке контента
                        Toggle activeToggle = workshopWindow.contentInfoToggleGroup.GetFirstActiveToggle();

                        //Если он не пуст
                        if (activeToggle != null)
                        {
                            //Пытаемся получить панель выбранного вида контента
                            if (activeToggle.TryGetComponent(out UIWorkshopContentInfoPanel workshopContentInfoPanel))
                            {
                                //Запрашиваем отображение выбранного дизайнера в текущем наборе контента
                                WorkshopActionRequest(
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
                            DesignerActionRequest(
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
                                DesignerActionRequest(
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
                                DesignerActionRequest(
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
                                    DesignerActionRequest(
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
                        DesignerActionRequest(
                            DesignerActionType.DisplayContentSetPanel,
                            false);
                    }
                    //Иначе, если событие запрашивает сокрытие панели прочих наборов контента
                    else if (clickEvent.WidgetName
                        == "HideOtherContentSets")
                    {
                        //Запрашиваем сокрытие панели прочих наборов контента
                        DesignerActionRequest(
                            DesignerActionType.HideContentSetPanel,
                            false);
                    }
                    //Иначе, если событие запрашивает отображение панели текущего набора контента
                    else if (clickEvent.WidgetName
                        == "DisplayCurrentContentSet")
                    {
                        //Запрашиваем отображение панели текущего набора контента
                        DesignerActionRequest(
                            DesignerActionType.DisplayContentSetPanel,
                            true);
                    }
                    //Иначе, если событие запрашивает сокрытие панели прочих наборов контента
                    else if (clickEvent.WidgetName
                        == "HideCurrentContentSet")
                    {
                        //Запрашиваем сокрытие панели текущего набора контента
                        DesignerActionRequest(
                            DesignerActionType.HideContentSetPanel,
                            true);
                    }
                    //Иначе, если активен дизайнер кораблей
                    else if (designerWindow.designerType == DesignerType.ShipClass)
                    {
                        //Берём ссылку на окно дизайнера кораблей
                        UIShipDesignerWindow shipClassDesignerWindow = eUI.Value.designerWindow.shipDesigner;

                        //Если событие не имеет подписи
                        if (clickEvent.WidgetName == "")
                        {
                            //Берём активный переключатель из списка доступных компонентов
                            Toggle activeToggleAvailableComponent = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                            //Берём активный переключатель из списка установленных компонентов
                            Toggle activeToggleInstalledComponent = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                            //Если переключатель в списке доступных компонентов не пуст
                            if (activeToggleAvailableComponent != null
                                //И является родительским объектом события
                                && activeToggleAvailableComponent.gameObject == clickEvent.Sender)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleAvailableComponent.TryGetComponent(out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                                {
                                    //Запрашиваем отображение подробной информации о компоненте
                                    DesignerShipClassActionRequest(
                                        DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                        shipClassDesignerWindow.currentAvailableComponentsType,
                                        contentObjectBriefInfoPanel.contentSetIndex,
                                        contentObjectBriefInfoPanel.objectIndex);

                                    //Если переключатель в списке установленных компонентов активен
                                    if (activeToggleInstalledComponent != null)
                                    {
                                        activeToggleInstalledComponent.isOn = false;
                                    }
                                }
                            }
                            //Иначе, если переключатель в списке установленных компонентов не пуст
                            else if (activeToggleInstalledComponent != null
                                //И является родительским объектом события
                                && activeToggleInstalledComponent.gameObject == clickEvent.Sender)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleInstalledComponent.TryGetComponent(out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                                {
                                    //Запрашиваем отображение подробной информации о компоненте
                                    DesignerShipClassActionRequest(
                                        DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                        componentBriefInfoPanel.componentType,
                                        componentBriefInfoPanel.contentSetIndex,
                                        componentBriefInfoPanel.componentIndex);

                                    //Если переключатель в списке доступных компонентов активен
                                    if (activeToggleAvailableComponent != null)
                                    {
                                        activeToggleAvailableComponent.isOn = false;
                                    }
                                }
                            }
                            //Иначе, если родительский объект события имеет компонент Toggle
                            else if (clickEvent.Sender.TryGetComponent(out Toggle eventSenderToggle))
                            {
                                //Если родительской ToggleGroup является список доступных компонентов
                                if (eventSenderToggle.group == shipClassDesignerWindow.availableComponentsListToggleGroup
                                    //ИЛИ если родительской ToggleGroup является список установленных компонентов
                                    || eventSenderToggle.group == shipClassDesignerWindow.installedComponentsListToggleGroup)
                                {
                                    //Запрашиваем сокрытие подробной информации о компоненте
                                    DesignerShipClassActionRequest(DesignerShipClassActionType.HideComponentDetailedInfo);
                                }
                            }
                        }
                        //Если событие запрашивает добавление выбранного компонента в редактируемый корабль
                        if (clickEvent.WidgetName == "AddComponentToShipClass")
                        {
                            //Берём активный переключатель из списка доступных компонентов
                            Toggle activeToggleAvailableComponent = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                            //Если переключатель не пуст
                            if (activeToggleAvailableComponent != null)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleAvailableComponent.TryGetComponent(out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                                {
                                    //Создаём событие добавления компонента
                                    DesignerShipClassActionRequest(
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
                                Toggle activeToggleInstalledComponent = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                                //Если переключатель не пуст
                                if (activeToggleInstalledComponent != null)
                                {
                                    //Пытаемся получить панель выбранного компонента
                                    if (activeToggleInstalledComponent.TryGetComponent(out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                                    {
                                        //Создаём событие добавления компонента
                                        DesignerShipClassActionRequest(
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
                        else if (clickEvent.WidgetName == "DeleteComponentFromShipClass")
                        {
                            //Берём активный переключатель из списка установленных компонентов
                            Toggle activeToggleInstalledComponent = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                            //Если переключатель не пуст
                            if (activeToggleInstalledComponent != null)
                            {
                                //Пытаемся получить панель выбранного компонента
                                if (activeToggleInstalledComponent.TryGetComponent(out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                                {
                                    //Создаём событие удаления компонента
                                    DesignerShipClassActionRequest(
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
                    else if (designerWindow.designerType == DesignerType.ComponentEngine)
                    {

                    }
                    //Иначе, если активен дизайнер реакторов
                    else if (designerWindow.designerType == DesignerType.ComponentReactor)
                    {

                    }
                    //Иначе, если активен дизайнер топливных баков
                    else if (designerWindow.designerType == DesignerType.ComponentHoldFuelTank)
                    {

                    }
                    //Иначе, если активен дизайнер оборудования для твёрдой добычи
                    else if (designerWindow.designerType == DesignerType.ComponentExtractionEquipmentSolid)
                    {

                    }
                    //Иначе, если активен дизайнер энергетических орудий
                    else if (designerWindow.designerType == DesignerType.ComponentGunEnergy)
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
                        DesignerActionRequest(
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
                            DesignerShipClassActionRequest(
                                DesignerShipClassActionType.ChangeAvailableComponentsType,
                                (ShipComponentType)shipDesignerWindow.availableComponentTypeDropdown.value);
                        }
                    }
                    //Иначе, если активен дизайнер двигателей
                    else if (designerWindow.designerType == DesignerType.ComponentEngine)
                    {
                        //Берём ссылку на окно дизайнера двигателей
                        UIEngineDesignerWindow engineDesignerWindow = designerWindow.engineDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей мощность двигателя на единицу размера
                        if (dropdownEvent.WidgetName == "ChangeEnginePowerPerSizeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере двигателей
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер реакторов
                    else if (designerWindow.designerType == DesignerType.ComponentReactor)
                    {
                        //Берём ссылку на окно дизайнера реакторов
                        UIReactorDesignerWindow reactorDesignerWindow = designerWindow.reactorDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей энергию реактора на единицу размера
                        if (dropdownEvent.WidgetName == "ChangeReactorEnergyPerSizeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере реакторов
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер топливных баков
                    else if (designerWindow.designerType == DesignerType.ComponentHoldFuelTank)
                    {
                        //Берём ссылку на окно дизайнера реакторов
                        UIFuelTankDesignerWindow fuelTankDesignerWindow = designerWindow.fuelTankDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей сжатие топливного бака
                        if (dropdownEvent.WidgetName == "ChangeFuelTankCompressionTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере топливных баков
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер оборудования для твёрдой добычи
                    else if (designerWindow.designerType == DesignerType.ComponentExtractionEquipmentSolid)
                    {
                        //Берём ссылку на окно дизайнера добывающего оборудования
                        UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow = designerWindow.extractionEquipmentDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей скорость на единицу размера
                        if (dropdownEvent.WidgetName == "ChangeExtractionEquipmentSolidSpeedPerSizeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере оборудования для твёрдой добычи
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //Иначе, если активен дизайнер энергетических орудий
                    else if (designerWindow.designerType == DesignerType.ComponentGunEnergy)
                    {
                        //Берём ссылку на окно дизайнера энергетических орудий
                        UIGunEnergyDesignerWindow energyGunDesignerWindow = designerWindow.energyGunDesigner;

                        //Если событие запрашивает смену основной технологии, определяющей перезарядку
                        if (dropdownEvent.WidgetName == "ChangeEnergyGunRechargeTechnology")
                        {
                            //Запрашиваем изменение основной технологии в дизайнере энергетических орудий
                            DesignerComponentActionRequest(
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

            //Если активен какой-либо режим карты
            if (inputData.Value.mapMode != MapMode.None)
            {
                //Ввод перемещения камеры
                //InputCameraMoving();

                //Ввод поворота камеры
                CameraInputRotating();

                //Ввод приближения камеры
                CameraInputZoom();
            }

            //Прочий ввод
            InputOther();

            //Ввод мыши
            MouseInput();

            /*if (Input.GetMouseButton(0)
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
            {
                HandleInput();
            }
            else
            {
                //Очищаем предыдущую ячейку
                inputData.Value.previousRegionPE = new();
            }*/

            foreach (int regionEntity in regionFilter.Value)
            {
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

                if (region.Elevation < mapGenerationData.Value.waterLevel)
                {
                    RegionSetColor(ref region, Color.blue);
                }
                else if (region.TerrainTypeIndex == 0)
                {
                    RegionSetColor(ref region, Color.yellow);
                }
                else if (region.TerrainTypeIndex == 1)
                {
                    RegionSetColor(ref region, Color.green);
                }
                else if (region.TerrainTypeIndex == 2)
                {
                    RegionSetColor(ref region, Color.gray);
                }
                else if (region.TerrainTypeIndex == 3)
                {
                    RegionSetColor(ref region, Color.red);
                }
                else if (region.TerrainTypeIndex == 4)
                {
                    RegionSetColor(ref region, Color.white);
                }

                for (int a = 0; a < rAEO.organizationRAEOs.Length; a++)
                {
                    if (rAEO.organizationRAEOs[a].organizationRAEOType == ORAEOType.Economic)
                    {
                        RegionSetColor(ref region, Color.blue);

                        /*List<int> regions = RegionsData.GetRegionIndicesWithinSteps(
                            world.Value,
                            regionFilter.Value, regionPool.Value,
                            ref region, 2);

                        for (int b = 0; b < regions.Count; b++)
                        {
                            RegionsData.regionPEs[regions[b]].Unpack(world.Value, out int neighbourRegionEntity);
                            ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                            RegionSetColor(ref neighbourRegion, Color.blue);
                        }*/
                    }
                }
            }
        }

        //Главное меню
        void MainMenuActionRequest(
            MainMenuActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в главном меню
            int requestEntity = world.Value.NewEntity();
            ref RMainMenuAction mainMenuActionRequest = ref mainMenuActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в главном меню запрашивается
            mainMenuActionRequest.actionType = actionType;
        }

        //Меню новой игры
        void NewGameMenuActionRequest(
            NewGameMenuActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в меню новой игры
            int requestEntity = world.Value.NewEntity();
            ref RNewGameMenuAction newGameMenuActionRequest = ref newGameMenuActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в меню новой игры запрашивается
            newGameMenuActionRequest.actionType = actionType;
        }

        //Меню загрузки
        //

        //Мастерская
        void WorkshopActionRequest(
            WorkshopActionType actionType,
            int contentSetIndex = -1,
            DesignerType designerType = DesignerType.None)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в мастерской
            int requestEntity = world.Value.NewEntity();
            ref RWorkshopAction workshopActionRequest = ref workshopActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в мастерской запрашивается
            workshopActionRequest.actionType = actionType;

            //Указываем индекс набора контента, который требуется отобразить
            workshopActionRequest.contentSetIndex = contentSetIndex;

            //Указываем, какой тип дизайнера требуется открыть
            workshopActionRequest.designerType = designerType;
        }

        //Дизайнер
        void DesignerActionRequest(
            DesignerActionType actionType,
            bool isCurrentContentSet = true, int contentSetIndex = -1,
            int objectIndex = -1)
        {
            //Создаёнм новую сущность и назначаем ей компонент запроса действия в дизайнере
            int requestEntity = world.Value.NewEntity();
            ref RDesignerAction designerActionRequest = ref designerActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в дизайнере запрашивается
            designerActionRequest.actionType = actionType;

            //Указываем, с текущим ли набором контента требуется совершить действие
            designerActionRequest.isCurrentContentSet = isCurrentContentSet;

            //Указываем индекс набора контента
            designerActionRequest.contentSetIndex = contentSetIndex;

            //Указываем индекс объекта
            designerActionRequest.objectIndex = objectIndex;
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
        void DesignerShipClassActionRequest(
            DesignerShipClassActionType actionType,
            ShipComponentType componentType = ShipComponentType.None,
            int contentSetIndex = -1,
            int modelIndex = -1,
            int numberOfComponents = -1)
        {
            //Создаём новую сущность и назначаем ей компонент запроса обработки панели обзора компонента
            int eventEntity = world.Value.NewEntity();
            ref RDesignerShipClassAction designerShipClassActionRequest = ref designerShipClassActionRequestPool.Value.Add(eventEntity);

            //Указываем, какое действие в дизайнере кораблей запрашивается
            designerShipClassActionRequest.actionType = actionType;


            //Указываем тип компонента
            designerShipClassActionRequest.componentType = componentType;


            //Указываем индекс набора контента
            designerShipClassActionRequest.contentSetIndex = contentSetIndex;

            //Указываем индекс модели
            designerShipClassActionRequest.modelIndex = modelIndex;


            //Указываем число компонентов, которое требуется установить/удалить
            designerShipClassActionRequest.numberOfComponents = numberOfComponents;
        }


        void DesignerComponentActionRequest(
            DesignerComponentActionType actionType,
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyIndex)
        {
            //Создаём новую сущность и назначаем ей компонент запроса смены основной технологии
            int eventEntity = world.Value.NewEntity();
            ref RDesignerComponentAction designerComponentActionRequest = ref designerComponentActionRequestPool.Value.Add(eventEntity);

            //Указываем, какое действие в дизайнере компонентов запрашивается
            designerComponentActionRequest.actionType = actionType;

            //Указываем тип модификатора
            designerComponentActionRequest.componentCoreModifierType = componentCoreModifierType;

            //Указываем индекс выбранной технологии
            designerComponentActionRequest.technologyDropdownIndex = technologyIndex;
        }

        //Игра
        void GameActionRequest(
            GameActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в игре
            int requestEntity = world.Value.NewEntity();
            ref RGameAction gameActionRequest = ref gameActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в игре запрашивается
            gameActionRequest.actionType = actionType;
        }

        void GameOpenDesignerRequest(
            DesignerType designerType,
            int contentSetIndex)
        {
            //Создаём новую сущность и назначаем ей компонент запроса открытия дизайнера в игре
            int requestEntity = world.Value.NewEntity();
            ref RGameOpenDesigner gameOpenDesignerRequest = ref gameOpenDesignerRequestPool.Value.Add(requestEntity);

            //Указываем, какой дизайнер требуется открыть
            gameOpenDesignerRequest.designerType = designerType;

            //Указываем, какой набор контента требуется открыть
            gameOpenDesignerRequest.contentSetIndex = contentSetIndex;
        }

        void GameDisplayObjectPanelRequest(
            DisplayObjectPanelRequestType eventType,
            EcsPackedEntity objectPE,
            bool isRefresh = false)
        {
            //Создаём новую сущность и назначаем ей компонент запроса отображения панели объекта
            int requestEntity = world.Value.NewEntity();
            ref RGameDisplayObjectPanel gameDisplayObjectPanelRequest = ref gameDisplayObjectPanelRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            gameDisplayObjectPanelRequest = new(
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


        void CameraInputRotating()
        {
            //Берём Y-вращение
            float yRotationDelta = Input.GetAxis("Horizontal");
            //Если оно не равно нулю, то применяем
            if (yRotationDelta != 0f)
            {
                CameraAdjustYRotation(yRotationDelta);
            }

            //Берём X-вращение
            float xRotationDelta = Input.GetAxis("Vertical");
            //Если оно не равно нулю, то применяем
            if (xRotationDelta != 0f)
            {
                CameraAdjustXRotation(xRotationDelta);
            }
        }

        void CameraAdjustYRotation(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            inputData.Value.rotationAngleY -= rotationDelta * inputData.Value.rotationSpeed * UnityEngine.Time.deltaTime;

            //Выравниваем угол
            if (inputData.Value.rotationAngleY < 0f)
            {
                inputData.Value.rotationAngleY += 360f;
            }
            else if (inputData.Value.rotationAngleY >= 360f)
            {
                inputData.Value.rotationAngleY -= 360f;
            }

            //Применяем вращение
            inputData.Value.mapCamera.localRotation = Quaternion.Euler(
                0f, inputData.Value.rotationAngleY, 0f);
        }
        
        void CameraAdjustXRotation(
            float rotationDelta)
        {
            //Рассчитываем угол вращения
            inputData.Value.rotationAngleX += rotationDelta * inputData.Value.rotationSpeed * UnityEngine.Time.deltaTime;

            //Выравниваем угол
            inputData.Value.rotationAngleX = Mathf.Clamp(
                inputData.Value.rotationAngleX, inputData.Value.minAngleX, inputData.Value.maxAngleX);

            //Применяем вращение
            inputData.Value.swiwel.localRotation = Quaternion.Euler(
                inputData.Value.rotationAngleX, 0f, 0f);
        }

        void CameraInputZoom()
        {
            //Берём вращение колёсика мыши
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");

            //Если оно не равно нулю
            if (zoomDelta != 0)
            {
                //Применяем приближение
                CameraAdjustZoom(zoomDelta);
            }
        }

        void CameraAdjustZoom(
            float zoomDelta)
        {
            //Рассчитываем приближение камеры
            inputData.Value.zoom = Mathf.Clamp01(inputData.Value.zoom + zoomDelta);

            //Рассчитываем расстояние приближения и применяем его
            float zoomDistance = Mathf.Lerp(inputData.Value.stickMinZoom, inputData.Value.stickMaxZoom, inputData.Value.zoom);
            inputData.Value.stick.localPosition = new(0f, 0f, zoomDistance);

            //Рассчитываем поворот приближения и применяем его
            /*float zoomAngle = Mathf.Lerp(inputData.Value.swiwelMinZoom, inputData.Value.swiwelMaxZoom, inputData.Value.zoom);
            inputData.Value.swiwel.localRotation = Quaternion.Euler(zoomAngle, 0f, 0f);*/
        }

        void InputOther()
        {
            //Пауза тиков
            if (Input.GetKeyUp(KeyCode.Space))
            {
                //Если игра активна
                if (runtimeData.Value.isGameActive == true)
                {
                    //Запрашиваем включение паузы
                    GameActionRequest(GameActionType.PauseOn);
                }
                //Иначе
                else
                {
                    //Запрашиваем выключение паузы
                    GameActionRequest(GameActionType.PauseOff);
                }
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                //Если активно окно игры
                if (eUI.Value.activeMainWindowType == MainWindowType.Game)
                {
                    //Создаём запрос выхода из игры
                    QuitGameRequest();
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
                        DesignerActionRequest(
                            DesignerActionType.OpenGame);
                    }
                    //Иначе
                    else
                    {
                        //Запрашиваем открытие окна мастерской
                        DesignerActionRequest(
                            DesignerActionType.OpenWorkshop);
                    }
                }
                //Иначе, если активно окно мастерской
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Workshop)
                {
                    //Запрашиваем открытие окна главного меню
                    WorkshopActionRequest(
                        WorkshopActionType.OpenMainMenu);
                }
                //Иначе, если активно окно меню новой игры
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {
                    //Запрашиваем открытие окна главного меню
                    NewGameMenuActionRequest(
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
            //Если курсор не находится над объектом интерфейса
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
            {
                //Проверяем взаимодействие мыши со сферой
                HexasphereCheckMouseInteraction();

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
                        /*if (mapObject.objectType == MapObjectType.ShipGroup)
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
                        }*/
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
            //Иначе
            else
            {
                //Курсор точно не находится над гексасферой
                InputData.isMouseOver = false;
            }
        }

        void HandleInput()
        {
            //Берём регион под курсором
            /*if (GetRegionUnderCursor().Unpack(world.Value, out int regionEntity))
            {
                //Берём компонент региона
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
                        //Если стартовый регион задан
                        if (inputData.Value.searchFromRegion.Unpack(world.Value, out int fromRegionEntity))
                        {
                            //Берём компонент стартового региона
                            ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                            //Отключаем подсветку стартового региона
                            fromRegion.DisableHighlight();
                        }

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
                //Иначе, если стартовый регион задан
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
                    GameDisplayObjectPanelRequest(
                        DisplayObjectPanelRequestType.Region,
                        currentRegion.selfPE);
                }

                //Если какой-либо корабль активен
                //if (inputData.Value.activeShipPE.Unpack(world.Value, out int activeShipEntity))
                //{
                //    //Берём компонент корабля
                //    ref CShip activeShip
                //        = ref shipPool.Value.Get(activeShipEntity);

                //    //Если корабль находится в режиме ожидания
                //    if (activeShip.shipMode
                //        == ShipMode.Idle)
                //    {
                //        //Переводим корабль в режим поиска пути
                //        activeShip.shipMode
                //            = ShipMode.Pathfinding;

                //        //Указываем целевой объект
                //        activeShip.targetPE
                //            = currentCell.selfPE;

                //        //Указываем тип целевого объекта
                //        activeShip.targetType
                //            = MovementTargetType.Cell;
                //    }
                //}

                //Сохраняем регион как предыдущий для следующего кадра
                inputData.Value.previousRegionPE = currentRegion.selfPE;
            }
            else
            {
                //Очищаем предыдущий регион
                inputData.Value.previousRegionPE = new();
            }*/
        }

        void HexasphereCheckMouseInteraction()
        {
            if (HexasphereCheckMousePosition(out Vector3 position, out Ray ray, out EcsPackedEntity regionPE))
            {
                regionPE.Unpack(world.Value, out int regionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(regionEntity);

                //Если нажата ЛКМ
                if (Input.GetMouseButton(0))
                {
                    //Если нажата кнопка
                    if (Input.GetKey(KeyCode.LeftShift)
                        && inputData.Value.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //Если стартовый регион не совпадает с текущей
                        if (inputData.Value.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                        {
                            //Если стартовый регион задан
                            if (inputData.Value.searchFromRegion.Unpack(world.Value, out int fromRegionEntity))
                            {
                                //Берём компонент стартового региона
                                ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                                //Отключаем подсветку стартового региона
                                RegionSetColor(ref fromRegion, MapGenerationData.DefaultShadedColor);
                            }

                            //Обновляем стартовый регион
                            inputData.Value.searchFromRegion = currentRegion.selfPE;

                            //Если конечный регион задан
                            /*if (inputData.Value.searchToRegion.Unpack(world.Value, out int toRegionEntity))
                            {
                                //Берём компонент конечного региона
                                ref CHexRegion toRegion = ref regionPool.Value.Get(toRegionEntity);

                                //Ищем путь
                                FindPath(ref currentRegion, ref toRegion);
                            }*/
                        }
                    }
                    //Иначе, если стартовый регион задан
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

                            //RegionSetColor(ref fromRegion, Color.blue);

                            List<int> steps = RegionsData.FindPath(
                                world.Value,
                                regionFilter.Value, regionPool.Value,
                                ref fromRegion, ref currentRegion);

                            if (steps != null)
                            {
                                for (int a = 0; a < steps.Count; a++)
                                {
                                    RegionsData.regionPEs[steps[a]].Unpack(world.Value, out int region2Entity);
                                    ref CHexRegion region = ref regionPool.Value.Get(region2Entity);

                                    RegionSetColor(ref region, Color.white);
                                }

                                List<int> endRegions = RegionsData.GetRegionIndicesWithinSteps(
                                    world.Value,
                                    regionFilter.Value, regionPool.Value,
                                    ref currentRegion, 5, 5);

                                for (int a = 0; a < endRegions.Count; a++)
                                {
                                    RegionsData.regionPEs[endRegions[a]].Unpack(world.Value, out int endRegionEntity);
                                    ref CHexRegion endRegion = ref regionPool.Value.Get(endRegionEntity);

                                    RegionSetColor(ref endRegion, Color.red);
                                }

                                //RegionSetColor(ref currentRegion, Color.red);
                            }
                        }
                    }
                    else
                    {
                        //Запрашиваем отображение подпанели объекта региона
                        GameDisplayObjectPanelRequest(
                            DisplayObjectPanelRequestType.Region,
                            currentRegion.selfPE);
                    }
                }
            }
        }

        bool HexasphereCheckMousePosition(
            out Vector3 position, out Ray ray,
            out EcsPackedEntity regionPE)
        {
            //Проверяем, находится ли курсор над гексасферой
            InputData.isMouseOver = HexasphereGetHitPoint(out position, out ray);

            regionPE = new();

            //Если курсор находится над гексасферой
            if (InputData.isMouseOver == true)
            {
                //Определяем индекс региона, над которым находится курсор
                int regionIndex = RegionGetInRayDirection(
                    ray, position,
                    out Vector3 hitPosition);

                //Если индекс региона больше нуля
                if (regionIndex >= 0)
                {
                    //Берём регион
                    RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //Обновляем индекс последнего региона, над которым находился курсор
                    InputData.lastHoverRegionIndex = region.Index;

                    //Если индекс региона не равен индексу последнего подсвеченного
                    if (region.Index != InputData.lastHighlightedRegionIndex)
                    {
                        //Если индекс последнего подсвеченного региона больше нуля
                        if (InputData.lastHighlightedRegionIndex > 0)
                        {
                            //Скрываем подсветку
                            RegionHideHighlighted();
                        }

                        //Обновляем подсвеченный регион
                        InputData.lastHighlightedRegionPE = region.selfPE;
                        InputData.lastHighlightedRegionIndex = region.Index;

                        //Подсвечиваем регион
                        RegionSetMaterial(
                            regionIndex,
                            mapGenerationData.Value.regionHighlightMaterial,
                            true);
                    }

                    regionPE = region.selfPE;

                    return true;
                }

                //Если индекс региона больше нуля и текущий индекс - не индекс последнего подсвеченного
                /*if (regionIndex >= 0 && regionIndex != InputData.lastHighlightedRegionIndex)
                {
                    //Берём текущий регион
                    RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //Обновляем индекс последнего региона, над которым находился курсор
                    InputData.lastHoverRegionIndex = region.Index;

                    //Если последний подсвеченный регион не пуст
                    if (InputData.lastHighlightedRegionPE.Unpack(world.Value, out int highlightedRegionEntity))
                    {
                        //Берём подсвеченный регион
                        ref CHexRegion highlightedRegion = ref regionPool.Value.Get(highlightedRegionEntity);

                        //Скрываем подсветку
                        RegionHideHighlighted();
                    }

                    //Обновляем текущий подсвеченный регион
                    InputData.lastHighlightedRegionPE = region.selfPE;
                    InputData.lastHighlightedRegionIndex = region.Index;

                    //Подсвечиваем регион
                    RegionSetMaterial(
                        regionIndex,
                        mapGenerationData.Value.regionHighlightMaterial,
                        true);
                }*/
                //Иначе, если индекс региона меньше нуля и индекс последнего подсвеченного региона больше нуля
                else if (regionIndex < 0 && InputData.lastHighlightedRegionIndex >= 0)
                {
                    //Скрываем подсветку
                    RegionHideHighlighted();
                }
            }
            
            return false;
        }

        bool HexasphereGetHitPoint(
            out Vector3 position,
            out Ray ray)
        {
            //Берём луч из положения мыши
            ray = inputData.Value.camera.ScreenPointToRay(Input.mousePosition);

            //Вызываем функцию
            return HexasphereGetHitPoint(ray, out position);
        }

        bool HexasphereGetHitPoint(
            Ray ray,
            out Vector3 position)
        {
            //Если луч касается объекта
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //Если луч касается объекта гексасферы
                if (hit.collider.gameObject == SceneData.HexasphereGO)
                {
                    //Определяем точку касания
                    position = hit.point;

                    //И возвращаем true
                    return true;
                }
            }

            position = Vector3.zero;
            return false;
        }

        int RegionGetInRayDirection(
            Ray ray,
            Vector3 worldPosition,
            out Vector3 hitPosition)
        {
            hitPosition = worldPosition;

            //Определяем итоговую точку касания
            Vector3 minPoint = worldPosition;
            Vector3 maxPoint = worldPosition + 0.5f * MapGenerationData.hexasphereScale * ray.direction;

            float rangeMin = MapGenerationData.hexasphereScale * 0.5f;
            rangeMin *= rangeMin;

            float rangeMax = worldPosition.sqrMagnitude;

            float distance;
            Vector3 bestPoint = maxPoint;

            //Уточняем точку
            for (int a = 0; a < 10; a++)
            {
                Vector3 midPoint = (minPoint + maxPoint) * 0.5f;

                distance = midPoint.sqrMagnitude;

                if (distance < rangeMin)
                {
                    maxPoint = midPoint;
                    bestPoint = midPoint;
                }
                else if (distance > rangeMax)
                {
                    maxPoint = midPoint;
                }
                else
                {
                    minPoint = midPoint;
                }
            }

            //Берём индекс региона 
            int nearestRegionIndex = RegionGetAtLocalPosition(SceneData.HexasphereGO.transform.InverseTransformPoint(worldPosition));
            //Если индекс меньше нуля, выходим из функции
            if (nearestRegionIndex < 0)
            {
                return -1;
            }

            //Определяем индекс региона
            Vector3 currentPoint = worldPosition;

            //Берём ближайший регион
            RegionsData.regionPEs[nearestRegionIndex].Unpack(world.Value, out int nearestRegionEntity);
            ref CHexRegion nearestRegion = ref regionPool.Value.Get(nearestRegionEntity);

            //Определяем верхнюю точку региона
            Vector3 regionTop = SceneData.HexasphereGO.transform.TransformPoint(
                nearestRegion.center * (1.0f + nearestRegion.ExtrudeAmount * MapGenerationData.extrudeMultiplier));

            //Определяем высоту региона и высоту луча
            float regionHeight = regionTop.sqrMagnitude;
            float rayHeight = currentPoint.sqrMagnitude;

            float minDistance = 1e6f;
            distance = minDistance;

            //Определяем индекс региона-кандидата
            int candidateRegionIndex = -1;

            const int NUM_STEPS = 10;
            //Уточняем точку
            for (int a = 1; a <= NUM_STEPS; a++)
            {
                distance = Mathf.Abs(rayHeight - regionHeight);

                //Если расстояние меньше минимального
                if (distance < minDistance)
                {
                    //Обновляем минимальное расстояние и кандидата
                    minDistance = distance;
                    candidateRegionIndex = nearestRegionIndex;
                    hitPosition = currentPoint;
                }

                if (rayHeight < regionHeight)
                {
                    return candidateRegionIndex;
                }

                float t = a / (float)NUM_STEPS;

                currentPoint = worldPosition * (1f - t) + bestPoint * t;

                nearestRegionIndex = RegionGetAtLocalPosition(SceneData.HexasphereGO.transform.InverseTransformPoint(currentPoint));

                if (nearestRegionIndex < 0)
                {
                    break;
                }

                //Обновляем ближайший регион
                RegionsData.regionPEs[nearestRegionIndex].Unpack(world.Value, out nearestRegionEntity);
                nearestRegion = ref regionPool.Value.Get(nearestRegionEntity);

                regionTop = SceneData.HexasphereGO.transform.TransformPoint(
                    nearestRegion.center * (1.0f + nearestRegion.ExtrudeAmount * MapGenerationData.extrudeMultiplier));

                //Определяем высоту региона и высоту луча
                regionHeight = regionTop.sqrMagnitude;
                rayHeight = currentPoint.sqrMagnitude;
            }

            //Если расстояние меньше минимального
            if (distance < minDistance)
            {
                //Обновляем минимальное расстояние и кандидата
                minDistance = distance;
                candidateRegionIndex = nearestRegionIndex;
                hitPosition = currentPoint;
            }

            if (rayHeight < regionHeight)
            {
                return candidateRegionIndex;
            }
            else
            {
                return -1;
            }
        }

        int RegionGetAtLocalPosition(
            Vector3 localPosition)
        {
            //Проверяем, не последний ли это регион
            if (InputData.lastHitRegionIndex >= 0 && InputData.lastHitRegionIndex < RegionsData.regionPEs.Length)
            {
                //Берём последний регион
                RegionsData.regionPEs[InputData.lastHitRegionIndex].Unpack(world.Value, out int lastHitRegionEntity);
                ref CHexRegion lastHitRegion = ref regionPool.Value.Get(lastHitRegionEntity);

                //Определяем расстояние до центра региона
                float distance = Vector3.SqrMagnitude(lastHitRegion.center - localPosition);

                bool isValid = true;

                //Для каждого соседнего региона
                for (int a = 0; a < lastHitRegion.neighbourRegionPEs.Length; a++)
                {
                    //Берём соседний регион
                    lastHitRegion.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //Определяем расстояние до центре региона
                    float otherDistance = Vector3.SqrMagnitude(neighbourRegion.center - localPosition);

                    //Если оно меньше расстояния до последнего региона
                    if (otherDistance < distance)
                    {
                        //Отмечаем это и выходим из цикла
                        isValid = false;
                        break;
                    }
                }

                //Если это последний регион
                if (isValid == true)
                {
                    return InputData.lastHitRegionIndex;
                }
            }
            //Иначе
            else
            {
                //Обнуляем индекс последнего региона
                InputData.lastHitRegionIndex = 0;
            }

            //Следуем кратчайшему пути к минимальному расстоянию
            RegionsData.regionPEs[InputData.lastHitRegionIndex].Unpack(world.Value, out int nearestRegionEntity);
            ref CHexRegion nearestRegion = ref regionPool.Value.Get(nearestRegionEntity);

            float minDist = 1e6f;

            //Для каждого региона
            for (int a = 0; a < RegionsData.regionPEs.Length; a++)
            {
                //Берём ближайший регион 
                RegionGetNearestToPosition(
                    ref nearestRegion.neighbourRegionPEs,
                    localPosition,
                    out float regionDistance).Unpack(world.Value, out int newNearestRegionEntity);

                //Если расстояние меньше минимального
                if (regionDistance < minDist)
                {
                    //Обновляем регион и минимальное расстояние 
                    nearestRegion = ref regionPool.Value.Get(newNearestRegionEntity);

                    minDist = regionDistance;
                }
                //Иначе выходим из цикла
                else
                {
                    break;
                }
            }

            //Индекс последнего региона - это индекс ближайшего
            InputData.lastHitRegionIndex = nearestRegion.Index;

            return InputData.lastHitRegionIndex;
        }

        EcsPackedEntity RegionGetNearestToPosition(
            ref EcsPackedEntity[] regionPEs,
            Vector3 localPosition,
            out float minDistance)
        {
            minDistance = float.MaxValue;

            EcsPackedEntity nearestRegionPE = new();

            //Для каждого региона в массиве
            for (int a = 0; a < regionPEs.Length; a++)
            {
                //Берём регион
                regionPEs[a].Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //Берём центр региона
                Vector3 center = region.center;

                //Рассчитываем расстояние
                float distance 
                    = (center.x - localPosition.x) * (center.x - localPosition.x) 
                    + (center.y - localPosition.y) * (center.y - localPosition.y) 
                    + (center.z - localPosition.z) * (center.z - localPosition.z);

                //Если расстояние меньше минимального
                if (distance < minDistance)
                {
                    //Обновляем регион и минимальное расстояние
                    nearestRegionPE = region.selfPE;
                    minDistance = distance;
                }
            }

            return nearestRegionPE;
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
        }


        bool RegionSetMaterial(
            int regionIndex,
            Material material,
            bool temporary = false)
        {
            //Берём регион
            RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

            //Если назначается временный материал
            if (temporary == true)
            {
                //Если этот временный материал уже назначен региону
                if (region.tempMaterial == material)
                {
                    //То ничего не меняется
                    return false;
                }

                //Назначаем временный материал рендереру региона
                region.renderer.sharedMaterial = material;
                region.renderer.enabled = true;
            }
            //Иначе
            else
            {
                //Если этот основной материал уже назначен региону
                if (region.customMaterial == material)
                {
                    //То ничего не меняется
                    return false;
                }

                //Берём цвет материала
                Color32 materialColor = Color.white;
                if (material.HasProperty(ShaderParameters.Color) == true)
                {
                    materialColor = material.color;
                }
                else if (material.HasProperty(ShaderParameters.BaseColor))
                {
                    materialColor = material.GetColor(ShaderParameters.BaseColor);
                }

                //Отмечаем, что требуется обновление цветов
                mapGenerationData.Value.isColorUpdated = true;

                //Берём текстуру материала
                Texture materialTexture = null;
                if (material.HasProperty(ShaderParameters.MainTex))
                {
                    materialTexture = material.mainTexture;
                }
                else if (material.HasProperty(ShaderParameters.BaseMap))
                {
                    materialTexture = material.GetTexture(ShaderParameters.BaseMap);
                }

                //Если текстура не пуста
                if (materialTexture != null)
                {
                    //Отмечаем, что требуется обновление массива текстур
                    mapGenerationData.Value.isTextureArrayUpdated = true;
                }
                //Иначе
                else
                {
                    List<Color32> colorChunk = MapGenerationData.colorShaded[region.uvShadedChunkIndex];
                    for (int k = 0; k < region.uvShadedChunkLength; k++)
                    {
                        colorChunk[region.uvShadedChunkStart + k] = materialColor;
                    }
                    MapGenerationData.colorShadedDirty[region.uvShadedChunkIndex] = true;
                }
            }

            //Если материал - не материал подсветки
            if (material != mapGenerationData.Value.regionHighlightMaterial)
            {
                //Если это временный материал
                if (temporary == true)
                {
                    region.tempMaterial = material;
                }
                //Иначе
                else
                {
                    region.customMaterial = material;
                }
            }

            //Если материал подсветки не пуст и регион - это последний подсвеченный регион
            if (mapGenerationData.Value.regionHighlightMaterial != null && InputData.lastHighlightedRegionIndex == region.Index)
            {
                //Задаём рендереру материал подсветки
                region.renderer.sharedMaterial = mapGenerationData.Value.regionHighlightMaterial;

                //Берём исходный материал 
                Material sourceMaterial = null;
                if (region.tempMaterial != null)
                {
                    sourceMaterial = region.tempMaterial;
                }
                else if (region.customMaterial != null)
                {
                    sourceMaterial = region.customMaterial;
                }

                //Если исходный материал не пуст
                if (sourceMaterial != null)
                {
                    //Берём цвет исходного материала
                    Color32 color = Color.white;
                    if (sourceMaterial.HasProperty(ShaderParameters.Color) == true)
                    {
                        color = sourceMaterial.color;
                    }
                    else if (sourceMaterial.HasProperty(ShaderParameters.BaseColor))
                    {
                        color = sourceMaterial.GetColor(ShaderParameters.BaseColor);
                    }
                    //Устанавливаем вторичный цвет материалу подсветки
                    mapGenerationData.Value.regionHighlightMaterial.SetColor(ShaderParameters.Color2, color);

                    //Берём текстуру исходного материала
                    Texture tempMaterialTexture = null;
                    if (sourceMaterial.HasProperty(ShaderParameters.MainTex))
                    {
                        tempMaterialTexture = sourceMaterial.mainTexture;
                    }
                    else if (sourceMaterial.HasProperty(ShaderParameters.BaseMap))
                    {
                        tempMaterialTexture = sourceMaterial.GetTexture(ShaderParameters.BaseMap);
                    }

                    //Если текстура не пуста
                    if (tempMaterialTexture != null)
                    {
                        mapGenerationData.Value.regionHighlightMaterial.mainTexture = tempMaterialTexture;
                    }
                }
            }

            return true;
        }

        void RegionRestoreTemporaryMaterial(
            int regionIndex)
        {
            //Берём регион
            RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

            //Восстанавливаем материал рендерера
            RegionRestoreTemporaryMaterial(ref region);
        }

        void RegionRestoreTemporaryMaterial(
            ref CHexRegion region)
        {
            //Восстанавливаем материал рендерера
            region.renderer.sharedMaterial = region.tempMaterial;
        }

        void RegionHideHighlighted()
        {
            //Если подсвеченный регион существует
            if (InputData.lastHighlightedRegionPE.Unpack(world.Value, out int highlightedRegionEntity))
            {
                //Берём регион
                ref CHexRegion highlightedRegion = ref regionPool.Value.Get(highlightedRegionEntity);

                //Если материал рендерера региона - материал подсветки
                if (highlightedRegion.renderer.sharedMaterial == mapGenerationData.Value.regionHighlightMaterial)
                {
                    //Если временный материал региона не пуст
                    if (highlightedRegion.tempMaterial != null)
                    {
                        //Восстанавливаем временный материал
                        RegionRestoreTemporaryMaterial(ref highlightedRegion);
                    }

                    //Отключаем рендерер
                    highlightedRegion.renderer.enabled = false;
                }
            }

            //Очищаем материал подсветки
            ResetHighlightMaterial();

            //Удаляем последний подсвеченный регион
            InputData.lastHighlightedRegionPE = new();
            InputData.lastHighlightedRegionIndex = -1;
        }

        void RegionRefreshHighlighted()
        {
            //Если индекс последнего подсвеченного региона валиден
            if (InputData.lastHighlightedRegionIndex >= 0 && InputData.lastHighlightedRegionIndex < RegionsData.regionPEs.Length)
            {
                //Переустанавливаем материал подсветки
                RegionSetMaterial(
                    InputData.lastHighlightedRegionIndex,
                    mapGenerationData.Value.regionHighlightMaterial,
                    true);
            }
        }

        void ResetHighlightMaterial()
        {
            //Берём цвет материала
            Color color = mapGenerationData.Value.regionHighlightMaterial.color;

            //Обновляем его прозрачность до стандартной
            color.a = 0.2f;

            //Приводим вторичный цвет материала и текстуру к стандарту
            mapGenerationData.Value.regionHighlightMaterial.SetColor(ShaderParameters.Color2, color);
            mapGenerationData.Value.regionHighlightMaterial.mainTexture = null;
        }

        void RegionSetExtrudeAmount(
            ref CHexRegion region,
            float extrudeAmount)
        {
            //Если высота региона уже равна значению, выходим из функции
            if (region.ExtrudeAmount == extrudeAmount)
            {
                return;
            }

            //Ограничиваем высоту
            extrudeAmount = Mathf.Clamp01(extrudeAmount);

            //Обновляем высоту региона
            region.ExtrudeAmount = extrudeAmount;

            //Если регион подсвечен
            if (InputData.lastHighlightedRegionPE.EqualsTo(region.selfPE))
            {
                //Обновляем подсветку региона
                RegionRefreshHighlighted();
            }

            //Берём чанк региона
            List<Vector4> uvShadedChunk = MapGenerationData.uvShaded[region.uvShadedChunkIndex];

            //Для каждых UV-координат региона
            for (int a = 0; a < region.uvShadedChunkLength; a++)
            {
                //Берём координаты
                Vector4 uv4 = uvShadedChunk[region.uvShadedChunkStart + a];

                //Обновляем W-компонет
                uv4.w = region.ExtrudeAmount;

                //Обновляем UV-координаты в чанке
                uvShadedChunk[region.uvShadedChunkStart + a] = uv4;
            }

            //Отмечаем, что UV-координаты требуют обновления
            MapGenerationData.uvShadedDirty[region.uvShadedChunkIndex] = true;
            mapGenerationData.Value.isUVUpdatedFast = true;
        }

        void RegionSetColor(
            ref CHexRegion region,
            Color color)
        {
            //Берём кэшированный материал
            Material material;

            //Если материал такого цвета уже существует в кэше цветных материалов
            if (MapGenerationData.colorCache.ContainsKey(color) == false)
            {
                //То создаём новый материал и кэшируем его
                material = GameObject.Instantiate(mapGenerationData.Value.regionColoredMaterial);
                MapGenerationData.colorCache.Add(color, material);

                //Заполняем основные данные материала
                material.hideFlags = HideFlags.DontSave;
                material.color = color;
                material.SetFloat(ShaderParameters.RegionAlpha, 1f);
            }
            //Иначе
            else
            {
                //Берём материал из словаря
                material = MapGenerationData.colorCache[color];
            }

            //Устанавливаем материал региона
            RegionSetMaterial(
                region.Index,
                material);
        }

        void RegionAddFeature(
            ref CHexRegion region,
            GameObject featureGO)
        {
            //Берём центр ергиона
            Vector3 regionCenter = region.GetCenter();

            //Перемещаем GO в центр региона
            featureGO.transform.position = regionCenter;
            featureGO.transform.SetParent(SceneData.HexasphereGO.transform);
            featureGO.transform.LookAt(SceneData.HexasphereGO.transform);
        }

        void QuitGameRequest()
        {
            //Создаём новую сущность и назначаем ей компонент запроса выхода из игры
            int requestEntity = world.Value.NewEntity();
            ref RQuitGame quitGameRequest = ref quitGameRequestPool.Value.Add(requestEntity);
        }
    }
}