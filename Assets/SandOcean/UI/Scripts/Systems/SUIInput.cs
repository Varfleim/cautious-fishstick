
using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Unity.Ugui;

using SandOcean.UI.Events;
using SandOcean.UI.MainMenu.Events;
using SandOcean.UI.NewGameMenu.Events;
using SandOcean.UI.WorkshopWindow;
using SandOcean.UI.WorkshopWindow.Events;
using SandOcean.UI.DesignerWindow;
using SandOcean.UI.DesignerWindow.Events;
using SandOcean.UI.DesignerWindow.ShipClassDesigner;
using SandOcean.UI.DesignerWindow.ShipClassDesigner.Events;
using SandOcean.UI.GameWindow;
using SandOcean.UI.GameWindow.Object;
using SandOcean.UI.GameWindow.Object.Events;
using SandOcean.UI.GameWindow.Object.FleetManager;
using SandOcean.UI.GameWindow.Object.FleetManager.Events;
using SandOcean.UI.GameWindow.Object.FleetManager.Fleets;
using SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates;
using SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates.Designer;
using SandOcean.UI.GameWindow.Object.Region.ORAEOs;
using SandOcean.Player;
using SandOcean.Organization;
using SandOcean.Map;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;
using SandOcean.Designer;
using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.Fleet.Moving;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.Ship;
using SandOcean.Warfare.TaskForce;

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

        //Карта
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        //Флоты
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsPoolInject<CFleetDisplayedSummaryPanel> fleetDisplayedSummaryPanelPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;

        readonly EcsPoolInject<CTaskForcePMissionSearch> taskForcePatrolMissionSearchPool = default;
        readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> taskForceTargetMissionStrikeGroupPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceMovement>> taskForceMovementFilter = default;
        readonly EcsPoolInject<CTaskForceMovement> taskForceMovementPool = default;


        //События административно-экономических объектов
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        //События флотов
        readonly EcsPoolInject<RFleetCreating> fleetCreatingRequestPool = default;

        readonly EcsPoolInject<RTaskForceCreating> taskForceCreatingRequestPool = default;
        readonly EcsPoolInject<RTaskForceAction> taskForceActionRequestPool = default;

        readonly EcsPoolInject<RTFTemplateCreating> tFTemplateCreatingRequestPool = default;

        readonly EcsPoolInject<RTFTemplateAction> tFTemplateActionRequestPool = default;

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
        //readonly EcsCustomInject<SceneData> sceneData = default;
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
            //Определяем состояние клавиш
            CheckButtons();

            //Для каждого события клика по интерфейсу
            foreach (int clickEventUIEntity in clickEventUIFilter)
            {
                //Берём событие
                ref EcsUguiClickEvent clickEvent = ref clickEventUIPool.Get(clickEventUIEntity);

                Debug.LogWarning("Click! " + clickEvent.WidgetName);

                //Если активно окно игры
                if (eUI.Value.activeMainWindowType == MainWindowType.Game)
                {
                    //Проверяем клики в окне игры
                    ClickActionGame(ref clickEvent);
                }
                //Иначе, если активно окно главного меню
                else if (eUI.Value.activeMainWindowType == MainWindowType.MainMenu)
                {
                    //Проверяем клики в окне главного меню
                    ClickActionMainMenu(ref clickEvent);
                }
                //Если активно меню новой игры
                else if (eUI.Value.activeMainWindowType == MainWindowType.NewGameMenu)
                {
                    //Проверяем клики в окне меню новой игры
                    ClickActionNewGameMenu(ref clickEvent);
                }
                //Если активна мастерская
                else if (eUI.Value.activeMainWindowType == MainWindowType.Workshop)
                {
                    //Проверяем клики в окне мастерской
                    ClickActionWorkshop(ref clickEvent);
                }
                //Если активен дизайнер
                else if (eUI.Value.activeMainWindowType == MainWindowType.Designer)
                {
                    //Проверяем клики в окне дизайнера
                    ClickActionDesigner(ref clickEvent);
                }

                uguiUIWorld.DelEntity(clickEventUIEntity);
            }

            //Для каждого события изменения значения выпадающего списка
            foreach (int dropdownEventUIEntity in dropdownEventUIFilter)
            {
                //Берём компонент события
                ref EcsUguiTmpDropdownChangeEvent dropdownEvent = ref dropdownEventUIPool.Get(dropdownEventUIEntity);

                Debug.LogWarning("Dropdown change! " + dropdownEvent.WidgetName + " ! " + dropdownEvent.Value);

                //Если активен дизайнер
                if (eUI.Value.activeMainWindowType == MainWindowType.Designer)
                {
                    //Берём ссылку на окно дизайнера
                    UIDesignerWindow designerWindow = eUI.Value.designerWindow;

                    //Если событие запрашивает смену набора контента на панели прочих наборов контента
                    if (dropdownEvent.WidgetName == "ChangeOtherContentSet")
                    {
                        //Определяем истинный индекс набора контента
                        int contentSetIndex = -1;

                        //Если индекс выпадающего списка меньше, чем индекс текущего набора контента
                        if (dropdownEvent.Value < designerWindow.currentContentSetIndex)
                        {
                            //То настоящий индекс соответствует индексу из списка
                            contentSetIndex = dropdownEvent.Value;
                        }
                        //Иначе
                        else
                        {
                            //Настоящий индекс больше на 1
                            contentSetIndex = dropdownEvent.Value + 1;
                        }

                        //Запрашиваем смену набора контента на панели прочих наборов контента
                        DesignerActionRequest(
                            DesignerActionType.DisplayContentSetPanelList,
                            false,
                            contentSetIndex);
                    }
                    //Иначе, если активен дизайнер кораблей
                    else if (designerWindow.designerType == DesignerType.ShipClass)
                    {
                        //Берём ссылку на окно дизайнера кораблей
                        UIShipClassDesignerWindow shipDesignerWindow
                            = designerWindow.shipClassDesigner;

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
            foreach (int sliderEventUIEntity in sliderEventUIFilter)
            {
                //Берём компонент события
                ref EcsUguiSliderChangeEvent
                    sliderEvent
                    = ref sliderEventUIPool.Get(sliderEventUIEntity);

                Debug.LogWarning("Slider change! " + sliderEvent.WidgetName + " ! " + sliderEvent.Value);

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

                Debug.LogWarning("Click! " + clickEvent.WidgetName);

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

            foreach (int regionEntity in regionFilter.Value)
            {
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

                RegionSetColor(ref region, MapGenerationData.DefaultShadedColor);

                /*if (region.Elevation < mapGenerationData.Value.waterLevel)
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
                }*/

                if (region.taskForcePEs.Count > 0)
                {
                    RegionSetColor(ref region, Color.blue);
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

            foreach (int taskForceEntity in taskForceMovementFilter.Value)
            {
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool.Value.Get(taskForceEntity);

                //Берём компонент текущего региона
                taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                RegionSetColor(ref currentRegion, Color.blue);

                //Для каждого региона в маршруте
                for (int a = 0; a < tFMovement.pathRegionPEs.Count; a++)
                {
                    tFMovement.pathRegionPEs[a].Unpack(world.Value, out int regionEntity);
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    RegionSetColor(ref region, Color.red);
                }

                tFMovement.pathRegionPEs[0].Unpack(world.Value, out currentRegionEntity);
                currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                RegionSetColor(ref currentRegion, Color.magenta);
            }
        }

        #region MainMenu
        void ClickActionMainMenu(
            ref EcsUguiClickEvent clickEvent)
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

        readonly EcsPoolInject<RMainMenuAction> mainMenuActionRequestPool = default;
        void MainMenuActionRequest(
            MainMenuActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в главном меню
            int requestEntity = world.Value.NewEntity();
            ref RMainMenuAction mainMenuActionRequest = ref mainMenuActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в главном меню запрашивается
            mainMenuActionRequest.actionType = actionType;
        }
        #endregion

        #region NewGameMenu
        void ClickActionNewGameMenu(
            ref EcsUguiClickEvent clickEvent)
        {
            //Если запрашивается начало новой игры
            if (clickEvent.WidgetName == "StartNewGame")
            {
                //Создаём запрос начала новой игры
                NewGameMenuActionRequest(NewGameMenuActionType.StartNewGame);
            }
        }

        readonly EcsPoolInject<RNewGameMenuAction> newGameMenuActionRequestPool = default;
        void NewGameMenuActionRequest(
            NewGameMenuActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в меню новой игры
            int requestEntity = world.Value.NewEntity();
            ref RNewGameMenuAction newGameMenuActionRequest = ref newGameMenuActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в меню новой игры запрашивается
            newGameMenuActionRequest.actionType = actionType;
        }
        #endregion

        #region Workshop
        void ClickActionWorkshop(
            ref EcsUguiClickEvent clickEvent)
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

        readonly EcsPoolInject<RWorkshopAction> workshopActionRequestPool = default;
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
        #endregion

        #region Designer
        void ClickActionDesigner(
            ref EcsUguiClickEvent clickEvent)
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
                UIShipClassDesignerWindow shipClassDesignerWindow = eUI.Value.designerWindow.shipClassDesigner;

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

        readonly EcsPoolInject<RDesignerAction> designerActionRequestPool = default;
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
                else if (designerWindow.designerType
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

        readonly EcsPoolInject<RDesignerShipClassAction> designerShipClassActionRequestPool = default;
        void DesignerShipClassActionRequest(
            DesignerShipClassActionType actionType,
            ShipComponentType componentType = ShipComponentType.None,
            int contentSetIndex = -1,
            int modelIndex = -1,
            int numberOfComponents = -1)
        {
            //Создаём новую сущность и назначаем ей компонент запроса обработки панели обзора компонента
            int requestEntity = world.Value.NewEntity();
            ref RDesignerShipClassAction requestComp = ref designerShipClassActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в дизайнере кораблей запрашивается
            requestComp.actionType = actionType;


            //Указываем тип компонента
            requestComp.componentType = componentType;


            //Указываем индекс набора контента
            requestComp.contentSetIndex = contentSetIndex;

            //Указываем индекс модели
            requestComp.modelIndex = modelIndex;


            //Указываем число компонентов, которое требуется установить/удалить
            requestComp.numberOfComponents = numberOfComponents;
        }

        readonly EcsPoolInject<RDesignerComponentAction> designerComponentActionRequestPool = default;
        void DesignerComponentActionRequest(
            DesignerComponentActionType actionType,
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyIndex)
        {
            //Создаём новую сущность и назначаем ей компонент запроса смены основной технологии
            int requestEntity = world.Value.NewEntity();
            ref RDesignerComponentAction requestComp = ref designerComponentActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в дизайнере компонентов запрашивается
            requestComp.actionType = actionType;

            //Указываем тип модификатора
            requestComp.componentCoreModifierType = componentCoreModifierType;

            //Указываем индекс выбранной технологии
            requestComp.technologyDropdownIndex = technologyIndex;
        }
        #endregion

        #region Game
        void ClickActionGame(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Берём организацию игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //ТЕСТ
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
            //Иначе, если запрашивается открытие менеджера флотов
            else if (clickEvent.WidgetName == "OpenFleetManager")
            {
                //Запрашиваем открытие подпанели менеджера флотов
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.FleetManager,
                    playerOrganization.selfPE);
            }
            //ТЕСТ

            //Если активна панель объекта
            if (gameWindow.activeMainPanelType == MainPanelType.Object)
            {
                //Проверяем клики в панели объекта
                ClickActionGameObject(ref clickEvent);
            }
        }

        #region GameObject
        void ClickActionGameObject(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём организацию игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //ТЕСТ
            //Если нажата кнопка закрытия панели объекта
            if (clickEvent.WidgetName == "CloseObjectPanel")
            {
                //Запрашиваем закрытие панели объекта
                GameObjectPanelActionRequest(ObjectPanelActionRequestType.CloseObjectPanel);
            }
            //ТЕСТ

            //Иначе, если активна подпанель менеджера флотов
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
            {
                //Проверяем клики в подпанели менеджера флотов
                ClickActionGameObjectFleetManager(ref clickEvent);
            }
            //Иначе, если активна подпанель организации
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
            {
                //Проверяем клики в подпанели организации
                ClickActionGameObjectOrganization(ref clickEvent);
            }
            //Иначе, если активна подпанель региона
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
            {
                //Проверяем клики в подпанели региона
                ClickActionGameObjectRegion(ref clickEvent);
            }
            //Иначе, если активна подпанель ORAEO
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
            {
                //Проверяем клики в подпанели ORAEO
                ClickActionGameObjectORAEO(ref clickEvent);
            }
        }

        #region GameObjectFleetManager
        void ClickActionGameObjectFleetManager(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём организацию игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //Если нажата кнопка вкладки флотов
            if (clickEvent.WidgetName == "FleetManagerFleetsTab")
            {
                //Запрашиваем отображение вкладки флотов
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.FleetManagerFleets,
                    objectPanel.activeObjectPE);
            }
            //Иначе, если нажата кнопка вкладки шаблонов оперативных групп
            else if (clickEvent.WidgetName == "FleetManagerTaskForceTemplatesTab")
            {
                //Запрашиваем отображение вкладки шаблонов групп
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.FleetManagerTaskForceTemplates,
                    objectPanel.activeObjectPE);
            }

            //Иначе, если активна вкладка флотов
            else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.fleetsTab.selfTabButton)
            {
                //Проверяем клики во вкладке флотов
                ClickActionGameObjectFleetManagerFleets(ref clickEvent);
            }
            //Иначе, если активна вкладка шаблонов групп
            else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.taskForceTemplatesTab.selfTabButton)
            {
                //Проверяем клики во вкладке шаблонов групп
                ClickActionGameObjectFleetManagerTaskForceTemplates(ref clickEvent);
            }
        }

        void ClickActionGameObjectFleetManagerFleets(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём организацию игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //Берём вкладку флотов
            UIFleetsTab fleetsTab = fleetManagerSubpanel.fleetsTab;

            //Проверяем, не пуст ли список активных оперативных групп
            bool isActiveTaskForces = InputData.activeTaskForcePEs.Count > 0;

            //Если список активных оперативных групп не пуст
            //И нажата кнопка миссии поиска
            if (isActiveTaskForces == true
                && clickEvent.WidgetName == "SearchMissionFM")
            {
                //Для каждой активной оперативной группы
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //Берём оперативную группу
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Если группа не имеет компонента миссии поиска
                    if (taskForcePatrolMissionSearchPool.Value.Has(taskForceEntity) == false)
                    {
                        //Запрашиваем изменение миссии группы на миссию поиска
                        TaskForceActionRequest(
                            TaskForceActionType.ChangeMissionSearch,
                            ref taskForce);
                    }
                }
            }
            //Иначе, если список активных оперативных групп не пуст
            //И нажата кнопка второй миссии
            else if (isActiveTaskForces == true
                && clickEvent.WidgetName == "StrikeGroupMissionFM")
            {
                //Для каждой активной оперативной группы
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //Берём оперативную группу
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Если группа не имеет компонента миссии ударной группы
                    if (taskForceTargetMissionStrikeGroupPool.Value.Has(taskForceEntity) == false)
                    {
                        //Запрашиваем изменение миссии группы на вторую миссию
                        TaskForceActionRequest(
                            TaskForceActionType.ChangeMissionStrikeGroup,
                            ref taskForce);
                    }
                }
            }
            //Иначе, если список активных оперативных групп не пуст
            //И нажата кнопка удержания
            else if (isActiveTaskForces == true
                && clickEvent.WidgetName == "HoldMissionFM")
            {
                //Для каждой активной оперативной группы
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //Берём оперативную группу
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Запрашиваем изменение миссии группы на миссию удержания
                    TaskForceActionRequest(
                        TaskForceActionType.ChangeMissionHold,
                        ref taskForce);
                }
            }

            //Иначе, если источник события имеет компонент FleetSummaryPanel
            else if (clickEvent.Sender.TryGetComponent(out UIFleetSummaryPanel fleetSummaryPanel))
            {
                //Для каждой выбранной оперативной группы
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //Берём компонент отображаемой обзорной панели
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel = ref taskForceDisplayedSummaryPanelPool.Value.Get(taskForceEntity);

                    //Делаем её неактивной
                    taskForceDisplayedSummaryPanel.taskForceSummaryPanel.toggle.isOn = false;
                }
                //Очищаем список выбранных оперативных групп
                InputData.activeTaskForcePEs.Clear();

                //Если панель неактивна
                if (fleetSummaryPanel.toggle.isOn == false)
                {
                    //Делаём её активной и отмечаем как активную
                    fleetSummaryPanel.toggle.isOn = true;
                    InputData.activeFleetPE = fleetSummaryPanel.selfPE;

                    //Запрашиваем отображение регионов действия флота

                }
                //Иначе
                else
                {
                    //Делаем её неактивной и отмечаем, что нет активного флота
                    fleetSummaryPanel.toggle.isOn = false;
                    InputData.activeFleetPE = new();

                    //Запрашиваем сокрытие регионов действия флота

                }
            }
            //Иначе, если источник события имеет компонент TaskForceSummaryPanel
            else if (clickEvent.Sender.TryGetComponent(out UITaskForceSummaryPanel taskForceSummaryPanel))
            {
                //Если есть активный флот
                if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                {
                    //Берём компонент отображаемой обзорной панели
                    ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Get(fleetEntity);

                    //Делаем её неактивной
                    fleetDisplayedSummaryPanel.fleetSummaryPanel.toggle.isOn = false;

                    //Отмечаем, что нет активного флота
                    InputData.activeFleetPE = new();
                }

                //Если нажат LeftShift
                if (InputData.leftShiftKeyPressed)
                {
                    //То эта оперативная группа будет добавлена к выбранным или удалена

                    //Если панель неактивна
                    if (taskForceSummaryPanel.toggle.isOn == false)
                    {
                        //Делаем её активной и заносим в список активных
                        taskForceSummaryPanel.toggle.isOn = true;
                        InputData.activeTaskForcePEs.Add(taskForceSummaryPanel.selfPE);
                    }
                    //Иначе
                    else
                    {
                        //Делаем её неактивной и удаляем из списка активных
                        taskForceSummaryPanel.toggle.isOn = false;
                        InputData.activeTaskForcePEs.Remove(taskForceSummaryPanel.selfPE);
                    }
                }
                //Иначе нажата только ЛКМ 
                else
                {
                    //То только эта оперативная группа будет выбрана или удалена

                    //Если панель была активна и в списке было больше одной панели, то очищаем список и активируем панель
                    //Иначе отключаем панель
                    bool isActivation
                        = taskForceSummaryPanel.toggle.isOn && (InputData.activeTaskForcePEs.Count > 1)
                        || (taskForceSummaryPanel.toggle.isOn == false);

                    //Для каждой выбранной оперативной группы
                    for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                    {
                        //Берём компонент отображаемой обзорной панели
                        InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                        ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel = ref taskForceDisplayedSummaryPanelPool.Value.Get(taskForceEntity);

                        //Делаем её неактивной
                        taskForceDisplayedSummaryPanel.taskForceSummaryPanel.toggle.isOn = false;
                    }
                    //Очищаем список выбранных оперативных групп
                    InputData.activeTaskForcePEs.Clear();

                    //Если требуется активировать панель
                    if (isActivation == true)
                    {
                        //Делаем её активной и заносим в список активных
                        taskForceSummaryPanel.toggle.isOn = true;
                        InputData.activeTaskForcePEs.Add(taskForceSummaryPanel.selfPE);
                    }
                }
            }

            //Иначе, если дважды родитель источника события имеет компонент TaskForceSummaryPanel
            else if (clickEvent.Sender.transform.parent.parent.TryGetComponent(
                out UITaskForceSummaryPanel taskForceSummaryPanel2))
            {
                //Если источник события - это кнопка шаблона группы
                if (clickEvent.Sender == taskForceSummaryPanel2.templateSummaryButton.gameObject)
                {
                    //Если список для смены шаблона группы активен
                    if (fleetsTab.templatesChangingList.isActiveAndEnabled == true)
                    {
                        //Запрашиваем заполнение списка для смены шаблона группы
                        GameFleetManagerSubpanelActionRequest(
                            FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesChangeList,
                            objectPanel.activeObjectPE,
                            taskForceSummaryPanel2.selfPE);
                    }
                }
            }
            //Иначе, если источник события имеет компонент TemplateSummaryPanel
            else if (clickEvent.Sender.TryGetComponent(
                out UITFTemplateSummaryPanel templateSummaryPanel))
            {
                //Если родительским списком панели является список для смены шаблона группы
                if(templateSummaryPanel.parentList == fleetsTab.templatesChangingList)
                {
                    //Берём группу, шаблон которой изменяется
                    fleetsTab.templateChangingTaskForcePanel.selfPE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce parentTaskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Запрашиваем смену шаблона для данной группы
                    TaskForceActionRequest(
                        TaskForceActionType.ChangeTemplate,
                        ref parentTaskForce,
                        templateSummaryPanel.template);
                }
                //Иначе, если родительским списком панели является список для создания группы
                else if(templateSummaryPanel.parentList == fleetsTab.templatesCreatingList)
                {
                    //Если есть активный флот
                    if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                    {
                        //Берём флот
                        ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                        //Запрашиваем создание новой оперативной группы
                        TaskForceCreatingRequest(
                            ref fleet,
                            templateSummaryPanel.template);
                    }
                }
            }

            //Иначе, если нажата кнопка создания новой оперативной группы
            else if (clickEvent.WidgetName == "CreateNewTaskForceFM")
            {
                //Если список для создания группы активен
                if (fleetsTab.templatesCreatingList.isActiveAndEnabled == true)
                {
                    //Запрашиваем заполнение списка для создания группы
                    GameFleetManagerSubpanelActionRequest(
                        FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesNewList,
                        objectPanel.activeObjectPE);
                }
            }
            //Иначе, если нажата кнопка создания нового флота
            else if (clickEvent.WidgetName == "CreateNewFleetFM")
            {
                //Запрашиваем создание нового флота
                FleetCreatingRequest(ref playerOrganization);
            }
        }

        void ClickActionGameObjectFleetManagerTaskForceTemplates(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //Берём вкладку шаблонов групп
            UITFTemplatesTab tFTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

            //Берём организацию
            objectPanel.activeObjectPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Если активна подвкладка списка шаблонов групп
            if (tFTemplatesTab.listSubtab.isActiveAndEnabled == true)
            {
                //Если нажата кнопка создания нового шаблона группы
                if (clickEvent.WidgetName == "CreateNewTaskForceTemplateFM")
                {
                    //Обнуляем ссылку на шаблон в дизайнере
                    tFTemplatesTab.designerSubtab.template = null;

                    //Запрашиваем отображение подвкладки дизайнера шаблона группы
                    GameObjectPanelActionRequest(
                        ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner,
                        objectPanel.activeObjectPE);
                }
                //Иначе, если дважды родитель источника события имеет компонент UITFTemplateSummaryPanel
                else if (clickEvent.Sender.transform.parent.parent.TryGetComponent(
                    out GameWindow.Object.FleetManager.TaskForceTemplates.List.UITFTemplateSummaryPanel tFTemplateSummaryPanel))
                {
                    //Если источник события - это кнопка редактирования
                    if (clickEvent.Sender == tFTemplateSummaryPanel.editButton.gameObject)
                    {
                        //Если кнопка активна
                        if (tFTemplateSummaryPanel.editButton.interactable == true)
                        {
                            //Указываем в подвкладке дизайнера индекс шаблона
                            tFTemplatesTab.designerSubtab.template = tFTemplateSummaryPanel.template;

                            //Запрашиваем отображение подвкладки дизайнера шаблона группы
                            GameObjectPanelActionRequest(
                                ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner,
                                objectPanel.activeObjectPE);
                        }
                    }
                    //Иначе, если источник события - это кнопка удаления
                    else if (clickEvent.Sender == tFTemplateSummaryPanel.deleteButton.gameObject)
                    {
                        //Если кнопка активна
                        if (tFTemplateSummaryPanel.deleteButton.interactable == true)
                        {
                            //Запрашиваем удаление шаблона группы
                            TaskForceTemplateActionRequest(
                                objectPanel.activeObjectPE,
                                tFTemplateSummaryPanel.template,
                                TFTemplateActionType.Delete);
                        }
                    }
                }

            }
            //Иначе, если активна подвкладка дизайнера шаблонов групп
            else if (tFTemplatesTab.designerSubtab.isActiveAndEnabled == true)
            {
                //Берём подвкладку дизайнера шаблонов групп
                UIDesignerSubtab taskForceTemplatesDesignerSubtab = tFTemplatesTab.designerSubtab;

                //Если нажата кнопка сохранения шаблона группы
                if (clickEvent.WidgetName == "SaveTaskForceTemplateFM")
                {
                    //Проверяем, возможно ли сохранение шаблона
                    bool isSavePossible = true;

                    //Для каждого шаблона группы в данных организации
                    for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
                    {
                        //Если название шаблона совпадает с текущим, то отмечаем, что сохранение невозможно
                        if (OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a].selfName
                            == tFTemplatesTab.designerSubtab.tFTemplateName.text)
                        {
                            isSavePossible = false;

                            break;
                        }
                    }

                    //Если сохранение возможно
                    if (isSavePossible == true)
                    {
                        //Если шаблон в дизайнере не пуст
                        if (taskForceTemplatesDesignerSubtab.template != null)
                        {
                            //Запрашиваем обновление шаблона группы
                            TaskForceTemplateCreatingRequest(
                                taskForceTemplatesDesignerSubtab,
                                objectPanel.activeObjectPE,
                                tFTemplatesTab.designerSubtab.tFTemplateName.text,
                                true,
                                tFTemplatesTab.designerSubtab.template);
                        }
                        //Иначе
                        else
                        {
                            //Запрашиваем сохранение шаблона группы
                            TaskForceTemplateCreatingRequest(
                                taskForceTemplatesDesignerSubtab,
                                objectPanel.activeObjectPE,
                                tFTemplatesTab.designerSubtab.tFTemplateName.text);
                        }

                        //Запрашиваем отображение подвкладки списка шаблонов групп
                        GameObjectPanelActionRequest(
                            ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesList,
                            objectPanel.activeObjectPE);
                    }
                }
            }
        }

        readonly EcsPoolInject<RGameFleetManagerSubpanelAction> gameFleetManagerSubpanelActionRequestPool = default;
        void GameFleetManagerSubpanelActionRequest(
            FleetManagerSubpanelActionRequestType requestType,
            EcsPackedEntity organizationPE,
            EcsPackedEntity taskForcePE = default)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия менеджера флотов
            int requestEntity = world.Value.NewEntity();
            ref RGameFleetManagerSubpanelAction requestComp = ref gameFleetManagerSubpanelActionRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                requestType,
                organizationPE,
                taskForcePE);
        }
        #endregion

        #region GameObjectOrganization
        void ClickActionGameObjectOrganization(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель организации
            UIOrganizationSubpanel organizationSubpanel = objectPanel.organizationSubpanel;

            //Если нажата кнопка обзорной вкладки
            if (clickEvent.WidgetName == "OrganizationOverviewTab")
            {
                //Запрашиваем отображение обзорной вкладки
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.OrganizationOverview,
                    objectPanel.activeObjectPE);
            }
        }
        #endregion

        #region GameObjectRegion
        void ClickActionGameObjectRegion(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём организацию игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель региона
            UIRegionSubpanel regionSubpanel = objectPanel.regionSubpanel;

            //Если нажата кнопка обзорной вкладки
            if (clickEvent.WidgetName == "RegionOverviewTab")
            {
                //Запрашиваем отображение обзорной вкладки
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.RegionOverview,
                    objectPanel.activeObjectPE);
            }
            //Иначе, если нажата кнопка вкладки организаций
            else if (clickEvent.WidgetName == "RegionORAEOsTab")
            {
                //Запрашиваем отображение вкладки организаций
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.RegionOrganizations,
                    objectPanel.activeObjectPE);
            }

            //Иначе, если активна обзорная вкладка
            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.overviewTab.selfTabButton)
            {
                //Проверяем клики в обзорной вкладке
                ClickActionGameObjectRegionOverview(ref clickEvent);
            }
            //Иначе, если активна вкладка организаций
            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.oRAEOsTab.selfTabButton)
            {
                //Проверяем клики во вкладке ORAEO
                ClickActionGameObjectRegionORAEOs(ref clickEvent);
            }
        }

        void ClickActionGameObjectRegionOverview(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём организацию игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Если нажата кнопка колонизации
            if (clickEvent.WidgetName == "RAEOColonize")
            {
                //Берём RAEO
                objectPanel.activeObjectPE.Unpack(world.Value, out int rAEOEntity);
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //Берём ExORAEO данной организации
                rAEO.organizationRAEOs[playerOrganization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);

                //Назначаем сущности ORAEO самозапрос действия
                ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Add(oRAEOEntity);

                //Заполняем данные самозапроса
                oRAEOActionSR = new(ORAEOActionType.Colonization);
            }
        }

        void ClickActionGameObjectRegionORAEOs(
            ref EcsUguiClickEvent clickEvent)
        {
            //Если источник события имееет компонент ORAEOBriefInfoPanel
            if (clickEvent.Sender.TryGetComponent(out UIORAEOSummaryPanel briefInfoPanel))
            {
                //Запрашиваем отображение подпанели ORAEO
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.ORAEO,
                    briefInfoPanel.selfPE);
            }
        }
        #endregion

        #region GameObjectORAEO
        void ClickActionGameObjectORAEO(
            ref EcsUguiClickEvent clickEvent)
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Берём подпанель ORAEO
            UIORAEOSubpanel oRAEOSubpanel = objectPanel.oRAEOSubpanel;

            //Если нажата кнопка обзорной вкладки
            if (clickEvent.WidgetName == "ORAEOOverviewTab")
            {
                //Запрашиваем отображение обзорной вкладки
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.ORAEOOverview,
                    objectPanel.activeObjectPE);
            }
        }
        #endregion

        readonly EcsPoolInject<RGameObjectPanelAction> gameObjectPanelActionRequestPool = default;
        void GameObjectPanelActionRequest(
            ObjectPanelActionRequestType requestType,
            EcsPackedEntity objectPE = new(),
            bool isRefresh = false)
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

        readonly EcsPoolInject<RGameAction> gameActionRequestPool = default;
        void GameActionRequest(
            GameActionType actionType)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия в игре
            int requestEntity = world.Value.NewEntity();
            ref RGameAction requestComp = ref gameActionRequestPool.Value.Add(requestEntity);

            //Указываем, какое действие в игре запрашивается
            requestComp.actionType = actionType;
        }

        readonly EcsPoolInject<RGameOpenDesigner> gameOpenDesignerRequestPool = default;
        void GameOpenDesignerRequest(
            DesignerType designerType,
            int contentSetIndex)
        {
            //Создаём новую сущность и назначаем ей компонент запроса открытия дизайнера в игре
            int requestEntity = world.Value.NewEntity();
            ref RGameOpenDesigner requestComp = ref gameOpenDesignerRequestPool.Value.Add(requestEntity);

            //Указываем, какой дизайнер требуется открыть
            requestComp.designerType = designerType;

            //Указываем, какой набор контента требуется открыть
            requestComp.contentSetIndex = contentSetIndex;
        }

        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;
        void MapChangeModeRequest(
            ChangeMapModeRequestType requestType,
            EcsPackedEntity objectPE)
        {
            //Создаём новую сущность и назначаем ей компонент запроса смены режима карты
            int requestEntity = world.Value.NewEntity();
            ref RChangeMapMode requestComp = ref changeMapModeRequestPool.Value.Add(requestEntity);

            //Указываем требуемый режим карты
            requestComp.requestType = requestType;

            requestComp.mapMode = MapMode.Distance;

            //Указываем, карта какого объекта открывается
            requestComp.activeObject = objectPE;
        }

        void FleetCreatingRequest(
            ref COrganization organization,
            bool isReserve = false)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания флота
            int requestEntity = world.Value.NewEntity();
            ref RFleetCreating requestComp = ref fleetCreatingRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                organization.selfPE,
                isReserve);
        }

        void TaskForceCreatingRequest(
            ref CFleet fleet,
            DTFTemplate template = null)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания оперативной группы
            int requestEntity = world.Value.NewEntity();
            ref RTaskForceCreating requestComp = ref taskForceCreatingRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                fleet.selfPE,
                template);
        }

        void TaskForceActionRequest(
            TaskForceActionType requestType,
            ref CTaskForce taskForce,
            DTFTemplate template = null)
        {
            //Создаём новую сущность и назначаем ей компонент запроса действия оперативной группы
            int requestEntity = world.Value.NewEntity();
            ref RTaskForceAction requestComp = ref taskForceActionRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                taskForce.selfPE,
                requestType,
                template);
        }

        void TaskForceTemplateCreatingRequest(
            UIDesignerSubtab taskForceTemplatesDesignerSubtab,
            EcsPackedEntity organizationPE,
            string taskForceTemplateName,
            bool isUpdate = false,
            DTFTemplate updatingTemplate = null)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания нового шаблона оперативной группы
            int requestEntity = world.Value.NewEntity();
            ref RTFTemplateCreating requestComp = ref tFTemplateCreatingRequestPool.Value.Add(requestEntity);

            //Создаём список типов кораблей
            List<DCountedShipType> shipTypes = new();

            //Берём панель группы малой дальности
            UIBattleGroupPanel shortRangeBattleGroupPanel = taskForceTemplatesDesignerSubtab.shortRangeGroup;

            //Для каждого имеющегося типа корабля в группе малой дальности
            for (int a = 0; a < shortRangeBattleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //Если количество кораблей данного типа больше нуля
                if (shortRangeBattleGroupPanel.currentShipTypePanels[a].CurrentShipCountValue > 0)
                {
                    //Заносим тип корабля в список
                    shipTypes.Add(new(
                        shortRangeBattleGroupPanel.currentShipTypePanels[a].shipType,
                        shortRangeBattleGroupPanel.currentShipTypePanels[a].CurrentShipCountValue));
                }
            }

            //Заполняем данные запроса
            requestComp = new(
                organizationPE,
                taskForceTemplateName,
                shipTypes,
                isUpdate,
                updatingTemplate);
        }

        void TaskForceTemplateActionRequest(
            EcsPackedEntity organizationPE,
            DTFTemplate template,
            TFTemplateActionType requestType)
        {
            //Создаём новую сущность и назначаем ей компонент запрса действия шаблона оперативной группы
            int requestEntity = world.Value.NewEntity();
            ref RTFTemplateAction requestComp = ref tFTemplateActionRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                organizationPE,
                template,
                requestType);
        }
        #endregion
        
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
                MapChangeModeRequest(
                    ChangeMapModeRequestType.Distance,
                    new EcsPackedEntity());
            }
        }

        void CheckButtons()
        {
            //Определяем состояние ЛКМ
            InputData.leftMouseButtonClick = Input.GetMouseButtonDown(0);
            InputData.leftMouseButtonPressed = InputData.leftMouseButtonClick || Input.GetMouseButton(0);
            InputData.leftMouseButtonRelease = Input.GetMouseButtonUp(0);

            //Определяем состояние ПКМ
            InputData.rightMouseButtonClick = Input.GetMouseButtonDown(1);
            InputData.rightMouseButtonPressed = InputData.leftMouseButtonClick || Input.GetMouseButton(1);
            InputData.rightMouseButtonRelease = Input.GetMouseButtonUp(1);

            //Определяем состояние LeftShift
            InputData.leftShiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
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
                    //if (hit.transform.gameObject.TryGetComponent(
                    //    out GOMapObject gOMapObject))
                    //{
                        //Берём компонент родительского MO
                        //gOMapObject.mapObjectPE.Unpack(world.Value, out int mapObjectEntity);
                        //ref CMapObject mapObject = ref mapObjectPool.Value.Get(mapObjectEntity);

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

                        //Debug.LogWarning(mapObject.objectType);
                    //}
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
                if (InputData.leftMouseButtonClick)
                {
                    //Если нажата кнопка
                    if (InputData.leftShiftKeyPressed
                        && InputData.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //Если стартовый регион не совпадает с текущей
                        if (InputData.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                        {
                            //Если стартовый регион задан
                            if (InputData.searchFromRegion.Unpack(world.Value, out int fromRegionEntity))
                            {
                                //Берём компонент стартового региона
                                ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                                //Отключаем подсветку стартового региона
                                RegionSetColor(ref fromRegion, MapGenerationData.DefaultShadedColor);
                            }

                            //Обновляем стартовый регион
                            InputData.searchFromRegion = currentRegion.selfPE;

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
                    else if (InputData.searchFromRegion.Unpack(world.Value, out int fromRegionEntity)
                        //И стартовый регион не совпадает с конечным
                        && InputData.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //Если конечный регион не совпадает с текущим
                        if (InputData.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                        {
                            //Сохраняем конечный регион
                            InputData.searchToRegion = currentRegion.selfPE;

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
                        GameObjectPanelActionRequest(
                            ObjectPanelActionRequestType.Region,
                            currentRegion.selfPE);
                    }
                }
                //Иначе, если нажата ПКМ
                else if (InputData.rightMouseButtonClick)
                {
                    //Если нажаты ПКМ+LeftShift
                    if (InputData.RMBAndLeftShift)
                    {
                        //Если открыт менеджер флотов
                        if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                        {
                            //Если выбран флот
                            if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                            {
                                //Берём флот
                                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                                int fleetRegionIndex = -1;

                                //Для каждого региона флота
                                for (int a = 0; a < fleet.fleetRegions.Count; a++)
                                {
                                    //Если это текущий регион
                                    if (fleet.fleetRegions[a].regionPE.EqualsTo(currentRegion.selfPE) == true)
                                    {
                                        //Удаляем ссылку на него из его соседей
                                        fleet.fleetRegions[a].RemoveNeigbours();

                                        //Указываем индекс региона
                                        fleetRegionIndex = a;

                                        break;
                                    }
                                }

                                //Если индекс больше или равен нулю
                                if (fleetRegionIndex >= 0)
                                {
                                    //Удаляем PE региона из списка
                                    fleet.fleetRegions.RemoveAt(fleetRegionIndex);

                                    //Отключаем подсветку региона
                                    RegionHideFleetHighlight(ref currentRegion);
                                }
                            }
                        }
                    }
                    //Иначе
                    else
                    {
                        //Если открыт менеджер флотов
                        if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                        {
                            //Если выбран флот
                            if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                            {
                                //Берём флот
                                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                                //Создаём класс, хранящий данные региона флота
                                DFleetRegion currentFleetRegion = new(
                                    currentRegion.selfPE);

                                //Если список не содержит данный регион
                                if (fleet.fleetRegions.Contains(currentFleetRegion) == false)
                                {
                                    //Добавляем в список соседей текущего региона флота уже существующие регионы
                                    currentFleetRegion.AddNeighbours(
                                        ref currentRegion,
                                        fleet.fleetRegions);

                                    //Заносим PE региона в список
                                    fleet.fleetRegions.Add(currentFleetRegion);

                                    //Включаем подсветку региона
                                    RegionShowFleetHighlight(ref currentRegion);

                                    Debug.LogWarning(currentRegion.Index);
                                }
                            }
                        }
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

                    //Если индекс региона не равен индексу последнего подсвеченного
                    if (region.Index != InputData.lastHighlightedRegionIndex)
                    {
                        //Если индекс последнего подсвеченного региона больше нуля
                        if (InputData.lastHighlightedRegionIndex > 0)
                        {
                            //Скрываем подсветку наведения
                            RegionHideHoverHighlight();
                        }

                        //Обновляем подсвеченный регион
                        InputData.lastHighlightedRegionPE = region.selfPE;
                        InputData.lastHighlightedRegionIndex = region.Index;

                        //Подсвечиваем регион
                        RegionShowHoverHighlight(ref region);
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
                    //Скрываем подсветку наведения
                    RegionHideHoverHighlight();
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
                region.hoverRenderer.sharedMaterial = material;
                region.hoverRenderer.enabled = true;
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

            //Если материал - не материал подсветки наведения
            if (material != mapGenerationData.Value.hoverRegionHighlightMaterial)
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
            if (mapGenerationData.Value.hoverRegionHighlightMaterial != null && InputData.lastHighlightedRegionIndex == region.Index)
            {
                //Задаём рендереру материал подсветки наведения
                region.hoverRenderer.sharedMaterial = mapGenerationData.Value.hoverRegionHighlightMaterial;

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
                    //Устанавливаем вторичный цвет материалу подсветки наведения
                    mapGenerationData.Value.hoverRegionHighlightMaterial.SetColor(ShaderParameters.Color2, color);

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
                        mapGenerationData.Value.hoverRegionHighlightMaterial.mainTexture = tempMaterialTexture;
                    }
                }
            }

            return true;
        }

        void RegionShowHoverHighlight(
            ref CHexRegion region)
        {
            //Активируем рендерер наведения
            region.hoverRenderer.enabled = true;
        }

        void RegionHideHoverHighlight()
        {
            //Если подсвеченный регион существует
            if (InputData.lastHighlightedRegionPE.Unpack(world.Value, out int highlightedRegionEntity))
            {
                //Берём регион
                ref CHexRegion highlightedRegion = ref regionPool.Value.Get(highlightedRegionEntity);

                //Отключаем рендерер наведения
                highlightedRegion.hoverRenderer.enabled = false;
            }

            //Удаляем последний подсвеченный регион
            InputData.lastHighlightedRegionPE = new();
            InputData.lastHighlightedRegionIndex = -1;
        }

        void RegionShowFleetHighlight(
            ref CHexRegion region)
        {
            //Активируем рендерер флота
            region.fleetRenderer.enabled = true;
        }

        void RegionHideFleetHighlight(
            ref CHexRegion region)
        {
            //Отключаем рендерер флота
            region.fleetRenderer.enabled = false;
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