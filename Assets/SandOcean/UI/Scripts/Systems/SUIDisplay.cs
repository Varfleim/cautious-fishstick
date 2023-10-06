using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using SandOcean.UI.Events;
using SandOcean.UI.MainMenu.Events;
using SandOcean.UI.NewGameMenu.Events;
using SandOcean.UI.WorkshopWindow;
using SandOcean.UI.WorkshopWindow.Events;
using SandOcean.UI.DesignerWindow;
using SandOcean.UI.DesignerWindow.Events;
using SandOcean.UI.DesignerWindow.ShipClassDesigner;
using SandOcean.UI.DesignerWindow.ShipClassDesigner.Events;
using SandOcean.UI.DesignerWindow.EngineDesigner;
using SandOcean.UI.DesignerWindow.ReactorDesigner;
using SandOcean.UI.DesignerWindow.FuelTankDesigner;
using SandOcean.UI.DesignerWindow.ExtractionEquipmentDesigner;
using SandOcean.UI.DesignerWindow.GunDesigner;
using SandOcean.UI.GameWindow;
using SandOcean.UI.GameWindow.Object;
using SandOcean.UI.GameWindow.Object.FleetManager;
using SandOcean.UI.GameWindow.Object.FleetManager.Fleets;
using SandOcean.UI.GameWindow.Object.FleetManager.TaskForceTemplates;
using SandOcean.UI.GameWindow.Object.FleetManager.Events;
using SandOcean.UI.GameWindow.Object.Region;
using SandOcean.UI.GameWindow.Object.Region.ORAEOs;
using SandOcean.UI.GameWindow.Object.Region.Events;
using SandOcean.UI.GameWindow.Object.ORAEO;
using SandOcean.UI.GameWindow.Object.ORAEO.Buildings;
using SandOcean.UI.GameWindow.Object.Events;

using SandOcean.Player;
using SandOcean.Organization;
using SandOcean.AEO.RAEO;
using SandOcean.Economy.Building;
using SandOcean.Technology;
using SandOcean.Designer;
using SandOcean.Designer.Workshop;
using SandOcean.Designer.Game;
using SandOcean.Map;
using SandOcean.Map.Events;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.UI
{
    public class SUIDisplay : IEcsInitSystem, IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Игроки

        //Карта
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Организации
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Экономика
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;
        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        readonly EcsPoolInject<CBuilding> buildingPool = default;
        readonly EcsFilterInject<Inc<CBuilding, CBuildingDisplayedSummaryPanel>> buildingDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CBuildingDisplayedSummaryPanel> buildingDisplayedSummaryPanelPool = default;

        //Флоты
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsFilterInject<Inc<CFleet, CFleetDisplayedSummaryPanel>> fleetDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CFleetDisplayedSummaryPanel> fleetDisplayedSummaryPanelPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceDisplayedSummaryPanel>> tFDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> tFDisplayedSummaryPanelPool = default;

        readonly EcsFilterInject<Inc<CTFTemplateDisplayedSummaryPanel>> tFTemplateDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CTFTemplateDisplayedSummaryPanel> tFTemplateDisplayedSummaryPanelPool = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<SRRefreshRAEOObjectPanel, CRegionAEO>> refreshRAEOObjectPanelSelfRequestFilter = default;
        readonly EcsPoolInject<SRRefreshRAEOObjectPanel> refreshRAEOObjectPanelSelfRequestPool = default;

        //Общие события
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;

        readonly EcsPoolInject<RTechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;

        readonly EcsPoolInject<RSaveContentSet> saveContentSetRequestPool = default;

        readonly EcsPoolInject<EcsGroupSystemState> ecsGroupSystemStatePool = default;

        readonly EcsFilterInject<Inc<RQuitGame>> quitGameRequestFilter = default;
        readonly EcsPoolInject<RQuitGame> quitGameRequestPool = default;


        //Данные
        //readonly EcsCustomInject<StaticData> staticData = default;
        //readonly EcsCustomInject<SceneData> sceneData = default;
        readonly EcsCustomInject<ContentData> contentData = default;
        //readonly EcsCustomInject<MapData> mapData = default;
        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;
        readonly EcsCustomInject<DesignerData> designerData = default;
        //readonly EcsCustomInject<ModifierData> modifierData = default;

        readonly EcsCustomInject<RuntimeData> runtimeData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<EUI> eUI = default;

        public void Init(IEcsSystems systems)
        {
            //Открываем окно главного меню
            MainMenuOpenWindow();

            //ТЕСТ
            //Очищаем список типов компонентов в дизайнере кораблей
            eUI.Value.designerWindow.shipClassDesigner.availableComponentTypeDropdown.ClearOptions();
            //Заполняем список типов компонентов в дизайнере кораблей
            eUI.Value.designerWindow.shipClassDesigner.availableComponentTypeDropdown.AddOptions(new List<string>()
            {
                ((ShipComponentType)0).ToString(),
                ((ShipComponentType)1).ToString(),
                ((ShipComponentType)2).ToString(),
                ((ShipComponentType)3).ToString(),
                ((ShipComponentType)4).ToString(),
                ((ShipComponentType)5).ToString(),
                ((ShipComponentType)6).ToString()
            });
            eUI.Value.designerWindow.shipClassDesigner.currentAvailableComponentsType = 0;
            //ТЕСТ
        }

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса выхода из игры
            foreach (int quitGameRequestEntity in quitGameRequestFilter.Value)
            {
                //Берём запрос
                ref RQuitGame quitGameRequest = ref quitGameRequestPool.Value.Get(quitGameRequestEntity);

                Debug.LogError("Quit game!");

                world.Value.DelEntity(quitGameRequestEntity);

                //Выходим из игры
                Application.Quit();
            }

            //Eсли активно окно игры
            if (eUI.Value.activeMainWindowType == MainWindowType.Game)
            {
                //Проверяем события в окне игры
                EventCheckGame();
            }
            //Иначе, если активно главное меню
            else if (eUI.Value.activeMainWindowType == MainWindowType.MainMenu)
            {
                //Проверяем события в окне главного меню
                EventCheckMainMenu();
            }
            //Иначе, если активно меню новой игры
            else if (eUI.Value.activeMainWindowType == MainWindowType.NewGameMenu)
            {
                //Проверяем события в окне меню новой игры
                EventCheckNewGameMenu();
            }
            //Иначе, если активна мастерская
            else if (eUI.Value.activeMainWindowType == MainWindowType.Workshop)
            {
                //Проверяем события в окне мастерской
                EventCheckWorkshop();
            }
            //Иначе, если активен дизайнер
            else if (eUI.Value.activeMainWindowType == MainWindowType.Designer)
            {
                //Проверяем события в окне дизайнера
                EventCheckDesigner();
            }
        }

        void CloseMainWindow()
        {
            //Если какое-то окно было активным
            if (eUI.Value.activeMainWindow
                != null)
            {
                //Делаем его неактивным
                eUI.Value.activeMainWindow.SetActive(
                    false);
            }

            //Указываем, что не активно ни одно главное окно
            eUI.Value.activeMainWindowType
                = MainWindowType.None;
        }

        #region MainMenu
        readonly EcsFilterInject<Inc<RMainMenuAction>> mainMenuActionRequestFilter = default;
        readonly EcsPoolInject<RMainMenuAction> mainMenuActionRequestPool = default;
        void EventCheckMainMenu()
        {
            //Для каждого запроса действия в главном меню
            foreach (int mainMenuActionRequestEntity in mainMenuActionRequestFilter.Value)
            {
                //Берём запрос
                ref RMainMenuAction mainMenuActionRequest = ref mainMenuActionRequestPool.Value.Get(mainMenuActionRequestEntity);

                //Если запрашивается открытие меню новой игры
                if (mainMenuActionRequest.actionType == MainMenuActionType.OpenNewGameMenu)
                {
                    //Открываем окно меню новой игры
                    NewGameMenuOpenWindow();
                }
                //Иначе, если запрашивается открытие меню загрузки игры
                else if (mainMenuActionRequest.actionType == MainMenuActionType.OpenLoadGameMenu)
                {
                    //Открываем окно меню загрузки игры

                }
                //Иначе, если запрашивается открытие окна мастерской
                else if (mainMenuActionRequest.actionType == MainMenuActionType.OpenWorkshop)
                {
                    //Открываем окно мастерской
                    WorkshopOpenWindow();
                }
                //Иначе, если запрашивается открытие окна главных настроек
                else if (mainMenuActionRequest.actionType == MainMenuActionType.OpenMainSettings)
                {
                    //Открываем окно главных настроек

                }

                world.Value.DelEntity(mainMenuActionRequestEntity);
            }
        }

        void MainMenuOpenWindow()
        {
            //Закрываем открытое главное окно
            CloseMainWindow();

            //Берём ссылку на окно главного меню
            UIMainMenuWindow mainMenuWindow
                = eUI.Value.mainMenuWindow;

            //Делаем окно главного меню активным
            mainMenuWindow.gameObject.SetActive(
                true);
            //И указываем его как активное окно в EUI
            eUI.Value.activeMainWindow
                = eUI.Value.mainMenuWindow.gameObject;

            //Указываем, что активно окно главного меню
            eUI.Value.activeMainWindowType
                = MainWindowType.MainMenu;


        }
        #endregion

        #region NewGameMenu
        readonly EcsFilterInject<Inc<RNewGameMenuAction>> newGameMenuActionRequestFilter = default;
        readonly EcsPoolInject<RNewGameMenuAction> newGameMenuActionRequestPool = default;
        void EventCheckNewGameMenu()
        {
            //Для каждого запроса действия в меню новой игры
            foreach (int newGameMenuActionRequestEntity in newGameMenuActionRequestFilter.Value)
            {
                //Берём запрос
                ref RNewGameMenuAction newGameMenuActionRequest = ref newGameMenuActionRequestPool.Value.Get(newGameMenuActionRequestEntity);

                //Если запрашивается открытие главного меню
                if (newGameMenuActionRequest.actionType == NewGameMenuActionType.OpenMainMenu)
                {
                    //Открываем главное меню
                    MainMenuOpenWindow();
                }
                //Иначе, если запрашивается начало новой игры
                else if (newGameMenuActionRequest.actionType == NewGameMenuActionType.StartNewGame)
                {
                    //Создаём запрос создания новой игры
                    NewGameMenuStartNewGameRequest();

                    //Инициализируем настройки новой игры по окну меню новой игры
                    NewGameInitialization();

                    //Открываем окно игры
                    GameOpenWindow();
                }

                world.Value.DelEntity(newGameMenuActionRequestEntity);
            }
        }

        void NewGameMenuOpenWindow()
        {
            //Закрываем открытое главное окно
            CloseMainWindow();

            //Берём ссылку на окно меню новой игры
            UINewGameMenuWindow newGameMenuWindow
                = eUI.Value.newGameMenuWindow;

            //Делаем окно меню новой игры активным
            newGameMenuWindow.gameObject.SetActive(
                true);
            //И указываем его как активное окно в EUI
            eUI.Value.activeMainWindow
                = eUI.Value.newGameMenuWindow.gameObject;

            //Указываем, что активно окно меню новой игры
            eUI.Value.activeMainWindowType
                = MainWindowType.NewGameMenu;
        }

        void NewGameMenuStartNewGameRequest()
        {
            //Создаём новую сущность и назначаем ей компонент запроса начала новой игры
            int requestEntity = world.Value.NewEntity();
            ref RStartNewGame startNewGameRequest = ref startNewGameEventPool.Value.Add(requestEntity);

            //Запрашиваем включение группы систем "NewGame"
            EcsGroupSystemStateEvent("NewGame", true);
        }

        void NewGameInitialization()
        {
            //Берём ссылку на окно новой игры
            UINewGameMenuWindow newGameMenuWindow
                = eUI.Value.newGameMenuWindow;

            //Записываем модификатор размера сектора из окна меню новой игры
            mapGenerationData.Value.sectorSizeModifier
                = newGameMenuWindow.sectorSizeSlider.value;

        }

        void TechnologyCalculateFactionModifiersEvent(
            ref COrganization faction)
        {
            //Создаём новую сущность и назначаем ей компонент события рассчёта модификаторов технологий
            int eventEntity = world.Value.NewEntity();
            ref RTechnologyCalculateModifiers technologyCalculateModifiersEvent
                = ref technologyCalculateModifiersEventPool.Value.Add(eventEntity);

            //Указываем ссылку на сущность фракции, для которой требуется пересчитать модификаторы
            technologyCalculateModifiersEvent.organizationPE
                = faction.selfPE;
        }
        #endregion

        #region Workshop
        readonly EcsFilterInject<Inc<RWorkshopAction>> workshopActionRequestFilter = default;
        readonly EcsPoolInject<RWorkshopAction> workshopActionRequestPool = default;
        void EventCheckWorkshop()
        {
            //Для каждого запроса действия в мастерской
            foreach (int workshopActionRequestEntity in workshopActionRequestFilter.Value)
            {
                //Берём запрос
                ref RWorkshopAction workshopActionRequest = ref workshopActionRequestPool.Value.Get(workshopActionRequestEntity);

                //Если запрашивается открытие главного меню
                if (workshopActionRequest.actionType == WorkshopActionType.OpenMainMenu)
                {
                    //Открываем главное меню
                    MainMenuOpenWindow();
                }
                //Иначе, если запрашивается отображение набора контента
                else if (workshopActionRequest.actionType == WorkshopActionType.DisplayContentSet)
                {
                    //Отображаем запрошенный набор контента
                    WorkshopDisplayContentSet(workshopActionRequest.contentSetIndex);
                }
                //Иначе, если запрашивается открытие окна дизайнера
                else if (workshopActionRequest.actionType == WorkshopActionType.OpenDesigner)
                {
                    //Отображаем запрошенное окно дизайнера
                    DesignerOpenWindow(
                        workshopActionRequest.contentSetIndex,
                        workshopActionRequest.designerType,
                        false);
                }

                world.Value.DelEntity(workshopActionRequestEntity);
            }
        }

        void WorkshopOpenWindow()
        {
            //Закрываем открытое главное окно
            CloseMainWindow();

            //Берём ссылку на окно мастерской
            UIWorkshopWindow workshopWindow
                = eUI.Value.workshopWindow;

            //Делаем окно мастерской активным
            workshopWindow.gameObject.SetActive(true);
            //И указываем его как активное окно в EUI
            eUI.Value.activeMainWindow = eUI.Value.workshopWindow.gameObject;

            //Указываем, что активно окно мастерской
            eUI.Value.activeMainWindowType = MainWindowType.Workshop;


            //Если в списке наборов контента были наборы контента
            if (workshopWindow.contentSetPanels.Count != 0)
            {
                //Очищаем список
                workshopWindow.ClearContentSetPanels();
            }

            //Для каждого набора контента в данных мастерской
            for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
            {
                //Берём ссылку на данные набора контента
                ref readonly WDContentSet contentSet
                    = ref contentData.Value.wDContentSets[a];

                //Создаём панель набора контента в списке
                UIWorkshopContentSetPanel workshopContentSetPanel
                    = workshopWindow.InstantiateWorkshopContentSetPanel(
                        a,
                        in contentSet);

                //Если это первый набор контента
                if (a == 0)
                {
                    //Делаем его панель активной
                    workshopContentSetPanel.panelToggle.isOn = true;
                }
            }


            //Отображаем данные первого набора контента
            WorkshopDisplayContentSet(0);
        }

        void WorkshopDisplayContentSet(
            int contentSetIndex)
        {
            //Берём ссылку на окно мастерской
            UIWorkshopWindow workshopWindow
                = eUI.Value.workshopWindow;


            //Берём ссылку на описание набора контента
            ref readonly WDContentSetDescription contentSetDescription
                = ref contentData.Value.wDContentSetDescriptions[contentSetIndex];

            //Берём ссылку на данные набора контента
            ref readonly WDContentSet contentSet
                = ref contentData.Value.wDContentSets[contentSetIndex];

            //Указываем индекс набора контента
            workshopWindow.currentContentSetIndex
                = contentSetIndex;

            //Отображаем название набора контента
            workshopWindow.currentContentSetName.text
                = contentSetDescription.ContentSetName;

            //Отображаем версию игры, для которой создан набор контента
            workshopWindow.currentContentSetGameVersion.text
                = contentSetDescription.GameVersion;

            //Отображаем версию набора контента
            workshopWindow.currentContentSetVersion.text
                = contentSetDescription.ContentSetVersion;


            //Выключаем выбранный переключатель в списке содержимого набора
            workshopWindow.contentObjectCountToggleGroup.SetAllTogglesOff();

            //Отображаем информацию о кораблях в данном наборе контента
            workshopWindow.shipsInfoPanel.contentAmount.text
                = contentSet.shipClasses.Length.ToString();

            //Отображаем информацию о двигателях в данном наборе контента
            workshopWindow.enginesInfoPanel.contentAmount.text
                = contentSet.engines.Length.ToString();

            //Отображаем информацию о реакторах в данном наборе контента
            workshopWindow.reactorsInfoPanel.contentAmount.text
                = contentSet.reactors.Length.ToString();

            //Отображаем информацию о топливных баках в данном наборе контента
            workshopWindow.fuelTanksInfoPanel.contentAmount.text
                = contentSet.fuelTanks.Length.ToString();

            //Отображаем информацию об оборудовании для твёрдой добычи в данном наборе контента
            workshopWindow.solidExtractionEquipmentsInfoPanel.contentAmount.text
                = contentSet.solidExtractionEquipments.Length.ToString();

            //Отображаем информацию об энергетических орудиях в данном наборе контента
            workshopWindow.energyGunsInfoPanel.contentAmount.text
                = contentSet.energyGuns.Length.ToString();
        }
        #endregion

        #region Designer
        readonly EcsFilterInject<Inc<RDesignerAction>> designerActionRequestFilter = default;
        readonly EcsPoolInject<RDesignerAction> designerActionRequestPool = default;

        readonly EcsFilterInject<Inc<RDesignerShipClassAction>> designerShipClassActionRequestFilter = default;
        readonly EcsPoolInject<RDesignerShipClassAction> designerShipClassActionRequestPool = default;

        readonly EcsFilterInject<Inc<RDesignerComponentAction>> designerComponentActionRequestFilter = default;
        readonly EcsPoolInject<RDesignerComponentAction> designerComponentActionRequestPool = default;
        void EventCheckDesigner()
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow = eUI.Value.designerWindow;

            //Для каждого запроса в дизайнере
            foreach (int designerActionRequestEntity in designerActionRequestFilter.Value)
            {
                //Берём запрос
                ref RDesignerAction designerActionRequest = ref designerActionRequestPool.Value.Get(designerActionRequestEntity);

                //Если запрашивается сохранение объекта контента
                if (designerActionRequest.actionType == DesignerActionType.SaveContentObject)
                {
                    //Если объект требуется сохранить в текущий набор контента
                    if (designerActionRequest.isCurrentContentSet == true)
                    {
                        //Сохраняем запрошенный объект в текущий набор контента
                        DesignerSaveContentObject(designerWindow.currentContentSetIndex);
                    }
                }
                //Иначе, если запрашивается загрузку объекта контента
                else if (designerActionRequest.actionType == DesignerActionType.LoadContentSetObject)
                {
                    //Загружаем запрошенный объект
                    DesignerLoadContentObject(
                        designerActionRequest.contentSetIndex,
                        designerActionRequest.objectIndex);
                }
                //Иначе, если запрашивается удаление объекта контента
                else if (designerActionRequest.actionType == DesignerActionType.DeleteContentSetObject)
                {
                    //Удаляем запрошенный объект
                    DesignerDeleteContentObject(
                        designerActionRequest.contentSetIndex,
                        designerActionRequest.objectIndex);
                }
                //Иначе, если запрашивается отображение списка на панели набора контента
                else if (designerActionRequest.actionType == DesignerActionType.DisplayContentSetPanelList)
                {
                    //Отображаем список соответственно запросу
                    DesignerDisplayContentSetPanelList(
                        designerActionRequest.isCurrentContentSet,
                        designerActionRequest.contentSetIndex);
                }
                //Иначе, если запрашивается отображение панели набора контента
                else if (designerActionRequest.actionType == DesignerActionType.DisplayContentSetPanel)
                {
                    //Если запрашивается отображение панели текущего набора контента
                    if (designerActionRequest.isCurrentContentSet == true)
                    {
                        //Отображаем панель текущего набора контента
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            true);
                    }
                    //Иначе
                    else
                    {
                        //Отображаем панель прочих наборов контента
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            true);
                    }
                }
                //Иначе, если запрашивается сокрытие панели набора контента
                else if (designerActionRequest.actionType == DesignerActionType.HideContentSetPanel)
                {
                    //Если запрашивается сокрытие панели текущего набора контента
                    if (designerActionRequest.isCurrentContentSet == true)
                    {
                        //Скрываем панель текущего набора контента
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            false);
                    }
                    //Иначе
                    else
                    {
                        //Скрываем панель прочих наборов контента
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            false);
                    }
                }
                //Иначе, если запрашивается открытие мастерской
                else if (designerActionRequest.actionType == DesignerActionType.OpenWorkshop)
                {
                    //Скрываем панель прочих и текущего наборов контента
                    DesignerDisplayContentSetPanel(
                        false,
                        false);

                    DesignerDisplayContentSetPanel(
                        true,
                        false);

                    //Запрашиваем сохранение набора контента
                    SaveContentSetRequest(designerWindow.currentContentSetIndex);

                    //Открываем окно мастерской
                    WorkshopOpenWindow();
                }
                //Иначе, если запрашивается открытие окна игры
                else if (designerActionRequest.actionType == DesignerActionType.OpenGame)
                {
                    //Скрываем панель прочих и текущего наборов контента
                    DesignerDisplayContentSetPanel(
                        false,
                        false);

                    DesignerDisplayContentSetPanel(
                        true,
                        false);

                    //Открываем окно игры
                    GameOpenWindow();
                }

                world.Value.DelEntity(designerActionRequestEntity);
            }

            //Если активен дизайнер кораблей
            if (designerWindow.designerType == DesignerType.ShipClass)
            {
                //Для каждого запроса действия в дизайнере классов кораблей
                foreach (int designerShipClassActionRequestEntity in designerShipClassActionRequestFilter.Value)
                {
                    //Берём запрос
                    ref RDesignerShipClassAction designerShipClassActionRequest = ref designerShipClassActionRequestPool.Value.Get(designerShipClassActionRequestEntity);

                    //Если запрашивается добавление компонента в класс корабля
                    if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.AddComponentToClass)
                    {
                        //Добавляем запрошенный компонент
                        DesignerShipClassAddComponentFirst(
                            designerShipClassActionRequest.componentType,
                            designerShipClassActionRequest.contentSetIndex,
                            designerShipClassActionRequest.modelIndex,
                            designerShipClassActionRequest.numberOfComponents);
                    }
                    //Иначе, если запрашивается удаление компонента из класса корабля
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.DeleteComponentFromClass)
                    {
                        //Удаляем запрошенный компонент
                        DesignerShipClassDeleteComponentFirst(
                            designerShipClassActionRequest.componentType,
                            designerShipClassActionRequest.contentSetIndex,
                            designerShipClassActionRequest.modelIndex,
                            designerShipClassActionRequest.numberOfComponents);
                    }
                    //Иначе, если запрашивается отображение подробной информации о компоненте
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.DisplayComponentDetailedInfo)
                    {
                        //Отображаем подробную информацию о компоненте
                        DesignerShipClassDisplayComponentDetailedInfo(
                            true,
                            designerShipClassActionRequest.contentSetIndex,
                            designerShipClassActionRequest.modelIndex,
                            designerShipClassActionRequest.componentType);

                    }
                    //Иначе, если запрашивается сокрытие подробной информации о компоненте
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.HideComponentDetailedInfo)
                    {
                        //Скрываем подробную информацию о компоненте
                        DesignerShipClassDisplayComponentDetailedInfo(false);
                    }
                    //Иначе, если запрашивается смена типа доступных компонентов
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.ChangeAvailableComponentsType)
                    {
                        //Отображаем доступные компоненты запрошенного типа
                        DesignerShipClassDisplayAvailableComponentsType(designerShipClassActionRequest.componentType);
                    }

                    world.Value.DelEntity(designerShipClassActionRequestEntity);
                }
            }
            //Иначе, если активен дизайнер компонентов
            else if (designerWindow.designerType >= DesignerType.ComponentEngine
                && designerWindow.designerType <= DesignerType.ComponentGunEnergy)
            {
                //Для каждого запроса действия в дизайнере компонентов
                foreach (int designerComponentActionRequestEntity in designerComponentActionRequestFilter.Value)
                {
                    //Берём запрос
                    ref RDesignerComponentAction designerComponentActionRequest = ref designerComponentActionRequestPool.Value.Get(designerComponentActionRequestEntity);

                    //Если запрашивается изменение основной технологии
                    if (designerComponentActionRequest.actionType == DesignerComponentActionType.ChangeCoreTechnology)
                    {
                        //Изменяем запрошенную основную технологию
                        DesignerComponentChangeCoreTechnology(
                            designerComponentActionRequest.componentCoreModifierType,
                            designerComponentActionRequest.technologyDropdownIndex);
                    }

                    world.Value.DelEntity(designerComponentActionRequestEntity);
                }
            }
        }

        void DesignerOpenWindow(
            int contentSetIndex,
            DesignerType designerType,
            bool isInGameDesigner,
            EcsPackedEntity factionPE = new EcsPackedEntity())
        {
            //Закрываем открытое главное окно
            CloseMainWindow();

            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Делаем окно дизайнера активным
            designerWindow.gameObject.SetActive(
                true);
            //И указываем его как активное окно в EUI
            eUI.Value.activeMainWindow
                = eUI.Value.designerWindow.gameObject;

            //Указываем, что активно окно дизайнера
            eUI.Value.activeMainWindowType
                = MainWindowType.Designer;


            //Указываем название активного дизайнера
            designerWindow.designerTypeName.text
                = designerType.ToString();

            //Если какой-то другой дизайнер был активным
            if (designerWindow.designerType
                != designerType
                && designerWindow.activeDesigner
                != null)
            {
                //Делаем его неактивным
                designerWindow.activeDesigner.SetActive(
                    false);
            }

            //Указываем индекс текущего набора контента
            designerWindow.currentContentSetIndex
                = contentSetIndex;


            //Создаём временные модификаторы технологий
            DTechnologyModifiers tempModifiers
                = new();
            //Создаём временный массив словарей технологий фракции
            Dictionary<int, DOrganizationTechnology>[] tempFactionTechnologies
                = new Dictionary<int, DOrganizationTechnology>[0];

            //Создаём ссылку на модификаторы технологий
            ref readonly DTechnologyModifiers technologyModifiers
                = ref tempModifiers;
            //Создаём ссылку на массив словарей технологий фракции
            ref readonly Dictionary<int, DOrganizationTechnology>[] factionTechnologies
                = ref tempFactionTechnologies;

            //Определяем индекс базового набора контента
            int baseContentSetIndex
                = 0;

            //Указываем, является ли открытый дизайнер внутриигровым
            designerWindow.isInGameDesigner
                = isInGameDesigner;

            //Если активен внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == true)
            {
                //Скрываем панель прочих наборов контента
                designerWindow.otherContentSetsList.gameObject.SetActive(
                    false);

                //На панели текущего набора контента скрываем кнопку удаления объекта
                designerWindow.currentContentSetList.deleteButton.gameObject.SetActive(
                    false);

                //Если переданная ссылка ведёт на сущность фракции
                if (factionPE.Unpack(world.Value, out int factionEntity))
                {
                    //Берём компонент фракции
                    ref COrganization faction
                        = ref organizationPool.Value.Get(factionEntity);

                    //Берём ссылку на модификаторы технологий фракции
                    technologyModifiers
                        = ref faction.technologyModifiers;

                    //Берём ссылку на массив словарей технологий фракции
                    factionTechnologies
                        = ref faction.technologies;
                }
            }
            //Иначе
            else
            {
                //Отображаем панель прочих наборов контента
                designerWindow.otherContentSetsList.gameObject.SetActive(
                    true);

                //На панели текущего набора контента отображаем кнопку удаления объекта
                designerWindow.currentContentSetList.deleteButton.gameObject.SetActive(
                    true);

                //Берём ссылку на общие модификаторы технологий
                technologyModifiers
                    = ref contentData.Value.globalTechnologyModifiers;

                //Берём ссылку на массив словарей общих технологий
                factionTechnologies
                    = ref contentData.Value.globalTechnologies;

                //Если индекс текущего набора контента равен нулю
                if (contentSetIndex
                    == 0)
                {
                    //Если в системе существует больше одного набора контента
                    if (contentData.Value.wDContentSets.Length
                        > 1)
                    {
                        //То базовым набором контента будет второй
                        baseContentSetIndex
                            = 1;
                    }
                }
                //Иначе базовым набором контента будет первый
            }

            //Если требуется открыть дизайнер кораблей
            if (designerType
                == DesignerType.ShipClass)
            {
                //Делаем активным дизайнер кораблей
                designerWindow.shipClassDesigner.gameObject.SetActive(
                    true);
                //Указываем его как активный дизайнер в окне дизайнера
                designerWindow.activeDesigner
                    = designerWindow.shipClassDesigner.gameObject;

                //Указываем, что активен дизайнер кораблей
                designerWindow.designerType
                    = DesignerType.ShipClass;

                //Отображаем дизайнер кораблей
                DesignerOpenShipClassWindow(
                    baseContentSetIndex);
            }
            //Иначе, если требуется открыть дизайнер двигателей
            else if (designerType
                == DesignerType.ComponentEngine)
            {
                //Делаем активным дизайнер двигателей
                designerWindow.engineDesigner.gameObject.SetActive(
                    true);
                //Указываем его как активный дизайнер в окне дизайнера
                designerWindow.activeDesigner
                    = designerWindow.engineDesigner.gameObject;

                //Указываем, что активен дизайнер двигателей
                designerWindow.designerType
                    = DesignerType.ComponentEngine;

                //Отображаем дизайнер двигателей
                DesignerOpenEngineComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //Иначе, если требуется открыть дизайнер реакторов
            else if (designerType
                == DesignerType.ComponentReactor)
            {
                //Делаем активным дизайнер реакторов
                designerWindow.reactorDesigner.gameObject.SetActive(
                    true);
                //Указываем его как активный дизайнер в окне дизайнера
                designerWindow.activeDesigner
                    = designerWindow.reactorDesigner.gameObject;

                //Указываем, что активен дизайнер реакторов
                designerWindow.designerType
                    = DesignerType.ComponentReactor;

                //Отображаем дизайнер реакторов
                DesignerOpenReactorComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //Иначе, если требуется открыть дизайнер топливных баков
            else if (designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //Делаем активным дизайнер топливных баков
                designerWindow.fuelTankDesigner.gameObject.SetActive(
                    true);
                //Указываем его как активный дизайнер в окне дизайнера
                designerWindow.activeDesigner
                    = designerWindow.fuelTankDesigner.gameObject;

                //Указываем, что активен дизайнер топливных баков
                designerWindow.designerType
                    = DesignerType.ComponentHoldFuelTank;

                //Отображаем дизайнер топливных баков
                DesignerOpenFuelTankComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //Иначе, если требуется открыть дизайнер оборудования для твёрдой добычи
            else if (designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //Делаем активным дизайнер оборудования для твёрдой добычи
                designerWindow.extractionEquipmentDesigner.gameObject.SetActive(
                    true);
                //Указываем его как активный дизайнер в окне дизайнера
                designerWindow.activeDesigner
                    = designerWindow.extractionEquipmentDesigner.gameObject;

                //Указываем, что активен дизайнер оборудования для твёрдой добычи
                designerWindow.designerType
                    = DesignerType.ComponentExtractionEquipmentSolid;

                //Отображаем дизайнер оборудования для твёрдой добычи
                DesignerOpenExtractionEquipmentComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //Иначе, если требуется открыть дизайнер энергетических орудий
            else if (designerType
                == DesignerType.ComponentGunEnergy)
            {
                //Делаем активным дизайнер энергетических орудий
                designerWindow.energyGunDesigner.gameObject.SetActive(
                    true);
                //Указываем его как активный дизайнер в окне дизайнера
                designerWindow.activeDesigner
                    = designerWindow.energyGunDesigner.gameObject;

                //Указываем, что активен дизайнер энергетических орудий
                designerWindow.designerType
                    = DesignerType.ComponentGunEnergy;

                //Отображаем дизайнер энергетических орудий
                DesignerOpenEnergyGunComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
        }

        void DesignerDisplayContentSetPanel(
            bool isCurrentContentSet,
            bool isDisplay)
        {
            //Если указан список текущего набора контента
            if (isCurrentContentSet == true)
            {
                //Если требуется отобразить список текущего набора контента
                if (isDisplay == true)
                {
                    //Отображаем список
                    eUI.Value.designerWindow.currentContentSetList.listPanel.SetActive(true);

                    //Скрываем кнопку отображения списка
                    eUI.Value.designerWindow.currentContentSetList.displayButton.gameObject.SetActive(false);

                    //Отображаем кнопку сокрытия списка
                    eUI.Value.designerWindow.currentContentSetList.hideButton.gameObject.SetActive(true);
                }
                //Иначе
                else
                {
                    //Очищаем графу названия объекта
                    eUI.Value.designerWindow.currentContentSetList.objectName.text
                        = "";

                    //Скрываем список
                    eUI.Value.designerWindow.currentContentSetList.listPanel.SetActive(false);

                    //Скрываем кнопку сокрытия списка
                    eUI.Value.designerWindow.currentContentSetList.hideButton.gameObject.SetActive(false);

                    //Отображаем кнопку отображения списка
                    eUI.Value.designerWindow.currentContentSetList.displayButton.gameObject.SetActive(true);
                }
            }
            //Иначе указан список прочих наборов контента
            else
            {
                //Если требуется отобразить список прочих наборов контента
                if (isDisplay == true)
                {
                    //Отображаем список
                    eUI.Value.designerWindow.otherContentSetsList.listPanel.SetActive(true);

                    //Скрываем кнопку отображения списка
                    eUI.Value.designerWindow.otherContentSetsList.displayButton.gameObject.SetActive(false);

                    //Отображаем кнопку сокрытия списка
                    eUI.Value.designerWindow.otherContentSetsList.hideButton.gameObject.SetActive(true);
                }
                //Иначе
                else
                {
                    //Скрываем список
                    eUI.Value.designerWindow.otherContentSetsList.listPanel.SetActive(false);

                    //Скрываем кнопку сокрытия списка
                    eUI.Value.designerWindow.otherContentSetsList.hideButton.gameObject.SetActive(false);

                    //Отображаем кнопку отображения списка
                    eUI.Value.designerWindow.otherContentSetsList.displayButton.gameObject.SetActive(true);
                }
            }
        }

        void DesignerDisplayContentSetPanelList(
            bool isCurrentContentSet,
            int contentSetIndex = -1)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если обновить требуется панель текущего набора контента
            if (isCurrentContentSet
                == true)
            {
                //Отображаем список контента из текущего набора соответственно типу дизайнера
                DesignerDisplayContentPanels(
                    designerWindow.designerType,
                    DesignerDisplayContentType.ContentSet,
                    designerWindow.currentContentSetList.toggleGroup,
                    designerWindow.currentContentSetList.layoutGroup,
                    designerWindow.currentContentSetList.panelsList,
                    designerWindow.currentContentSetIndex);
            }
            //Иначе
            else
            {
                //Отображаем список контента на панели прочих наборов контента соответственно типу дизайнера
                DesignerDisplayContentPanels(
                    designerWindow.designerType,
                    DesignerDisplayContentType.ContentSet,
                    designerWindow.otherContentSetsList.toggleGroup,
                    designerWindow.otherContentSetsList.layoutGroup,
                    designerWindow.otherContentSetsList.panelsList,
                    contentSetIndex);
            }
        }

        void DesignerDisplayOtherContentSetDropdown(
            int contentSetIndex)
        {
            //Очищаем выпадающий список прочих наборов контента
            eUI.Value.designerWindow.otherContentSetsList.dropdown.ClearOptions();

            //Заполняем выпадающий список прочих наборов контента
            eUI.Value.designerWindow.otherContentSetsList.dropdown.AddOptions(
                new List<string>(
                    contentData.Value.contentSetNames));

            //Если индекс набора контента больше или равен нулю
            if (contentSetIndex
                >= 0)
            {
                //Удаляем текущий набор контента из выпадающего списка
                eUI.Value.designerWindow.otherContentSetsList.dropdown.options.RemoveAt(contentSetIndex);
            }
        }

        void DesignerDisplayContentPanels(
            DesignerType contentType,
            DesignerDisplayContentType displayType,
            ToggleGroup parentToggleGroup,
            VerticalLayoutGroup parentLayoutGroup,
            List<GameObject> parentPanelsList,
            int contentSetIndex = -2,
            bool isClear = true,
            EcsPackedEntity factionPE = new EcsPackedEntity())
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если требуется очистить родительский список
            if (isClear == true)
            {
                //Если в списке были панели
                for (int a = 0; a < parentPanelsList.Count; a++)
                {
                    MonoBehaviour.Destroy(parentPanelsList[a]);
                }

                //Очищаем список
                parentPanelsList.Clear();
            }

            //Если требуется отобразить корабли
            if (contentType == DesignerType.ShipClass)
            {
                //Если требуется отобразить отдельный набор контента
                if (displayType == DesignerDisplayContentType.ContentSet)
                {
                    //Создаём список для сортировки кораблей
                    List<Tuple<string, int>> shipsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner == true)
                    {
                        //Для каждого корабля в указанном наборе контента
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].shipClasses.Length; a++)
                        {
                            //Заносим неазвание и индекс корабля в список
                            shipsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].shipClasses[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        shipsSortList = shipsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого корабля в отсортированном списке
                        for (int a = 0; a < shipsSortList.Count; a++)
                        {
                            //Берём ссылку на данные корабля
                            ref readonly DShipClass shipClass
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .shipClasses[shipsSortList[a].Item2];

                            //Создаём обзорную панель корабля в указанном списке
                            UIShipClassBriefInfoPanel shipBriefInfoPanel
                                = designerWindow.InstantiateShipBriefInfoPanel(
                                    shipClass,
                                    contentSetIndex, shipsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого корабля в указанном наборе контента
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length; a++)
                        {
                            //Заносим неазвание и индекс корабля в список
                            shipsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        shipsSortList
                            = shipsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого корабля в отсортированном списке
                        for (int a = 0; a < shipsSortList.Count; a++)
                        {
                            //Берём ссылку на данные корабля
                            ref readonly WDShipClass shipClass
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .shipClasses[shipsSortList[a].Item2];

                            //Создаём обзорную панель корабля в указанном списке
                            UIShipClassBriefInfoPanel shipBriefInfoPanel
                                = designerWindow.InstantiateShipBriefInfoPanel(
                                    shipClass,
                                    contentSetIndex, shipsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
            }
            //Иначе, если требуется отобразить двигателм
            else if (contentType == DesignerType.ComponentEngine)
            {
                //Если требуется отобразить объекты из всех наборов контента
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //Создаём список для сортировки двигателей
                    List<Tuple<string, int, int>> enginesSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //Для каждого двигателя в наборе контента
                            for (int b = 0; b < contentData.Value.contentSets[a].engines.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс двигателя в список
                                enginesSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].engines[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого двигателя в отсортированном списке
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //Берём ссылку на данные двигателя
                            ref readonly DEngine engine
                                = ref contentData.Value
                                .contentSets[enginesSortList[a].Item2]
                                .engines[enginesSortList[a].Item3];

                            //Создаём обзорную панель двигателя в указанном списке
                            UIEngineBriefInfoPanel engineBriefInfoPanel
                                = designerWindow.InstantiateEngineBriefInfoPanel(
                                    engine,
                                    enginesSortList[a].Item2, enginesSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //Для каждого двигателя в наборе контента
                            for (int b = 0; b < contentData.Value.wDContentSets[a].engines.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс двигателя в список
                                enginesSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].engines[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого двигателя в отсортированном списке
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //Берём ссылку на данные двигателя
                            ref readonly WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[enginesSortList[a].Item2]
                                .engines[enginesSortList[a].Item3];

                            //Создаём обзорную панель двигателя в указанном списке
                            UIEngineBriefInfoPanel engineBriefInfoPanel
                                = designerWindow.InstantiateEngineBriefInfoPanel(
                                    engine,
                                    enginesSortList[a].Item2, enginesSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить отдельный набор контента
                else if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //Создаём список для сортировки двигателей
                    List<Tuple<string, int>> enginesSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого двигателя в указанном наборе контента
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].engines.Length; a++)
                        {
                            //Заносим название и индекс двигателя в список
                            enginesSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].engines[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого двигателя в отсортированном списке
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //Берём ссылку на данные двигателя
                            ref readonly DEngine engine
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .engines[enginesSortList[a].Item2];

                            //Создаём обзорную панель двигателя в указанном списке
                            UIEngineBriefInfoPanel engineBriefInfoPanel
                                = designerWindow.InstantiateEngineBriefInfoPanel(
                                    engine,
                                    contentSetIndex, enginesSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого двигателя в указанном наборе контента
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].engines.Length; a++)
                        {
                            //Заносим название и индекс двигателя в список
                            enginesSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].engines[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого двигателя в отсортированном списке
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //Берём ссылку на данные двигателя
                            ref readonly WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .engines[enginesSortList[a].Item2];

                            //Создаём обзорную панель двигателя в указанном списке
                            UIEngineBriefInfoPanel engineBriefInfoPanel
                                = designerWindow.InstantiateEngineBriefInfoPanel(
                                    engine,
                                    contentSetIndex, enginesSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить объекты, включённые в редактируемый корабль
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //Берём ссылку на окно дизайнера корабля
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //Создаём список для сортировки двигателей
                    List<Tuple<string, int, int, int>> enginesSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого двигателя в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.engines.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс двигателя в список
                            enginesSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .contentSets[shipDesignerWindow.currentGameShipClass.engines[a].ContentSetIndex]
                                    .engines[shipDesignerWindow.currentGameShipClass.engines[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentGameShipClass.engines[a].numberOfComponents,
                                    shipDesignerWindow.currentGameShipClass.engines[a].ContentSetIndex,
                                    shipDesignerWindow.currentGameShipClass.engines[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого двигателя в отсортированном списке
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //Берём ссылку на данные двигателя
                            ref readonly DEngine engine
                                = ref contentData.Value
                                .contentSets[enginesSortList[a].Item3]
                                .engines[enginesSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel engineInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    enginesSortList[a].Item3, enginesSortList[a].Item4,
                                    ShipComponentType.Engine,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название двигателя
                            engineInstalledComponentPanel.modelName.text
                                = engine.ObjectName;

                            //Указываем название типа компонента
                            engineInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Engine.ToString();

                            //Указываем число двигателей
                            engineInstalledComponentPanel.componentNumber.text
                                = enginesSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            engineInstalledComponentPanel.componentTotalSize.text
                                = (enginesSortList[a].Item2 * engine.EngineSize).ToString();
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого двигателя в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.engines.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс двигателя в список
                            enginesSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .wDContentSets[shipDesignerWindow.currentWorkshopShipClass.engines[a].ContentSetIndex]
                                    .engines[shipDesignerWindow.currentWorkshopShipClass.engines[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentWorkshopShipClass.engines[a].numberOfComponents,
                                    shipDesignerWindow.currentWorkshopShipClass.engines[a].ContentSetIndex,
                                    shipDesignerWindow.currentWorkshopShipClass.engines[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого двигателя в отсортированном списке
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //Берём ссылку на данные двигателя
                            ref readonly WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[enginesSortList[a].Item3]
                                .engines[enginesSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel engineInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    enginesSortList[a].Item3, enginesSortList[a].Item4,
                                    ShipComponentType.Engine,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название двигателя
                            engineInstalledComponentPanel.modelName.text
                                = engine.ObjectName;

                            //Указываем название типа компонента
                            engineInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Engine.ToString();

                            //Указываем число двигателей
                            engineInstalledComponentPanel.componentNumber.text
                                = enginesSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            engineInstalledComponentPanel.componentTotalSize.text
                                = (enginesSortList[a].Item2 * engine.EngineSize).ToString();
                        }
                    }
                }
            }
            //Иначе, если требуется отобразить реакторы
            else if (contentType == DesignerType.ComponentReactor)
            {
                //Если требуется отобразить объекты из всех наборов контента
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //Создаём список для сортировки реакторов
                    List<Tuple<string, int, int>> reactorsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //Для каждого реактора в наборе контента
                            for (int b = 0; b < contentData.Value.contentSets[a].reactors.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс реактора в список
                                reactorsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].reactors[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого реактора в отсортированном списке
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //Берём ссылку на данные реактора
                            ref readonly DReactor reactor
                                = ref contentData.Value
                                .contentSets[reactorsSortList[a].Item2]
                                .reactors[reactorsSortList[a].Item3];

                            //Создаём обзорную панель реактора в указанном списке
                            UIReactorBriefInfoPanel reactorBriefInfoPanel
                                = designerWindow.InstantiateReactorBriefInfoPanel(
                                    reactor,
                                    reactorsSortList[a].Item2, reactorsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //Для каждого реактора в наборе контента
                            for (int b = 0; b < contentData.Value.wDContentSets[a].reactors.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс реактора в список
                                reactorsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].reactors[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого реактора в отсортированном списке
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //Берём ссылку на данные реактора
                            ref readonly WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[reactorsSortList[a].Item2]
                                .reactors[reactorsSortList[a].Item3];

                            //Создаём обзорную панель реактора в указанном списке
                            UIReactorBriefInfoPanel reactorBriefInfoPanel
                                = designerWindow.InstantiateReactorBriefInfoPanel(
                                    reactor,
                                    reactorsSortList[a].Item2, reactorsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить отдельный набор контента
                if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //Создаём список для сортировки реакторов
                    List<Tuple<string, int>> reactorsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого реактора в указанном наборе контента
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].reactors.Length; a++)
                        {
                            //Заносим название и индекс реактора в список
                            reactorsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].reactors[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого реактора в отсортированном списке
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //Берём ссылку на данные реактора
                            ref readonly DReactor reactor
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .reactors[reactorsSortList[a].Item2];

                            //Создаём обзорную панель реактора в указанном списке
                            UIReactorBriefInfoPanel reactorBriefInfoPanel
                                = designerWindow.InstantiateReactorBriefInfoPanel(
                                    reactor,
                                    contentSetIndex, reactorsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого реактора в указанном наборе контента
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].reactors.Length; a++)
                        {
                            //Заносим название и индекс реактора в список
                            reactorsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].reactors[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого реактора в отсортированном списке
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //Берём ссылку на данные реактора
                            ref readonly WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .reactors[reactorsSortList[a].Item2];

                            //Создаём обзорную панель реактора в указанном списке
                            UIReactorBriefInfoPanel reactorBriefInfoPanel
                                = designerWindow.InstantiateReactorBriefInfoPanel(
                                    reactor,
                                    contentSetIndex, reactorsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить объекты, включённые в редактируемый корабль
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //Берём ссылку на окно дизайнера корабля
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //Создаём список для сортировки реакторов
                    List<Tuple<string, int, int, int>> reactorsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого реактора в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.reactors.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс реактора в список
                            reactorsSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .contentSets[shipDesignerWindow.currentGameShipClass.reactors[a].ContentSetIndex]
                                    .reactors[shipDesignerWindow.currentGameShipClass.reactors[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentGameShipClass.reactors[a].numberOfComponents,
                                    shipDesignerWindow.currentGameShipClass.reactors[a].ContentSetIndex,
                                    shipDesignerWindow.currentGameShipClass.reactors[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого реактора в отсортированном списке
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //Берём ссылку на данные реактора
                            ref readonly DReactor reactor
                                = ref contentData.Value
                                .contentSets[reactorsSortList[a].Item3]
                                .reactors[reactorsSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel reactorInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    reactorsSortList[a].Item3, reactorsSortList[a].Item4,
                                    ShipComponentType.Reactor,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название реактора
                            reactorInstalledComponentPanel.modelName.text
                                = reactor.ObjectName;

                            //Указываем название типа компонента
                            reactorInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Reactor.ToString();

                            //Указываем число реакторов
                            reactorInstalledComponentPanel.componentNumber.text
                                = reactorsSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            reactorInstalledComponentPanel.componentTotalSize.text
                                = (reactorsSortList[a].Item2 * reactor.ReactorSize).ToString();
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого реактора в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.reactors.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс реактора в список
                            reactorsSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .wDContentSets[shipDesignerWindow.currentWorkshopShipClass.reactors[a].ContentSetIndex]
                                    .reactors[shipDesignerWindow.currentWorkshopShipClass.reactors[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentWorkshopShipClass.reactors[a].numberOfComponents,
                                    shipDesignerWindow.currentWorkshopShipClass.reactors[a].ContentSetIndex,
                                    shipDesignerWindow.currentWorkshopShipClass.reactors[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого реактора в отсортированном списке
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //Берём ссылку на данные реактора
                            ref readonly WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[reactorsSortList[a].Item3]
                                .reactors[reactorsSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel reactorInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    reactorsSortList[a].Item3, reactorsSortList[a].Item4,
                                    ShipComponentType.Reactor,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название реактора
                            reactorInstalledComponentPanel.modelName.text
                                = reactor.ObjectName;

                            //Указываем название типа компонента
                            reactorInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Reactor.ToString();

                            //Указываем число реакторов
                            reactorInstalledComponentPanel.componentNumber.text
                                = reactorsSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            reactorInstalledComponentPanel.componentTotalSize.text
                                = (reactorsSortList[a].Item2 * reactor.ReactorSize).ToString();
                        }
                    }
                }
            }
            //Иначе, если требуется отобразить топливные баки
            else if (contentType == DesignerType.ComponentHoldFuelTank)
            {
                //Если требуется отобразить объекты из всех наборов контента
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //Создаём список для сортировки топливных баков
                    List<Tuple<string, int, int>> fuelTanksSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //Для каждого топливного бака в наборе контента
                            for (int b = 0; b < contentData.Value.contentSets[a].fuelTanks.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс топливного бака в список
                                fuelTanksSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].fuelTanks[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого топливного бака в отсортированном списке
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //Берём ссылку на данные топливного бака
                            ref readonly DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[fuelTanksSortList[a].Item2]
                                .fuelTanks[fuelTanksSortList[a].Item3];

                            //Создаём обзорную панель топливного бака в указанном списке
                            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                                = designerWindow.InstantiateFuelTankBriefInfoPanel(
                                    fuelTank,
                                    fuelTanksSortList[a].Item2, fuelTanksSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //Для каждого топливного бака в наборе контента
                            for (int b = 0; b < contentData.Value.wDContentSets[a].fuelTanks.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс топливного бака в список
                                fuelTanksSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].fuelTanks[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого топливного бака в отсортированном списке
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //Берём ссылку на данные топливного бака
                            ref readonly WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[fuelTanksSortList[a].Item2]
                                .fuelTanks[fuelTanksSortList[a].Item3];

                            //Создаём обзорную панель топливного бака в указанном списке
                            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                                = designerWindow.InstantiateFuelTankBriefInfoPanel(
                                    fuelTank,
                                    fuelTanksSortList[a].Item2, fuelTanksSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить отдельный набор контента
                if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //Создаём список для сортировки топливных баков
                    List<Tuple<string, int>> fuelTanksSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого топливного бака в указанном наборе контента
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].fuelTanks.Length; a++)
                        {
                            //Заносим название и индекс топливного бака в список
                            fuelTanksSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].fuelTanks[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого топливного бака в отсортированном списке
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //Берём ссылку на данные топливного бака
                            ref readonly DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .fuelTanks[fuelTanksSortList[a].Item2];

                            //Создаём обзорную панель топливного бака в указанном списке
                            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                                = designerWindow.InstantiateFuelTankBriefInfoPanel(
                                    fuelTank,
                                    contentSetIndex, fuelTanksSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого топливного бака в указанном наборе контента
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length; a++)
                        {
                            //Заносим название и индекс топливного бака в список
                            fuelTanksSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].fuelTanks[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого топливного бака в отсортированном списке
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //Берём ссылку на данные топливного бака
                            ref readonly WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .fuelTanks[fuelTanksSortList[a].Item2];

                            //Создаём обзорную панель топливного бака в указанном списке
                            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                                = designerWindow.InstantiateFuelTankBriefInfoPanel(
                                    fuelTank,
                                    contentSetIndex, fuelTanksSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить объекты, включённые в редактируемый корабль
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //Берём ссылку на окно дизайнера корабля
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //Создаём список для сортировки топливных баков
                    List<Tuple<string, int, int, int>> fuelTanksSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого топливного бака в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.fuelTanks.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс топливного бака в список
                            fuelTanksSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .contentSets[shipDesignerWindow.currentGameShipClass.fuelTanks[a].ContentSetIndex]
                                    .fuelTanks[shipDesignerWindow.currentGameShipClass.fuelTanks[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentGameShipClass.fuelTanks[a].numberOfComponents,
                                    shipDesignerWindow.currentGameShipClass.fuelTanks[a].ContentSetIndex,
                                    shipDesignerWindow.currentGameShipClass.fuelTanks[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого топливного бака в отсортированном списке
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //Берём ссылку на данные топливного бака
                            ref readonly DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[fuelTanksSortList[a].Item3]
                                .fuelTanks[fuelTanksSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel fuelTankInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    fuelTanksSortList[a].Item3, fuelTanksSortList[a].Item4,
                                    ShipComponentType.HoldFuelTank,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название топливного бака
                            fuelTankInstalledComponentPanel.modelName.text
                                = fuelTank.ObjectName;

                            //Указываем название типа компонента
                            fuelTankInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.HoldFuelTank.ToString();

                            //Указываем число топливных баков
                            fuelTankInstalledComponentPanel.componentNumber.text
                                = fuelTanksSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            fuelTankInstalledComponentPanel.componentTotalSize.text
                                = (fuelTanksSortList[a].Item2 * fuelTank.Size).ToString();
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого топливного бака в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.fuelTanks.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс топливного бака в список
                            fuelTanksSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .wDContentSets[shipDesignerWindow.currentWorkshopShipClass.fuelTanks[a].ContentSetIndex]
                                    .fuelTanks[shipDesignerWindow.currentWorkshopShipClass.fuelTanks[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentWorkshopShipClass.fuelTanks[a].numberOfComponents,
                                    shipDesignerWindow.currentWorkshopShipClass.fuelTanks[a].ContentSetIndex,
                                    shipDesignerWindow.currentWorkshopShipClass.fuelTanks[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого топливного бака в отсортированном списке
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //Берём ссылку на данные топливного бака
                            ref readonly WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[fuelTanksSortList[a].Item3]
                                .fuelTanks[fuelTanksSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel fuelTankInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    fuelTanksSortList[a].Item3, fuelTanksSortList[a].Item4,
                                    ShipComponentType.HoldFuelTank,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название топливного бака
                            fuelTankInstalledComponentPanel.modelName.text
                                = fuelTank.ObjectName;

                            //Указываем название типа компонента
                            fuelTankInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.HoldFuelTank.ToString();

                            //Указываем число топливных баков
                            fuelTankInstalledComponentPanel.componentNumber.text
                                = fuelTanksSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            fuelTankInstalledComponentPanel.componentTotalSize.text
                                = (fuelTanksSortList[a].Item2 * fuelTank.Size).ToString();
                        }
                    }
                }
            }
            //Иначе, если требуется отобразить оборудование для твёрдой добычи
            else if (contentType == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //Если требуется отобразить объекты из всех наборов контента
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //Создаём список для сортировки оборудования для твёрдой добычи
                    List<Tuple<string, int, int>> extractionEquipmentSolidsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //Для каждого оборудования для твёрдой добычи в наборе контента
                            for (int b = 0; b < contentData.Value.contentSets[a].solidExtractionEquipments.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс оборудования для твёрдой добычи в список
                                extractionEquipmentSolidsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].solidExtractionEquipments[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого оборудования для твёрдой добычи в отсортированном списке
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //Берём ссылку на данные оборудования для твёрдой добычи
                            ref readonly DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[extractionEquipmentSolidsSortList[a].Item2]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item3];

                            //Создаём обзорную панель оборудования для твёрдой добычи в указанном списке
                            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                                = designerWindow.InstantiateExtractionEquipmentBriefInfoPanel(
                                    extractionEquipmentSolid,
                                    extractionEquipmentSolidsSortList[a].Item2, extractionEquipmentSolidsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //Для каждого оборудования для твёрдой добычи в наборе контента
                            for (int b = 0; b < contentData.Value.wDContentSets[a].solidExtractionEquipments.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс оборудования для твёрдой добычи в список
                                extractionEquipmentSolidsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].solidExtractionEquipments[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого оборудования для твёрдой добычи в отсортированном списке
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //Берём ссылку на данные оборудования для твёрдой добычи
                            ref readonly WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[extractionEquipmentSolidsSortList[a].Item2]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item3];

                            //Создаём обзорную панель оборудования для твёрдой добычи в указанном списке
                            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                                = designerWindow.InstantiateExtractionEquipmentBriefInfoPanel(
                                    extractionEquipmentSolid,
                                    extractionEquipmentSolidsSortList[a].Item2, extractionEquipmentSolidsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить отдельный набор контента
                if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //Создаём список для сортировки оборудования для твёрдой добычи
                    List<Tuple<string, int>> extractionEquipmentsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого оборудования для твёрдой добычи в указанном наборе контента
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                        {
                            //Заносим название и индекс оборудования для твёрдой добычи в список
                            extractionEquipmentsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        extractionEquipmentsSortList
                            = extractionEquipmentsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого оборудования для твёрдой добычи в отсортированном списке
                        for (int a = 0; a < extractionEquipmentsSortList.Count; a++)
                        {
                            //Берём ссылку на данные оборудования для твёрдой добычи
                            ref readonly DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .solidExtractionEquipments[extractionEquipmentsSortList[a].Item2];

                            //Создаём обзорную панель оборудования для твёрдой добычи в указанном списке
                            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                                = designerWindow.InstantiateExtractionEquipmentBriefInfoPanel(
                                    extractionEquipmentSolid,
                                    contentSetIndex, extractionEquipmentsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого оборудования для твёрдой добычи в указанном наборе контента
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                        {
                            //Заносим название и индекс оборудования для твёрдой добычи в список
                            extractionEquipmentsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        extractionEquipmentsSortList
                            = extractionEquipmentsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого оборудования для твёрдой добычи в отсортированном списке
                        for (int a = 0; a < extractionEquipmentsSortList.Count; a++)
                        {
                            //Берём ссылку на данные оборудования для твёрдой добычи
                            ref readonly WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .solidExtractionEquipments[extractionEquipmentsSortList[a].Item2];

                            //Создаём обзорную панель оборудования для твёрдой добычи в указанном списке
                            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                                = designerWindow.InstantiateExtractionEquipmentBriefInfoPanel(
                                    extractionEquipmentSolid,
                                    contentSetIndex, extractionEquipmentsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить объекты, включённые в редактируемый корабль
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //Берём ссылку на окно дизайнера корабля
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //Создаём список для сортировки оборудования для твёрдой добычи
                    List<Tuple<string, int, int, int>> extractionEquipmentSolidsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого оборудования для твёрдой добычи в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс оборудования для твёрдой добычи в список
                            extractionEquipmentSolidsSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .contentSets[shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids[a].ContentSetIndex]
                                    .solidExtractionEquipments[shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids[a].numberOfComponents,
                                    shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids[a].ContentSetIndex,
                                    shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого оборудования для твёрдой добычи в отсортированном списке
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //Берём ссылку на данные оборудования для твёрдой добычи
                            ref readonly DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[extractionEquipmentSolidsSortList[a].Item3]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel extractionEquipmentInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    extractionEquipmentSolidsSortList[a].Item3, extractionEquipmentSolidsSortList[a].Item4,
                                    ShipComponentType.ExtractionEquipmentSolid,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название оборудования для твёрдой добычи
                            extractionEquipmentInstalledComponentPanel.modelName.text
                                = extractionEquipmentSolid.ObjectName;

                            //Указываем название типа компонента
                            extractionEquipmentInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.ExtractionEquipmentSolid.ToString();

                            //Указываем число оборудования для твёрдой добычи
                            extractionEquipmentInstalledComponentPanel.componentNumber.text
                                = extractionEquipmentSolidsSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            extractionEquipmentInstalledComponentPanel.componentTotalSize.text
                                = (extractionEquipmentSolidsSortList[a].Item2 * extractionEquipmentSolid.Size).ToString();
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого оборудования для твёрдой добычи в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс оборудования для твёрдой добычи в список
                            extractionEquipmentSolidsSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .wDContentSets[shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids[a].ContentSetIndex]
                                    .solidExtractionEquipments[shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids[a].numberOfComponents,
                                    shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids[a].ContentSetIndex,
                                    shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого оборудования для твёрдой добычи в отсортированном списке
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //Берём ссылку на данные оборудования для твёрдой добычи
                            ref readonly WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[extractionEquipmentSolidsSortList[a].Item3]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel extractionEquipmentInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    extractionEquipmentSolidsSortList[a].Item3, extractionEquipmentSolidsSortList[a].Item4,
                                    ShipComponentType.ExtractionEquipmentSolid,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название оборудования для твёрдой добычи
                            extractionEquipmentInstalledComponentPanel.modelName.text
                                = extractionEquipmentSolid.ObjectName;

                            //Указываем название типа компонента
                            extractionEquipmentInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.ExtractionEquipmentSolid.ToString();

                            //Указываем число оборудования для твёрдой добычи
                            extractionEquipmentInstalledComponentPanel.componentNumber.text
                                = extractionEquipmentSolidsSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            extractionEquipmentInstalledComponentPanel.componentTotalSize.text
                                = (extractionEquipmentSolidsSortList[a].Item2 * extractionEquipmentSolid.Size).ToString();
                        }
                    }
                }
            }
            //Иначе, если требуется отобразить энергетические орудия
            else if (contentType == DesignerType.ComponentGunEnergy)
            {
                //Если требуется отобразить объекты из всех наборов контента
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //Создаём список для сортировки энергетических орудий
                    List<Tuple<string, int, int>> energyGunsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //Для каждого энергетического орудия в наборе контента
                            for (int b = 0; b < contentData.Value.contentSets[a].energyGuns.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс энергетического орудия в список
                                energyGunsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].energyGuns[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого энергетического орудия в отсортированном списке
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //Берём ссылку на данные энергетического орудия
                            ref readonly DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[energyGunsSortList[a].Item2]
                                .energyGuns[energyGunsSortList[a].Item3];

                            //Создаём обзорную панель энергетического орудия в указанном списке
                            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                                = designerWindow.InstantiateEnergyGunBriefInfoPanel(
                                    energyGun,
                                    energyGunsSortList[a].Item2, energyGunsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого набора контента
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //Для каждого энергетического орудия в наборе контента
                            for (int b = 0; b < contentData.Value.wDContentSets[a].energyGuns.Length; b++)
                            {
                                //Заносим название, индекс набора контента и индекс энергетического орудия в список
                                energyGunsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].energyGuns[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //Сортируем список по названию в алфавитном порядке
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого энергетического орудия в отсортированном списке
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //Берём ссылку на данные энергетического орудия
                            ref readonly WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[energyGunsSortList[a].Item2]
                                .energyGuns[energyGunsSortList[a].Item3];

                            //Создаём обзорную панель энергетического орудия в указанном списке
                            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                                = designerWindow.InstantiateEnergyGunBriefInfoPanel(
                                    energyGun,
                                    energyGunsSortList[a].Item2, energyGunsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить отдельный набор контента
                else if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //Создаём список для сортировки энергетических орудий
                    List<Tuple<string, int>> energyGunsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого энергетического орудия в указанном наборе контента
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].energyGuns.Length; a++)
                        {
                            //Заносим название и индекс энергетического орудия в список
                            energyGunsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].energyGuns[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого энергетического орудия в отсортированном списке
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //Берём ссылку на данные энергетического орудия
                            ref readonly DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .energyGuns[energyGunsSortList[a].Item2];

                            //Создаём обзорную панель энергетического орудия в указанном списке
                            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                                = designerWindow.InstantiateEnergyGunBriefInfoPanel(
                                    energyGun,
                                    contentSetIndex, energyGunsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого энергетического орудия в указанном наборе контента
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length; a++)
                        {
                            //Заносим название и индекс энергетического орудия в список
                            energyGunsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].energyGuns[a].ObjectName,
                                    a));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого энергетического орудия в отсортированном списке
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //Берём ссылку на данные энергетического орудия
                            ref readonly WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .energyGuns[energyGunsSortList[a].Item2];

                            //Создаём обзорную панель энергетического орудия в указанном списке
                            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                                = designerWindow.InstantiateEnergyGunBriefInfoPanel(
                                    energyGun,
                                    contentSetIndex, energyGunsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                }
                //Иначе, если требуется отобразить объекты, включённые в редактируемый корабль
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //Берём ссылку на окно дизайнера корабля
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //Создаём список для сортировки энергетических орудий
                    List<Tuple<string, int, int, int>> energyGunsSortList
                        = new();

                    //Если активен внутриигровой дизайнер
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //Для каждого энергетического орудия в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.energyGuns.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс энергетического орудия в список
                            energyGunsSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .contentSets[shipDesignerWindow.currentGameShipClass.energyGuns[a].ContentSetIndex]
                                    .energyGuns[shipDesignerWindow.currentGameShipClass.energyGuns[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentGameShipClass.energyGuns[a].numberOfComponents,
                                    shipDesignerWindow.currentGameShipClass.energyGuns[a].ContentSetIndex,
                                    shipDesignerWindow.currentGameShipClass.energyGuns[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого энергетического орудия в отсортированном списке
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //Берём ссылку на данные энергетического орудия
                            ref readonly DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[energyGunsSortList[a].Item3]
                                .energyGuns[energyGunsSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel energyGunInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    energyGunsSortList[a].Item3, energyGunsSortList[a].Item4,
                                    ShipComponentType.GunEnergy,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название энергетического орудия
                            energyGunInstalledComponentPanel.modelName.text
                                = energyGun.ObjectName;

                            //Указываем название типа компонента
                            energyGunInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.GunEnergy.ToString();

                            //Указываем число энергетических орудий
                            energyGunInstalledComponentPanel.componentNumber.text
                                = energyGunsSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            energyGunInstalledComponentPanel.componentTotalSize.text
                                = (energyGunsSortList[a].Item2 * energyGun.Size).ToString();
                        }
                    }
                    //Иначе
                    else
                    {
                        //Для каждого энергетического орудия в данных редактируемого корабля
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.energyGuns.Length; a++)
                        {
                            //Заносим название, число компонентов, индекс набора контента и индекс энергетического орудия в список
                            energyGunsSortList.Add(
                                new Tuple<string, int, int, int>(
                                    contentData.Value
                                    .wDContentSets[shipDesignerWindow.currentWorkshopShipClass.energyGuns[a].ContentSetIndex]
                                    .energyGuns[shipDesignerWindow.currentWorkshopShipClass.energyGuns[a].ObjectIndex]
                                    .ObjectName,
                                    shipDesignerWindow.currentWorkshopShipClass.energyGuns[a].numberOfComponents,
                                    shipDesignerWindow.currentWorkshopShipClass.energyGuns[a].ContentSetIndex,
                                    shipDesignerWindow.currentWorkshopShipClass.energyGuns[a].ObjectIndex));
                        }

                        //Сортируем список по названию в алфавитном порядке
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //Для каждого энергетического орудия в отсортированном списке
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //Берём ссылку на данные энергетического орудия
                            ref readonly WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[energyGunsSortList[a].Item3]
                                .energyGuns[energyGunsSortList[a].Item4];

                            //Создаём обзорную панель установленного компонента в указанном списке
                            UIInstalledComponentBriefInfoPanel energyGunInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    energyGunsSortList[a].Item3, energyGunsSortList[a].Item4,
                                    ShipComponentType.GunEnergy,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //Указываем название энергетического орудия
                            energyGunInstalledComponentPanel.modelName.text
                                = energyGun.ObjectName;

                            //Указываем название типа компонента
                            energyGunInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.GunEnergy.ToString();

                            //Указываем число энергетических орудий
                            energyGunInstalledComponentPanel.componentNumber.text
                                = energyGunsSortList[a].Item2.ToString();

                            //Указываем суммарный размер компонентов
                            energyGunInstalledComponentPanel.componentTotalSize.text
                                = (energyGunsSortList[a].Item2 * energyGun.Size).ToString();
                        }
                    }
                }
            }
        }

        void DesignerSaveContentObject(
            int contentSetIndex)
        {
            Debug.LogWarning("!");

            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём название сохраняемого объекта
            string objectName
                = designerWindow.currentContentSetList.objectName.text;

            //Если активен дизайнер кораблей
            if (designerWindow.designerType
                == DesignerType.ShipClass)
            {
                //Берём ссылку на окно дизайнера кораблей
                UIShipClassDesignerWindow shipDesignerWindow
                    = designerWindow.shipClassDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого корабля
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].shipClasses.Length; a++)
                    {
                        //Если название корабля совпадает
                        if (contentData.Value.contentSets[contentSetIndex].shipClasses[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный корабль
                            DesignerShipClassDeleteAllComponentsRefs(
                                contentSetIndex,
                                a);

                            //То перезаписываем данные корабля
                            contentData.Value.contentSets[contentSetIndex].shipClasses[a]
                                = shipDesignerWindow.currentGameShipClass;
                            contentData.Value.contentSets[contentSetIndex].shipClasses[a].ObjectName
                                = objectName;

                            //Пересчитываем характеристики корабля
                            contentData.Value.contentSets[contentSetIndex].shipClasses[a].CalculateCharacteristics(
                                contentData.Value);

                            //Заносим ссылки на данный корабль во все установленные компоненты
                            DesignerShipClassAddAllComponentsRefs(
                                contentSetIndex,
                                a);


                            //Отмечаем, что корабль был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если корабль не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Обновляем размер массива кораблей
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].shipClasses,
                            contentData.Value.contentSets[contentSetIndex].shipClasses.Length + 1);

                        //Заносим редактируемый корабль в массив как последний элемент
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .shipClasses[contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1]
                            = shipDesignerWindow.currentGameShipClass;
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .shipClasses[contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1].ObjectName
                            = objectName;


                        //Пересчитываем характеристики корабля
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .shipClasses[contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1].CalculateCharacteristics(
                            contentData.Value);

                        //Заносим ссылки на данный корабль во все установленные компоненты
                        DesignerShipClassAddAllComponentsRefs(
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1);
                    }
                }
                //Иначе
                else
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого корабля
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length; a++)
                    {
                        //Если название корабля совпадает
                        if (contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный корабль
                            DesignerShipClassDeleteAllComponentsRefs(
                                contentSetIndex,
                                a);

                            //То перезаписываем данные корабля
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses[a]
                                = shipDesignerWindow.currentWorkshopShipClass;
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].ObjectName
                                = objectName;

                            //Пересчитываем характеристики корабля
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].CalculateCharacteristics(
                                contentData.Value);

                            //Заносим ссылки на данный корабль во все установленные компоненты
                            DesignerShipClassAddAllComponentsRefs(
                                contentSetIndex,
                                a);


                            //Отмечаем, что корабль был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если корабль не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Обновляем размер массива кораблей
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].shipClasses,
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length + 1);

                        //Заносим редактируемый корабль в массив как последний элемент
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .shipClasses[contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1]
                            = shipDesignerWindow.currentWorkshopShipClass;
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .shipClasses[contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1].ObjectName
                            = objectName;


                        //Пересчитываем характеристики корабля
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .shipClasses[contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1].CalculateCharacteristics(
                            contentData.Value);

                        //Заносим ссылки на данный корабль во все установленные компоненты
                        DesignerShipClassAddAllComponentsRefs(
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1);
                    }
                }

                //Отображаем список кораблей из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер двигателей
            else if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //Берём ссылку на окно дизайнера двигателей
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого двигателя
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].engines.Length; a++)
                    {
                        //Если название двигателя совпадает
                        if (contentData.Value.contentSets[contentSetIndex].engines[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный двигатель
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные двигателя

                            //Берём ссылку на двигатель
                            ref DEngine engine
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .engines[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющуюу мощность на единицу размера
                            ref DComponentCoreTechnology powerPerSizeTechnology
                                = ref engine.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            powerPerSizeTechnology.ModifierValue
                                = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //Параметры

                            //Размер двигателя
                            engine.EngineSize
                                = engineDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //Разгон двигателя
                            engine.EngineBoost
                                = engineDesignerWindow.boostParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики двигателя
                            engine.CalculateCharacteristics();

                            //Заносим ссылки на данный двигатель во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);


                            //Отмечаем, что двигатель был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если двигатель не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новый двигатель, заполняя данными из события и дизайнера
                        DEngine engine
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length],
                                engineDesignerWindow.sizeParameterPanel.currentParameterValue,
                                engineDesignerWindow.boostParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей мощность на единицу размера
                        Tuple<int, int, float> powerPerSizeTechnology
                            = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий двигателя технологию мощности на единицу размера
                        engine.coreTechnologies[0]
                            = new(
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                powerPerSizeTechnology.Item3);


                        //Обновляем размер массива двигателей
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].engines,
                            contentData.Value.contentSets[contentSetIndex].engines.Length + 1);

                        //Заносим новый двигатель в массив как последний элемент
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .engines[contentData.Value.contentSets[contentSetIndex].engines.Length - 1]
                            = engine;


                        //Пересчитываем характеристики двигателя
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .engines[contentData.Value.contentSets[contentSetIndex].engines.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данный двигатель во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Engine,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].engines.Length - 1);
                    }
                }
                //Иначе
                else
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого двигателя
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].engines.Length; a++)
                    {
                        //Если название двигателя совпадает
                        if (contentData.Value.wDContentSets[contentSetIndex].engines[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный двигатель
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные двигателя

                            //Берём ссылку на двигатель
                            ref WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .engines[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющуюу мощность на единицу размера
                            ref WDComponentCoreTechnology powerPerSizeTechnology
                                = ref engine.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            powerPerSizeTechnology.ModifierValue
                                = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //Параметры

                            //Размер двигателя
                            engine.EngineSize
                                = engineDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //Разгон двигателя
                            engine.EngineBoost
                                = engineDesignerWindow.boostParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики двигателя
                            engine.CalculateCharacteristics();

                            //Заносим ссылки на данный двигатель во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);


                            //Отмечаем, что двигатель был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если двигатель не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новый двигатель, заполняя данными из события и дизайнера
                        WDEngine engine
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length],
                                engineDesignerWindow.sizeParameterPanel.currentParameterValue,
                                engineDesignerWindow.boostParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей мощность на единицу размера
                        Tuple<int, int, float> powerPerSizeTechnology
                            = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий двигателя технологию мощности на единицу размера
                        engine.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.EnginePowerPerSize],
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].technologies[powerPerSizeTechnology.Item2].ObjectName,
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                true,
                                powerPerSizeTechnology.Item3);


                        //Обновляем размер массива двигателей
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].engines,
                            contentData.Value.wDContentSets[contentSetIndex].engines.Length + 1);

                        //Заносим новый двигатель в массив как последний элемент
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .engines[contentData.Value.wDContentSets[contentSetIndex].engines.Length - 1]
                            = engine;


                        //Пересчитываем характеристики двигателя
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .engines[contentData.Value.wDContentSets[contentSetIndex].engines.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данный двигатель во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Engine,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].engines.Length - 1);
                    }
                }

                //Отображаем список двигателей из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер реакторов
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //Берём ссылку на окно дизайнера реакторов
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого реактора
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].reactors.Length; a++)
                    {
                        //Если название реактора совпадает
                        if (contentData.Value.contentSets[contentSetIndex].reactors[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный топливный бак
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные реактора

                            //Берём ссылку на реактор
                            ref DReactor reactor
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .reactors[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую энергию на единицу размера
                            ref DComponentCoreTechnology energyPerSizeTechnology
                                = ref reactor.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            energyPerSizeTechnology.ModifierValue
                                = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //Параметры

                            //Размер реактора
                            reactor.ReactorSize
                                = reactorDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //Разгон реактора
                            reactor.ReactorBoost
                                = reactorDesignerWindow.boostParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики реактора
                            reactor.CalculateCharacteristics();

                            //Заносим ссылки на данный реактор во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);


                            //Отмечаем, что реактор был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если реактор не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новый реактор, заполняя данными из события и дизайнера
                        DReactor reactor
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length],
                                reactorDesignerWindow.sizeParameterPanel.currentParameterValue,
                                reactorDesignerWindow.boostParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей энергию на единицу размера
                        Tuple<int, int, float> energyPerSizeTechnology
                            = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий реактора технологию энергии на единицу размера
                        reactor.coreTechnologies[0]
                            = new(
                                new(energyPerSizeTechnology.Item1, energyPerSizeTechnology.Item2),
                                energyPerSizeTechnology.Item3);

                        //Обновляем размер массива реакторов
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].reactors,
                            contentData.Value.contentSets[contentSetIndex].reactors.Length + 1);

                        //Заносим новый реактора в массив как последний элемент
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .reactors[contentData.Value.contentSets[contentSetIndex].reactors.Length - 1]
                            = reactor;

                        //Пересчитываем характеристики реактора
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .reactors[contentData.Value.contentSets[contentSetIndex].reactors.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данный реактор во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Reactor,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].reactors.Length - 1);
                    }
                }
                //Иначе
                else
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого реактора
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].reactors.Length; a++)
                    {
                        //Если название реактора совпадает
                        if (contentData.Value.wDContentSets[contentSetIndex].reactors[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный двигатель
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные реактора

                            //Берём ссылку на реактор
                            ref WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .reactors[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую энергию на единицу размера
                            ref WDComponentCoreTechnology energyPerSizeTechnology
                                = ref reactor.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            energyPerSizeTechnology.ModifierValue
                                = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //Параметры

                            //Размер реактора
                            reactor.ReactorSize
                                = reactorDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //Разгон реактора
                            reactor.ReactorBoost
                                = reactorDesignerWindow.boostParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики реактора
                            reactor.CalculateCharacteristics();

                            //Заносим ссылки на данный реактор во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);


                            //Отмечаем, что реактор был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если реактор не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новый реактор, заполняя данными из события и дизайнера
                        WDReactor reactor
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length],
                                reactorDesignerWindow.sizeParameterPanel.currentParameterValue,
                                reactorDesignerWindow.boostParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей энергию на единицу размера
                        Tuple<int, int, float> energyPerSizeTechnology
                            = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий реактора технологию энергии на единицу размера
                        reactor.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.ReactorEnergyPerSize],
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                contentData.Value.wDContentSets[energyPerSizeTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[energyPerSizeTechnology.Item1].technologies[energyPerSizeTechnology.Item2].ObjectName,
                                new(energyPerSizeTechnology.Item1, energyPerSizeTechnology.Item2),
                                true,
                                energyPerSizeTechnology.Item3);

                        //Обновляем размер массива реакторов
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].reactors,
                            contentData.Value.wDContentSets[contentSetIndex].reactors.Length + 1);

                        //Заносим новый реактора в массив как последний элемент
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .reactors[contentData.Value.wDContentSets[contentSetIndex].reactors.Length - 1]
                            = reactor;

                        //Пересчитываем характеристики реактора
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .reactors[contentData.Value.wDContentSets[contentSetIndex].reactors.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данный реактор во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Reactor,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].reactors.Length - 1);
                    }
                }

                //Отображаем список реакторов из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер топливных баков
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //Берём ссылку на окно дизайнера топливных баков
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого топливного бака
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].fuelTanks.Length; a++)
                    {
                        //Если название топливного бака совпадает
                        if (contentData.Value.contentSets[contentSetIndex].fuelTanks[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный топливный бак
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные топливного бака

                            //Берём ссылку на топливный бак
                            ref DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .fuelTanks[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую сжатие
                            ref DComponentCoreTechnology compressionTechnology
                                = ref fuelTank.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            compressionTechnology.ContentObjectLink
                                = new(
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            compressionTechnology.ModifierValue
                                = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //Параметры

                            //Размер топливного бака
                            fuelTank.Size
                                = fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики топливного бака
                            fuelTank.CalculateCharacteristics();

                            //Заносим ссылки на данный топливный бак во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);


                            //Отмечаем, что топливный бак был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если топливный бак не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новый топливный бак, заполняя данными из события и дизайнера
                        DHoldFuelTank fuelTank
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length],
                                fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей сжатие
                        Tuple<int, int, float> compressionTechnology
                            = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий топливного бака технологию сжатия
                        fuelTank.coreTechnologies[0]
                            = new(
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                compressionTechnology.Item3);

                        //Обновляем размер массива топливных баков
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].fuelTanks,
                            contentData.Value.contentSets[contentSetIndex].fuelTanks.Length + 1);

                        //Заносим новый топливный бак в массив как последний элемент
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.contentSets[contentSetIndex].fuelTanks.Length - 1]
                            = fuelTank;

                        //Пересчитываем характеристики топливного бака
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.contentSets[contentSetIndex].fuelTanks.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данный топливный бак во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.HoldFuelTank,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].fuelTanks.Length - 1);
                    }
                }
                //Иначе
                else
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого топливного бака
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length; a++)
                    {
                        //Если название топливного бака совпадает
                        if (contentData.Value.wDContentSets[contentSetIndex].fuelTanks[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данный топливный бак
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные топливного бака

                            //Берём ссылку на топливный бак
                            ref WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .fuelTanks[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую сжатие
                            ref WDComponentCoreTechnology energyPerSizeTechnology
                                = ref fuelTank.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            energyPerSizeTechnology.ModifierValue
                                = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //Параметры

                            //Размер топливного бака
                            fuelTank.Size
                                = fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики топливного бака
                            fuelTank.CalculateCharacteristics();

                            //Заносим ссылки на данный топливный бак во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);


                            //Отмечаем, что топливный бак был перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если топливный бак не был перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новый топливный бак, заполняя данными из события и дизайнера
                        WDHoldFuelTank fuelTank
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length],
                                fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей сжатие
                        Tuple<int, int, float> compressionTechnology
                            = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий топливного бака технологию сжатия
                        fuelTank.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.FuelTankCompression],
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].technologies[compressionTechnology.Item2].ObjectName,
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                true,
                                compressionTechnology.Item3);

                        //Обновляем размер массива топливных баков
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].fuelTanks,
                            contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length + 1);

                        //Заносим новый топливный бак в массив как последний элемент
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length - 1]
                            = fuelTank;

                        //Пересчитываем характеристики топливного бака
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данный топливный бак во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.HoldFuelTank,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length - 1);
                    }
                }

                //Отображаем список топливных баков из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер оборудования для твёрдой добычи
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //Берём ссылку на окно дизайнера добывающего оборудования 
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого оборудования для твёрдой добычи
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                    {
                        //Если название оборудования для твёрдой добычи совпадает
                        if (contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данное оборудование для твёрдой добычи
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные оборудования для твёрдой добычи

                            //Берём ссылку на оборудование для твёрдой добычи
                            ref DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .solidExtractionEquipments[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую скорость на единицу размера
                            ref DComponentCoreTechnology compressionTechnology
                                = ref extractionEquipmentSolid.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            compressionTechnology.ContentObjectLink
                                = new(
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            compressionTechnology.ModifierValue
                                = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //Параметры

                            //Размер оборудования для твёрдой добычи
                            extractionEquipmentSolid.Size
                                = extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики оборудования для твёрдой добычи
                            extractionEquipmentSolid.CalculateCharacteristics();

                            //Заносим ссылки на данное оборудование для твёрдой добычи во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);


                            //Отмечаем, что оборудование для твёрдой добычи было перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если оборудование для твёрдой добычи не было перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новое оборудование для твёрдой добычи, заполняя данными из события и дизайнера
                        DExtractionEquipment extractionEquipmentSolid
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.solidExtractionEquipmentCoreModifierTypes.Length],
                                extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей скорость на единицу размера
                        Tuple<int, int, float> compressionTechnology
                            = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий оборудования для твёрдой добычи технологию скорости на единицу размера
                        extractionEquipmentSolid.coreTechnologies[0]
                            = new(
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                compressionTechnology.Item3);

                        //Обновляем размер массива оборудования для твёрдой добычи
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments,
                            contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length + 1);

                        //Заносим новое оборудование для твёрдой добычи в массив как последний элемент
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length - 1]
                            = extractionEquipmentSolid;

                        //Пересчитываем характеристики оборудования для твёрдой добычи
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данное оборудование для твёрдой добычи во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.ExtractionEquipmentSolid,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length - 1);
                    }
                }
                //Иначе
                else
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого оборудования для твёрдой добычи
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                    {
                        //Если название оборудования для твёрдой добычи совпадает
                        if (contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данное оборудование для твёрдой добычи
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные оборудования для твёрдой добычи

                            //Берём ссылку на оборудование для твёрдой добычи
                            ref WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .solidExtractionEquipments[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую скорость на единицу размера
                            ref WDComponentCoreTechnology energyPerSizeTechnology
                                = ref extractionEquipmentSolid.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            energyPerSizeTechnology.ModifierValue
                                = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //Параметры

                            //Размер оборудования для твёрдой добычи
                            extractionEquipmentSolid.Size
                                = extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики оборудования для твёрдой добычи
                            extractionEquipmentSolid.CalculateCharacteristics();

                            //Заносим ссылки на данное оборудование для твёрдой добычи во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);


                            //Отмечаем, что оборудование для твёрдой добычи было перезаписан
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если оборудование для твёрдой добычи не было перезаписан
                    if (isOverride
                        == false)
                    {
                        //Создаём новое оборудование для твёрдой добычи, заполняя данными из события и дизайнера
                        WDExtractionEquipment extractionEquipmentSolid
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.solidExtractionEquipmentCoreModifierTypes.Length],
                                extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей скорость на единицу размера
                        Tuple<int, int, float> compressionTechnology
                            = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий оборудования для твёрдой добычи технологию скорости на единицу размера
                        extractionEquipmentSolid.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize],
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].technologies[compressionTechnology.Item2].ObjectName,
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                true,
                                compressionTechnology.Item3);

                        //Обновляем размер массива оборудования для твёрдой добычи
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments,
                            contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length + 1);

                        //Заносим новое оборудование для твёрдой добычи в массив как последний элемент
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length - 1]
                            = extractionEquipmentSolid;

                        //Пересчитываем характеристики оборудования для твёрдой добычи
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данное оборудование для твёрдой добычи во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.ExtractionEquipmentSolid,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length - 1);
                    }
                }

                //Отображаем список оборудования для твёрдой добычи из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер энергетических орудий
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //Берём ссылку на окно дизайнера энергетических орудий
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого энергетического орудия
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].energyGuns.Length; a++)
                    {
                        //Если название энергетического орудия совпадает
                        if (contentData.Value.contentSets[contentSetIndex].energyGuns[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данное энергетическое орудие
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные энергетического орудия

                            //Берём ссылку на энергетическое орудие
                            ref DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .energyGuns[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющую перезарядку
                            ref DComponentCoreTechnology powerPerSizeTechnology
                                = ref energyGun.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            powerPerSizeTechnology.ModifierValue
                                = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //Параметры

                            //Калибр энергетического орудия
                            energyGun.GunCaliber
                                = energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue;

                            //Длину ствола энергетического орудия
                            energyGun.GunBarrelLength
                                = energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики энергетического орудия
                            energyGun.CalculateCharacteristics();

                            //Заносим ссылки на данное энергетическое орудие во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);


                            //Отмечаем, что энергетическое орудие было перезаписано
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если энергетическое орудие не было перезаписано
                    if (isOverride
                        == false)
                    {
                        //Создаём новое энергетическое орудие, заполняя данными из события и дизайнера
                        DGunEnergy energyGun
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length],
                                energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue,
                                energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей перезарядку
                        Tuple<int, int, float> powerPerSizeTechnology
                            = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий энергетического орудия технологию перезарядки
                        energyGun.coreTechnologies[0]
                            = new(
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                powerPerSizeTechnology.Item3);


                        //Обновляем размер массива энергетических орудий
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].energyGuns,
                            contentData.Value.contentSets[contentSetIndex].energyGuns.Length + 1);

                        //Заносим новое энергетическое орудие в массив как последний элемент
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .energyGuns[contentData.Value.contentSets[contentSetIndex].energyGuns.Length - 1]
                            = energyGun;


                        //Пересчитываем характеристики энергетического орудия
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .energyGuns[contentData.Value.contentSets[contentSetIndex].energyGuns.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данное энергетическое орудие во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.GunEnergy,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].energyGuns.Length - 1);
                    }
                }
                //Иначе
                else
                {
                    //Создаём переменную для отслеживания перезаписи
                    bool isOverride
                        = false;

                    //Для каждого энергетического орудия
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length; a++)
                    {
                        //Если название энергетического орудия совпадает
                        if (contentData.Value.wDContentSets[contentSetIndex].energyGuns[a].ObjectName
                            == objectName)
                        {
                            //Удаляем все ссылки на данное энергетическое орудие
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);

                            //То перезаписываем данные энергетического орудия

                            //Берём ссылку на энергетическое орудие
                            ref WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .energyGuns[a];

                            //Основные технологии

                            //Берём ссылку на технологию, определяющуюу перезарядку
                            ref WDComponentCoreTechnology powerPerSizeTechnology
                                = ref energyGun.coreTechnologies[0];

                            //Перезаписываем индексы технологии
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //И значение модификатора
                            powerPerSizeTechnology.ModifierValue
                                = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //Параметры

                            //Калибр энергетического орудия
                            energyGun.GunCaliber
                                = energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue;

                            //Длину ствола энергетического орудия
                            energyGun.GunBarrelLength
                                = energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue;


                            //Пересчитываем характеристики энергетического орудия
                            energyGun.CalculateCharacteristics();

                            //Заносим ссылки на данное энергетическое орудие во все его основные технологии
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);


                            //Отмечаем, что энергетическое орудие было перезаписано
                            isOverride
                                = true;

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Если энергетическое орудие не был перезаписано
                    if (isOverride
                        == false)
                    {
                        //Создаём новое энергетическое орудие, заполняя данными из события и дизайнера
                        WDGunEnergy energyGun
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length],
                                energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue,
                                energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue);

                        //Берём ссылочные данные технологии, определяющей перезарядку
                        Tuple<int, int, float> powerPerSizeTechnology
                            = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex];

                        //Заносим в массив основных технологий энергетического орудия технологию перезарядки
                        energyGun.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.GunEnergyRecharge],
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].technologies[powerPerSizeTechnology.Item2].ObjectName,
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                true,
                                powerPerSizeTechnology.Item3);


                        //Обновляем размер массива энергетических орудий
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].energyGuns,
                            contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length + 1);

                        //Заносим новое энергетическое орудие в массив как последний элемент
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .energyGuns[contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length - 1]
                            = energyGun;


                        //Пересчитываем характеристики энергетического орудия
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .energyGuns[contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length - 1].CalculateCharacteristics();

                        //Заносим ссылки на данное энергетическое орудие во все его основные технологии
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.GunEnergy,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length - 1);
                    }
                }

                //Отображаем список энергетических орудий из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }

            //Отключаем переключатели в обоих списках набора контента
            designerWindow.otherContentSetsList.toggleGroup.SetAllTogglesOff();
            designerWindow.currentContentSetList.toggleGroup.SetAllTogglesOff();
        }

        void DesignerLoadContentObject(
            int contentSetIndex,
            int objectIndex)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если активен дизайнер кораблей
            if (designerWindow.designerType
                == DesignerType.ShipClass)
            {
                //Берём ссылку на окно дизайнера кораблей
                UIShipClassDesignerWindow shipDesignerWindow
                    = designerWindow.shipClassDesigner;

                //Очистка окна

                //Очищаем панель подробной информации о компоненте
                DesignerShipClassDisplayComponentDetailedInfo(
                    false);

                //Отключаем переключатель в списке доступных компонентов
                shipDesignerWindow.availableComponentsListToggleGroup.SetAllTogglesOff();


                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на класс корабля
                    ref readonly DShipClass shipClass
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .shipClasses[objectIndex];

                    //Загружаем класс корабля в редактор
                    shipDesignerWindow.currentGameShipClass
                        = new(
                            "",
                            (DShipClassComponent[])shipClass.engines.Clone(),
                            (DShipClassComponent[])shipClass.reactors.Clone(),
                            (DShipClassComponent[])shipClass.fuelTanks.Clone(),
                            (DShipClassComponent[])shipClass.extractionEquipmentSolids.Clone(),
                            (DShipClassComponent[])shipClass.energyGuns.Clone(),
                            new DShipClassPart[0]);

                    //Обнуляем название текущего класса корабля
                    /*shipDesignerWindow.currentGameShipClass.ObjectName
                        = "";*/
                }
                //Иначе
                else
                {
                    //Берём ссылку на класс корабля
                    ref readonly WDShipClass shipClass
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .shipClasses[objectIndex];

                    //Загружаем класс корабля в редактор
                    shipDesignerWindow.currentWorkshopShipClass
                        = new(
                            "",
                            (WDShipClassComponent[])shipClass.engines.Clone(),
                            (WDShipClassComponent[])shipClass.reactors.Clone(),
                            (WDShipClassComponent[])shipClass.fuelTanks.Clone(),
                            (WDShipClassComponent[])shipClass.extractionEquipmentSolids.Clone(),
                            (WDShipClassComponent[])shipClass.energyGuns.Clone(),
                            new WDShipClassPart[0]);

                    //Обнуляем название текущего класса корабля
                    /*shipDesignerWindow.currentWorkshopShipClass.ObjectName
                        = "";*/
                }

                //Отображаем все установленные компоненты
                DesignerShipClassDisplayInstalledComponents(
                    ShipComponentType.None);

                //Пересчитываем и отображаем характеристики корабля
                shipDesignerWindow.CalculateCharacteristics(
                    contentData.Value,
                    eUI.Value.designerWindow.isInGameDesigner);
            }
            //Иначе, если активен дизайнер двигателей
            else if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //Берём ссылку на окно дизайнера двигателей
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на данные двигателя
                    ref readonly DEngine engine
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .engines[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих мощность на единицу размера
                    for (int a = 0; a < engineDesignerWindow.powerPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией двигателя
                        if (engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item1
                            == engine.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item2
                            == engine.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            engineDesignerWindow.powerPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение мощности на единицу размера
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры двигателя
                    engineDesignerWindow.DisplayParameters(
                        engine.EngineSize,
                        engine.EngineBoost);
                }
                //Иначе
                else
                {
                    //Берём ссылку на данные двигателя
                    ref readonly WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .engines[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих мощность на единицу размера
                    for (int a = 0; a < engineDesignerWindow.powerPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией двигателя
                        if (engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item1
                            == engine.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item2
                            == engine.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            engineDesignerWindow.powerPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение мощности на единицу размера
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры двигателя
                    engineDesignerWindow.DisplayParameters(
                        engine.EngineSize,
                        engine.EngineBoost);
                }

                //Пересчитываем и отображаем характеристики двигателя
                engineDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер реакторов
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //Берём ссылку на окно дизайнера реакторов
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на данные реактора
                    ref readonly DReactor reactor
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .reactors[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих энергию на единицу размера
                    for (int a = 0; a < reactorDesignerWindow.energyPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией реактора
                        if (reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item1
                            == reactor.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item2
                            == reactor.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение энергии на единицу размера
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры реактора
                    reactorDesignerWindow.DisplayParameters(
                        reactor.ReactorSize,
                        reactor.ReactorBoost);
                }
                //Иначе
                else
                {
                    //Берём ссылку на данные реактора
                    ref readonly WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .reactors[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих энергию на единицу размера
                    for (int a = 0; a < reactorDesignerWindow.energyPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией реактора
                        if (reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item1
                            == reactor.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item2
                            == reactor.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение энергии на единицу размера
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры реактора
                    reactorDesignerWindow.DisplayParameters(
                        reactor.ReactorSize,
                        reactor.ReactorBoost);
                }

                //Пересчитываем и отображаем характеристики реактора
                reactorDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер топливных баков
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //Берём ссылку на окно дизайнера топливных баков
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на данные топливного бака
                    ref readonly DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .fuelTanks[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих сжатие
                    for (int a = 0; a < fuelTankDesignerWindow.compressionCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией топливного бака
                        if (fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item1
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item2
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            fuelTankDesignerWindow.compressionCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение сжатия
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры топливного бака
                    fuelTankDesignerWindow.DisplayParameters(
                        fuelTank.Size);
                }
                //Иначе
                else
                {
                    //Берём ссылку на данные топливного бака
                    ref readonly WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .fuelTanks[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих сжатие
                    for (int a = 0; a < fuelTankDesignerWindow.compressionCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией топливного бака
                        if (fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item1
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item2
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            fuelTankDesignerWindow.compressionCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение сжатия
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры топливного бака
                    fuelTankDesignerWindow.DisplayParameters(
                        fuelTank.Size);
                }

                //Пересчитываем и отображаем характеристики топливного бака
                fuelTankDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер оборудования для твёрдой добычи
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //Берём ссылку на окно дизайнера добывающего оборудования
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на данные оборудования для твёрдой добычи
                    ref readonly DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .solidExtractionEquipments[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих скорость на единицу размера
                    for (int a = 0; a < extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией оборудования для твёрдой добычи
                        if (extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item1
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item2
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение скорости на единицу размера
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры оборудования для твёрдой добычи
                    extractionEquipmentDesignerWindow.DisplayParameters(
                        extractionEquipmentSolid.Size);
                }
                //Иначе
                else
                {
                    //Берём ссылку на данные оборудования для твёрдой добычи
                    ref readonly WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .solidExtractionEquipments[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих скорость на единицу размера
                    for (int a = 0; a < extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией оборудования для твёрдой добычи
                        if (extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item1
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item2
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение скорости на единицу размера
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры оборудования для твёрдой добычи
                    extractionEquipmentDesignerWindow.DisplayParameters(
                        extractionEquipmentSolid.Size);
                }

                //Пересчитываем и отображаем характеристики оборудования для твёрдой добычи
                extractionEquipmentDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер энергетических орудий
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //Берём ссылку на окно дизайнера энергетических орудий
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на данные энергетического орудия
                    ref readonly DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .energyGuns[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих перезарядку
                    for (int a = 0; a < energyGunDesignerWindow.rechargeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией энергетического орудия
                        if (energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item1
                            == energyGun.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item2
                            == energyGun.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            energyGunDesignerWindow.rechargeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение перезарядки
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры энергетического орудия
                    energyGunDesignerWindow.DisplayParameters(
                        energyGun.GunCaliber,
                        energyGun.GunBarrelLength);
                }
                //Иначе
                else
                {
                    //Берём ссылку на данные энергетического орудия
                    ref readonly WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .energyGuns[objectIndex];

                    //Отображение основных технологий

                    //Для каждой технологии в списке основных технологий, определяющих перезарядку
                    for (int a = 0; a < energyGunDesignerWindow.rechargeCoreTechnologiesList.Count; a++)
                    {
                        //Если индекс набора контента и индекс технологии совпадают с технологией энергетического орудия
                        if (energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item1
                            == energyGun.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item2
                            == energyGun.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //Изменяем выбранную основную технологию в выпадающем списке
                            energyGunDesignerWindow.rechargeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //Обновляем отображение перезарядки
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }

                    //Отображаем параметры энергетического орудия
                    energyGunDesignerWindow.DisplayParameters(
                        energyGun.GunCaliber,
                        energyGun.GunBarrelLength);
                }

                //Пересчитываем и отображаем характеристики энергетического орудия
                energyGunDesignerWindow.CalculateCharacteristics();
            }

            //Отключаем переключатели в обоих списках набора контента
            designerWindow.otherContentSetsList.toggleGroup.SetAllTogglesOff();
            designerWindow.currentContentSetList.toggleGroup.SetAllTogglesOff();
        }

        void DesignerDeleteContentObject(
            int contentSetIndex,
            int objectIndex)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если активен дизайнер кораблей
            if (designerWindow.designerType
                == DesignerType.ShipClass)
            {
                //Берём ссылку на окно дизайнера кораблей
                UIShipClassDesignerWindow shipDesignerWindow
                    = designerWindow.shipClassDesigner;

                //Удаляем все ссылки на данный корабль
                DesignerShipClassDeleteAllComponentsRefs(
                    contentSetIndex,
                    objectIndex);

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Копируем массив кораблей во временный список
                    List<DShipClass> shipClasses
                        = new(
                            contentData.Value.contentSets[contentSetIndex].shipClasses);

                    //Удаляем объект по указанному индексу
                    shipClasses.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.contentSets[contentSetIndex].shipClasses
                        = shipClasses.ToArray();
                }
                //Иначе
                else
                {
                    //Копируем массив кораблей во временный список
                    List<WDShipClass> shipClasses
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses);

                    //Удаляем объект по указанному индексу
                    shipClasses.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.wDContentSets[contentSetIndex].shipClasses
                        = shipClasses.ToArray();
                }

                //Отображаем список кораблей из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер двигателей
            else if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //Берём ссылку на окно дизайнера двигателей
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //Удаляем все ссылки на данный двигатель
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.Engine,
                    contentSetIndex,
                    objectIndex);

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Копируем массив кораблей во временный список
                    List<DEngine> engines
                        = new(
                            contentData.Value.contentSets[contentSetIndex].engines);

                    //Удаляем объект по указанному индексу
                    engines.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.contentSets[contentSetIndex].engines
                        = engines.ToArray();
                }
                //Иначе
                else
                {
                    //Копируем массив кораблей во временный список
                    List<WDEngine> engines
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].engines);

                    //Удаляем объект по указанному индексу
                    engines.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.wDContentSets[contentSetIndex].engines
                        = engines.ToArray();
                }

                //Отображаем список двигателей из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер реакторов
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //Берём ссылку на окно дизайнера реакторов
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //Удаляем все ссылки на данный реактор
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.Reactor,
                    contentSetIndex,
                    objectIndex);

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Копируем массив реакторов во временный список
                    List<DReactor> reactors
                        = new(
                            contentData.Value.contentSets[contentSetIndex].reactors);

                    //Удаляем объект по указанному индексу
                    reactors.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.contentSets[contentSetIndex].reactors
                        = reactors.ToArray();
                }
                //Иначе
                else
                {
                    //Копируем массив реакторов во временный список
                    List<WDReactor> reactors
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].reactors);

                    //Удаляем объект по указанному индексу
                    reactors.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.wDContentSets[contentSetIndex].reactors
                        = reactors.ToArray();
                }

                //Отображаем список реакторов из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер топливных баков
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //Берём ссылку на окно дизайнера топливных баков
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //Удаляем все ссылки на данный топливный бак
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.HoldFuelTank,
                    contentSetIndex,
                    objectIndex);

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Копируем массив топливных баков во временный список
                    List<DHoldFuelTank> fuelTanks
                        = new(
                            contentData.Value.contentSets[contentSetIndex].fuelTanks);

                    //Удаляем объект по указанному индексу
                    fuelTanks.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.contentSets[contentSetIndex].fuelTanks
                        = fuelTanks.ToArray();
                }
                //Иначе
                else
                {
                    //Копируем массив топливных баков во временный список
                    List<WDHoldFuelTank> fuelTanks
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].fuelTanks);

                    //Удаляем объект по указанному индексу
                    fuelTanks.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.wDContentSets[contentSetIndex].fuelTanks
                        = fuelTanks.ToArray();
                }

                //Отображаем список топливных баков из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер оборудования для твёрдой добычи
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //Берём ссылку на окно дизайнера добывающего оборудования
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //Удаляем все ссылки на данное оборудование для твёрдой добычи
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.ExtractionEquipmentSolid,
                    contentSetIndex,
                    objectIndex);

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Копируем массив оборудования для твёрдой добычи во временный список
                    List<DExtractionEquipment> extractionEquipmentSolids
                        = new(
                            contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments);

                    //Удаляем объект по указанному индексу
                    extractionEquipmentSolids.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments
                        = extractionEquipmentSolids.ToArray();
                }
                //Иначе
                else
                {
                    //Копируем массив оборудования для твёрдой добычи во временный список
                    List<WDExtractionEquipment> extractionEquipmentSolids
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments);

                    //Удаляем объект по указанному индексу
                    extractionEquipmentSolids.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments
                        = extractionEquipmentSolids.ToArray();
                }

                //Отображаем список оборудования для твёрдой добычи из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //Иначе, если активен дизайнер энергетических орудий
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //Берём ссылку на окно дизайнера энергетических орудий
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //Удаляем все ссылки на данное энергетическое орудие
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.GunEnergy,
                    contentSetIndex,
                    objectIndex);

                //Если активен внутриигровой дизайнер
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //Копируем массив кораблей во временный список
                    List<DGunEnergy> energyGuns
                        = new(
                            contentData.Value.contentSets[contentSetIndex].energyGuns);

                    //Удаляем объект по указанному индексу
                    energyGuns.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.contentSets[contentSetIndex].energyGuns
                        = energyGuns.ToArray();
                }
                //Иначе
                else
                {
                    //Копируем массив кораблей во временный список
                    List<WDGunEnergy> energyGuns
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].energyGuns);

                    //Удаляем объект по указанному индексу
                    energyGuns.RemoveAt(
                        objectIndex);

                    //Перезаписываем массив
                    contentData.Value.wDContentSets[contentSetIndex].energyGuns
                        = energyGuns.ToArray();
                }

                //Отображаем список энергетических орудий из текущего набора контента
                DesignerDisplayContentSetPanelList(
                    true);
            }

            //Отключаем переключатели в обоих списках набора контента
            designerWindow.otherContentSetsList.toggleGroup.SetAllTogglesOff();
            designerWindow.currentContentSetList.toggleGroup.SetAllTogglesOff();
        }

        //Дизайнер кораблей
        void DesignerOpenShipClassWindow(
            int baseContentSetIndex)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём ссылку на окно дизайнера кораблей
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;


            //Заполняем выпадающий список прочих наборов контента
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);


            //Очищаем список установленных компонентов
            for (int a = 0; a < shipDesignerWindow.installedComponentsPanelsList.Count; a++)
            {
                GameObject.Destroy(
                    shipDesignerWindow.installedComponentsPanelsList[a]);
            }
            shipDesignerWindow.installedComponentsPanelsList.Clear();


            //Очищаем панель подробной информации о компоненте
            DesignerShipClassDisplayComponentDetailedInfo(
                false);

            //Отображаем список доступных для установки двигателей
            DesignerShipClassDisplayAvailableComponentsType(
                0);


            //Отображаем массив кораблей из текущего набора контента
            DesignerDisplayContentSetPanelList(
                true);


            //Если активен внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == true)
            {
                //Очищаем редактируемый класс корабля из данных игры
                shipDesignerWindow.currentGameShipClass
                    = new(
                        "",
                        new DShipClassComponent[0],
                        new DShipClassComponent[0],
                        new DShipClassComponent[0],
                        new DShipClassComponent[0],
                        new DShipClassComponent[0],
                        new DShipClassPart[0]);
            }
            //Иначе
            else
            {
                //Если индекс текущего набора контента не равен нулю и не равен индексу базового набора контента
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //Отображаем массив кораблей из базового набора контента
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }

                //Очищаем редактируемый класс корабля из данных мастерской
                shipDesignerWindow.currentWorkshopShipClass
                    = new(
                        "",
                        new WDShipClassComponent[0],
                        new WDShipClassComponent[0],
                        new WDShipClassComponent[0],
                        new WDShipClassComponent[0],
                        new WDShipClassComponent[0],
                        new WDShipClassPart[0]);
            }

            //Пересчитываем и отображаем характеристики корабля
            shipDesignerWindow.CalculateCharacteristics(
                contentData.Value,
                designerWindow.isInGameDesigner);
        }

        void DesignerShipClassDisplayInstalledComponents(
            ShipComponentType componentType)
        {
            //Берём ссылку на окно дизайнера кораблей
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //Если требуемый тип компонентов не указан
            if (componentType
                == ShipComponentType.None)
            {
                //Отображаем установленные двигатели
                DesignerDisplayContentPanels(
                    DesignerType.ComponentEngine,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);

                //Отображаем установленные реакторы
                DesignerDisplayContentPanels(
                    DesignerType.ComponentReactor,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //Отображаем установленные топливные баки
                DesignerDisplayContentPanels(
                    DesignerType.ComponentHoldFuelTank,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //Отображаем установленное оборудование для твёрдой добычи
                DesignerDisplayContentPanels(
                    DesignerType.ComponentExtractionEquipmentSolid,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //Отображаем установленные энергетические орудия
                DesignerDisplayContentPanels(
                    DesignerType.ComponentGunEnergy,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //Отображаем установленные прочие компоненты
            }
            //Иначе, если требуется отобразить список двигателей
            else if (componentType
                == ShipComponentType.Engine)
            {
                //Отображаем установленные двигатели
                DesignerDisplayContentPanels(
                    DesignerType.ComponentEngine,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //Иначе, если требуется отобразить список реакторов
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Отображаем установленные реакторы
                DesignerDisplayContentPanels(
                    DesignerType.ComponentReactor,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //Иначе, если требуется отобразить список топливных баков
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Отображаем установленные топливные баки
                DesignerDisplayContentPanels(
                    DesignerType.ComponentHoldFuelTank,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //Иначе, если требуется отобразить список оборудования для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Отображаем установленное оборудование для твёрдой добычи
                DesignerDisplayContentPanels(
                    DesignerType.ComponentExtractionEquipmentSolid,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //Иначе, если требуется отобразить список энергетических орудий
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Отображаем установленные энергетические орудия
                DesignerDisplayContentPanels(
                    DesignerType.ComponentGunEnergy,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
        }

        void DesignerShipClassAddComponentFirst(
            ShipComponentType componentType,
            int contentSetIndex,
            int componentIndex,
            int numberOfComponents)
        {
            //Берём ссылку на окно дизайнера кораблей
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //Создаём переменную для отслеживания, был ли компонент установлен ранее
            bool isOldComponent
                = false;

            //Создаём переменную для отслеживания, активен ли переключатель в списке установленных компонентов
            bool isInstalledActive
                = false;

            //Если какой-либо переключатель в списке установленных активен
            if (shipDesignerWindow.installedComponentsListToggleGroup.AnyTogglesOn()
                == true)
            {
                isInstalledActive
                    = true;
            }

            //Если событие запрашивает добавление двигателя
            if (componentType
                == ShipComponentType.Engine)
            {
                //Добавляем двигатель
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.engines,
                    ref shipDesignerWindow.currentGameShipClass.engines,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает добавление реактора
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Добавляем реактор
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.reactors,
                    ref shipDesignerWindow.currentGameShipClass.reactors,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает добавление топливного бака
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Добавляем топливный бак
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.fuelTanks,
                    ref shipDesignerWindow.currentGameShipClass.fuelTanks,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает добавление оборудования для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Добавляем оборудование для твёрдой добычи
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids,
                    ref shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает добавление энергетического орудия
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Добавляем энергетическое орудие
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.energyGuns,
                    ref shipDesignerWindow.currentGameShipClass.energyGuns,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }

            //Обновляем список установленных компонентов
            DesignerShipClassDisplayInstalledComponents(
                ShipComponentType.None);

            //Пересчитываем и отображаем характеристики корабля
            shipDesignerWindow.CalculateCharacteristics(
                contentData.Value,
                eUI.Value.designerWindow.isInGameDesigner);

            //Если компонент был установлен ранее
            if (isOldComponent
                == true)
            {
                //И какой-либо переключатель в списке установленных активен
                if (isInstalledActive
                    == true)
                {
                    //Для каждой панели в списке установленных компонентов
                    for (int a = 0; a < shipDesignerWindow.installedComponentsPanelsList.Count; a++)
                    {
                        //Берём компонент обзорной панели установленного компонента
                        if (shipDesignerWindow.installedComponentsPanelsList[a].TryGetComponent(
                            out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                        {
                            //Если её данные соответствую запрошенному компоненту
                            if (installedComponentBriefInfoPanel.componentType
                                == componentType
                                && installedComponentBriefInfoPanel.contentSetIndex
                                == contentSetIndex
                                && installedComponentBriefInfoPanel.componentIndex
                                == componentIndex)
                            {
                                //Активируем переключатель панели
                                installedComponentBriefInfoPanel.panelToggle.isOn
                                    = true;
                            }
                        }
                    }
                }
            }
        }

        void DesignerShipClassAddComponentSecond(
            ref WDShipClassComponent[] wDShipClassComponents,
            ref DShipClassComponent[] dShipClassComponents,
            ShipComponentType componentType,
            int contentSetIndex,
            int componentIndex,
            int numberOfComponents,
            out bool isOldComponent)
        {
            //Отмечаем, что компонент не был установлен ранее
            isOldComponent
                = false;

            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Для каждого компонента в массиве
                for (int a = 0; a < dShipClassComponents.Length; a++)
                {
                    //Если индекс набора контента и индекс двигателя соответствуют запрошенному компоненту
                    if (dShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && dShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //Обновляем число компонентов
                        dShipClassComponents[a].numberOfComponents
                            += numberOfComponents;

                        //Отмечаем, что это был старый компонент
                        isOldComponent
                            = true;

                        //Выходим из функции
                        return;
                    }
                }

                //Записываем данные компонента
                DShipClassComponent newComponent
                    = new(
                        contentSetIndex,
                        componentIndex,
                        numberOfComponents);

                //Изменяем размер массива компонентов
                Array.Resize(
                    ref dShipClassComponents,
                    dShipClassComponents.Length + 1);

                //Заносим новый компонент в массив как последний элемент
                dShipClassComponents[dShipClassComponents.Length - 1]
                    = newComponent;
            }
            //Иначе
            else
            {
                //Для каждого компонента в массиве
                for (int a = 0; a < wDShipClassComponents.Length; a++)
                {
                    //Если индекс набора контента и индекс двигателя соответствуют запрошенному компоненту
                    if (wDShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && wDShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //Обновляем число компонентов
                        wDShipClassComponents[a].numberOfComponents
                            += numberOfComponents;

                        //Отмечаем, что это был старый компонент
                        isOldComponent
                            = true;

                        //Выходим из функции
                        return;
                    }
                }

                //Определяем название компонента
                string componentName
                    = "";

                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Берём название двигателя
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .engines[componentIndex].ObjectName;
                }
                //Иначе, если тип компонента - реактор
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //Берём название топливного бака
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .reactors[componentIndex].ObjectName;
                }
                //Иначе, если тип компонента - топливный бак
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Берём название топливного бака
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .fuelTanks[componentIndex].ObjectName;
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Берём название оборудования для твёрдой добычи
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .solidExtractionEquipments[componentIndex].ObjectName;
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Берём название энергетического орудия
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .energyGuns[componentIndex].ObjectName;
                }

                //Записываем данные компонента
                WDShipClassComponent newComponent
                    = new(
                        contentData.Value.wDContentSets[contentSetIndex].ContentSetName,
                        componentName,
                        numberOfComponents,
                        contentSetIndex,
                        componentIndex,
                        true);

                //Изменяем размер массива компонентов
                Array.Resize(
                    ref wDShipClassComponents,
                    wDShipClassComponents.Length + 1);

                //Заносим новый компонент в массив как последний элемент
                wDShipClassComponents[wDShipClassComponents.Length - 1]
                    = newComponent;
            }
        }

        void DesignerShipClassDeleteComponentFirst(
            ShipComponentType componentType,
            int contentSetIndex,
            int componentIndex,
            int numberOfComponents)
        {
            //Берём ссылку на окно дизайнера кораблей
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //Создаём переменную для отслеживания, был ли компонент установлен ранее
            bool isOldComponent
                = false;

            //Создаём переменную для отслеживания, активен ли переключатель в списке установленных компонентов
            bool isInstalledActive
                = false;

            //Если какой-либо переключатель в списке установленных активен
            if (shipDesignerWindow.installedComponentsListToggleGroup.AnyTogglesOn()
                == true)
            {
                isInstalledActive
                    = true;
            }

            //Если событие запрашивает удаление двигателя
            if (componentType
                == ShipComponentType.Engine)
            {
                //Удаляем двигатель
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.engines,
                    ref shipDesignerWindow.currentGameShipClass.engines,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает удаление реактора
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Удаляем реактор
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.reactors,
                    ref shipDesignerWindow.currentGameShipClass.reactors,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает удаление топливного бака
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Удаляем топливный бак
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.fuelTanks,
                    ref shipDesignerWindow.currentGameShipClass.fuelTanks,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает удаление оборудования для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Удаляем оборудование для твёрдой добычи
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids,
                    ref shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //Иначе, если событие запрашивает удаление энергетического орудия
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Удаляем энергетическое орудие
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.energyGuns,
                    ref shipDesignerWindow.currentGameShipClass.energyGuns,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }

            //Обновляем список установленных компонентов
            DesignerShipClassDisplayInstalledComponents(
                ShipComponentType.None);

            //Пересчитываем и отображаем характеристики корабля
            shipDesignerWindow.CalculateCharacteristics(
                contentData.Value,
                eUI.Value.designerWindow.isInGameDesigner);

            //Если компонент был установлен ранее
            if (isOldComponent
                == true)
            {
                //И какой-либо переключатель в списке установленных активен
                if (isInstalledActive
                    == true)
                {
                    //Для каждой панели в списке установленных компонентов
                    for (int a = 0; a < shipDesignerWindow.installedComponentsPanelsList.Count; a++)
                    {
                        //Берём компонент обзорной панели установленного компонента
                        if (shipDesignerWindow.installedComponentsPanelsList[a].TryGetComponent(
                            out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                        {
                            //Если её данные соответствую сохранённым
                            if (installedComponentBriefInfoPanel.componentType
                                == componentType
                                && installedComponentBriefInfoPanel.contentSetIndex
                                == contentSetIndex
                                && installedComponentBriefInfoPanel.componentIndex
                                == componentIndex)
                            {
                                //Активируем переключатель панели
                                installedComponentBriefInfoPanel.panelToggle.isOn
                                    = true;
                            }
                        }
                    }
                }
            }
            //Иначе
            else
            {
                //Очищаем панель подробной информации о компоненте
                DesignerShipClassDisplayComponentDetailedInfo(
                    false);
            }
        }

        void DesignerShipClassDeleteComponentSecond(
            ref WDShipClassComponent[] wDShipClassComponents,
            ref DShipClassComponent[] dShipClassComponents,
            int contentSetIndex,
            int componentIndex,
            int numberOfComponents,
            out bool isOldComponent)
        {
            //Отмечаем, что компонент не был установлен ранее
            isOldComponent
                = false;

            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Для каждого компонента в массиве
                for (int a = 0; a < dShipClassComponents.Length; a++)
                {
                    //Если индекс набора контента и индекс двигателя соответствуют запрошенному компоненту
                    if (dShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && dShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //Если требуется удалить меньше компонентов, чем установлено
                        if (dShipClassComponents[a].numberOfComponents
                            > numberOfComponents)
                        {
                            //Обновляем число двигателей
                            dShipClassComponents[a].numberOfComponents
                                -= numberOfComponents;

                            //Отмечаем, что это был старый компонент
                            isOldComponent
                                = true;
                        }
                        //Иначе
                        else
                        {
                            //Удаляем компонент полностью

                            //Копируем массив в список
                            List<DShipClassComponent> shipClassComponents
                                = new(dShipClassComponents);

                            //Удаляем компонент с указанным индексом
                            shipClassComponents.RemoveAt(
                                a);

                            //Перезаписываем массив
                            dShipClassComponents
                                = shipClassComponents.ToArray();
                        }

                        //Выходим из цикла
                        break;
                    }
                }
            }
            //Иначе
            else
            {
                //Для каждого компонента в массиве
                for (int a = 0; a < wDShipClassComponents.Length; a++)
                {
                    //Если индекс набора контента и индекс двигателя соответствуют запрошенному компоненту
                    if (wDShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && wDShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //Если требуется удалить меньше компонентов, чем установлено
                        if (wDShipClassComponents[a].numberOfComponents
                            > numberOfComponents)
                        {
                            //Обновляем число двигателей
                            wDShipClassComponents[a].numberOfComponents
                                -= numberOfComponents;

                            //Отмечаем, что это был старый компонент
                            isOldComponent
                                = true;
                        }
                        //Иначе
                        else
                        {
                            //Удаляем компонент полностью

                            //Копируем массив в список
                            List<WDShipClassComponent> shipClassComponents
                                = new(wDShipClassComponents);

                            //Удаляем компонент с указанным индексом
                            shipClassComponents.RemoveAt(
                                a);

                            //Перезаписываем массив
                            wDShipClassComponents
                                = shipClassComponents.ToArray();
                        }

                        //Выходим из цикла
                        break;
                    }
                }
            }
        }

        void DesignerShipClassDisplayComponentDetailedInfo(
            bool isDisplay,
            int contentSetIndex = -1,
            int componentIndex = -1,
            ShipComponentType componentType = ShipComponentType.None)
        {
            //Берём ссылку на окно дизайнера кораблей
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //Если какая-то панель подробной информации о компоненте была активна
            if (shipDesignerWindow.activeComponentDetailedInfoPanel
                != null)
            {
                //Делаем её неактивной
                shipDesignerWindow.activeComponentDetailedInfoPanel.gameObject.SetActive(
                    false);
            }

            //Если требуется отобразить панель подробной информации
            if (isDisplay
                == true)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Если тип компонента - двигатель
                    if (componentType
                        == ShipComponentType.Engine)
                    {
                        //Берём ссылку на двигатель
                        ref readonly DEngine engine
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .engines[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.EngineDetailedInfoPanelControl(
                            engine,
                            engine);
                    }
                    //Иначе, если тип компонента - реактор
                    else if (componentType
                        == ShipComponentType.Reactor)
                    {
                        //Берём ссылку на реактор
                        ref readonly DReactor reactor
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .reactors[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.ReactorDetailedInfoPanelControl(
                            reactor,
                            reactor);
                    }
                    //Иначе, если тип компонента - топливный бак
                    else if (componentType
                        == ShipComponentType.HoldFuelTank)
                    {
                        //Берём ссылку на топливный бак
                        ref readonly DHoldFuelTank fuelTank
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .fuelTanks[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.FuelTankDetailedInfoPanelControl(
                            fuelTank,
                            fuelTank,
                            fuelTank);
                    }
                    //Иначе, если тип компонента - оборудование для твёрдой добычи
                    else if (componentType
                        == ShipComponentType.ExtractionEquipmentSolid)
                    {
                        //Берём ссылку на оборудование для твёрдой добычи
                        ref readonly DExtractionEquipment extractionEquipmentSolid
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .solidExtractionEquipments[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.ExtractionEquipmentDetailedInfoPanelControl(
                            extractionEquipmentSolid,
                            extractionEquipmentSolid);
                    }
                    //Иначе, если тип компонента - энергетическое орудие
                    else if (componentType
                        == ShipComponentType.GunEnergy)
                    {
                        //Берём ссылку на энергетическое орудие
                        ref readonly DGunEnergy energyGun
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .energyGuns[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.EnergyGunDetailedInfoPanelControl(
                            energyGun,
                            energyGun,
                            energyGun);
                    }
                }
                //Иначе
                else
                {
                    //Если тип компонента - двигатель
                    if (componentType
                        == ShipComponentType.Engine)
                    {
                        //Берём ссылку на двигатель
                        ref readonly WDEngine engine
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .engines[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.EngineDetailedInfoPanelControl(
                            engine,
                            engine);
                    }
                    //Иначе, если тип компонента - реактор
                    else if (componentType
                        == ShipComponentType.Reactor)
                    {
                        //Берём ссылку на реактор
                        ref readonly WDReactor reactor
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .reactors[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.ReactorDetailedInfoPanelControl(
                            reactor,
                            reactor);
                    }
                    //Иначе, если тип компонента - топливный бак
                    else if (componentType
                        == ShipComponentType.HoldFuelTank)
                    {
                        //Берём ссылку на реактор
                        ref readonly WDHoldFuelTank fuelTank
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .fuelTanks[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.FuelTankDetailedInfoPanelControl(
                            fuelTank,
                            fuelTank,
                            fuelTank);
                    }
                    //Иначе, если тип компонента - оборудование для твёрдой добычи
                    else if (componentType
                        == ShipComponentType.ExtractionEquipmentSolid)
                    {
                        //Берём ссылку на оборудование для твёрдой добычи
                        ref readonly WDExtractionEquipment extractionEquipmentSolid
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .solidExtractionEquipments[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.ExtractionEquipmentDetailedInfoPanelControl(
                            extractionEquipmentSolid,
                            extractionEquipmentSolid);
                    }
                    //Иначе, если тип компонента - энергетическое орудие
                    else if (componentType
                        == ShipComponentType.GunEnergy)
                    {
                        //Берём ссылку на энергетическое орудие
                        ref readonly WDGunEnergy energyGun
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .energyGuns[componentIndex];

                        //Отображаем подробную информацию
                        shipDesignerWindow.EnergyGunDetailedInfoPanelControl(
                            energyGun,
                            energyGun,
                            energyGun);
                    }
                }
            }
            //Иначе
            else
            {
                //Стираем название компонента
                shipDesignerWindow.currentComponentName.text
                    = "";

                //И тип текущего компонента
                shipDesignerWindow.currentComponentType.text
                    = "";
            }
        }

        void DesignerShipClassDisplayAvailableComponentsType(
            ShipComponentType componentType)
        {
            //Берём ссылку на окно дизайнера кораблей
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //Указываем текущий тип доступных компонентов корабля
            shipDesignerWindow.currentAvailableComponentsType
                = componentType;

            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Запрашиваем отображение соответствующего типа компонентов
                DesignerDisplayContentPanels(
                    designerData.Value.ShipComponentToDesignerType(
                        componentType),
                    DesignerDisplayContentType.ContentSet,
                    shipDesignerWindow.availableComponentsListToggleGroup,
                    shipDesignerWindow.availableComponentsListLayoutGroup,
                    shipDesignerWindow.availableComponentsPanelsList,
                    eUI.Value.designerWindow.currentContentSetIndex);
            }
            //Иначе
            else
            {
                //Запрашиваем отображение соответствующего типа компонентов
                DesignerDisplayContentPanels(
                    designerData.Value.ShipComponentToDesignerType(
                        componentType),
                    DesignerDisplayContentType.ContentSetsAll,
                    shipDesignerWindow.availableComponentsListToggleGroup,
                    shipDesignerWindow.availableComponentsListLayoutGroup,
                    shipDesignerWindow.availableComponentsPanelsList,
                    eUI.Value.designerWindow.currentContentSetIndex);
            }
        }

        void DesignerShipClassDeleteAllComponentsRefs(
            int shipClassContentSetIndex,
            int shipClassIndex)
        {
            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Берём ссылку на класс корабля
                ref DShipClass shipClass
                    = ref contentData.Value
                    .contentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //Для каждого двигателя, на который ссылается корабль
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого реактора, на который ссылается корабль
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого топливного бака, на который ссылается корабль
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого оборудования для твёрдой добычи, на которое ссылается корабль
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого энергетического орудия, на которое ссылается корабль
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.energyGuns[a],
                        ShipComponentType.GunEnergy,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }
            }
            //Иначе
            else
            {
                //Берём ссылку на класс корабля
                ref WDShipClass shipClass
                    = ref contentData.Value
                    .wDContentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //Для каждого двигателя, на который ссылается корабль
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого реактора, на который ссылается корабль
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого топливного бака, на который ссылается корабль
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого оборудования для твёрдой добычи, на которое ссылается корабль
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого энергетического орудия, на которое ссылается корабль
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //Удаляем ссылку на корабль
                    DesignerShipClassDeleteComponentRef(
                        shipClass.energyGuns[a],
                        ShipComponentType.GunEnergy,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }
            }
        }

        void DesignerShipClassDeleteComponentRef(
            IContentObjectLink shipClassComponentRef,
            ShipComponentType componentType,
            int shipClassContentSetIndex,
            int shipClassIndex)
        {
            //Если тип компонента - двигатель
            if (componentType
                == ShipComponentType.Engine)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на двигатель
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данный двигатель
                    for (int a = 0; a < engine.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (engine.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && engine.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            engine.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на двигатель
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данный двигатель
                    for (int a = 0; a < engine.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (engine.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && engine.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            engine.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
            //Иначе, если тип компонента - реактор
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на реактор
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данный реактор
                    for (int a = 0; a < reactor.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (reactor.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && reactor.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            reactor.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на реактор
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данный реактор
                    for (int a = 0; a < reactor.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (reactor.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && reactor.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            reactor.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
            //Иначе, если тип компонента - топливный бак
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на топливный бак
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данный топливный бак
                    for (int a = 0; a < fuelTank.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (fuelTank.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && fuelTank.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            fuelTank.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на топливный бак
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данный топливный бак
                    for (int a = 0; a < fuelTank.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (fuelTank.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && fuelTank.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            fuelTank.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
            //Иначе, если тип компонента - оборудование для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данное оборудование для твёрдой добычи
                    for (int a = 0; a < extractionEquipmentSolid.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (extractionEquipmentSolid.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && extractionEquipmentSolid.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            extractionEquipmentSolid.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на данное оборудование для твёрдой добычи
                    for (int a = 0; a < extractionEquipmentSolid.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (extractionEquipmentSolid.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && extractionEquipmentSolid.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            extractionEquipmentSolid.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
            //Иначе, если тип компонента - энергетическое орудие
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на энергетическое орудие
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на энергетическое орудие
                    for (int a = 0; a < energyGun.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (energyGun.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && energyGun.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            energyGun.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на энергетическое орудие
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //Для каждого корабля, ссылающегося на энергетическое орудие
                    for (int a = 0; a < energyGun.ShipClasses.Count; a++)
                    {
                        //Если индексы набора контента и класса корабля совпадают
                        if (energyGun.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && energyGun.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //Удаляем данный корабль из списка ссылающихся
                            energyGun.ShipClasses.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
        }

        void DesignerShipClassAddAllComponentsRefs(
            int shipClassContentSetIndex,
            int shipClassIndex)
        {
            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Берём ссылку на класс корабля
                ref DShipClass shipClass
                    = ref contentData.Value
                    .contentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //Для каждого двигателя, на который ссылается корабль
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого реактора, на который ссылается корабль
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого топливного бака, на который ссылается корабль
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого оборудования для твёрдой добычи, на которое ссылается корабль
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого энергетического орудия, на которое ссылается корабль
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.energyGuns[a],
                        ShipComponentType.GunEnergy,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }
            }
            //Иначе
            else
            {
                //Берём ссылку на класс корабля
                ref WDShipClass shipClass
                    = ref contentData.Value
                    .wDContentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //Для каждого двигателя, на который ссылается корабль
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого реактора, на который ссылается корабль
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого топливного бака, на который ссылается корабль
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого энергетического орудия, на которое ссылается корабль
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //Для каждого энергетического орудия, на которое ссылается корабль
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //Добавляем ссылку на корабль
                    DesignerShipClassAddComponentRef(
                        shipClass.energyGuns[a],
                        ShipComponentType.GunEnergy,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }
            }
        }

        void DesignerShipClassAddComponentRef(
            IContentObjectLink shipClassComponentRef,
            ShipComponentType componentType,
            int shipClassContentSetIndex,
            int shipClassIndex)
        {
            //Если тип компонента - двигатель
            if (componentType
                == ShipComponentType.Engine)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на двигатель
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данный двигатель
                    engine.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе
                else
                {
                    //Берём ссылку на двигатель
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данный двигатель
                    engine.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //Иначе, если тип компонента - реактор
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на реактор
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данный реактор
                    reactor.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе
                else
                {
                    //Берём ссылку на реактор
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данный реактор
                    reactor.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //Иначе, если тип компонента - топливный бак
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на топливный бак
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данный топливный бак
                    fuelTank.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе
                else
                {
                    //Берём ссылку на топливный бак
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данный топливный бак
                    fuelTank.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //Иначе, если тип компонента - оборудование для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данное оборудование для твёрдой добычи
                    extractionEquipmentSolid.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе
                else
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данное оборудование для твёрдой добычи
                    extractionEquipmentSolid.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //Иначе, если тип компонента - энергетическое орудие
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на энергетическое орудие
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данное энергетическое орудие
                    energyGun.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //Иначе
                else
                {
                    //Берём ссылку на энергетическое орудие
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //Добавляем текущий корабль в список ссылающихся на данное энергетическое орудие
                    energyGun.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
        }

        //Дизайнер компонентов
        void DesignerComponentChangeCoreTechnology(
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyDropdownIndex)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Если активен дизайнер двигателей
            if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //Берём ссылку на окно дизайнера двигателей
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //Если тип модификатора технологии - мощность на единицу размера
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.EnginePowerPerSize)
                {
                    //Изменяем значение модификатора на соответствующей панели

                    //Указываем текущее значение модификатора
                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentParameterText.text
                        = engineDesignerWindow.powerPerSizeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //Указываем индекс текущей технологии
                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //Пересчитываем и отображаем характеристики двигателя
                engineDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер реакторов
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //Берём ссылку на окно дизайнера реакторов
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //Если тип модификатора технологии - энергия на единицу размера
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.ReactorEnergyPerSize)
                {
                    //Изменяем значение модификатора на соответствующей панели

                    //Указываем текущее значение модификатора
                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentParameterText.text
                        = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //Указываем индекс текущей технологии
                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //Пересчитываем и отображаем характеристики реактора
                reactorDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер топливных баков
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //Берём ссылку на окно дизайнера топливных баков
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //Если тип модификатора технологии - сжатие
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.FuelTankCompression)
                {
                    //Изменяем значение модификатора на соответствующей панели

                    //Указываем текущее значение модификатора
                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentParameterText.text
                        = fuelTankDesignerWindow.compressionCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //Указываем индекс текущей технологии
                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //Пересчитываем и отображаем характеристики топливного бака
                fuelTankDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер оборудования для твёрдой добычи
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //Берём ссылку на окно дизайнера добывающего оборудования
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //Если тип модификатора технологии - скорость на единицу размера
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize)
                {
                    //Изменяем значение модификатора на соответствующей панели

                    //Указываем текущее значение модификатора
                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentParameterText.text
                        = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //Указываем индекс текущей технологии
                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //Пересчитываем и отображаем характеристики оборудования для твёрдой добычи
                extractionEquipmentDesignerWindow.CalculateCharacteristics();
            }
            //Иначе, если активен дизайнер энергетических орудий
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //Берём ссылку на окно дизайнера энергетических орудий
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //Если тип модификатора технологии - перезарядка
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.GunEnergyRecharge)
                {
                    //Изменяем значение модификатора на соответствующей панели

                    //Указываем текущее значение модификатора
                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentParameterText.text
                        = energyGunDesignerWindow.rechargeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //Указываем индекс текущей технологии
                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //Пересчитываем и отображаем характеристики энергетического орудия
                energyGunDesignerWindow.CalculateCharacteristics();
            }
        }

        void DesignerComponentDisplayFactionCoreTechnologies(
            in DTechnologyModifierGlobalSort[] globalTechnologiesArray,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray,
            List<Tuple<int, int, float>> technologyModifiersList,
            UIComponentCoreTechnologyPanel componentCoreTechnologyPanel,
            TechnologyComponentCoreModifierType technologyComponentCoreModifierType)
        {
            //Создаём временный список названий основных технологий
            List<string> coreComponentTechnologyNames
                = new();

            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Для каждой технологии в переданном глобальном массиве технологий
                for (int a = 0; a < globalTechnologiesArray.Length; a++)
                {
                    //Берём ссылку на данные технологии
                    ref readonly DTechnologyModifierGlobalSort technologyGlobalSortData
                        = ref globalTechnologiesArray[a];

                    //Если запись о данной технологии есть в переданном словаре фракции
                    if (factionTechnologiesArray[technologyGlobalSortData.contentSetIndex].TryGetValue(
                        technologyGlobalSortData.technologyIndex,
                        out DOrganizationTechnology tempFactionTechnology))
                    {
                        //Если данная технология исследована
                        if (tempFactionTechnology.isResearched
                            == true)
                        {
                            //Берём ссылку на данные технологии
                            ref readonly DTechnology technology
                                = ref contentData.Value
                                .contentSets[technologyGlobalSortData.contentSetIndex]
                                .technologies[technologyGlobalSortData.technologyIndex];

                            //Заносим её название в список названий технологий
                            coreComponentTechnologyNames.Add(
                                technology.ObjectName);

                            //Для каждого модификатора основной технологии компонента
                            for (int b = 0; b < technology.technologyComponentCoreModifiers.Length; b++)
                            {
                                //Если тип модификатора соответствует запрошенному
                                if (technology.technologyComponentCoreModifiers[b].ModifierType
                                    == technologyComponentCoreModifierType)
                                {
                                    //Заносим данные технологии в переданный список технологий
                                    technologyModifiersList.Add(
                                        new Tuple<int, int, float>(
                                            technologyGlobalSortData.contentSetIndex,
                                            technologyGlobalSortData.technologyIndex,
                                            technologyGlobalSortData.modifierValue));
                                }
                            }
                        }
                    }
                }
            }
            //Иначе
            else
            {
                //Для каждой технологии в переданном глобальном массиве технологий
                for (int a = 0; a < globalTechnologiesArray.Length; a++)
                {
                    //Берём ссылку на данные технологии
                    ref readonly DTechnologyModifierGlobalSort technologyGlobalSortData
                        = ref globalTechnologiesArray[a];

                    //Если запись о данной технологии есть в переданном словаре фракции
                    if (factionTechnologiesArray[technologyGlobalSortData.contentSetIndex].TryGetValue(
                        technologyGlobalSortData.technologyIndex,
                        out DOrganizationTechnology tempFactionTechnology))
                    {
                        //Если данная технология исследована
                        if (tempFactionTechnology.isResearched
                            == true)
                        {
                            //Берём ссылку на данные технологии
                            ref readonly WDTechnology technology
                                = ref contentData.Value
                                .wDContentSets[technologyGlobalSortData.contentSetIndex]
                                .technologies[technologyGlobalSortData.technologyIndex];

                            //Заносим её название в список названий технологий
                            coreComponentTechnologyNames.Add(
                                technology.ObjectName);

                            //Для каждого модификатора основной технологии компонента
                            for (int b = 0; b < technology.technologyComponentCoreModifiers.Length; b++)
                            {
                                //Если тип модификатора соответствует запрошенному
                                if (technology.technologyComponentCoreModifiers[b].ModifierType
                                    == technologyComponentCoreModifierType)
                                {
                                    //Заносим данные технологии в переданный список технологий
                                    technologyModifiersList.Add(
                                        new Tuple<int, int, float>(
                                            technologyGlobalSortData.contentSetIndex,
                                            technologyGlobalSortData.technologyIndex,
                                            technologyGlobalSortData.modifierValue));
                                }
                            }
                        }
                    }
                }
            }

            //Настраиваем переданную панель основной технологии
            eUI.Value.designerWindow.ComponentCoreTechnologyPanelControl(
                coreComponentTechnologyNames,
                componentCoreTechnologyPanel);

            //Если число названий в списке больше нуля
            if (coreComponentTechnologyNames.Count
                > 0
                //И число технологий в списке больше нуля
                && technologyModifiersList.Count
                > 0)
            {
                //Указываем значение модификатора
                componentCoreTechnologyPanel.currentParameterText.text
                    = technologyModifiersList[technologyModifiersList.Count - 1].Item3.ToString();

                //Указываем индекс текущей технологии
                componentCoreTechnologyPanel.currentTechnologyIndex
                    = technologyModifiersList.Count - 1;

                //Изменяем выбранную технологию в выпадающем списке
                componentCoreTechnologyPanel.technologiesDropdown.SetValueWithoutNotify(
                    technologyModifiersList.Count - 1);
            }
            //Иначе
            else
            {
                //Очищаем панель значения модификатора
                componentCoreTechnologyPanel.currentParameterText.text
                    = "";

                //Указываем, что индекс текущей технологии - -1
                componentCoreTechnologyPanel.currentTechnologyIndex
                    = -1;
            }
        }

        void DesignerComponentDeleteAllTechnologiesRefs(
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //Если тип компонента - двигатель
            if (componentType
                == ShipComponentType.Engine)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на двигатель
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //Для каждой основной технологии, на которую ссылается двигатель
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на двигатель
                        DesignerComponentDeleteTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на двигатель
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //Для каждой основной технологии, на которую ссылается двигатель
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на двигатель
                        DesignerComponentDeleteTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - реактор
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на реактор
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //Для каждой основной технологии, на которую ссылается реактор
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на реактор
                        DesignerComponentDeleteTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на реактор
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //Для каждой основной технологии, на которую ссылается реактор
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на реактор
                        DesignerComponentDeleteTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - топливный бак
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на топливный бак
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //Для каждой основной технологии, на которую ссылается топливный бак
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на топливный бак
                        DesignerComponentDeleteTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на топливный бак
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //Для каждой основной технологии, на которую ссылается топливный бак
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на топливный бак
                        DesignerComponentDeleteTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - оборудование для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //Для каждой основной технологии, на которую ссылается оборудование для твёрдой добычи
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на оборудование для твёрдой добычи
                        DesignerComponentDeleteTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //Для каждой основной технологии, на которую ссылается оборудование для твёрдой добычи
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на оборудование для твёрдой добычи
                        DesignerComponentDeleteTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - энергетическое орудие
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на энергетическое орудие
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //Для каждой основной технологии, на которую ссылается энергетическое орудие
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на энергетическое орудие
                        DesignerComponentDeleteTechnologyRef(
                            energyGun.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.GunEnergy,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на энергетическое орудие
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //Для каждой основной технологии, на которую ссылается энергетическое орудие
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //Удаляем ссылку на энергетическое орудие
                        DesignerComponentDeleteTechnologyRef(
                            energyGun.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.GunEnergy,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
        }

        void DesignerComponentDeleteTechnologyRef(
            IContentObjectLink coreTechnologyRef,
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Берём ссылку на технологию
                ref DTechnology technology
                    = ref contentData.Value
                    .contentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Для каждого двигателя, ссылающегося на данную технологию
                    for (int a = 0; a < technology.engines.Count; a++)
                    {
                        //Если индексы набора контента и двигателя совпадают
                        if (technology.engines[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.engines[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данный двигатель из списка ссылающихся
                            technology.engines.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - реактор
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //Для каждого реактора, ссылающегося на данную технологию
                    for (int a = 0; a < technology.reactors.Count; a++)
                    {
                        //Если индексы набора контента и реактора совпадают
                        if (technology.reactors[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.reactors[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данный реактор из списка ссылающихся
                            technology.reactors.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - топливный бак
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Для каждого топливного бака, ссылающегося на данную технологию
                    for (int a = 0; a < technology.fuelTanks.Count; a++)
                    {
                        //Если индексы набора контента и топливного бака совпадают
                        if (technology.fuelTanks[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.fuelTanks[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данный топливный бак из списка ссылающихся
                            technology.fuelTanks.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Для каждого оборудования для твёрдой добычи, ссылающегося на данную технологию
                    for (int a = 0; a < technology.extractionEquipmentSolids.Count; a++)
                    {
                        //Если индексы набора контента и оборудования для твёрдой добычи совпадают
                        if (technology.extractionEquipmentSolids[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.extractionEquipmentSolids[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данное оборудование для твёрдой добычи из списка ссылающихся
                            technology.extractionEquipmentSolids.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Для каждого энергетического орудия, ссылающегося на данную технологию
                    for (int a = 0; a < technology.energyGuns.Count; a++)
                    {
                        //Если индексы набора контента и энергетического орудия
                        if (technology.energyGuns[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.energyGuns[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данное энергетического орудия из списка ссылающихся
                            technology.energyGuns.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
            //Иначе
            else
            {
                //Берём ссылку на технологию
                ref WDTechnology technology
                    = ref contentData.Value
                    .wDContentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Для каждого двигателя, ссылающегося на данную технологию
                    for (int a = 0; a < technology.engines.Count; a++)
                    {
                        //Если индексы набора контента и двигателя совпадают
                        if (technology.engines[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.engines[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данный двигатель из списка ссылающихся
                            technology.engines.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - реактор
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //Для каждого реактора, ссылающегося на данную технологию
                    for (int a = 0; a < technology.reactors.Count; a++)
                    {
                        //Если индексы набора контента и реактора совпадают
                        if (technology.reactors[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.reactors[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данный реактор из списка ссылающихся
                            technology.reactors.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - топливный бак
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Для каждого топливного бака, ссылающегося на данную технологию
                    for (int a = 0; a < technology.fuelTanks.Count; a++)
                    {
                        //Если индексы набора контента и топливного бака совпадают
                        if (technology.fuelTanks[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.fuelTanks[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данный топливный бак из списка ссылающихся
                            technology.fuelTanks.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Для каждого оборудования для твёрдой добычи, ссылающегося на данную технологию
                    for (int a = 0; a < technology.extractionEquipmentSolids.Count; a++)
                    {
                        //Если индексы набора контента и оборудования для твёрдой добычи совпадают
                        if (technology.extractionEquipmentSolids[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.extractionEquipmentSolids[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данное оборудование для твёрдой добычи из списка ссылающихся
                            technology.extractionEquipmentSolids.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Для каждого энергетического орудия, ссылающегося на данную технологию
                    for (int a = 0; a < technology.energyGuns.Count; a++)
                    {
                        //Если индексы набора контента и энергетического орудия
                        if (technology.energyGuns[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.energyGuns[a].ObjectIndex
                            == componentIndex)
                        {
                            //Удаляем данное энергетического орудия из списка ссылающихся
                            technology.energyGuns.RemoveAt(
                                a);

                            //Выходим из цикла
                            break;
                        }
                    }
                }
            }
        }

        void DesignerComponentAddAllTechnologiesRefs(
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //Если тип компонента - двигатель
            if (componentType
                == ShipComponentType.Engine)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на двигатель
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //Для каждой основной технологии, на которую ссылается двигатель
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на двигатель
                        DesignerComponentAddTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на двигатель
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //Для каждой основной технологии, на которую ссылается двигатель
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на двигатель
                        DesignerComponentAddTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - реактор
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на реактор
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //Для каждой основной технологии, на которую ссылается реактор
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на реактор
                        DesignerComponentAddTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на реактор
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //Для каждой основной технологии, на которую ссылается реактор
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на реактор
                        DesignerComponentAddTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - топливный бак
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на топливный бак
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //Для каждой основной технологии, на которую ссылается топливный бак
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на топливный бак
                        DesignerComponentAddTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на топливный бак
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //Для каждой основной технологии, на которую ссылается топливный бак
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на топливный бак
                        DesignerComponentAddTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - оборудование для твёрдой добычи
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //Для каждой основной технологии, на которую ссылается оборудование для твёрдой добычи
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на оборудование для твёрдой добычи
                        DesignerComponentAddTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на оборудование для твёрдой добычи
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //Для каждой основной технологии, на которую ссылается оборудование для твёрдой добычи
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на оборудование для твёрдой добычи
                        DesignerComponentAddTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //Иначе, если тип компонента - энергетическое орудие
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //Если активен внутриигровой дизайнер
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //Берём ссылку на энергетическое орудие
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //Для каждой основной технологии, на которую ссылается энергетическое орудие
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на оборудование для твёрдой добычи
                        DesignerComponentAddTechnologyRef(
                            energyGun.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.GunEnergy,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //Иначе
                else
                {
                    //Берём ссылку на энергетическое орудие
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //Для каждой основной технологии, на которую ссылается энергетическое орудие
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //Добавляем ссылку на оборудование для твёрдой добычи
                        DesignerComponentAddTechnologyRef(
                            energyGun.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.GunEnergy,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
        }

        void DesignerComponentAddTechnologyRef(
            IContentObjectLink coreTechnologyRef,
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //Если активен внутриигровой дизайнер
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //Берём ссылку на технологию
                ref DTechnology technology
                    = ref contentData.Value
                    .contentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Добавляем текущий двигатель в список ссылающихся на данную технологию
                    technology.engines.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - реактор
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //Добавляем текущий реактор в список ссылающихся на данную технологию
                    technology.reactors.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - топливный бак
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Добавляем текущий топливный бак в список ссылающихся на данную технологию
                    technology.fuelTanks.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Добавляем текущее оборудование для твёрдой добычи в список ссылающихся на данную технологию
                    technology.extractionEquipmentSolids.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Добавляем текущее энергетическое орудие в список ссылающихся на данную технологию
                    technology.energyGuns.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
            }
            //Иначе
            else
            {
                //Берём ссылку на технологию
                ref WDTechnology technology
                    = ref contentData.Value
                    .wDContentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //Если тип компонента - двигатель
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //Добавляем текущий двигатель в список ссылающихся на данную технологию
                    technology.engines.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - реактор
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //Добавляем текущий реактор в список ссылающихся на данную технологию
                    technology.reactors.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - топливный бак
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //Добавляем текущий топливный бак в список ссылающихся на данную технологию
                    technology.fuelTanks.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - оборудование для твёрдой добычи
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //Добавляем текущее оборудование для твёрдой добычи в список ссылающихся на данную технологию
                    technology.extractionEquipmentSolids.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //Иначе, если тип компонента - энергетическое орудие
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //Добавляем текущее энергетическое орудие в список ссылающихся на данную технологию
                    technology.energyGuns.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
            }
        }


        //Дизайнер двигателей
        void DesignerOpenEngineComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём ссылку на окно дизайнера двигателей
            UIEngineDesignerWindow engineDesignerWindow
                = eUI.Value.designerWindow.engineDesigner;


            //Определяем индекс типа дизайнера двигателей
            int engineDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.Engine);

            //Берём ссылку на данные типа дизайнера
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[engineDesignerIndex];


            //Заполняем выпадающий список прочих наборов контента
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //Если активен не внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == false)
            {
                //Если индекс текущего набора контента не равен нулю и не равен индексу базового набора контента
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //Отображаем массив двигателей из базового набора контента
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //Отображаем массив двигателей из текущего набора контента
            DesignerDisplayContentSetPanelList(
                true);


            //Отображаем основные технологии

            //Отображаем технологию мощности двигателя на единицу размера
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesEnginePowerPerSize,
                in factionTechnologiesArray,
                engineDesignerWindow.powerPerSizeCoreTechnologiesList,
                engineDesignerWindow.powerPerSizeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.EnginePowerPerSize);

            //Отображаем параметры

            //Настраиваем панель размера двигателя
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinEngineSize,
                technologyModifiers.designerMaxEngineSize,
                engineDesignerWindow.sizeParameterPanel);

            //Настраиваем панель разгона двигателя
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[1],
                technologyModifiers.designerMinEngineBoost,
                technologyModifiers.designerMaxEngineBoost,
                engineDesignerWindow.boostParameterPanel);


            //Пересчитываем и отображаем характеристики двигателя
            engineDesignerWindow.CalculateCharacteristics();
        }
        //Дизайнер реакторов
        void DesignerOpenReactorComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём ссылку на окно дизайнера реакторов
            UIReactorDesignerWindow reactorDesignerWindow
                = eUI.Value.designerWindow.reactorDesigner;


            //Определяем индекс типа дизайнера реакторов
            int reactorDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.Reactor);

            //Берём ссылку на данные типа дизайнера
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[reactorDesignerIndex];


            //Заполняем выпадающий список прочих наборов контента
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //Если активен не внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == false)
            {
                //Если индекс текущего набора контента не равен нулю и не равен индексу базового набора контента
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //Отображаем массив реакторов из базового набора контента
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //Отображаем массив реакторов из текущего набора контента
            DesignerDisplayContentSetPanelList(
                true);


            //Отображаем основные технологии

            //Отображаем технологию энергии реактора на единицу размера
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesReactorEnergyPerSize,
                in factionTechnologiesArray,
                reactorDesignerWindow.energyPerSizeCoreTechnologiesList,
                reactorDesignerWindow.energyPerSizeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.ReactorEnergyPerSize);

            //Отображаем параметры

            //Настраиваем панель размера реактора
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinReactorSize,
                technologyModifiers.designerMaxReactorSize,
                reactorDesignerWindow.sizeParameterPanel);

            //Настраиваем панель разгона реактора
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[1],
                technologyModifiers.designerMinReactorBoost,
                technologyModifiers.designerMaxReactorBoost,
                reactorDesignerWindow.boostParameterPanel);


            //Пересчитываем и отображаем характеристики реактора
            reactorDesignerWindow.CalculateCharacteristics();
        }
        //Дизайнер топливных баков
        void DesignerOpenFuelTankComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём ссылку на окно дизайнера топливных баков
            UIFuelTankDesignerWindow fuelTankDesignerWindow
                = eUI.Value.designerWindow.fuelTankDesigner;


            //Определяем индекс типа дизайнера топливных баков
            int fuelTankDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.HoldFuelTank);

            //Берём ссылку на данные типа дизайнера
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[fuelTankDesignerIndex];


            //Заполняем выпадающий список прочих наборов контента
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //Если активен не внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == false)
            {
                //Если индекс текущего набора контента не равен нулю и не равен индексу базового набора контента
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //Отображаем массив топливных баков из базового набора контента
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //Отображаем массив топливных баков из текущего набора контента
            DesignerDisplayContentSetPanelList(
                true);


            //Отображаем основные технологии

            //Отображаем технологию сжатия топливного бака
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesReactorEnergyPerSize,
                in factionTechnologiesArray,
                fuelTankDesignerWindow.compressionCoreTechnologiesList,
                fuelTankDesignerWindow.compressionCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.FuelTankCompression);

            //Отображаем параметры

            //Настраиваем панель размера топливного бака
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinFuelTankSize,
                technologyModifiers.designerMaxFuelTankSize,
                fuelTankDesignerWindow.sizeParameterPanel);


            //Пересчитываем и отображаем характеристики топливного бака
            fuelTankDesignerWindow.CalculateCharacteristics();
        }
        //Дизайнер добывающего оборудования
        void DesignerOpenExtractionEquipmentComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём ссылку на окно дизайнера оборудования для добычи
            UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                = eUI.Value.designerWindow.extractionEquipmentDesigner;


            //Определяем индекс типа дизайнера оборудования для добычи
            int extractionEquipmentDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.ExtractionEquipmentSolid);

            //Берём ссылку на данные типа дизайнера
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[extractionEquipmentDesignerIndex];


            //Заполняем выпадающий список прочих наборов контента
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //Если активен не внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == false)
            {
                //Если индекс текущего набора контента не равен нулю и не равен индексу базового набора контента
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //Отображаем массив оборудования для добычи из базового набора контента
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //Отображаем массив оборудования для добычи из текущего набора контента
            DesignerDisplayContentSetPanelList(
                true);


            //Отображаем основные технологии

            //Отображаем технологию скорости добычи на единицу размера
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesSolidExtractionEquipmentSpeedPerSize,
                in factionTechnologiesArray,
                extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList,
                extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize);

            //Отображаем параметры

            //Настраиваем панель размера оборудования для добычи
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinSolidExtractionEquipmentSize,
                technologyModifiers.designerMaxSolidExtractionEquipmentSize,
                extractionEquipmentDesignerWindow.sizeParameterPanel);


            //Пересчитываем и отображаем характеристики оборудования для добычи
            extractionEquipmentDesignerWindow.CalculateCharacteristics();
        }
        //Дизайнер энергетических орудий
        void DesignerOpenEnergyGunComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //Берём ссылку на окно дизайнера
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //Берём ссылку на окно дизайнера энергетических орудий
            UIGunEnergyDesignerWindow energyGunDesignerWindow
                = eUI.Value.designerWindow.energyGunDesigner;


            //Определяем индекс типа дизайнера энергетических орудий
            int engineDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.GunEnergy);

            //Берём ссылку на данные типа дизайнера
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[engineDesignerIndex];


            //Заполняем выпадающий список прочих наборов контента
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //Если активен не внутриигровой дизайнер
            if (designerWindow.isInGameDesigner
                == false)
            {
                //Если индекс текущего набора контента не равен нулю и не равен индексу базового набора контента
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //Отображаем массив энергетических орудий из базового набора контента
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //Отображаем массив энергетических орудий из текущего набора контента
            DesignerDisplayContentSetPanelList(
                true);


            //Отображаем основные технологии

            //Отображаем технологию перезарядки энергетического орудия на единицу размера
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesEnergyGunRecharge,
                in factionTechnologiesArray,
                energyGunDesignerWindow.rechargeCoreTechnologiesList,
                energyGunDesignerWindow.rechargeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.GunEnergyRecharge);

            //Отображаем параметры

            //Настраиваем панель размера энергетического орудия
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinEnergyGunCaliber,
                technologyModifiers.designerMaxEnergyGunCaliber,
                energyGunDesignerWindow.gunCaliberParameterPanel);

            //Настраиваем панель разгона энергетического орудия
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[1],
                technologyModifiers.designerMinEnergyGunBarrelLength,
                technologyModifiers.designerMaxEnergyGunBarrelLength,
                energyGunDesignerWindow.gunBarrelLengthParameterPanel);


            //Пересчитываем и отображаем характеристики энергетического орудия
            energyGunDesignerWindow.CalculateCharacteristics();
        }
        #endregion

        #region Game
        void EventCheckGame()
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Проверяем запросы создания панелей
            GameCreatePanelRequest();

            //Проверяем запросы действий в игре
            GameActionRequest();

            //Проверяем запросы открытия дизайнера в игре
            GameOpenDesignerRequest();

            //Проверяем самозапросы обновления панели RAEO
            GameRefreshRAEOObjectPanelSelfRequest();

            //Проверяем запросы действия в панели объекта
            GameObjectActionRequest();

            //Проверяем запросы действия в менеджере флотов
            FleetManagerActionRequest();

            //Проверяем самозапросы обновления интерфейса
            GameRefreshUISelfRequest();
        }

        readonly EcsFilterInject<Inc<RGameCreatePanel>> gameCreatePanelRequestFilter = default;
        readonly EcsPoolInject<RGameCreatePanel> gameCreatePanelRequestPool = default;
        void GameCreatePanelRequest()
        {
            //Для каждого запроса создания панели
            foreach (int requestEntity in gameCreatePanelRequestFilter.Value)
            {
                //Берём запрос
                ref RGameCreatePanel requestComp = ref gameCreatePanelRequestPool.Value.Get(requestEntity);

                //Если запрашивается создание обзорной панели ORAEO
                if (requestComp.creatingPanelType == CreatingPanelType.ORAEOBriefInfoPanel)
                {
                    //Создаём обзорную панель ORAEO
                    RegionORAEOsCreateORAEOSummaryPanel(ref requestComp);
                }
                //Иначе, если запрашивается создание обзорной панели флота
                else if (requestComp.creatingPanelType == CreatingPanelType.FleetOverviewPanel)
                {
                    //Берём флот
                    requestComp.objectPE.Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //Создаём обзорную панель флота
                    FleetManagerFleetsCreateFleetSummaryPanel(ref fleet);
                }
                //Иначе, если запрашивается создание обзорной панели оперативной группы
                else if (requestComp.creatingPanelType == CreatingPanelType.TaskForceOverviewPanel)
                {
                    //Берём группу
                    requestComp.objectPE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Создаём обзорную панель группы
                    FleetManagerFleetsCreateTaskForceSummaryPanel(ref taskForce);
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RGameAction>> gameActionRequestFilter = default;
        readonly EcsPoolInject<RGameAction> gameActionRequestPool = default;
        void GameActionRequest()
        {
            //Для каждого запроса действия в игре
            foreach (int requestEntity in gameActionRequestFilter.Value)
            {
                //Берём запрос
                ref RGameAction requestComp = ref gameActionRequestPool.Value.Get(requestEntity);

                //Изменяем состояние паузы
                GamePause(requestComp.actionType);

                world.Value.DelEntity(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RGameOpenDesigner>> gameOpenDesignerRequestFilter = default;
        readonly EcsPoolInject<RGameOpenDesigner> gameOpenDesignerEventPool = default;
        void GameOpenDesignerRequest()
        {
            //Для каждого запроса открытия окна дизайнера
            foreach (int requestEntity in gameOpenDesignerRequestFilter.Value)
            {
                //Берём запрос
                ref RGameOpenDesigner requestComp = ref gameOpenDesignerEventPool.Value.Get(requestEntity);

                //Активируем паузу
                GamePause(GameActionType.PauseOn);

                //Отображаем нужное окно дизайнера
                DesignerOpenWindow(
                    requestComp.contentSetIndex,
                    requestComp.designerType,
                    true,
                    inputData.Value.playerOrganizationPE);

                world.Value.DelEntity(requestEntity);
            }
        }

        void GameRefreshRAEOObjectPanelSelfRequest()
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Для каждого самозапроса обновления панели объекта RAEO
            foreach (int selfRequestEntity in refreshRAEOObjectPanelSelfRequestFilter.Value)
            {
                //Если активна подпанель RAEO
                if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                {
                    //Берём регион и RAEO
                    ref CHexRegion region = ref regionPool.Value.Get(selfRequestEntity);
                    ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(selfRequestEntity);

                    //Если активна панель того же RAEO, что имеет самозапрос
                    if (gameWindow.objectPanel.activeObjectPE.EqualsTo(rAEO.selfPE))
                    {
                        //Берём панель RAEO
                        UIRegionSubpanel RAEOSubpanel = gameWindow.objectPanel.regionSubpanel;

                        //Если активна обзорная вкладка
                        if (RAEOSubpanel.tabGroup.selectedTab == RAEOSubpanel.overviewTab.selfTabButton)
                        {
                            //Отображаем обзорную вкладку RAEO
                            RegionShowOverview(
                                ref region,
                                ref rAEO,
                                false);
                        }
                        //Иначе, если активна вкладка ORAEO
                        else if (RAEOSubpanel.tabGroup.selectedTab == RAEOSubpanel.oRAEOsTab.selfTabButton)
                        {
                            //Отображаем вкладку организаций RAEO
                            RegionShowORAEOs(
                                ref rAEO,
                                false);
                        }
                    }
                }

                //Удаляем с сущности RAEO самозапрос
                refreshRAEOObjectPanelSelfRequestPool.Value.Del(selfRequestEntity);
            }
        }

        readonly EcsFilterInject<Inc<CBuildingDisplayedSummaryPanel, SRObjectRefreshUI>> buildingRefreshUISelfRequestFilter = default;
        readonly EcsFilterInject<Inc<CTaskForceDisplayedSummaryPanel, SRObjectRefreshUI>> taskForceRefreshUISelfRequestFilter = default;
        readonly EcsFilterInject<Inc<CTFTemplateDisplayedSummaryPanel, SRObjectRefreshUI>> tFTemplateRefreshUISelfRequestFilter = default;
        readonly EcsPoolInject<SRObjectRefreshUI> objectRefreshUISelfRequestPool = default;
        void GameRefreshUISelfRequest()
        {
            //Обновляем интерфейс сооружений
            GameRefreshUIBuiliding();

            //Обновляем интерфейс оперативных групп
            GameRefreshUITaskForce();

            //Обновляем интерфейсы шаблонов оперативных групп
            GameRefreshUITFTemplate();
        }

        void GameRefreshUIBuiliding()
        {
            //Для каждого сооружения, имеющей отображаемую панель и самозапрос обновления интерфейса
            foreach (int buildingEntity in buildingRefreshUISelfRequestFilter.Value)
            {
                //Берём компонент отображаемой панели и самозапрос
                ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel = ref buildingDisplayedSummaryPanelPool.Value.Get(buildingEntity);
                ref SRObjectRefreshUI selfRequestComp = ref objectRefreshUISelfRequestPool.Value.Get(buildingEntity);

                //Если запрашивается обновление интерфейса
                if (selfRequestComp.requestType == RefreshUIType.Refresh)
                {
                    //Берём сооружение
                    ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

                    //Если сооружение имеет отображаемую обзорную панель
                    if (buildingDisplayedSummaryPanel.buildingSummaryPanel != null)
                    {
                        //Обновляем её
                        buildingDisplayedSummaryPanel.buildingSummaryPanel.parentBuildingCategoryPanel.parentBuildingsTab.RefreshBuildingSummaryPanel(
                            ref building, buildingDisplayedSummaryPanel.buildingSummaryPanel);
                    }
                }
                //Иначе, если запрашивается удаление интерфейса
                else if (selfRequestComp.requestType == RefreshUIType.Delete)
                {
                    //Если сооружение имеет отображаемую обзорную панель
                    if (buildingDisplayedSummaryPanel.buildingSummaryPanel != null)
                    {
                        //Удаляем её
                        buildingDisplayedSummaryPanel.buildingSummaryPanel.parentBuildingCategoryPanel.parentBuildingsTab.CacheBuildingSummaryPanel(
                            ref buildingDisplayedSummaryPanel);
                    }

                    //Удаляем компонент отображаемой обзорной панели
                    buildingDisplayedSummaryPanelPool.Value.Del(buildingEntity);
                }

                //Удаляем самозапро обновления интерфейса
                objectRefreshUISelfRequestPool.Value.Del(buildingEntity);
            }
        }

        void GameRefreshUITaskForce()
        {
            //Для каждой оперативной группы, имеющей отображаемую панель и самозапрос обновления интерфейса
            foreach (int taskForceEntity in taskForceRefreshUISelfRequestFilter.Value)
            {
                //Берём компонент отображаемой панели и самозапрос
                ref CTaskForceDisplayedSummaryPanel tFDisplayedSummaryPanel = ref tFDisplayedSummaryPanelPool.Value.Get(taskForceEntity);
                ref SRObjectRefreshUI selfRequestComp = ref objectRefreshUISelfRequestPool.Value.Get(taskForceEntity);

                //Если запрашивается обновление интерфейса
                if (selfRequestComp.requestType == RefreshUIType.Refresh)
                {
                    //Берём группу
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Если группа имеет отображаемую обзорную панель
                    if (tFDisplayedSummaryPanel.taskForceSummaryPanel != null)
                    {
                        //Обновляем её
                        tFDisplayedSummaryPanel.taskForceSummaryPanel.parentFleetSummaryPanel.parentFleetsTab.RefreshTaskForceSummaryPanel(
                            ref taskForce, tFDisplayedSummaryPanel.taskForceSummaryPanel);
                    }
                }
                //Иначе, если запрашивается удаление интерфейса
                else if(selfRequestComp.requestType == RefreshUIType.Delete)
                {
                    //Если группа имеет отображаемую обзорную панель
                    if (tFDisplayedSummaryPanel.taskForceSummaryPanel != null)
                    {
                        //Удаляем её
                        tFDisplayedSummaryPanel.taskForceSummaryPanel.parentFleetSummaryPanel.parentFleetsTab.CacheTaskForceSummaryPanel(
                            ref tFDisplayedSummaryPanel);
                    }

                    //Удаляем компонент отображаемой обзорной панели
                    tFDisplayedSummaryPanelPool.Value.Del(taskForceEntity);
                }

                //Удаляем самозапрос обновления интерфейса
                objectRefreshUISelfRequestPool.Value.Del(taskForceEntity);
            }
        }

        void GameRefreshUITFTemplate()
        {
            //Для каждого шаблона оперативной группы, имеющего отображаемую панель и самозапрос обновления интерфейса
            foreach (int templateEntity in tFTemplateRefreshUISelfRequestFilter.Value)
            {
                //Берём компонент отображаемой панели и самозапрос
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);
                ref SRObjectRefreshUI selfRequestComp = ref objectRefreshUISelfRequestPool.Value.Get(templateEntity);

                //Если запрашивается удаление интерфейса
                if (selfRequestComp.requestType == RefreshUIType.Delete)
                {
                    //Если панель вкладки флотов не пуста
                    if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel != null)
                    {
                        //Кэшируем её
                        eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab
                            .CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                    }

                    //Если панель вкладки шаблонов не пуста
                    if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel != null)
                    {
                        //Кэшируем её
                        eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab.listSubtab
                            .CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                    }

                    //Удаляем компонент отображаемой обзорной панели
                    tFTemplateDisplayedSummaryPanelPool.Value.Del(templateEntity);
                }

                //Удаляем самозапрос обновления интерфейса
                objectRefreshUISelfRequestPool.Value.Del(templateEntity);
            }
        }

        #region GameObject
        readonly EcsFilterInject<Inc<RGameObjectPanelAction>> gameObjectPanelActionRequestFilter = default;
        readonly EcsPoolInject<RGameObjectPanelAction> gameObjectPanelActionRequestPool = default;
        void GameObjectActionRequest()
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Для каждого запроса действия панели объекта
            foreach (int requestEntity in gameObjectPanelActionRequestFilter.Value)
            {
                //Берём запрос
                ref RGameObjectPanelAction requestComp = ref gameObjectPanelActionRequestPool.Value.Get(requestEntity);

                //Отображаем панель объекта
                GameShowObjectPanel();

                //Если запрашивается отображение подпанели менеджера флотов
                if (requestComp.requestType == ObjectPanelActionRequestType.FleetManager)
                {
                    //Отображаем подпанель менеджера флотов
                    FleetManagerShow(ref requestComp);
                }
                //Иначе, если запрашивается отображение подпанели организации
                else if (requestComp.requestType == ObjectPanelActionRequestType.Organization)
                {
                    //Отображаем подпанель организации
                    OrganizationShow(ref requestComp);
                }
                //Иначе, если запрашивается отображение подпанели региона
                else if (requestComp.requestType == ObjectPanelActionRequestType.Region)
                {
                    //Отображаем подпанель региона
                    RegionShow(ref requestComp);
                }
                //Иначе, если запрашивается отображение подпанели ORAEO
                else if (requestComp.requestType == ObjectPanelActionRequestType.ORAEO)
                {
                    //Отображаем подпанель ORAEO
                    ORAEOShow(ref requestComp);
                }
                //Иначе, если запрашивается отображение подпанели сооружения
                else if (requestComp.requestType == ObjectPanelActionRequestType.Building)
                {
                    //Отображаем панель сооружения
                    BuildingShow(ref requestComp);
                }

                //Иначе, если активна подпанель менеджера флотов
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                {
                    //Берём подпанель менеджера флотов
                    UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

                    //Если запрашивается отображение вкладки флотов
                    if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerFleets)
                    {
                        //Берём организацию
                        requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //Отображаем вкладку флотов
                        FleetManagerShowFleets(
                            ref organization,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается отображение вкладки шаблонов оперативных групп
                    else if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerTaskForceTemplates)
                    {
                        //Берём организацию
                        requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //Отображаем вкладку шаблонов групп
                        FleetManagerShowTFTemplates(
                            ref organization,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается закрытие панели объекта
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //Скрываем подпанель менеджера флотов
                        FleetManagerHide();

                        //Скрываем подпанель объекта
                        GameHideObjectSubpanel();

                        //Скрываем панель объекта
                        GameHideObjectPanel();
                    }

                    //Иначе, если активна вкладка флотов
                    else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.fleetsTab.selfTabButton)
                    {
                        //Берём вкладку флотов 
                        UIFleetsTab fleetsTab = fleetManagerSubpanel.fleetsTab;


                    }
                    //Иначе, если активна вкладка шаблонов групп
                    else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.taskForceTemplatesTab.selfTabButton)
                    {
                        //Берём вкладку шаблонов групп
                        UITFTemplatesTab taskForceTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

                        //Если запрашивается отображение подвкладки списка шаблонов групп
                        if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesList)
                        {
                            //Берём организацию
                            requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                            //Отображаем подвкладку списка шаблонов групп
                            FleetManagerTFTemplatesShowList(
                                ref organization,
                                requestComp.isRefresh);
                        }
                        //Иначе, если запрашивается отображение подвкладки дизайнера шаблонов групп
                        else if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner)
                        {
                            //Берём организацию
                            requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                            //Отображаем подвкладку дизайнера шаблонов групп
                            FleetManagerTFTemplatesShowDesigner(
                                ref organization,
                                requestComp.isRefresh);
                        }

                        //Иначе, если активна подвкладка списка шаблонов групп
                        else if (taskForceTemplatesTab.listSubtab.isActiveAndEnabled == true)
                        {

                        }
                        //Иначе, если активна подвкладка дизайнера шаблонов групп
                        else if (taskForceTemplatesTab.designerSubtab.isActiveAndEnabled == true)
                        {

                        }
                    }
                }
                //Иначе, если активна подпанель организации
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
                {
                    //Если запрашивается отображение обзорной вкладки
                    if (requestComp.requestType == ObjectPanelActionRequestType.OrganizationOverview)
                    {
                        //Берём организацию
                        requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //Отображаем обзорную вкладку организации

                    }
                    //Иначе, если запрашивается закрытие панели объекта
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //Скрываем подпанель организации
                        OrganizationHide();

                        //Скрываем подпанель объекта
                        GameHideObjectSubpanel();

                        //Скрываем панель объекта
                        GameHideObjectPanel();
                    }
                }
                //Иначе, если активна подпанель региона
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                {
                    //Если запрашиваетсяся отображение обзорной вкладки 
                    if (requestComp.requestType == ObjectPanelActionRequestType.RegionOverview)
                    {
                        //Берём регион и RAEO
                        requestComp.objectPE.Unpack(world.Value, out int regionEntity);
                        ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                        ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

                        //Отображаем обзорную вкладку региона
                        RegionShowOverview(
                            ref region,
                            ref rAEO,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается отображение вкладки организаций
                    else if (requestComp.requestType == ObjectPanelActionRequestType.RegionOrganizations)
                    {
                        //Берём RAEO
                        requestComp.objectPE.Unpack(world.Value, out int regionEntity);
                        ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

                        //Отображаем вкладку организаций RAEO
                        RegionShowORAEOs(
                            ref rAEO,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается закрытие панели объекта
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //Скрываем подпанель региона
                        RegionHide();

                        //Скрываем подпанель объекта
                        GameHideObjectSubpanel();

                        //Скрываем панель объекта
                        GameHideObjectPanel();
                    }
                }
                //Иначе, если активна подпанель ORAEO
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
                {
                    //Если запрашивается отображение обзорной вкладки
                    if (requestComp.requestType == ObjectPanelActionRequestType.ORAEOOverview)
                    {
                        //Берём ExORAEO и EcORAEO
                        requestComp.objectPE.Unpack(world.Value, out int oRAEOEntity);
                        ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                        ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                        //Отображаем обзорную вкладку ORAEO
                        ORAEOShowOverview(
                            ref exORAEO,
                            ref ecORAEO,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается отображение вкладки сооружений
                    else if (requestComp.requestType == ObjectPanelActionRequestType.ORAEOBuildings)
                    {
                        //Берём EcORAEO
                        requestComp.objectPE.Unpack(world.Value, out int oRAEOEntity);
                        ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                        //Отображаем вкладку сооружений ORAEO
                        ORAEOShowBuildings(
                            ref ecORAEO,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается закрытие панели объекта
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //Скрываем подпанель ORAEO
                        ORAEOHide();

                        //Скрываем подпанель объекта
                        GameHideObjectSubpanel();

                        //Скрываем панель объекта
                        GameHideObjectPanel();
                    }
                }
                //Иначе, если активна подпанель сооружения
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Building)
                {
                    //Если запрашивается отображение обзорной вкладки
                    if (requestComp.requestType == ObjectPanelActionRequestType.BuildingOverview)
                    {
                        //Берём сооружение
                        requestComp.objectPE.Unpack(world.Value, out int buildingEntity);
                        ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

                        //Отображаем обзорную вкладку сооружения
                        BuildingShowOverview(
                            ref building,
                            requestComp.isRefresh);
                    }
                    //Иначе, если запрашивается закрытие панели объекта
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //Скрываем подпанель сооружения
                        BuildingHide();

                        //Скрываем подпанель объекта
                        GameHideObjectSubpanel();

                        //Скрываем панель объекта
                        GameHideObjectPanel();
                    }
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        void GameShowObjectPanel()
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Если какая-либо главная панель активна
            if (gameWindow.activeMainPanelType != MainPanelType.None)
            {
                //Скрываем её
                gameWindow.activeMainPanel.gameObject.SetActive(false);
            }

            //Делаем панель объекта активной
            gameWindow.objectPanel.gameObject.SetActive(true);

            //Указываем её как активную главную панель
            gameWindow.activeMainPanelType = MainPanelType.Object;
            gameWindow.activeMainPanel = gameWindow.objectPanel.gameObject;
        }

        void GameHideObjectPanel()
        {
            //Берём окно игры
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //Скрываем панель объекта
            gameWindow.objectPanel.gameObject.SetActive(false);

            //Указываем, что нет активной главной панели
            gameWindow.activeMainPanelType = MainPanelType.None;
            gameWindow.activeMainPanel = null;
        }

        void GameHideObjectSubpanel()
        {
            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Скрываем активную подпанель
            objectPanel.activeObjectSubpanel.gameObject.SetActive(false);

            //Указываем, что нет активной подпанели
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.None;
            objectPanel.activeObjectSubpanel = null;
            //Очищаем PE активного объекта
            objectPanel.activeObjectPE = new();
        }

        #region GameObjectFleetManager
        readonly EcsFilterInject<Inc<RGameFleetManagerSubpanelAction>> gameFleetManagerSubpanelActionRequestFilter = default;
        readonly EcsPoolInject<RGameFleetManagerSubpanelAction> gameFleetManagerSubpanelActionRequestPool = default;
        void FleetManagerActionRequest()
        {
            //Для каждого запроса действия менеджера флотов
            foreach (int requestEntity in gameFleetManagerSubpanelActionRequestFilter.Value)
            {
                //Берём подпанель менеджера флотов
                UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

                //Берём запрос
                ref RGameFleetManagerSubpanelAction requestComp = ref gameFleetManagerSubpanelActionRequestPool.Value.Get(requestEntity);

                //Берём организацию
                requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                //Если запрашивается заполнение списка шаблонов для смена шаблона группы
                if (requestComp.requestType == FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesChangeList)
                {
                    //Берём группу
                    requestComp.taskForcePE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Заполняем список шаблонов для смены шаблона группы
                    FleetManagerFleetsFillTFTemplatesChangingList(
                        ref organization,
                        ref taskForce);
                }
                //Иначе, если запрашивается заполнение списка шаблонов для создания новой группы
                else if(requestComp.requestType == FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesNewList)
                {
                    //Заполняем список шаблонов для создания новой группы
                    FleetManagerFleetsFillTFTemplatesCreatingList(ref organization);
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        void FleetManagerShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //Берём компонент организации
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Если какая-либо подпанель активна, скрываем её
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //Делаем подпанель менеджера флотов активной
            objectPanel.fleetManagerSubpanel.gameObject.SetActive(true);

            //Указываем её как активную подпанель
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.FleetManager;
            objectPanel.activeObjectSubpanel = objectPanel.fleetManagerSubpanel;
            //Указываем, менеджер флотов какой организации отображает подпанель
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;


            //Отображаем, что это менеджер флотов
            objectPanel.objectName.text = "Fleet Manager";

            //Отображаем вкладку флотов
            FleetManagerShowFleets(
                ref organization,
                false);
        }

        void FleetManagerHide()
        {
            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

            //Очищаем PE активного флота
            InputData.activeFleetPE = new();

            //Очищаем PE активных оперативных групп
            InputData.activeTaskForcePEs.Clear();

            //Скрываем список шаблонов групп во вкладке флотов
            fleetManagerSubpanel.fleetsTab.HideTFTemplatesChangingList();
        }

        void FleetManagerShowFleets(
            ref COrganization organization,
            bool isRefresh)
        {
            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

            //Берём вкладку флотов
            UIFleetsTab fleetsTab = fleetManagerSubpanel.fleetsTab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем вкладку флотов
                fleetManagerSubpanel.tabGroup.OnTabSelected(fleetsTab.selfTabButton);

                //Для каждой оперативной группы, имеющей обзорную панель
                foreach (int taskForceEntity in tFDisplayedSummaryPanelFilter.Value)
                {
                    //Берём компонент отображаемой обзорной панели
                    ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel = ref tFDisplayedSummaryPanelPool.Value.Get(taskForceEntity);

                    //Кэшируем обзорную панель
                    fleetsTab.CacheTaskForceSummaryPanel(ref taskForceDisplayedSummaryPanel);

                    //Удаляем компонент с сущности
                    tFDisplayedSummaryPanelPool.Value.Del(taskForceEntity);
                }

                //Для каждого флота, имеющего обзорную панель
                foreach (int fleetEntity in fleetDisplayedSummaryPanelFilter.Value)
                {
                    //Берём компонент отображаемой обзорной панели
                    ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Get(fleetEntity);

                    //Кэшируем обзорную панель
                    fleetsTab.CacheFleetSummaryPanel(ref fleetDisplayedSummaryPanel);

                    //Удаляем компонент с сущности
                    fleetDisplayedSummaryPanelPool.Value.Del(fleetEntity);
                }

                //Для каждого флота организации
                for (int a = 0; a < organization.ownedFleets.Count; a++)
                {
                    //Берём флот
                    organization.ownedFleets[a].Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //Создаём обзорную панель флота
                    FleetManagerFleetsCreateFleetSummaryPanel(ref fleet);
                }

                //Скрываем список для создания группы
                fleetsTab.HideTFTemplatesCreatingList();

                //Скрываем список для смены шаблона
                fleetsTab.HideTFTemplatesChangingList();
            }
        }

        void FleetManagerFleetsCreateFleetSummaryPanel(
            ref CFleet fleet)
        {
            //Берём сущность флота
            fleet.selfPE.Unpack(world.Value, out int fleetEntity);

            //Назначаем флоту компонент отображаемой обзорной панели
            ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Add(fleetEntity);

            //Берём вкладку флотов
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //Создаём панель
            fleetsTab.InstantiateFleetSummaryPanel(ref fleet, ref fleetDisplayedSummaryPanel);

            //Для каждой оперативной группы флота
            for (int a = 0; a < fleet.ownedTaskForcePEs.Count; a++)
            {
                //Берём группу
                fleet.ownedTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                //Создаём обзорную панель группы
                FleetManagerFleetsCreateTaskForceSummaryPanel(ref taskForce);
            }
        }

        void FleetManagerFleetsCreateTaskForceSummaryPanel(
            ref CTaskForce taskForce)
        {
            //Берём компонент отображаемой обзорной панели флота-владельца
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Get(fleetEntity);

            //Берём сущность оперативной группы
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);

            //Назначаем группе компонент отображаемой обзорной панели
            ref CTaskForceDisplayedSummaryPanel taskForceDisplayedOverviewPanel = ref tFDisplayedSummaryPanelPool.Value.Add(taskForceEntity);

            //Берём вкладку флотов
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //Создаём панель
            fleetsTab.InstantiateTaskForceSummaryPanel(
                fleetDisplayedSummaryPanel.fleetSummaryPanel,
                ref taskForce, ref taskForceDisplayedOverviewPanel);
        }

        void FleetManagerFleetsFillTFTemplatesChangingList(
            ref COrganization organization,
            ref CTaskForce taskForce)
        {
            //Берём вкладку флотов менеджера флотов
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //Берём список для смены шаблона
            UITFTemplateSummaryPanelsList templatesList = fleetsTab.templatesChangingList;

            //Очищаем список панелей
            templatesList.templatePanels.Clear();

            //Для каждого шаблона группы, имеющего обзорную панель
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //Берём компонент отображаемой панели
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //Если панель текущей вкладки не пуста
                if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel != null)
                {
                    //Кэшируем обзорную панель текущей вкладки
                    fleetsTab.CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                }

                //Если панель другой вкладки пуста
                if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel == null)
                {
                    //Удаляем компонент
                    world.Value.DelEntity(templateEntity);
                }
            }

            //Для каждого шаблона группы организации
            for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
            {
                //Берём шаблон
                DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                //Если это не тот шаблон, что группа уже имеет
                if(taskForce.template != template)
                {
                    //Создаём обзорную панель шаблона группы
                    FleetManagerFleetsCreateTFTemplateSummaryPanel(
                        template,
                        templatesList);
                }
            }

            //Если количество обзорных панелей шаблонов в списке шаблонов меньше пяти
            if (templatesList.templatePanels.Count < 5)
            {
                //Устанавливаем размер списка соответственно количеству панелей
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    templatesList.templatePanels.Count * 40);
            }
            //Иначе, если количество больше или равно пяти
            else if (templatesList.templatePanels.Count >= 5)
            {
                //Устанавливаем высоту списка как при пяти элементах
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    5 * 40);
            }
            //Иначе
            else
            {
                //Устанавливаем высоту списка как при одном элементе
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    1 * 40);
            }
        }

        void FleetManagerFleetsFillTFTemplatesCreatingList(
            ref COrganization organization)
        {
            //Берём вкладку флотов менеджера флотов
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //Берём список для создания группы
            UITFTemplateSummaryPanelsList templatesList = fleetsTab.templatesCreatingList;

            //Очищаем список панелей
            templatesList.templatePanels.Clear();

            //Для каждого шаблона группы, имеющего обзорную панель
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //Берём компонент отображаемой панели
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //Если панель текущей вкладки не пуста
                if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel != null)
                {
                    //Кэшируем обзорную панель текущей вкладки
                    fleetsTab.CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                }

                //Если панель другой вкладки пуста
                if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel == null)
                {
                    //Удаляем компонент
                    world.Value.DelEntity(templateEntity);
                }
            }

            //Для каждого шаблона группы организации
            for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
            {
                //Берём шаблон
                DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                //Создаём обзорную панель шаблона группы
                FleetManagerFleetsCreateTFTemplateSummaryPanel(
                    template,
                    templatesList);
            }

            //Если количество обзорных панелей шаблонов в списке шаблонов меньше пяти
            if (templatesList.templatePanels.Count < 5)
            {
                //Устанавливаем размер списка соответственно количеству панелей
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    templatesList.templatePanels.Count * 40);
            }
            //Иначе, если количество больше или равно пяти
            else if (templatesList.templatePanels.Count >= 5)
            {
                //Устанавливаем высоту списка как при пяти элементах
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    5 * 40);
            }
            //Иначе
            else
            {
                //Устанавливаем высоту списка как при одном элементе
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    1 * 40);
            }
        }

        void FleetManagerFleetsCreateTFTemplateSummaryPanel(
            DTFTemplate template,
            UITFTemplateSummaryPanelsList templatesList)
        {
            //Определяем, не существует ли уже панели для данного шаблона
            bool isPanelExist = false;
            int existTemplateEntity = -1;

            //Для каждой отображаемой обзорной панели шаблона
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //Берём компонент
                ref CTFTemplateDisplayedSummaryPanel existTemplateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //Если панель компонент отображает запрошенный шаблон
                if (existTemplateDisplayedSummaryPanel.CheckTemplate(template))
                {
                    //Отмечаем, что панель существует
                    isPanelExist = true;

                    //Сохраняем сущность 
                    existTemplateEntity = templateEntity;

                    break;
                }
            }

            //Берём вкладку флотов
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //Если панель существует
            if (isPanelExist == true)
            {
                //Берём компонент отображаемой панели
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(existTemplateEntity);

                //Создаём панель
                fleetsTab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel,
                    templatesList);
            }
            //Иначе
            else
            {
                //Создаём новую сущность и назначаем ей компонент отображаемой обзорной панели шаблона оперативной группы во вкладке флотов
                int templateEntity = world.Value.NewEntity();
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Add(templateEntity);

                //Заполняем данные компонента
                templateDisplayedSummaryPanel = new(world.Value.PackEntity(templateEntity));

                //Создаём панель
                fleetsTab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel,
                    templatesList);
            }
        }

        void FleetManagerShowTFTemplates(
            ref COrganization organization,
            bool isRefresh)
        {
            //Берём подпанель менеджера флотов
            UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

            //Берём вкладку шаблонов оперативных групп
            UITFTemplatesTab taskForceTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем вкладку шаблонов групп
                fleetManagerSubpanel.tabGroup.OnTabSelected(taskForceTemplatesTab.selfTabButton);

                //Отображаем список групп
                FleetManagerTFTemplatesShowList(
                    ref organization,
                    isRefresh);
            }
        }

        void FleetManagerTFTemplatesShowList(
            ref COrganization organization,
            bool isRefresh)
        {
            //Берём вкладку шаблонов оперативных групп
            UITFTemplatesTab templatesTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab;

            //Берём подвкладку списка шаблонов групп
            UIListSubtab listSubtab = templatesTab.listSubtab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Если дизайнер шаблонов групп отображается, скрываем его
                if (templatesTab.designerSubtab.isActiveAndEnabled == true)
                {
                    templatesTab.designerSubtab.gameObject.SetActive(false);
                }

                //Отображаем подвкладку списка шаблонов групп
                listSubtab.gameObject.SetActive(true);

                //Для каждого шаблона группы, имеющего обзорную панель
                foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
                {
                    //Берём компонент отображаемой обзорной панели
                    ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                    //Если панель текущей вкладки не пуста
                    if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel != null)
                    {
                        //Кэшируем обзорную текущей вкладки
                        listSubtab.CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                    }

                    //Если панель другой вкладки пуста
                    if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel == null)
                    {
                        //Удаляем компонент
                        world.Value.DelEntity(templateEntity);
                    }
                }

                //Для каждого шаблона группы организации
                for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
                {
                    //Берём шаблон
                    DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                    //Создаём обзорную панель шаблона группы
                    FleetManagerTFTemplatesListCreateTFTemplateSummaryPanel(
                        template);
                }
            }
        }

        void FleetManagerTFTemplatesListCreateTFTemplateSummaryPanel(
            DTFTemplate template)
        {
            //Определяем, не существует ли уже панели для данного шаблона
            bool isPanelExist = false;
            int existTemplateEntity = -1;

            //Для каждой отображаемой обзорной панели шаблона
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //Берём компонент
                ref CTFTemplateDisplayedSummaryPanel existTemplateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //Если панель компонент отображает запрошенный шаблон
                if (existTemplateDisplayedSummaryPanel.CheckTemplate(template))
                {
                    //Отмечаем, что панель существует
                    isPanelExist = true;

                    //Сохраняем сущность 
                    existTemplateEntity = templateEntity;

                    break;
                }
            }

            //Берём подвкладку списка шаблонов групп
            UIListSubtab listSubtab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab.listSubtab;

            //Если панель существует
            if (isPanelExist == true)
            {
                //Берём компонент отображаемой панели
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(existTemplateEntity);

                //Создаём панель
                listSubtab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel);
            }
            //Иначе
            else
            {
                //Создаём новую сущность и назначаем ей компонент отображаемой обзорной панели шаблона оперативной группы во вкладке флотов
                int templateEntity = world.Value.NewEntity();
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Add(templateEntity);


                //Заполняем данные компонента
                templateDisplayedSummaryPanel = new(world.Value.PackEntity(templateEntity));

                //Создаём панель
                listSubtab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel);
            }
        }

        void FleetManagerTFTemplatesShowDesigner(
            ref COrganization organization,
            bool isRefresh)
        {
            //Берём вкладку шаблонов оперативных групп
            UITFTemplatesTab taskForceTemplatesTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab;

            //Берём подвкладку дизайнера шаблонов групп
            UIDesignerSubtab designerSubtab = taskForceTemplatesTab.designerSubtab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Если список шаблонов групп отображается, скрываем его
                if (taskForceTemplatesTab.listSubtab.isActiveAndEnabled == true)
                {
                    taskForceTemplatesTab.listSubtab.gameObject.SetActive(false);
                }

                //ТЕСТ
                //Если боевые группы не отображены
                if (designerSubtab.isBattleGroupsDisplayed == false)
                {
                    eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab.designerSubtab.ClearBattleGroups();

                    for (int a = 0; a < contentData.Value.contentSets[0].shipTypes.Length; a++)
                    {
                        if (contentData.Value.contentSets[0].shipTypes[a].BattleGroup == TaskForceBattleGroup.ShortRange)
                        {
                            eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab.designerSubtab.InstantiateShipTypePanels(
                                contentData.Value.contentSets[0].shipTypes[a]);
                        }
                    }

                    designerSubtab.isBattleGroupsDisplayed = true;
                }
                //ТЕСТ

                //Отображаем подвкладку дизайнера шаблонов групп
                designerSubtab.gameObject.SetActive(true);

                //Очищаем подвкладку, отображая пустой шаблон
                designerSubtab.ClearTaskForceTemplate();

                //Если запрашивается отображение пустого шаблона
                if (designerSubtab.template == null)
                {
                    //Отображаем название шаблона группы по умолчанию - "Task Force Template + organization.taskForceCount"
                    designerSubtab.tFTemplateName.text = "Task Force Template";
                }
                //Иначе
                else
                {
                    //Берём запрошенный шаблон из данных организации
                    DTFTemplate template = designerSubtab.template;

                    //Отображаем название шаблона группы
                    designerSubtab.tFTemplateName.text = template.selfName;

                    //Для каждого типа корабля в шаблоне
                    for (int a = 0; a < template.shipTypes.Length; a++)
                    {
                        //Берём тип корабля
                        ref DShipType shipType = ref template.shipTypes[a].shipType;

                        //Если тип корабля относится к группе малой дальности
                        if (shipType.BattleGroup == TaskForceBattleGroup.ShortRange)
                        {
                            //Добавляем тип корабля 
                            designerSubtab.AddAvailableShipType(
                                template.shipTypes[a].shipType,
                                designerSubtab.shortRangeGroup, template.shipTypes[a].shipCount);
                        }
                    }
                }

            }
        }
        #endregion

        #region GameObjectOrganization
        void OrganizationShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {

        }

        void OrganizationHide()
        {

        }
        #endregion

        #region GameObjectRegion
        void RegionShow(
        ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //Берём компонент региона и RAEO
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
            ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Если какая-либо подпанель активна, скрываем её
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //Делаем подпанель региона активной 
            objectPanel.regionSubpanel.gameObject.SetActive(true);

            //Указываем её как активную подпанель
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.Region;
            objectPanel.activeObjectSubpanel = objectPanel.regionSubpanel;
            //Указываем, какой регион отображает подпанель
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //Берём подпанель региона
            UIRegionSubpanel regionSubpanel = objectPanel.regionSubpanel;


            //Отображаем название региона
            objectPanel.objectName.text = region.centerPoint.ToString();

            //Отображаем обзорную вкладку региона
            RegionShowOverview(
                ref region,
                ref rAEO,
                false);
        }

        void RegionHide()
        {

        }

        void RegionShowOverview(
            ref CHexRegion region,
            ref CRegionAEO rAEO,
            bool isRefresh)
        {
            //Берём подпанель региона
            UIRegionSubpanel regionSubpanel = eUI.Value.gameWindow.objectPanel.regionSubpanel;

            //Берём обзорную вкладку
            GameWindow.Object.Region.UIOverviewTab overviewTab = regionSubpanel.overviewTab;

            //Берём компонент организации игрока
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //Берём компонент ORAEO организации-владельца
            rAEO.organizationRAEOs[playerOrganization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int ownerORAEOEntity);
            ref CExplorationORAEO ownerExORAEO = ref explorationORAEOPool.Value.Get(ownerORAEOEntity);

            //Отображаем уровень исследования региона организацией-владельцем
            overviewTab.explorationLevel.text = region.Index.ToString();//= ownerExORAEO.explorationLevel.ToString();

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем обзорную вкладку
                regionSubpanel.tabGroup.OnTabSelected(overviewTab.selfTabButton);

                //Если организация игрока имеет EcORAEO
                if (rAEO.organizationRAEOs[playerOrganization.selfIndex].organizationRAEOType == ORAEOType.Economic)
                {
                    //Скрываем кнопку колонизации
                    overviewTab.colonizeButtonTest.gameObject.SetActive(false);
                }
                //Иначе
                else
                {
                    //Отображаем кнопку колонизации
                    overviewTab.colonizeButtonTest.gameObject.SetActive(true);
                }

            }
        }

        void RegionShowORAEOs(
            ref CRegionAEO regionRAEO,
            bool isRefresh)
        {
            //Берём подпанель региона
            UIRegionSubpanel regionSubpanel = eUI.Value.gameWindow.objectPanel.regionSubpanel;

            //Берём вкладку ORAEO
            UIORAEOsTab oRAEOsTab = regionSubpanel.oRAEOsTab;

            //Для каждого ORAEO в RAEO
            for (int a = 0; a < regionRAEO.organizationRAEOs.Length; a++)
            {
                //Если производится не обновление
                if (isRefresh == false)
                {
                    //Берём панель ORAEO 
                    UIORAEOSummaryPanel briefInfoPanel = oRAEOsTab.panelsList[a];

                    //Если у организации есть EcORAEO
                    if (regionRAEO.organizationRAEOs[a].organizationRAEOType == ORAEOType.Economic)
                    {
                        //Отображаем панель
                        briefInfoPanel.gameObject.SetActive(true);

                        //Берём компонент ExORAEO
                        regionRAEO.organizationRAEOs[a].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                        ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                        ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                        //Берём компонент организации
                        exORAEO.organizationPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //Указываем PE ORAEO
                        briefInfoPanel.selfPE = exORAEO.selfPE;
                    }
                    //Иначе
                    else
                    {
                        //Скрываем панель
                        briefInfoPanel.gameObject.SetActive(false);
                    }
                }
            }

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем вкладку организаций
                regionSubpanel.tabGroup.OnTabSelected(oRAEOsTab.selfTabButton);
            }
        }

        void RegionORAEOsCreateORAEOSummaryPanel(
            ref RGameCreatePanel requestComp)
        {
            //Берём организацию
            requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Берём вкладку ORAEO подпанели региона
            UIORAEOsTab organizationsTab = eUI.Value.gameWindow.objectPanel.regionSubpanel.oRAEOsTab;

            //Создаём обзорную панель
            UIORAEOSummaryPanel oRAEOSummaryPanel = organizationsTab.InstantiateORAEOSummaryPanel(ref organization);

            //Скрываем панель
            oRAEOSummaryPanel.gameObject.SetActive(false);
        }
        #endregion

        #region GameObjectORAEO
        void ORAEOShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //Берём ExORAEO и EcORAEO
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int oRAEOEntity);
            ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
            ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Если какая-либо подпанель активна, скрываем её
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //Делаем подпанель ORAEO активной 
            objectPanel.oRAEOSubpanel.gameObject.SetActive(true);

            //Указываем её как активную подпанель
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.ORAEO;
            objectPanel.activeObjectSubpanel = objectPanel.oRAEOSubpanel;
            //Указываем, какой ORAEO отображаем подпанель
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //Берём подпанель ORAEO
            UIORAEOSubpanel oRAEOSubpanel = objectPanel.oRAEOSubpanel;


            //Отображаем название ORAEO
            objectPanel.objectName.text = exORAEO.selfPE.ToString();

            //Отображаем обзорную вкладку ORAEO
            ORAEOShowOverview(
                ref exORAEO,
                ref ecORAEO,
                false);
        }

        void ORAEOHide()
        {

        }

        void ORAEOShowOverview(
            ref CExplorationORAEO exORAEO,
            ref CEconomicORAEO ecORAEO,
            bool isRefresh)
        {
            //Берём подпанель ORAEO
            UIORAEOSubpanel oRAEOSubpanel = eUI.Value.gameWindow.objectPanel.oRAEOSubpanel;

            //Берём обзорную вкладку
            GameWindow.Object.ORAEO.UIOverviewTab overviewTab = oRAEOSubpanel.overviewTab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем обзорную вкладку
                oRAEOSubpanel.tabGroup.OnTabSelected(overviewTab.selfTabButton);
            }

            overviewTab.testText.text = "! ! !";
        }

        void ORAEOShowBuildings(
            ref CEconomicORAEO ecORAEO,
            bool isRefresh)
        {
            //Берём подпанель ORAEO
            UIORAEOSubpanel oRAEOSubpanel = eUI.Value.gameWindow.objectPanel.oRAEOSubpanel;

            //Берём вкладку сооружений
            UIBuildingsTab buildingsTab = oRAEOSubpanel.buildingsTab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем вкладку сооружений
                oRAEOSubpanel.tabGroup.OnTabSelected(buildingsTab.selfTabButton);

                //Для каждого сооружения, имеющего обзорную панель
                foreach (int buildingEntity in buildingDisplayedSummaryPanelFilter.Value)
                {
                    //Берём компонент отображаемой обзорной панели
                    ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel = ref buildingDisplayedSummaryPanelPool.Value.Get(buildingEntity);

                    //Кэшируем обзорную панель
                    buildingsTab.CacheBuildingSummaryPanel(ref buildingDisplayedSummaryPanel);

                    //Удаляем компонент с сущности
                    buildingDisplayedSummaryPanelPool.Value.Del(buildingEntity);
                }

                //Для каждого сооружения EcORAEO
                for (int a = 0; a < ecORAEO.ownedBuildings.Count; a++)
                {
                    //Берём сооружение
                    ecORAEO.ownedBuildings[a].buildingPE.Unpack(world.Value, out int buildingEntity);
                    ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

                    //Создаём обзорную панель сооружения
                    ORAEOBuildingsCreateBuildingSummaryPanel(ref building);
                }
            }
        }

        void ORAEOBuildingsCreateBuildingSummaryPanel(
            ref CBuilding building)
        {
            //Берём сущность сооружения
            building.selfPE.Unpack(world.Value, out int buildingEntity);

            //Назначаем сооружению компонент отображаемой обзорной панели
            ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel = ref buildingDisplayedSummaryPanelPool.Value.Add(buildingEntity);

            //Берём вкладку сооружений
            UIBuildingsTab buildingsTab = eUI.Value.gameWindow.objectPanel.oRAEOSubpanel.buildingsTab;

            //Создаём панель
            buildingsTab.InstantiateBuildingSummaryPanel(ref building, ref buildingDisplayedSummaryPanel);
        }
        #endregion

        #region GameObjectBuilding
        void BuildingShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //Берём сооружение
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int buildingEntity);
            ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

            //Берём панель объекта
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //Если какая-либо подпанель активна, скрываем её
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //Делаем подпанель сооружения активной
            objectPanel.buildingSubpanel.gameObject.SetActive(true);

            //Указываем её как активную подпанель
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.Building;
            objectPanel.activeObjectSubpanel = objectPanel.buildingSubpanel;
            //Указываем, какое сооружение отображает панель
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //Берём подпанель сооружения
            UIBuildingSubpanel buildingSubpanel = objectPanel.buildingSubpanel;


            //Отображаем название сооружения
            objectPanel.objectName.text = building.buildingType.ObjectName;

            //Отображаем обзорную вкладку сооружения
            BuildingShowOverview(
                ref building,
                false);
        }

        void BuildingHide()
        {

        }

        void BuildingShowOverview(
            ref CBuilding building,
            bool isRefresh)
        {
            //Берём подпанель сооружения
            UIBuildingSubpanel buildingSubpanel = eUI.Value.gameWindow.objectPanel.buildingSubpanel;

            //Берём обзорную вкладку
            GameWindow.Object.Building.UIOverviewTab overviewTab = buildingSubpanel.overviewTab;

            //Если производится не обновление
            if (isRefresh == false)
            {
                //Отображаем обзорную вкладку
                buildingSubpanel.tabGroup.OnTabSelected(overviewTab.selfTabButton);
            }

            
        }
        #endregion
        #endregion

        void GameOpenWindow()
        {
            //Закрываем открытое главное окно
            CloseMainWindow();

            //Берём ссылку на окно игры
            UIGameWindow gameWindow
                = eUI.Value.gameWindow;

            //Делаем окно игры активным
            gameWindow.gameObject.SetActive(true);
            //И указываем его как активное окно в EUI
            eUI.Value.activeMainWindow
                = eUI.Value.gameWindow.gameObject;

            //Указываем, что активно окно игры
            eUI.Value.activeMainWindowType
                = MainWindowType.Game;
        }

        void GamePause(
            GameActionType pauseMode)
        {
            //Если требуется включить паузу
            if (pauseMode
                == GameActionType.PauseOn)
            {
                //Указываем, что игра неактивна
                runtimeData.Value.isGameActive
                    = false;
            }
            //Иначе
            else if (pauseMode
                == GameActionType.PauseOff)
            {
                //Указываем, что игра активна
                runtimeData.Value.isGameActive
                    = true;
            }
        }
        #endregion

        void SaveContentSetRequest(
            int contentSetIndex)
        {
            //Создаём новую сущность и назначаем ей компонент запроса сохранения набора контента
            int requestEntity = world.Value.NewEntity();
            ref RSaveContentSet saveContentSetRequest = ref saveContentSetRequestPool.Value.Add(requestEntity);

            //Указываем индекс набора контента
            saveContentSetRequest.contentSetIndex = contentSetIndex;
        }

        void EcsGroupSystemStateEvent(
            string systemGroupName,
            bool systemGroupState)
        {
            //Создаём новую сущность и назначаем ей компонент события смены состояния группы систем
            int eventEntity = world.Value.NewEntity();
            ref EcsGroupSystemState groupSystemStateEvent = ref ecsGroupSystemStatePool.Value.Add(eventEntity);

            //Указываем название группы систем
            groupSystemStateEvent.Name = systemGroupName;
            //Указываем состояние группы систем
            groupSystemStateEvent.State = systemGroupState;
        }
    }
}