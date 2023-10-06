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
        //����
        readonly EcsWorldInject world = default;


        //������

        //�����
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //�����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;
        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        readonly EcsPoolInject<CBuilding> buildingPool = default;
        readonly EcsFilterInject<Inc<CBuilding, CBuildingDisplayedSummaryPanel>> buildingDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CBuildingDisplayedSummaryPanel> buildingDisplayedSummaryPanelPool = default;

        //�����
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsFilterInject<Inc<CFleet, CFleetDisplayedSummaryPanel>> fleetDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CFleetDisplayedSummaryPanel> fleetDisplayedSummaryPanelPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceDisplayedSummaryPanel>> tFDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> tFDisplayedSummaryPanelPool = default;

        readonly EcsFilterInject<Inc<CTFTemplateDisplayedSummaryPanel>> tFTemplateDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CTFTemplateDisplayedSummaryPanel> tFTemplateDisplayedSummaryPanelPool = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<SRRefreshRAEOObjectPanel, CRegionAEO>> refreshRAEOObjectPanelSelfRequestFilter = default;
        readonly EcsPoolInject<SRRefreshRAEOObjectPanel> refreshRAEOObjectPanelSelfRequestPool = default;

        //����� �������
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;

        readonly EcsPoolInject<RTechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;

        readonly EcsPoolInject<RSaveContentSet> saveContentSetRequestPool = default;

        readonly EcsPoolInject<EcsGroupSystemState> ecsGroupSystemStatePool = default;

        readonly EcsFilterInject<Inc<RQuitGame>> quitGameRequestFilter = default;
        readonly EcsPoolInject<RQuitGame> quitGameRequestPool = default;


        //������
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
            //��������� ���� �������� ����
            MainMenuOpenWindow();

            //����
            //������� ������ ����� ����������� � ��������� ��������
            eUI.Value.designerWindow.shipClassDesigner.availableComponentTypeDropdown.ClearOptions();
            //��������� ������ ����� ����������� � ��������� ��������
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
            //����
        }

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ �� ����
            foreach (int quitGameRequestEntity in quitGameRequestFilter.Value)
            {
                //���� ������
                ref RQuitGame quitGameRequest = ref quitGameRequestPool.Value.Get(quitGameRequestEntity);

                Debug.LogError("Quit game!");

                world.Value.DelEntity(quitGameRequestEntity);

                //������� �� ����
                Application.Quit();
            }

            //E��� ������� ���� ����
            if (eUI.Value.activeMainWindowType == MainWindowType.Game)
            {
                //��������� ������� � ���� ����
                EventCheckGame();
            }
            //�����, ���� ������� ������� ����
            else if (eUI.Value.activeMainWindowType == MainWindowType.MainMenu)
            {
                //��������� ������� � ���� �������� ����
                EventCheckMainMenu();
            }
            //�����, ���� ������� ���� ����� ����
            else if (eUI.Value.activeMainWindowType == MainWindowType.NewGameMenu)
            {
                //��������� ������� � ���� ���� ����� ����
                EventCheckNewGameMenu();
            }
            //�����, ���� ������� ����������
            else if (eUI.Value.activeMainWindowType == MainWindowType.Workshop)
            {
                //��������� ������� � ���� ����������
                EventCheckWorkshop();
            }
            //�����, ���� ������� ��������
            else if (eUI.Value.activeMainWindowType == MainWindowType.Designer)
            {
                //��������� ������� � ���� ���������
                EventCheckDesigner();
            }
        }

        void CloseMainWindow()
        {
            //���� �����-�� ���� ���� ��������
            if (eUI.Value.activeMainWindow
                != null)
            {
                //������ ��� ����������
                eUI.Value.activeMainWindow.SetActive(
                    false);
            }

            //���������, ��� �� ������� �� ���� ������� ����
            eUI.Value.activeMainWindowType
                = MainWindowType.None;
        }

        #region MainMenu
        readonly EcsFilterInject<Inc<RMainMenuAction>> mainMenuActionRequestFilter = default;
        readonly EcsPoolInject<RMainMenuAction> mainMenuActionRequestPool = default;
        void EventCheckMainMenu()
        {
            //��� ������� ������� �������� � ������� ����
            foreach (int mainMenuActionRequestEntity in mainMenuActionRequestFilter.Value)
            {
                //���� ������
                ref RMainMenuAction mainMenuActionRequest = ref mainMenuActionRequestPool.Value.Get(mainMenuActionRequestEntity);

                //���� ������������� �������� ���� ����� ����
                if (mainMenuActionRequest.actionType == MainMenuActionType.OpenNewGameMenu)
                {
                    //��������� ���� ���� ����� ����
                    NewGameMenuOpenWindow();
                }
                //�����, ���� ������������� �������� ���� �������� ����
                else if (mainMenuActionRequest.actionType == MainMenuActionType.OpenLoadGameMenu)
                {
                    //��������� ���� ���� �������� ����

                }
                //�����, ���� ������������� �������� ���� ����������
                else if (mainMenuActionRequest.actionType == MainMenuActionType.OpenWorkshop)
                {
                    //��������� ���� ����������
                    WorkshopOpenWindow();
                }
                //�����, ���� ������������� �������� ���� ������� ��������
                else if (mainMenuActionRequest.actionType == MainMenuActionType.OpenMainSettings)
                {
                    //��������� ���� ������� ��������

                }

                world.Value.DelEntity(mainMenuActionRequestEntity);
            }
        }

        void MainMenuOpenWindow()
        {
            //��������� �������� ������� ����
            CloseMainWindow();

            //���� ������ �� ���� �������� ����
            UIMainMenuWindow mainMenuWindow
                = eUI.Value.mainMenuWindow;

            //������ ���� �������� ���� ��������
            mainMenuWindow.gameObject.SetActive(
                true);
            //� ��������� ��� ��� �������� ���� � EUI
            eUI.Value.activeMainWindow
                = eUI.Value.mainMenuWindow.gameObject;

            //���������, ��� ������� ���� �������� ����
            eUI.Value.activeMainWindowType
                = MainWindowType.MainMenu;


        }
        #endregion

        #region NewGameMenu
        readonly EcsFilterInject<Inc<RNewGameMenuAction>> newGameMenuActionRequestFilter = default;
        readonly EcsPoolInject<RNewGameMenuAction> newGameMenuActionRequestPool = default;
        void EventCheckNewGameMenu()
        {
            //��� ������� ������� �������� � ���� ����� ����
            foreach (int newGameMenuActionRequestEntity in newGameMenuActionRequestFilter.Value)
            {
                //���� ������
                ref RNewGameMenuAction newGameMenuActionRequest = ref newGameMenuActionRequestPool.Value.Get(newGameMenuActionRequestEntity);

                //���� ������������� �������� �������� ����
                if (newGameMenuActionRequest.actionType == NewGameMenuActionType.OpenMainMenu)
                {
                    //��������� ������� ����
                    MainMenuOpenWindow();
                }
                //�����, ���� ������������� ������ ����� ����
                else if (newGameMenuActionRequest.actionType == NewGameMenuActionType.StartNewGame)
                {
                    //������ ������ �������� ����� ����
                    NewGameMenuStartNewGameRequest();

                    //�������������� ��������� ����� ���� �� ���� ���� ����� ����
                    NewGameInitialization();

                    //��������� ���� ����
                    GameOpenWindow();
                }

                world.Value.DelEntity(newGameMenuActionRequestEntity);
            }
        }

        void NewGameMenuOpenWindow()
        {
            //��������� �������� ������� ����
            CloseMainWindow();

            //���� ������ �� ���� ���� ����� ����
            UINewGameMenuWindow newGameMenuWindow
                = eUI.Value.newGameMenuWindow;

            //������ ���� ���� ����� ���� ��������
            newGameMenuWindow.gameObject.SetActive(
                true);
            //� ��������� ��� ��� �������� ���� � EUI
            eUI.Value.activeMainWindow
                = eUI.Value.newGameMenuWindow.gameObject;

            //���������, ��� ������� ���� ���� ����� ����
            eUI.Value.activeMainWindowType
                = MainWindowType.NewGameMenu;
        }

        void NewGameMenuStartNewGameRequest()
        {
            //������ ����� �������� � ��������� �� ��������� ������� ������ ����� ����
            int requestEntity = world.Value.NewEntity();
            ref RStartNewGame startNewGameRequest = ref startNewGameEventPool.Value.Add(requestEntity);

            //����������� ��������� ������ ������ "NewGame"
            EcsGroupSystemStateEvent("NewGame", true);
        }

        void NewGameInitialization()
        {
            //���� ������ �� ���� ����� ����
            UINewGameMenuWindow newGameMenuWindow
                = eUI.Value.newGameMenuWindow;

            //���������� ����������� ������� ������� �� ���� ���� ����� ����
            mapGenerationData.Value.sectorSizeModifier
                = newGameMenuWindow.sectorSizeSlider.value;

        }

        void TechnologyCalculateFactionModifiersEvent(
            ref COrganization faction)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������������� ����������
            int eventEntity = world.Value.NewEntity();
            ref RTechnologyCalculateModifiers technologyCalculateModifiersEvent
                = ref technologyCalculateModifiersEventPool.Value.Add(eventEntity);

            //��������� ������ �� �������� �������, ��� ������� ��������� ����������� ������������
            technologyCalculateModifiersEvent.organizationPE
                = faction.selfPE;
        }
        #endregion

        #region Workshop
        readonly EcsFilterInject<Inc<RWorkshopAction>> workshopActionRequestFilter = default;
        readonly EcsPoolInject<RWorkshopAction> workshopActionRequestPool = default;
        void EventCheckWorkshop()
        {
            //��� ������� ������� �������� � ����������
            foreach (int workshopActionRequestEntity in workshopActionRequestFilter.Value)
            {
                //���� ������
                ref RWorkshopAction workshopActionRequest = ref workshopActionRequestPool.Value.Get(workshopActionRequestEntity);

                //���� ������������� �������� �������� ����
                if (workshopActionRequest.actionType == WorkshopActionType.OpenMainMenu)
                {
                    //��������� ������� ����
                    MainMenuOpenWindow();
                }
                //�����, ���� ������������� ����������� ������ ��������
                else if (workshopActionRequest.actionType == WorkshopActionType.DisplayContentSet)
                {
                    //���������� ����������� ����� ��������
                    WorkshopDisplayContentSet(workshopActionRequest.contentSetIndex);
                }
                //�����, ���� ������������� �������� ���� ���������
                else if (workshopActionRequest.actionType == WorkshopActionType.OpenDesigner)
                {
                    //���������� ����������� ���� ���������
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
            //��������� �������� ������� ����
            CloseMainWindow();

            //���� ������ �� ���� ����������
            UIWorkshopWindow workshopWindow
                = eUI.Value.workshopWindow;

            //������ ���� ���������� ��������
            workshopWindow.gameObject.SetActive(true);
            //� ��������� ��� ��� �������� ���� � EUI
            eUI.Value.activeMainWindow = eUI.Value.workshopWindow.gameObject;

            //���������, ��� ������� ���� ����������
            eUI.Value.activeMainWindowType = MainWindowType.Workshop;


            //���� � ������ ������� �������� ���� ������ ��������
            if (workshopWindow.contentSetPanels.Count != 0)
            {
                //������� ������
                workshopWindow.ClearContentSetPanels();
            }

            //��� ������� ������ �������� � ������ ����������
            for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
            {
                //���� ������ �� ������ ������ ��������
                ref readonly WDContentSet contentSet
                    = ref contentData.Value.wDContentSets[a];

                //������ ������ ������ �������� � ������
                UIWorkshopContentSetPanel workshopContentSetPanel
                    = workshopWindow.InstantiateWorkshopContentSetPanel(
                        a,
                        in contentSet);

                //���� ��� ������ ����� ��������
                if (a == 0)
                {
                    //������ ��� ������ ��������
                    workshopContentSetPanel.panelToggle.isOn = true;
                }
            }


            //���������� ������ ������� ������ ��������
            WorkshopDisplayContentSet(0);
        }

        void WorkshopDisplayContentSet(
            int contentSetIndex)
        {
            //���� ������ �� ���� ����������
            UIWorkshopWindow workshopWindow
                = eUI.Value.workshopWindow;


            //���� ������ �� �������� ������ ��������
            ref readonly WDContentSetDescription contentSetDescription
                = ref contentData.Value.wDContentSetDescriptions[contentSetIndex];

            //���� ������ �� ������ ������ ��������
            ref readonly WDContentSet contentSet
                = ref contentData.Value.wDContentSets[contentSetIndex];

            //��������� ������ ������ ��������
            workshopWindow.currentContentSetIndex
                = contentSetIndex;

            //���������� �������� ������ ��������
            workshopWindow.currentContentSetName.text
                = contentSetDescription.ContentSetName;

            //���������� ������ ����, ��� ������� ������ ����� ��������
            workshopWindow.currentContentSetGameVersion.text
                = contentSetDescription.GameVersion;

            //���������� ������ ������ ��������
            workshopWindow.currentContentSetVersion.text
                = contentSetDescription.ContentSetVersion;


            //��������� ��������� ������������� � ������ ����������� ������
            workshopWindow.contentObjectCountToggleGroup.SetAllTogglesOff();

            //���������� ���������� � �������� � ������ ������ ��������
            workshopWindow.shipsInfoPanel.contentAmount.text
                = contentSet.shipClasses.Length.ToString();

            //���������� ���������� � ���������� � ������ ������ ��������
            workshopWindow.enginesInfoPanel.contentAmount.text
                = contentSet.engines.Length.ToString();

            //���������� ���������� � ��������� � ������ ������ ��������
            workshopWindow.reactorsInfoPanel.contentAmount.text
                = contentSet.reactors.Length.ToString();

            //���������� ���������� � ��������� ����� � ������ ������ ��������
            workshopWindow.fuelTanksInfoPanel.contentAmount.text
                = contentSet.fuelTanks.Length.ToString();

            //���������� ���������� �� ������������ ��� ������ ������ � ������ ������ ��������
            workshopWindow.solidExtractionEquipmentsInfoPanel.contentAmount.text
                = contentSet.solidExtractionEquipments.Length.ToString();

            //���������� ���������� �� �������������� ������� � ������ ������ ��������
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
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow = eUI.Value.designerWindow;

            //��� ������� ������� � ���������
            foreach (int designerActionRequestEntity in designerActionRequestFilter.Value)
            {
                //���� ������
                ref RDesignerAction designerActionRequest = ref designerActionRequestPool.Value.Get(designerActionRequestEntity);

                //���� ������������� ���������� ������� ��������
                if (designerActionRequest.actionType == DesignerActionType.SaveContentObject)
                {
                    //���� ������ ��������� ��������� � ������� ����� ��������
                    if (designerActionRequest.isCurrentContentSet == true)
                    {
                        //��������� ����������� ������ � ������� ����� ��������
                        DesignerSaveContentObject(designerWindow.currentContentSetIndex);
                    }
                }
                //�����, ���� ������������� �������� ������� ��������
                else if (designerActionRequest.actionType == DesignerActionType.LoadContentSetObject)
                {
                    //��������� ����������� ������
                    DesignerLoadContentObject(
                        designerActionRequest.contentSetIndex,
                        designerActionRequest.objectIndex);
                }
                //�����, ���� ������������� �������� ������� ��������
                else if (designerActionRequest.actionType == DesignerActionType.DeleteContentSetObject)
                {
                    //������� ����������� ������
                    DesignerDeleteContentObject(
                        designerActionRequest.contentSetIndex,
                        designerActionRequest.objectIndex);
                }
                //�����, ���� ������������� ����������� ������ �� ������ ������ ��������
                else if (designerActionRequest.actionType == DesignerActionType.DisplayContentSetPanelList)
                {
                    //���������� ������ �������������� �������
                    DesignerDisplayContentSetPanelList(
                        designerActionRequest.isCurrentContentSet,
                        designerActionRequest.contentSetIndex);
                }
                //�����, ���� ������������� ����������� ������ ������ ��������
                else if (designerActionRequest.actionType == DesignerActionType.DisplayContentSetPanel)
                {
                    //���� ������������� ����������� ������ �������� ������ ��������
                    if (designerActionRequest.isCurrentContentSet == true)
                    {
                        //���������� ������ �������� ������ ��������
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            true);
                    }
                    //�����
                    else
                    {
                        //���������� ������ ������ ������� ��������
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            true);
                    }
                }
                //�����, ���� ������������� �������� ������ ������ ��������
                else if (designerActionRequest.actionType == DesignerActionType.HideContentSetPanel)
                {
                    //���� ������������� �������� ������ �������� ������ ��������
                    if (designerActionRequest.isCurrentContentSet == true)
                    {
                        //�������� ������ �������� ������ ��������
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            false);
                    }
                    //�����
                    else
                    {
                        //�������� ������ ������ ������� ��������
                        DesignerDisplayContentSetPanel(
                            designerActionRequest.isCurrentContentSet,
                            false);
                    }
                }
                //�����, ���� ������������� �������� ����������
                else if (designerActionRequest.actionType == DesignerActionType.OpenWorkshop)
                {
                    //�������� ������ ������ � �������� ������� ��������
                    DesignerDisplayContentSetPanel(
                        false,
                        false);

                    DesignerDisplayContentSetPanel(
                        true,
                        false);

                    //����������� ���������� ������ ��������
                    SaveContentSetRequest(designerWindow.currentContentSetIndex);

                    //��������� ���� ����������
                    WorkshopOpenWindow();
                }
                //�����, ���� ������������� �������� ���� ����
                else if (designerActionRequest.actionType == DesignerActionType.OpenGame)
                {
                    //�������� ������ ������ � �������� ������� ��������
                    DesignerDisplayContentSetPanel(
                        false,
                        false);

                    DesignerDisplayContentSetPanel(
                        true,
                        false);

                    //��������� ���� ����
                    GameOpenWindow();
                }

                world.Value.DelEntity(designerActionRequestEntity);
            }

            //���� ������� �������� ��������
            if (designerWindow.designerType == DesignerType.ShipClass)
            {
                //��� ������� ������� �������� � ��������� ������� ��������
                foreach (int designerShipClassActionRequestEntity in designerShipClassActionRequestFilter.Value)
                {
                    //���� ������
                    ref RDesignerShipClassAction designerShipClassActionRequest = ref designerShipClassActionRequestPool.Value.Get(designerShipClassActionRequestEntity);

                    //���� ������������� ���������� ���������� � ����� �������
                    if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.AddComponentToClass)
                    {
                        //��������� ����������� ���������
                        DesignerShipClassAddComponentFirst(
                            designerShipClassActionRequest.componentType,
                            designerShipClassActionRequest.contentSetIndex,
                            designerShipClassActionRequest.modelIndex,
                            designerShipClassActionRequest.numberOfComponents);
                    }
                    //�����, ���� ������������� �������� ���������� �� ������ �������
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.DeleteComponentFromClass)
                    {
                        //������� ����������� ���������
                        DesignerShipClassDeleteComponentFirst(
                            designerShipClassActionRequest.componentType,
                            designerShipClassActionRequest.contentSetIndex,
                            designerShipClassActionRequest.modelIndex,
                            designerShipClassActionRequest.numberOfComponents);
                    }
                    //�����, ���� ������������� ����������� ��������� ���������� � ����������
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.DisplayComponentDetailedInfo)
                    {
                        //���������� ��������� ���������� � ����������
                        DesignerShipClassDisplayComponentDetailedInfo(
                            true,
                            designerShipClassActionRequest.contentSetIndex,
                            designerShipClassActionRequest.modelIndex,
                            designerShipClassActionRequest.componentType);

                    }
                    //�����, ���� ������������� �������� ��������� ���������� � ����������
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.HideComponentDetailedInfo)
                    {
                        //�������� ��������� ���������� � ����������
                        DesignerShipClassDisplayComponentDetailedInfo(false);
                    }
                    //�����, ���� ������������� ����� ���� ��������� �����������
                    else if (designerShipClassActionRequest.actionType == DesignerShipClassActionType.ChangeAvailableComponentsType)
                    {
                        //���������� ��������� ���������� ������������ ����
                        DesignerShipClassDisplayAvailableComponentsType(designerShipClassActionRequest.componentType);
                    }

                    world.Value.DelEntity(designerShipClassActionRequestEntity);
                }
            }
            //�����, ���� ������� �������� �����������
            else if (designerWindow.designerType >= DesignerType.ComponentEngine
                && designerWindow.designerType <= DesignerType.ComponentGunEnergy)
            {
                //��� ������� ������� �������� � ��������� �����������
                foreach (int designerComponentActionRequestEntity in designerComponentActionRequestFilter.Value)
                {
                    //���� ������
                    ref RDesignerComponentAction designerComponentActionRequest = ref designerComponentActionRequestPool.Value.Get(designerComponentActionRequestEntity);

                    //���� ������������� ��������� �������� ����������
                    if (designerComponentActionRequest.actionType == DesignerComponentActionType.ChangeCoreTechnology)
                    {
                        //�������� ����������� �������� ����������
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
            //��������� �������� ������� ����
            CloseMainWindow();

            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //������ ���� ��������� ��������
            designerWindow.gameObject.SetActive(
                true);
            //� ��������� ��� ��� �������� ���� � EUI
            eUI.Value.activeMainWindow
                = eUI.Value.designerWindow.gameObject;

            //���������, ��� ������� ���� ���������
            eUI.Value.activeMainWindowType
                = MainWindowType.Designer;


            //��������� �������� ��������� ���������
            designerWindow.designerTypeName.text
                = designerType.ToString();

            //���� �����-�� ������ �������� ��� ��������
            if (designerWindow.designerType
                != designerType
                && designerWindow.activeDesigner
                != null)
            {
                //������ ��� ����������
                designerWindow.activeDesigner.SetActive(
                    false);
            }

            //��������� ������ �������� ������ ��������
            designerWindow.currentContentSetIndex
                = contentSetIndex;


            //������ ��������� ������������ ����������
            DTechnologyModifiers tempModifiers
                = new();
            //������ ��������� ������ �������� ���������� �������
            Dictionary<int, DOrganizationTechnology>[] tempFactionTechnologies
                = new Dictionary<int, DOrganizationTechnology>[0];

            //������ ������ �� ������������ ����������
            ref readonly DTechnologyModifiers technologyModifiers
                = ref tempModifiers;
            //������ ������ �� ������ �������� ���������� �������
            ref readonly Dictionary<int, DOrganizationTechnology>[] factionTechnologies
                = ref tempFactionTechnologies;

            //���������� ������ �������� ������ ��������
            int baseContentSetIndex
                = 0;

            //���������, �������� �� �������� �������� �������������
            designerWindow.isInGameDesigner
                = isInGameDesigner;

            //���� ������� ������������� ��������
            if (designerWindow.isInGameDesigner
                == true)
            {
                //�������� ������ ������ ������� ��������
                designerWindow.otherContentSetsList.gameObject.SetActive(
                    false);

                //�� ������ �������� ������ �������� �������� ������ �������� �������
                designerWindow.currentContentSetList.deleteButton.gameObject.SetActive(
                    false);

                //���� ���������� ������ ���� �� �������� �������
                if (factionPE.Unpack(world.Value, out int factionEntity))
                {
                    //���� ��������� �������
                    ref COrganization faction
                        = ref organizationPool.Value.Get(factionEntity);

                    //���� ������ �� ������������ ���������� �������
                    technologyModifiers
                        = ref faction.technologyModifiers;

                    //���� ������ �� ������ �������� ���������� �������
                    factionTechnologies
                        = ref faction.technologies;
                }
            }
            //�����
            else
            {
                //���������� ������ ������ ������� ��������
                designerWindow.otherContentSetsList.gameObject.SetActive(
                    true);

                //�� ������ �������� ������ �������� ���������� ������ �������� �������
                designerWindow.currentContentSetList.deleteButton.gameObject.SetActive(
                    true);

                //���� ������ �� ����� ������������ ����������
                technologyModifiers
                    = ref contentData.Value.globalTechnologyModifiers;

                //���� ������ �� ������ �������� ����� ����������
                factionTechnologies
                    = ref contentData.Value.globalTechnologies;

                //���� ������ �������� ������ �������� ����� ����
                if (contentSetIndex
                    == 0)
                {
                    //���� � ������� ���������� ������ ������ ������ ��������
                    if (contentData.Value.wDContentSets.Length
                        > 1)
                    {
                        //�� ������� ������� �������� ����� ������
                        baseContentSetIndex
                            = 1;
                    }
                }
                //����� ������� ������� �������� ����� ������
            }

            //���� ��������� ������� �������� ��������
            if (designerType
                == DesignerType.ShipClass)
            {
                //������ �������� �������� ��������
                designerWindow.shipClassDesigner.gameObject.SetActive(
                    true);
                //��������� ��� ��� �������� �������� � ���� ���������
                designerWindow.activeDesigner
                    = designerWindow.shipClassDesigner.gameObject;

                //���������, ��� ������� �������� ��������
                designerWindow.designerType
                    = DesignerType.ShipClass;

                //���������� �������� ��������
                DesignerOpenShipClassWindow(
                    baseContentSetIndex);
            }
            //�����, ���� ��������� ������� �������� ����������
            else if (designerType
                == DesignerType.ComponentEngine)
            {
                //������ �������� �������� ����������
                designerWindow.engineDesigner.gameObject.SetActive(
                    true);
                //��������� ��� ��� �������� �������� � ���� ���������
                designerWindow.activeDesigner
                    = designerWindow.engineDesigner.gameObject;

                //���������, ��� ������� �������� ����������
                designerWindow.designerType
                    = DesignerType.ComponentEngine;

                //���������� �������� ����������
                DesignerOpenEngineComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //�����, ���� ��������� ������� �������� ���������
            else if (designerType
                == DesignerType.ComponentReactor)
            {
                //������ �������� �������� ���������
                designerWindow.reactorDesigner.gameObject.SetActive(
                    true);
                //��������� ��� ��� �������� �������� � ���� ���������
                designerWindow.activeDesigner
                    = designerWindow.reactorDesigner.gameObject;

                //���������, ��� ������� �������� ���������
                designerWindow.designerType
                    = DesignerType.ComponentReactor;

                //���������� �������� ���������
                DesignerOpenReactorComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //�����, ���� ��������� ������� �������� ��������� �����
            else if (designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //������ �������� �������� ��������� �����
                designerWindow.fuelTankDesigner.gameObject.SetActive(
                    true);
                //��������� ��� ��� �������� �������� � ���� ���������
                designerWindow.activeDesigner
                    = designerWindow.fuelTankDesigner.gameObject;

                //���������, ��� ������� �������� ��������� �����
                designerWindow.designerType
                    = DesignerType.ComponentHoldFuelTank;

                //���������� �������� ��������� �����
                DesignerOpenFuelTankComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //�����, ���� ��������� ������� �������� ������������ ��� ������ ������
            else if (designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //������ �������� �������� ������������ ��� ������ ������
                designerWindow.extractionEquipmentDesigner.gameObject.SetActive(
                    true);
                //��������� ��� ��� �������� �������� � ���� ���������
                designerWindow.activeDesigner
                    = designerWindow.extractionEquipmentDesigner.gameObject;

                //���������, ��� ������� �������� ������������ ��� ������ ������
                designerWindow.designerType
                    = DesignerType.ComponentExtractionEquipmentSolid;

                //���������� �������� ������������ ��� ������ ������
                DesignerOpenExtractionEquipmentComponentWindow(
                    baseContentSetIndex,
                    in technologyModifiers,
                    in factionTechnologies);
            }
            //�����, ���� ��������� ������� �������� �������������� ������
            else if (designerType
                == DesignerType.ComponentGunEnergy)
            {
                //������ �������� �������� �������������� ������
                designerWindow.energyGunDesigner.gameObject.SetActive(
                    true);
                //��������� ��� ��� �������� �������� � ���� ���������
                designerWindow.activeDesigner
                    = designerWindow.energyGunDesigner.gameObject;

                //���������, ��� ������� �������� �������������� ������
                designerWindow.designerType
                    = DesignerType.ComponentGunEnergy;

                //���������� �������� �������������� ������
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
            //���� ������ ������ �������� ������ ��������
            if (isCurrentContentSet == true)
            {
                //���� ��������� ���������� ������ �������� ������ ��������
                if (isDisplay == true)
                {
                    //���������� ������
                    eUI.Value.designerWindow.currentContentSetList.listPanel.SetActive(true);

                    //�������� ������ ����������� ������
                    eUI.Value.designerWindow.currentContentSetList.displayButton.gameObject.SetActive(false);

                    //���������� ������ �������� ������
                    eUI.Value.designerWindow.currentContentSetList.hideButton.gameObject.SetActive(true);
                }
                //�����
                else
                {
                    //������� ����� �������� �������
                    eUI.Value.designerWindow.currentContentSetList.objectName.text
                        = "";

                    //�������� ������
                    eUI.Value.designerWindow.currentContentSetList.listPanel.SetActive(false);

                    //�������� ������ �������� ������
                    eUI.Value.designerWindow.currentContentSetList.hideButton.gameObject.SetActive(false);

                    //���������� ������ ����������� ������
                    eUI.Value.designerWindow.currentContentSetList.displayButton.gameObject.SetActive(true);
                }
            }
            //����� ������ ������ ������ ������� ��������
            else
            {
                //���� ��������� ���������� ������ ������ ������� ��������
                if (isDisplay == true)
                {
                    //���������� ������
                    eUI.Value.designerWindow.otherContentSetsList.listPanel.SetActive(true);

                    //�������� ������ ����������� ������
                    eUI.Value.designerWindow.otherContentSetsList.displayButton.gameObject.SetActive(false);

                    //���������� ������ �������� ������
                    eUI.Value.designerWindow.otherContentSetsList.hideButton.gameObject.SetActive(true);
                }
                //�����
                else
                {
                    //�������� ������
                    eUI.Value.designerWindow.otherContentSetsList.listPanel.SetActive(false);

                    //�������� ������ �������� ������
                    eUI.Value.designerWindow.otherContentSetsList.hideButton.gameObject.SetActive(false);

                    //���������� ������ ����������� ������
                    eUI.Value.designerWindow.otherContentSetsList.displayButton.gameObject.SetActive(true);
                }
            }
        }

        void DesignerDisplayContentSetPanelList(
            bool isCurrentContentSet,
            int contentSetIndex = -1)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� �������� ��������� ������ �������� ������ ��������
            if (isCurrentContentSet
                == true)
            {
                //���������� ������ �������� �� �������� ������ �������������� ���� ���������
                DesignerDisplayContentPanels(
                    designerWindow.designerType,
                    DesignerDisplayContentType.ContentSet,
                    designerWindow.currentContentSetList.toggleGroup,
                    designerWindow.currentContentSetList.layoutGroup,
                    designerWindow.currentContentSetList.panelsList,
                    designerWindow.currentContentSetIndex);
            }
            //�����
            else
            {
                //���������� ������ �������� �� ������ ������ ������� �������� �������������� ���� ���������
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
            //������� ���������� ������ ������ ������� ��������
            eUI.Value.designerWindow.otherContentSetsList.dropdown.ClearOptions();

            //��������� ���������� ������ ������ ������� ��������
            eUI.Value.designerWindow.otherContentSetsList.dropdown.AddOptions(
                new List<string>(
                    contentData.Value.contentSetNames));

            //���� ������ ������ �������� ������ ��� ����� ����
            if (contentSetIndex
                >= 0)
            {
                //������� ������� ����� �������� �� ����������� ������
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
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ��������� �������� ������������ ������
            if (isClear == true)
            {
                //���� � ������ ���� ������
                for (int a = 0; a < parentPanelsList.Count; a++)
                {
                    MonoBehaviour.Destroy(parentPanelsList[a]);
                }

                //������� ������
                parentPanelsList.Clear();
            }

            //���� ��������� ���������� �������
            if (contentType == DesignerType.ShipClass)
            {
                //���� ��������� ���������� ��������� ����� ��������
                if (displayType == DesignerDisplayContentType.ContentSet)
                {
                    //������ ������ ��� ���������� ��������
                    List<Tuple<string, int>> shipsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner == true)
                    {
                        //��� ������� ������� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].shipClasses.Length; a++)
                        {
                            //������� ��������� � ������ ������� � ������
                            shipsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].shipClasses[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        shipsSortList = shipsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������� � ��������������� ������
                        for (int a = 0; a < shipsSortList.Count; a++)
                        {
                            //���� ������ �� ������ �������
                            ref readonly DShipClass shipClass
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .shipClasses[shipsSortList[a].Item2];

                            //������ �������� ������ ������� � ��������� ������
                            UIShipClassBriefInfoPanel shipBriefInfoPanel
                                = designerWindow.InstantiateShipBriefInfoPanel(
                                    shipClass,
                                    contentSetIndex, shipsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length; a++)
                        {
                            //������� ��������� � ������ ������� � ������
                            shipsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        shipsSortList
                            = shipsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������� � ��������������� ������
                        for (int a = 0; a < shipsSortList.Count; a++)
                        {
                            //���� ������ �� ������ �������
                            ref readonly WDShipClass shipClass
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .shipClasses[shipsSortList[a].Item2];

                            //������ �������� ������ ������� � ��������� ������
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
            //�����, ���� ��������� ���������� ���������
            else if (contentType == DesignerType.ComponentEngine)
            {
                //���� ��������� ���������� ������� �� ���� ������� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //������ ������ ��� ���������� ����������
                    List<Tuple<string, int, int>> enginesSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //��� ������� ��������� � ������ ��������
                            for (int b = 0; b < contentData.Value.contentSets[a].engines.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ��������� � ������
                                enginesSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].engines[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������� � ��������������� ������
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������
                            ref readonly DEngine engine
                                = ref contentData.Value
                                .contentSets[enginesSortList[a].Item2]
                                .engines[enginesSortList[a].Item3];

                            //������ �������� ������ ��������� � ��������� ������
                            UIEngineBriefInfoPanel engineBriefInfoPanel
                                = designerWindow.InstantiateEngineBriefInfoPanel(
                                    engine,
                                    enginesSortList[a].Item2, enginesSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //��� ������� ��������� � ������ ��������
                            for (int b = 0; b < contentData.Value.wDContentSets[a].engines.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ��������� � ������
                                enginesSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].engines[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������� � ��������������� ������
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������
                            ref readonly WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[enginesSortList[a].Item2]
                                .engines[enginesSortList[a].Item3];

                            //������ �������� ������ ��������� � ��������� ������
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
                //�����, ���� ��������� ���������� ��������� ����� ��������
                else if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //������ ������ ��� ���������� ����������
                    List<Tuple<string, int>> enginesSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ��������� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].engines.Length; a++)
                        {
                            //������� �������� � ������ ��������� � ������
                            enginesSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].engines[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������� � ��������������� ������
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������
                            ref readonly DEngine engine
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .engines[enginesSortList[a].Item2];

                            //������ �������� ������ ��������� � ��������� ������
                            UIEngineBriefInfoPanel engineBriefInfoPanel
                                = designerWindow.InstantiateEngineBriefInfoPanel(
                                    engine,
                                    contentSetIndex, enginesSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ��������� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].engines.Length; a++)
                        {
                            //������� �������� � ������ ��������� � ������
                            enginesSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].engines[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������� � ��������������� ������
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������
                            ref readonly WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .engines[enginesSortList[a].Item2];

                            //������ �������� ������ ��������� � ��������� ������
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
                //�����, ���� ��������� ���������� �������, ���������� � ������������� �������
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //���� ������ �� ���� ��������� �������
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //������ ������ ��� ���������� ����������
                    List<Tuple<string, int, int, int>> enginesSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ��������� � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.engines.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ��������� � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������� � ��������������� ������
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������
                            ref readonly DEngine engine
                                = ref contentData.Value
                                .contentSets[enginesSortList[a].Item3]
                                .engines[enginesSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel engineInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    enginesSortList[a].Item3, enginesSortList[a].Item4,
                                    ShipComponentType.Engine,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ���������
                            engineInstalledComponentPanel.modelName.text
                                = engine.ObjectName;

                            //��������� �������� ���� ����������
                            engineInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Engine.ToString();

                            //��������� ����� ����������
                            engineInstalledComponentPanel.componentNumber.text
                                = enginesSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            engineInstalledComponentPanel.componentTotalSize.text
                                = (enginesSortList[a].Item2 * engine.EngineSize).ToString();
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ��������� � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.engines.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ��������� � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        enginesSortList
                            = enginesSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������� � ��������������� ������
                        for (int a = 0; a < enginesSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������
                            ref readonly WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[enginesSortList[a].Item3]
                                .engines[enginesSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel engineInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    enginesSortList[a].Item3, enginesSortList[a].Item4,
                                    ShipComponentType.Engine,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ���������
                            engineInstalledComponentPanel.modelName.text
                                = engine.ObjectName;

                            //��������� �������� ���� ����������
                            engineInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Engine.ToString();

                            //��������� ����� ����������
                            engineInstalledComponentPanel.componentNumber.text
                                = enginesSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            engineInstalledComponentPanel.componentTotalSize.text
                                = (enginesSortList[a].Item2 * engine.EngineSize).ToString();
                        }
                    }
                }
            }
            //�����, ���� ��������� ���������� ��������
            else if (contentType == DesignerType.ComponentReactor)
            {
                //���� ��������� ���������� ������� �� ���� ������� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //������ ������ ��� ���������� ���������
                    List<Tuple<string, int, int>> reactorsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //��� ������� �������� � ������ ��������
                            for (int b = 0; b < contentData.Value.contentSets[a].reactors.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ �������� � ������
                                reactorsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].reactors[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� �������� � ��������������� ������
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������
                            ref readonly DReactor reactor
                                = ref contentData.Value
                                .contentSets[reactorsSortList[a].Item2]
                                .reactors[reactorsSortList[a].Item3];

                            //������ �������� ������ �������� � ��������� ������
                            UIReactorBriefInfoPanel reactorBriefInfoPanel
                                = designerWindow.InstantiateReactorBriefInfoPanel(
                                    reactor,
                                    reactorsSortList[a].Item2, reactorsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //��� ������� �������� � ������ ��������
                            for (int b = 0; b < contentData.Value.wDContentSets[a].reactors.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ �������� � ������
                                reactorsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].reactors[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� �������� � ��������������� ������
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������
                            ref readonly WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[reactorsSortList[a].Item2]
                                .reactors[reactorsSortList[a].Item3];

                            //������ �������� ������ �������� � ��������� ������
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
                //�����, ���� ��������� ���������� ��������� ����� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //������ ������ ��� ���������� ���������
                    List<Tuple<string, int>> reactorsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� �������� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].reactors.Length; a++)
                        {
                            //������� �������� � ������ �������� � ������
                            reactorsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].reactors[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� �������� � ��������������� ������
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������
                            ref readonly DReactor reactor
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .reactors[reactorsSortList[a].Item2];

                            //������ �������� ������ �������� � ��������� ������
                            UIReactorBriefInfoPanel reactorBriefInfoPanel
                                = designerWindow.InstantiateReactorBriefInfoPanel(
                                    reactor,
                                    contentSetIndex, reactorsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� �������� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].reactors.Length; a++)
                        {
                            //������� �������� � ������ �������� � ������
                            reactorsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].reactors[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� �������� � ��������������� ������
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������
                            ref readonly WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .reactors[reactorsSortList[a].Item2];

                            //������ �������� ������ �������� � ��������� ������
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
                //�����, ���� ��������� ���������� �������, ���������� � ������������� �������
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //���� ������ �� ���� ��������� �������
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //������ ������ ��� ���������� ���������
                    List<Tuple<string, int, int, int>> reactorsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� �������� � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.reactors.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ �������� � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� �������� � ��������������� ������
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������
                            ref readonly DReactor reactor
                                = ref contentData.Value
                                .contentSets[reactorsSortList[a].Item3]
                                .reactors[reactorsSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel reactorInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    reactorsSortList[a].Item3, reactorsSortList[a].Item4,
                                    ShipComponentType.Reactor,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ��������
                            reactorInstalledComponentPanel.modelName.text
                                = reactor.ObjectName;

                            //��������� �������� ���� ����������
                            reactorInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Reactor.ToString();

                            //��������� ����� ���������
                            reactorInstalledComponentPanel.componentNumber.text
                                = reactorsSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            reactorInstalledComponentPanel.componentTotalSize.text
                                = (reactorsSortList[a].Item2 * reactor.ReactorSize).ToString();
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� �������� � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.reactors.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ �������� � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        reactorsSortList
                            = reactorsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� �������� � ��������������� ������
                        for (int a = 0; a < reactorsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������
                            ref readonly WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[reactorsSortList[a].Item3]
                                .reactors[reactorsSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel reactorInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    reactorsSortList[a].Item3, reactorsSortList[a].Item4,
                                    ShipComponentType.Reactor,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ��������
                            reactorInstalledComponentPanel.modelName.text
                                = reactor.ObjectName;

                            //��������� �������� ���� ����������
                            reactorInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.Reactor.ToString();

                            //��������� ����� ���������
                            reactorInstalledComponentPanel.componentNumber.text
                                = reactorsSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            reactorInstalledComponentPanel.componentTotalSize.text
                                = (reactorsSortList[a].Item2 * reactor.ReactorSize).ToString();
                        }
                    }
                }
            }
            //�����, ���� ��������� ���������� ��������� ����
            else if (contentType == DesignerType.ComponentHoldFuelTank)
            {
                //���� ��������� ���������� ������� �� ���� ������� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //������ ������ ��� ���������� ��������� �����
                    List<Tuple<string, int, int>> fuelTanksSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //��� ������� ���������� ���� � ������ ��������
                            for (int b = 0; b < contentData.Value.contentSets[a].fuelTanks.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ���������� ���� � ������
                                fuelTanksSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].fuelTanks[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ���������� ���� � ��������������� ������
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������� ����
                            ref readonly DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[fuelTanksSortList[a].Item2]
                                .fuelTanks[fuelTanksSortList[a].Item3];

                            //������ �������� ������ ���������� ���� � ��������� ������
                            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                                = designerWindow.InstantiateFuelTankBriefInfoPanel(
                                    fuelTank,
                                    fuelTanksSortList[a].Item2, fuelTanksSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //��� ������� ���������� ���� � ������ ��������
                            for (int b = 0; b < contentData.Value.wDContentSets[a].fuelTanks.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ���������� ���� � ������
                                fuelTanksSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].fuelTanks[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ���������� ���� � ��������������� ������
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������� ����
                            ref readonly WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[fuelTanksSortList[a].Item2]
                                .fuelTanks[fuelTanksSortList[a].Item3];

                            //������ �������� ������ ���������� ���� � ��������� ������
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
                //�����, ���� ��������� ���������� ��������� ����� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //������ ������ ��� ���������� ��������� �����
                    List<Tuple<string, int>> fuelTanksSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ���������� ���� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].fuelTanks.Length; a++)
                        {
                            //������� �������� � ������ ���������� ���� � ������
                            fuelTanksSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].fuelTanks[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ���������� ���� � ��������������� ������
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������� ����
                            ref readonly DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .fuelTanks[fuelTanksSortList[a].Item2];

                            //������ �������� ������ ���������� ���� � ��������� ������
                            UIFuelTankBriefInfoPanel fuelTankBriefInfoPanel
                                = designerWindow.InstantiateFuelTankBriefInfoPanel(
                                    fuelTank,
                                    contentSetIndex, fuelTanksSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ���������� ���� � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length; a++)
                        {
                            //������� �������� � ������ ���������� ���� � ������
                            fuelTanksSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].fuelTanks[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ���������� ���� � ��������������� ������
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������� ����
                            ref readonly WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .fuelTanks[fuelTanksSortList[a].Item2];

                            //������ �������� ������ ���������� ���� � ��������� ������
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
                //�����, ���� ��������� ���������� �������, ���������� � ������������� �������
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //���� ������ �� ���� ��������� �������
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //������ ������ ��� ���������� ��������� �����
                    List<Tuple<string, int, int, int>> fuelTanksSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ���������� ���� � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.fuelTanks.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ���������� ���� � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ���������� ���� � ��������������� ������
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������� ����
                            ref readonly DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[fuelTanksSortList[a].Item3]
                                .fuelTanks[fuelTanksSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel fuelTankInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    fuelTanksSortList[a].Item3, fuelTanksSortList[a].Item4,
                                    ShipComponentType.HoldFuelTank,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ���������� ����
                            fuelTankInstalledComponentPanel.modelName.text
                                = fuelTank.ObjectName;

                            //��������� �������� ���� ����������
                            fuelTankInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.HoldFuelTank.ToString();

                            //��������� ����� ��������� �����
                            fuelTankInstalledComponentPanel.componentNumber.text
                                = fuelTanksSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            fuelTankInstalledComponentPanel.componentTotalSize.text
                                = (fuelTanksSortList[a].Item2 * fuelTank.Size).ToString();
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ���������� ���� � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.fuelTanks.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ���������� ���� � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        fuelTanksSortList
                            = fuelTanksSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ���������� ���� � ��������������� ������
                        for (int a = 0; a < fuelTanksSortList.Count; a++)
                        {
                            //���� ������ �� ������ ���������� ����
                            ref readonly WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[fuelTanksSortList[a].Item3]
                                .fuelTanks[fuelTanksSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel fuelTankInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    fuelTanksSortList[a].Item3, fuelTanksSortList[a].Item4,
                                    ShipComponentType.HoldFuelTank,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ���������� ����
                            fuelTankInstalledComponentPanel.modelName.text
                                = fuelTank.ObjectName;

                            //��������� �������� ���� ����������
                            fuelTankInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.HoldFuelTank.ToString();

                            //��������� ����� ��������� �����
                            fuelTankInstalledComponentPanel.componentNumber.text
                                = fuelTanksSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            fuelTankInstalledComponentPanel.componentTotalSize.text
                                = (fuelTanksSortList[a].Item2 * fuelTank.Size).ToString();
                        }
                    }
                }
            }
            //�����, ���� ��������� ���������� ������������ ��� ������ ������
            else if (contentType == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //���� ��������� ���������� ������� �� ���� ������� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //������ ������ ��� ���������� ������������ ��� ������ ������
                    List<Tuple<string, int, int>> extractionEquipmentSolidsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //��� ������� ������������ ��� ������ ������ � ������ ��������
                            for (int b = 0; b < contentData.Value.contentSets[a].solidExtractionEquipments.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ������������ ��� ������ ������ � ������
                                extractionEquipmentSolidsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].solidExtractionEquipments[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������������ ��� ������ ������ � ��������������� ������
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ������������ ��� ������ ������
                            ref readonly DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[extractionEquipmentSolidsSortList[a].Item2]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item3];

                            //������ �������� ������ ������������ ��� ������ ������ � ��������� ������
                            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                                = designerWindow.InstantiateExtractionEquipmentBriefInfoPanel(
                                    extractionEquipmentSolid,
                                    extractionEquipmentSolidsSortList[a].Item2, extractionEquipmentSolidsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //��� ������� ������������ ��� ������ ������ � ������ ��������
                            for (int b = 0; b < contentData.Value.wDContentSets[a].solidExtractionEquipments.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ������������ ��� ������ ������ � ������
                                extractionEquipmentSolidsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].solidExtractionEquipments[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������������ ��� ������ ������ � ��������������� ������
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ������������ ��� ������ ������
                            ref readonly WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[extractionEquipmentSolidsSortList[a].Item2]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item3];

                            //������ �������� ������ ������������ ��� ������ ������ � ��������� ������
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
                //�����, ���� ��������� ���������� ��������� ����� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //������ ������ ��� ���������� ������������ ��� ������ ������
                    List<Tuple<string, int>> extractionEquipmentsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������������ ��� ������ ������ � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                        {
                            //������� �������� � ������ ������������ ��� ������ ������ � ������
                            extractionEquipmentsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        extractionEquipmentsSortList
                            = extractionEquipmentsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������������ ��� ������ ������ � ��������������� ������
                        for (int a = 0; a < extractionEquipmentsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ������������ ��� ������ ������
                            ref readonly DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .solidExtractionEquipments[extractionEquipmentsSortList[a].Item2];

                            //������ �������� ������ ������������ ��� ������ ������ � ��������� ������
                            UIExtractionEquipmentBriefInfoPanel extractionEquipmentBriefInfoPanel
                                = designerWindow.InstantiateExtractionEquipmentBriefInfoPanel(
                                    extractionEquipmentSolid,
                                    contentSetIndex, extractionEquipmentsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������������ ��� ������ ������ � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                        {
                            //������� �������� � ������ ������������ ��� ������ ������ � ������
                            extractionEquipmentsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        extractionEquipmentsSortList
                            = extractionEquipmentsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������������ ��� ������ ������ � ��������������� ������
                        for (int a = 0; a < extractionEquipmentsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ������������ ��� ������ ������
                            ref readonly WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .solidExtractionEquipments[extractionEquipmentsSortList[a].Item2];

                            //������ �������� ������ ������������ ��� ������ ������ � ��������� ������
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
                //�����, ���� ��������� ���������� �������, ���������� � ������������� �������
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //���� ������ �� ���� ��������� �������
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //������ ������ ��� ���������� ������������ ��� ������ ������
                    List<Tuple<string, int, int, int>> extractionEquipmentSolidsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������������ ��� ������ ������ � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ������������ ��� ������ ������ � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������������ ��� ������ ������ � ��������������� ������
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ������������ ��� ������ ������
                            ref readonly DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[extractionEquipmentSolidsSortList[a].Item3]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel extractionEquipmentInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    extractionEquipmentSolidsSortList[a].Item3, extractionEquipmentSolidsSortList[a].Item4,
                                    ShipComponentType.ExtractionEquipmentSolid,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ������������ ��� ������ ������
                            extractionEquipmentInstalledComponentPanel.modelName.text
                                = extractionEquipmentSolid.ObjectName;

                            //��������� �������� ���� ����������
                            extractionEquipmentInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.ExtractionEquipmentSolid.ToString();

                            //��������� ����� ������������ ��� ������ ������
                            extractionEquipmentInstalledComponentPanel.componentNumber.text
                                = extractionEquipmentSolidsSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            extractionEquipmentInstalledComponentPanel.componentTotalSize.text
                                = (extractionEquipmentSolidsSortList[a].Item2 * extractionEquipmentSolid.Size).ToString();
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������������ ��� ������ ������ � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ������������ ��� ������ ������ � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        extractionEquipmentSolidsSortList
                            = extractionEquipmentSolidsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ������������ ��� ������ ������ � ��������������� ������
                        for (int a = 0; a < extractionEquipmentSolidsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ������������ ��� ������ ������
                            ref readonly WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[extractionEquipmentSolidsSortList[a].Item3]
                                .solidExtractionEquipments[extractionEquipmentSolidsSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel extractionEquipmentInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    extractionEquipmentSolidsSortList[a].Item3, extractionEquipmentSolidsSortList[a].Item4,
                                    ShipComponentType.ExtractionEquipmentSolid,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ������������ ��� ������ ������
                            extractionEquipmentInstalledComponentPanel.modelName.text
                                = extractionEquipmentSolid.ObjectName;

                            //��������� �������� ���� ����������
                            extractionEquipmentInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.ExtractionEquipmentSolid.ToString();

                            //��������� ����� ������������ ��� ������ ������
                            extractionEquipmentInstalledComponentPanel.componentNumber.text
                                = extractionEquipmentSolidsSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            extractionEquipmentInstalledComponentPanel.componentTotalSize.text
                                = (extractionEquipmentSolidsSortList[a].Item2 * extractionEquipmentSolid.Size).ToString();
                        }
                    }
                }
            }
            //�����, ���� ��������� ���������� �������������� ������
            else if (contentType == DesignerType.ComponentGunEnergy)
            {
                //���� ��������� ���������� ������� �� ���� ������� ��������
                if (displayType
                    == DesignerDisplayContentType.ContentSetsAll)
                {
                    //������ ������ ��� ���������� �������������� ������
                    List<Tuple<string, int, int>> energyGunsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets.Length; a++)
                        {
                            //��� ������� ��������������� ������ � ������ ��������
                            for (int b = 0; b < contentData.Value.contentSets[a].energyGuns.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ��������������� ������ � ������
                                energyGunsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.contentSets[a].energyGuns[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������������� ������ � ��������������� ������
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������������� ������
                            ref readonly DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[energyGunsSortList[a].Item2]
                                .energyGuns[energyGunsSortList[a].Item3];

                            //������ �������� ������ ��������������� ������ � ��������� ������
                            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                                = designerWindow.InstantiateEnergyGunBriefInfoPanel(
                                    energyGun,
                                    energyGunsSortList[a].Item2, energyGunsSortList[a].Item3,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets.Length; a++)
                        {
                            //��� ������� ��������������� ������ � ������ ��������
                            for (int b = 0; b < contentData.Value.wDContentSets[a].energyGuns.Length; b++)
                            {
                                //������� ��������, ������ ������ �������� � ������ ��������������� ������ � ������
                                energyGunsSortList.Add(
                                    new Tuple<string, int, int>(
                                        contentData.Value.wDContentSets[a].energyGuns[b].ObjectName,
                                        a,
                                        b));
                            }
                        }

                        //��������� ������ �� �������� � ���������� �������
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������������� ������ � ��������������� ������
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������������� ������
                            ref readonly WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[energyGunsSortList[a].Item2]
                                .energyGuns[energyGunsSortList[a].Item3];

                            //������ �������� ������ ��������������� ������ � ��������� ������
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
                //�����, ���� ��������� ���������� ��������� ����� ��������
                else if (displayType
                    == DesignerDisplayContentType.ContentSet)
                {
                    //������ ������ ��� ���������� �������������� ������
                    List<Tuple<string, int>> energyGunsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ��������������� ������ � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].energyGuns.Length; a++)
                        {
                            //������� �������� � ������ ��������������� ������ � ������
                            energyGunsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.contentSets[contentSetIndex].energyGuns[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������������� ������ � ��������������� ������
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������������� ������
                            ref readonly DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .energyGuns[energyGunsSortList[a].Item2];

                            //������ �������� ������ ��������������� ������ � ��������� ������
                            UIGunEnergyBriefInfoPanel energyGunBriefInfoPanel
                                = designerWindow.InstantiateEnergyGunBriefInfoPanel(
                                    energyGun,
                                    contentSetIndex, energyGunsSortList[a].Item2,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ��������������� ������ � ��������� ������ ��������
                        for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length; a++)
                        {
                            //������� �������� � ������ ��������������� ������ � ������
                            energyGunsSortList.Add(
                                new Tuple<string, int>(
                                    contentData.Value.wDContentSets[contentSetIndex].energyGuns[a].ObjectName,
                                    a));
                        }

                        //��������� ������ �� �������� � ���������� �������
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������������� ������ � ��������������� ������
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������������� ������
                            ref readonly WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .energyGuns[energyGunsSortList[a].Item2];

                            //������ �������� ������ ��������������� ������ � ��������� ������
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
                //�����, ���� ��������� ���������� �������, ���������� � ������������� �������
                else if (displayType
                    == DesignerDisplayContentType.ShipComponents)
                {
                    //���� ������ �� ���� ��������� �������
                    UIShipClassDesignerWindow shipDesignerWindow
                        = designerWindow.shipClassDesigner;

                    //������ ������ ��� ���������� �������������� ������
                    List<Tuple<string, int, int, int>> energyGunsSortList
                        = new();

                    //���� ������� ������������� ��������
                    if (designerWindow.isInGameDesigner
                        == true)
                    {
                        //��� ������� ��������������� ������ � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentGameShipClass.energyGuns.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ��������������� ������ � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������������� ������ � ��������������� ������
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������������� ������
                            ref readonly DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[energyGunsSortList[a].Item3]
                                .energyGuns[energyGunsSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel energyGunInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    energyGunsSortList[a].Item3, energyGunsSortList[a].Item4,
                                    ShipComponentType.GunEnergy,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ��������������� ������
                            energyGunInstalledComponentPanel.modelName.text
                                = energyGun.ObjectName;

                            //��������� �������� ���� ����������
                            energyGunInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.GunEnergy.ToString();

                            //��������� ����� �������������� ������
                            energyGunInstalledComponentPanel.componentNumber.text
                                = energyGunsSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
                            energyGunInstalledComponentPanel.componentTotalSize.text
                                = (energyGunsSortList[a].Item2 * energyGun.Size).ToString();
                        }
                    }
                    //�����
                    else
                    {
                        //��� ������� ��������������� ������ � ������ �������������� �������
                        for (int a = 0; a < shipDesignerWindow.currentWorkshopShipClass.energyGuns.Length; a++)
                        {
                            //������� ��������, ����� �����������, ������ ������ �������� � ������ ��������������� ������ � ������
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

                        //��������� ������ �� �������� � ���������� �������
                        energyGunsSortList
                            = energyGunsSortList.OrderBy(x => x.Item1).ToList();

                        //��� ������� ��������������� ������ � ��������������� ������
                        for (int a = 0; a < energyGunsSortList.Count; a++)
                        {
                            //���� ������ �� ������ ��������������� ������
                            ref readonly WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[energyGunsSortList[a].Item3]
                                .energyGuns[energyGunsSortList[a].Item4];

                            //������ �������� ������ �������������� ���������� � ��������� ������
                            UIInstalledComponentBriefInfoPanel energyGunInstalledComponentPanel
                                = shipDesignerWindow.InstantiateInstalledComponentBriefInfoPanel(
                                    energyGunsSortList[a].Item3, energyGunsSortList[a].Item4,
                                    ShipComponentType.GunEnergy,
                                    parentToggleGroup,
                                    parentLayoutGroup,
                                    parentPanelsList);

                            //��������� �������� ��������������� ������
                            energyGunInstalledComponentPanel.modelName.text
                                = energyGun.ObjectName;

                            //��������� �������� ���� ����������
                            energyGunInstalledComponentPanel.componentTypeName.text
                                = ShipComponentType.GunEnergy.ToString();

                            //��������� ����� �������������� ������
                            energyGunInstalledComponentPanel.componentNumber.text
                                = energyGunsSortList[a].Item2.ToString();

                            //��������� ��������� ������ �����������
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

            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� �������� ������������ �������
            string objectName
                = designerWindow.currentContentSetList.objectName.text;

            //���� ������� �������� ��������
            if (designerWindow.designerType
                == DesignerType.ShipClass)
            {
                //���� ������ �� ���� ��������� ��������
                UIShipClassDesignerWindow shipDesignerWindow
                    = designerWindow.shipClassDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� �������
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].shipClasses.Length; a++)
                    {
                        //���� �������� ������� ���������
                        if (contentData.Value.contentSets[contentSetIndex].shipClasses[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ �������
                            DesignerShipClassDeleteAllComponentsRefs(
                                contentSetIndex,
                                a);

                            //�� �������������� ������ �������
                            contentData.Value.contentSets[contentSetIndex].shipClasses[a]
                                = shipDesignerWindow.currentGameShipClass;
                            contentData.Value.contentSets[contentSetIndex].shipClasses[a].ObjectName
                                = objectName;

                            //������������� �������������� �������
                            contentData.Value.contentSets[contentSetIndex].shipClasses[a].CalculateCharacteristics(
                                contentData.Value);

                            //������� ������ �� ������ ������� �� ��� ������������� ����������
                            DesignerShipClassAddAllComponentsRefs(
                                contentSetIndex,
                                a);


                            //��������, ��� ������� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ������� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //��������� ������ ������� ��������
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].shipClasses,
                            contentData.Value.contentSets[contentSetIndex].shipClasses.Length + 1);

                        //������� ������������� ������� � ������ ��� ��������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .shipClasses[contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1]
                            = shipDesignerWindow.currentGameShipClass;
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .shipClasses[contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1].ObjectName
                            = objectName;


                        //������������� �������������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .shipClasses[contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1].CalculateCharacteristics(
                            contentData.Value);

                        //������� ������ �� ������ ������� �� ��� ������������� ����������
                        DesignerShipClassAddAllComponentsRefs(
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].shipClasses.Length - 1);
                    }
                }
                //�����
                else
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� �������
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length; a++)
                    {
                        //���� �������� ������� ���������
                        if (contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ �������
                            DesignerShipClassDeleteAllComponentsRefs(
                                contentSetIndex,
                                a);

                            //�� �������������� ������ �������
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses[a]
                                = shipDesignerWindow.currentWorkshopShipClass;
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].ObjectName
                                = objectName;

                            //������������� �������������� �������
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses[a].CalculateCharacteristics(
                                contentData.Value);

                            //������� ������ �� ������ ������� �� ��� ������������� ����������
                            DesignerShipClassAddAllComponentsRefs(
                                contentSetIndex,
                                a);


                            //��������, ��� ������� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ������� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //��������� ������ ������� ��������
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].shipClasses,
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length + 1);

                        //������� ������������� ������� � ������ ��� ��������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .shipClasses[contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1]
                            = shipDesignerWindow.currentWorkshopShipClass;
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .shipClasses[contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1].ObjectName
                            = objectName;


                        //������������� �������������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .shipClasses[contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1].CalculateCharacteristics(
                            contentData.Value);

                        //������� ������ �� ������ ������� �� ��� ������������� ����������
                        DesignerShipClassAddAllComponentsRefs(
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses.Length - 1);
                    }
                }

                //���������� ������ �������� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ����������
            else if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //���� ������ �� ���� ��������� ����������
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ���������
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].engines.Length; a++)
                    {
                        //���� �������� ��������� ���������
                        if (contentData.Value.contentSets[contentSetIndex].engines[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ���������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ���������

                            //���� ������ �� ���������
                            ref DEngine engine
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .engines[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������� �������� �� ������� �������
                            ref DComponentCoreTechnology powerPerSizeTechnology
                                = ref engine.coreTechnologies[0];

                            //�������������� ������� ����������
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            powerPerSizeTechnology.ModifierValue
                                = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //���������

                            //������ ���������
                            engine.EngineSize
                                = engineDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //������ ���������
                            engine.EngineBoost
                                = engineDesignerWindow.boostParameterPanel.currentParameterValue;


                            //������������� �������������� ���������
                            engine.CalculateCharacteristics();

                            //������� ������ �� ������ ��������� �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);


                            //��������, ��� ��������� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ��������� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� ���������, �������� ������� �� ������� � ���������
                        DEngine engine
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length],
                                engineDesignerWindow.sizeParameterPanel.currentParameterValue,
                                engineDesignerWindow.boostParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ �������� �� ������� �������
                        Tuple<int, int, float> powerPerSizeTechnology
                            = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ��������� ���������� �������� �� ������� �������
                        engine.coreTechnologies[0]
                            = new(
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                powerPerSizeTechnology.Item3);


                        //��������� ������ ������� ����������
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].engines,
                            contentData.Value.contentSets[contentSetIndex].engines.Length + 1);

                        //������� ����� ��������� � ������ ��� ��������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .engines[contentData.Value.contentSets[contentSetIndex].engines.Length - 1]
                            = engine;


                        //������������� �������������� ���������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .engines[contentData.Value.contentSets[contentSetIndex].engines.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ��������� �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Engine,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].engines.Length - 1);
                    }
                }
                //�����
                else
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ���������
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].engines.Length; a++)
                    {
                        //���� �������� ��������� ���������
                        if (contentData.Value.wDContentSets[contentSetIndex].engines[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ���������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ���������

                            //���� ������ �� ���������
                            ref WDEngine engine
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .engines[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������� �������� �� ������� �������
                            ref WDComponentCoreTechnology powerPerSizeTechnology
                                = ref engine.coreTechnologies[0];

                            //�������������� ������� ����������
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                        engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            powerPerSizeTechnology.ModifierValue
                                = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //���������

                            //������ ���������
                            engine.EngineSize
                                = engineDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //������ ���������
                            engine.EngineBoost
                                = engineDesignerWindow.boostParameterPanel.currentParameterValue;


                            //������������� �������������� ���������
                            engine.CalculateCharacteristics();

                            //������� ������ �� ������ ��������� �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Engine,
                                contentSetIndex,
                                a);


                            //��������, ��� ��������� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ��������� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� ���������, �������� ������� �� ������� � ���������
                        WDEngine engine
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.engineCoreModifierTypes.Length],
                                engineDesignerWindow.sizeParameterPanel.currentParameterValue,
                                engineDesignerWindow.boostParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ �������� �� ������� �������
                        Tuple<int, int, float> powerPerSizeTechnology
                            = engineDesignerWindow.powerPerSizeCoreTechnologiesList[
                                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ��������� ���������� �������� �� ������� �������
                        engine.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.EnginePowerPerSize],
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].technologies[powerPerSizeTechnology.Item2].ObjectName,
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                true,
                                powerPerSizeTechnology.Item3);


                        //��������� ������ ������� ����������
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].engines,
                            contentData.Value.wDContentSets[contentSetIndex].engines.Length + 1);

                        //������� ����� ��������� � ������ ��� ��������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .engines[contentData.Value.wDContentSets[contentSetIndex].engines.Length - 1]
                            = engine;


                        //������������� �������������� ���������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .engines[contentData.Value.wDContentSets[contentSetIndex].engines.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ��������� �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Engine,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].engines.Length - 1);
                    }
                }

                //���������� ������ ���������� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ���������
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //���� ������ �� ���� ��������� ���������
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ��������
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].reactors.Length; a++)
                    {
                        //���� �������� �������� ���������
                        if (contentData.Value.contentSets[contentSetIndex].reactors[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ��������� ���
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ��������

                            //���� ������ �� �������
                            ref DReactor reactor
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .reactors[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ ������� �� ������� �������
                            ref DComponentCoreTechnology energyPerSizeTechnology
                                = ref reactor.coreTechnologies[0];

                            //�������������� ������� ����������
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            energyPerSizeTechnology.ModifierValue
                                = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //���������

                            //������ ��������
                            reactor.ReactorSize
                                = reactorDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //������ ��������
                            reactor.ReactorBoost
                                = reactorDesignerWindow.boostParameterPanel.currentParameterValue;


                            //������������� �������������� ��������
                            reactor.CalculateCharacteristics();

                            //������� ������ �� ������ ������� �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);


                            //��������, ��� ������� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ������� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� �������, �������� ������� �� ������� � ���������
                        DReactor reactor
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length],
                                reactorDesignerWindow.sizeParameterPanel.currentParameterValue,
                                reactorDesignerWindow.boostParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ ������� �� ������� �������
                        Tuple<int, int, float> energyPerSizeTechnology
                            = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� �������� ���������� ������� �� ������� �������
                        reactor.coreTechnologies[0]
                            = new(
                                new(energyPerSizeTechnology.Item1, energyPerSizeTechnology.Item2),
                                energyPerSizeTechnology.Item3);

                        //��������� ������ ������� ���������
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].reactors,
                            contentData.Value.contentSets[contentSetIndex].reactors.Length + 1);

                        //������� ����� �������� � ������ ��� ��������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .reactors[contentData.Value.contentSets[contentSetIndex].reactors.Length - 1]
                            = reactor;

                        //������������� �������������� ��������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .reactors[contentData.Value.contentSets[contentSetIndex].reactors.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ������� �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Reactor,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].reactors.Length - 1);
                    }
                }
                //�����
                else
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ��������
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].reactors.Length; a++)
                    {
                        //���� �������� �������� ���������
                        if (contentData.Value.wDContentSets[contentSetIndex].reactors[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ���������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ��������

                            //���� ������ �� �������
                            ref WDReactor reactor
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .reactors[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ ������� �� ������� �������
                            ref WDComponentCoreTechnology energyPerSizeTechnology
                                = ref reactor.coreTechnologies[0];

                            //�������������� ������� ����������
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                        reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            energyPerSizeTechnology.ModifierValue
                                = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //���������

                            //������ ��������
                            reactor.ReactorSize
                                = reactorDesignerWindow.sizeParameterPanel.currentParameterValue;

                            //������ ��������
                            reactor.ReactorBoost
                                = reactorDesignerWindow.boostParameterPanel.currentParameterValue;


                            //������������� �������������� ��������
                            reactor.CalculateCharacteristics();

                            //������� ������ �� ������ ������� �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.Reactor,
                                contentSetIndex,
                                a);


                            //��������, ��� ������� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ������� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� �������, �������� ������� �� ������� � ���������
                        WDReactor reactor
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.reactorCoreModifierTypes.Length],
                                reactorDesignerWindow.sizeParameterPanel.currentParameterValue,
                                reactorDesignerWindow.boostParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ ������� �� ������� �������
                        Tuple<int, int, float> energyPerSizeTechnology
                            = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[
                                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� �������� ���������� ������� �� ������� �������
                        reactor.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.ReactorEnergyPerSize],
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                contentData.Value.wDContentSets[energyPerSizeTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[energyPerSizeTechnology.Item1].technologies[energyPerSizeTechnology.Item2].ObjectName,
                                new(energyPerSizeTechnology.Item1, energyPerSizeTechnology.Item2),
                                true,
                                energyPerSizeTechnology.Item3);

                        //��������� ������ ������� ���������
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].reactors,
                            contentData.Value.wDContentSets[contentSetIndex].reactors.Length + 1);

                        //������� ����� �������� � ������ ��� ��������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .reactors[contentData.Value.wDContentSets[contentSetIndex].reactors.Length - 1]
                            = reactor;

                        //������������� �������������� ��������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .reactors[contentData.Value.wDContentSets[contentSetIndex].reactors.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ������� �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.Reactor,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].reactors.Length - 1);
                    }
                }

                //���������� ������ ��������� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ��������� �����
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //���� ������ �� ���� ��������� ��������� �����
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ���������� ����
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].fuelTanks.Length; a++)
                    {
                        //���� �������� ���������� ���� ���������
                        if (contentData.Value.contentSets[contentSetIndex].fuelTanks[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ��������� ���
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ���������� ����

                            //���� ������ �� ��������� ���
                            ref DHoldFuelTank fuelTank
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .fuelTanks[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ ������
                            ref DComponentCoreTechnology compressionTechnology
                                = ref fuelTank.coreTechnologies[0];

                            //�������������� ������� ����������
                            compressionTechnology.ContentObjectLink
                                = new(
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            compressionTechnology.ModifierValue
                                = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //���������

                            //������ ���������� ����
                            fuelTank.Size
                                = fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //������������� �������������� ���������� ����
                            fuelTank.CalculateCharacteristics();

                            //������� ������ �� ������ ��������� ��� �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);


                            //��������, ��� ��������� ��� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ��������� ��� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� ��������� ���, �������� ������� �� ������� � ���������
                        DHoldFuelTank fuelTank
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length],
                                fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ ������
                        Tuple<int, int, float> compressionTechnology
                            = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ���������� ���� ���������� ������
                        fuelTank.coreTechnologies[0]
                            = new(
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                compressionTechnology.Item3);

                        //��������� ������ ������� ��������� �����
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].fuelTanks,
                            contentData.Value.contentSets[contentSetIndex].fuelTanks.Length + 1);

                        //������� ����� ��������� ��� � ������ ��� ��������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.contentSets[contentSetIndex].fuelTanks.Length - 1]
                            = fuelTank;

                        //������������� �������������� ���������� ����
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.contentSets[contentSetIndex].fuelTanks.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ��������� ��� �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.HoldFuelTank,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].fuelTanks.Length - 1);
                    }
                }
                //�����
                else
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ���������� ����
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length; a++)
                    {
                        //���� �������� ���������� ���� ���������
                        if (contentData.Value.wDContentSets[contentSetIndex].fuelTanks[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ��������� ���
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ���������� ����

                            //���� ������ �� ��������� ���
                            ref WDHoldFuelTank fuelTank
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .fuelTanks[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ ������
                            ref WDComponentCoreTechnology energyPerSizeTechnology
                                = ref fuelTank.coreTechnologies[0];

                            //�������������� ������� ����������
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                        fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            energyPerSizeTechnology.ModifierValue
                                = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //���������

                            //������ ���������� ����
                            fuelTank.Size
                                = fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //������������� �������������� ���������� ����
                            fuelTank.CalculateCharacteristics();

                            //������� ������ �� ������ ��������� ��� �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.HoldFuelTank,
                                contentSetIndex,
                                a);


                            //��������, ��� ��������� ��� ��� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ��������� ��� �� ��� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� ��������� ���, �������� ������� �� ������� � ���������
                        WDHoldFuelTank fuelTank
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.fuelTankCoreModifierTypes.Length],
                                fuelTankDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ ������
                        Tuple<int, int, float> compressionTechnology
                            = fuelTankDesignerWindow.compressionCoreTechnologiesList[
                                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ���������� ���� ���������� ������
                        fuelTank.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.FuelTankCompression],
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].technologies[compressionTechnology.Item2].ObjectName,
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                true,
                                compressionTechnology.Item3);

                        //��������� ������ ������� ��������� �����
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].fuelTanks,
                            contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length + 1);

                        //������� ����� ��������� ��� � ������ ��� ��������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length - 1]
                            = fuelTank;

                        //������������� �������������� ���������� ����
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .fuelTanks[contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ��������� ��� �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.HoldFuelTank,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].fuelTanks.Length - 1);
                    }
                }

                //���������� ������ ��������� ����� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ������������ ��� ������ ������
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //���� ������ �� ���� ��������� ����������� ������������ 
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ������������ ��� ������ ������
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                    {
                        //���� �������� ������������ ��� ������ ������ ���������
                        if (contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ������������ ��� ������ ������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ������������ ��� ������ ������

                            //���� ������ �� ������������ ��� ������ ������
                            ref DExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .solidExtractionEquipments[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ �������� �� ������� �������
                            ref DComponentCoreTechnology compressionTechnology
                                = ref extractionEquipmentSolid.coreTechnologies[0];

                            //�������������� ������� ����������
                            compressionTechnology.ContentObjectLink
                                = new(
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            compressionTechnology.ModifierValue
                                = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //���������

                            //������ ������������ ��� ������ ������
                            extractionEquipmentSolid.Size
                                = extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //������������� �������������� ������������ ��� ������ ������
                            extractionEquipmentSolid.CalculateCharacteristics();

                            //������� ������ �� ������ ������������ ��� ������ ������ �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);


                            //��������, ��� ������������ ��� ������ ������ ���� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ������������ ��� ������ ������ �� ���� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� ������������ ��� ������ ������, �������� ������� �� ������� � ���������
                        DExtractionEquipment extractionEquipmentSolid
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.solidExtractionEquipmentCoreModifierTypes.Length],
                                extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ �������� �� ������� �������
                        Tuple<int, int, float> compressionTechnology
                            = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ������������ ��� ������ ������ ���������� �������� �� ������� �������
                        extractionEquipmentSolid.coreTechnologies[0]
                            = new(
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                compressionTechnology.Item3);

                        //��������� ������ ������� ������������ ��� ������ ������
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments,
                            contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length + 1);

                        //������� ����� ������������ ��� ������ ������ � ������ ��� ��������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length - 1]
                            = extractionEquipmentSolid;

                        //������������� �������������� ������������ ��� ������ ������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ������������ ��� ������ ������ �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.ExtractionEquipmentSolid,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments.Length - 1);
                    }
                }
                //�����
                else
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ������������ ��� ������ ������
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length; a++)
                    {
                        //���� �������� ������������ ��� ������ ������ ���������
                        if (contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ ������������ ��� ������ ������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ������������ ��� ������ ������

                            //���� ������ �� ������������ ��� ������ ������
                            ref WDExtractionEquipment extractionEquipmentSolid
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .solidExtractionEquipments[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ �������� �� ������� �������
                            ref WDComponentCoreTechnology energyPerSizeTechnology
                                = ref extractionEquipmentSolid.coreTechnologies[0];

                            //�������������� ������� ����������
                            energyPerSizeTechnology.ContentObjectLink
                                = new(
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                        extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            energyPerSizeTechnology.ModifierValue
                                = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex].Item1;


                            //���������

                            //������ ������������ ��� ������ ������
                            extractionEquipmentSolid.Size
                                = extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue;


                            //������������� �������������� ������������ ��� ������ ������
                            extractionEquipmentSolid.CalculateCharacteristics();

                            //������� ������ �� ������ ������������ ��� ������ ������ �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.ExtractionEquipmentSolid,
                                contentSetIndex,
                                a);


                            //��������, ��� ������������ ��� ������ ������ ���� �����������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� ������������ ��� ������ ������ �� ���� �����������
                    if (isOverride
                        == false)
                    {
                        //������ ����� ������������ ��� ������ ������, �������� ������� �� ������� � ���������
                        WDExtractionEquipment extractionEquipmentSolid
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.solidExtractionEquipmentCoreModifierTypes.Length],
                                extractionEquipmentDesignerWindow.sizeParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ �������� �� ������� �������
                        Tuple<int, int, float> compressionTechnology
                            = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[
                                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ������������ ��� ������ ������ ���������� �������� �� ������� �������
                        extractionEquipmentSolid.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize],
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[compressionTechnology.Item1].technologies[compressionTechnology.Item2].ObjectName,
                                new(compressionTechnology.Item1, compressionTechnology.Item2),
                                true,
                                compressionTechnology.Item3);

                        //��������� ������ ������� ������������ ��� ������ ������
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments,
                            contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length + 1);

                        //������� ����� ������������ ��� ������ ������ � ������ ��� ��������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length - 1]
                            = extractionEquipmentSolid;

                        //������������� �������������� ������������ ��� ������ ������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .solidExtractionEquipments[contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ ������������ ��� ������ ������ �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.ExtractionEquipmentSolid,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments.Length - 1);
                    }
                }

                //���������� ������ ������������ ��� ������ ������ �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� �������������� ������
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //���� ������ �� ���� ��������� �������������� ������
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ��������������� ������
                    for (int a = 0; a < contentData.Value.contentSets[contentSetIndex].energyGuns.Length; a++)
                    {
                        //���� �������� ��������������� ������ ���������
                        if (contentData.Value.contentSets[contentSetIndex].energyGuns[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ �������������� ������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ��������������� ������

                            //���� ������ �� �������������� ������
                            ref DGunEnergy energyGun
                                = ref contentData.Value
                                .contentSets[contentSetIndex]
                                .energyGuns[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������ �����������
                            ref DComponentCoreTechnology powerPerSizeTechnology
                                = ref energyGun.coreTechnologies[0];

                            //�������������� ������� ����������
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            powerPerSizeTechnology.ModifierValue
                                = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //���������

                            //������ ��������������� ������
                            energyGun.GunCaliber
                                = energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue;

                            //����� ������ ��������������� ������
                            energyGun.GunBarrelLength
                                = energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue;


                            //������������� �������������� ��������������� ������
                            energyGun.CalculateCharacteristics();

                            //������� ������ �� ������ �������������� ������ �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);


                            //��������, ��� �������������� ������ ���� ������������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� �������������� ������ �� ���� ������������
                    if (isOverride
                        == false)
                    {
                        //������ ����� �������������� ������, �������� ������� �� ������� � ���������
                        DGunEnergy energyGun
                            = new(
                                objectName,
                                new DComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length],
                                energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue,
                                energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ �����������
                        Tuple<int, int, float> powerPerSizeTechnology
                            = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ��������������� ������ ���������� �����������
                        energyGun.coreTechnologies[0]
                            = new(
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                powerPerSizeTechnology.Item3);


                        //��������� ������ ������� �������������� ������
                        Array.Resize(
                            ref contentData.Value.contentSets[contentSetIndex].energyGuns,
                            contentData.Value.contentSets[contentSetIndex].energyGuns.Length + 1);

                        //������� ����� �������������� ������ � ������ ��� ��������� �������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .energyGuns[contentData.Value.contentSets[contentSetIndex].energyGuns.Length - 1]
                            = energyGun;


                        //������������� �������������� ��������������� ������
                        contentData.Value
                            .contentSets[contentSetIndex]
                            .energyGuns[contentData.Value.contentSets[contentSetIndex].energyGuns.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ �������������� ������ �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.GunEnergy,
                            contentSetIndex,
                            contentData.Value.contentSets[contentSetIndex].energyGuns.Length - 1);
                    }
                }
                //�����
                else
                {
                    //������ ���������� ��� ������������ ����������
                    bool isOverride
                        = false;

                    //��� ������� ��������������� ������
                    for (int a = 0; a < contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length; a++)
                    {
                        //���� �������� ��������������� ������ ���������
                        if (contentData.Value.wDContentSets[contentSetIndex].energyGuns[a].ObjectName
                            == objectName)
                        {
                            //������� ��� ������ �� ������ �������������� ������
                            DesignerComponentDeleteAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);

                            //�� �������������� ������ ��������������� ������

                            //���� ������ �� �������������� ������
                            ref WDGunEnergy energyGun
                                = ref contentData.Value
                                .wDContentSets[contentSetIndex]
                                .energyGuns[a];

                            //�������� ����������

                            //���� ������ �� ����������, ������������� �����������
                            ref WDComponentCoreTechnology powerPerSizeTechnology
                                = ref energyGun.coreTechnologies[0];

                            //�������������� ������� ����������
                            powerPerSizeTechnology.ContentObjectLink
                                = new(
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item1,
                                    energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                        energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item2);
                            //� �������� ������������
                            powerPerSizeTechnology.ModifierValue
                                = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex].Item3;


                            //���������

                            //������ ��������������� ������
                            energyGun.GunCaliber
                                = energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue;

                            //����� ������ ��������������� ������
                            energyGun.GunBarrelLength
                                = energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue;


                            //������������� �������������� ��������������� ������
                            energyGun.CalculateCharacteristics();

                            //������� ������ �� ������ �������������� ������ �� ��� ��� �������� ����������
                            DesignerComponentAddAllTechnologiesRefs(
                                ShipComponentType.GunEnergy,
                                contentSetIndex,
                                a);


                            //��������, ��� �������������� ������ ���� ������������
                            isOverride
                                = true;

                            //������� �� �����
                            break;
                        }
                    }

                    //���� �������������� ������ �� ��� ������������
                    if (isOverride
                        == false)
                    {
                        //������ ����� �������������� ������, �������� ������� �� ������� � ���������
                        WDGunEnergy energyGun
                            = new(
                                objectName,
                                new WDComponentCoreTechnology[designerData.Value.energyGunCoreModifierTypes.Length],
                                energyGunDesignerWindow.gunCaliberParameterPanel.currentParameterValue,
                                energyGunDesignerWindow.gunBarrelLengthParameterPanel.currentParameterValue);

                        //���� ��������� ������ ����������, ������������ �����������
                        Tuple<int, int, float> powerPerSizeTechnology
                            = energyGunDesignerWindow.rechargeCoreTechnologiesList[
                                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex];

                        //������� � ������ �������� ���������� ��������������� ������ ���������� �����������
                        energyGun.coreTechnologies[0]
                            = new(
                                contentData.Value.technologyComponentCoreModifiersNames[(int)TechnologyComponentCoreModifierType.GunEnergyRecharge],
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].ContentSetName,
                                contentData.Value.wDContentSets[powerPerSizeTechnology.Item1].technologies[powerPerSizeTechnology.Item2].ObjectName,
                                new(powerPerSizeTechnology.Item1, powerPerSizeTechnology.Item2),
                                true,
                                powerPerSizeTechnology.Item3);


                        //��������� ������ ������� �������������� ������
                        Array.Resize(
                            ref contentData.Value.wDContentSets[contentSetIndex].energyGuns,
                            contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length + 1);

                        //������� ����� �������������� ������ � ������ ��� ��������� �������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .energyGuns[contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length - 1]
                            = energyGun;


                        //������������� �������������� ��������������� ������
                        contentData.Value
                            .wDContentSets[contentSetIndex]
                            .energyGuns[contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length - 1].CalculateCharacteristics();

                        //������� ������ �� ������ �������������� ������ �� ��� ��� �������� ����������
                        DesignerComponentAddAllTechnologiesRefs(
                            ShipComponentType.GunEnergy,
                            contentSetIndex,
                            contentData.Value.wDContentSets[contentSetIndex].energyGuns.Length - 1);
                    }
                }

                //���������� ������ �������������� ������ �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }

            //��������� ������������� � ����� ������� ������ ��������
            designerWindow.otherContentSetsList.toggleGroup.SetAllTogglesOff();
            designerWindow.currentContentSetList.toggleGroup.SetAllTogglesOff();
        }

        void DesignerLoadContentObject(
            int contentSetIndex,
            int objectIndex)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������� �������� ��������
            if (designerWindow.designerType
                == DesignerType.ShipClass)
            {
                //���� ������ �� ���� ��������� ��������
                UIShipClassDesignerWindow shipDesignerWindow
                    = designerWindow.shipClassDesigner;

                //������� ����

                //������� ������ ��������� ���������� � ����������
                DesignerShipClassDisplayComponentDetailedInfo(
                    false);

                //��������� ������������� � ������ ��������� �����������
                shipDesignerWindow.availableComponentsListToggleGroup.SetAllTogglesOff();


                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ����� �������
                    ref readonly DShipClass shipClass
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .shipClasses[objectIndex];

                    //��������� ����� ������� � ��������
                    shipDesignerWindow.currentGameShipClass
                        = new(
                            "",
                            (DShipClassComponent[])shipClass.engines.Clone(),
                            (DShipClassComponent[])shipClass.reactors.Clone(),
                            (DShipClassComponent[])shipClass.fuelTanks.Clone(),
                            (DShipClassComponent[])shipClass.extractionEquipmentSolids.Clone(),
                            (DShipClassComponent[])shipClass.energyGuns.Clone(),
                            new DShipClassPart[0]);

                    //�������� �������� �������� ������ �������
                    /*shipDesignerWindow.currentGameShipClass.ObjectName
                        = "";*/
                }
                //�����
                else
                {
                    //���� ������ �� ����� �������
                    ref readonly WDShipClass shipClass
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .shipClasses[objectIndex];

                    //��������� ����� ������� � ��������
                    shipDesignerWindow.currentWorkshopShipClass
                        = new(
                            "",
                            (WDShipClassComponent[])shipClass.engines.Clone(),
                            (WDShipClassComponent[])shipClass.reactors.Clone(),
                            (WDShipClassComponent[])shipClass.fuelTanks.Clone(),
                            (WDShipClassComponent[])shipClass.extractionEquipmentSolids.Clone(),
                            (WDShipClassComponent[])shipClass.energyGuns.Clone(),
                            new WDShipClassPart[0]);

                    //�������� �������� �������� ������ �������
                    /*shipDesignerWindow.currentWorkshopShipClass.ObjectName
                        = "";*/
                }

                //���������� ��� ������������� ����������
                DesignerShipClassDisplayInstalledComponents(
                    ShipComponentType.None);

                //������������� � ���������� �������������� �������
                shipDesignerWindow.CalculateCharacteristics(
                    contentData.Value,
                    eUI.Value.designerWindow.isInGameDesigner);
            }
            //�����, ���� ������� �������� ����������
            else if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //���� ������ �� ���� ��������� ����������
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������ ���������
                    ref readonly DEngine engine
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .engines[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ �������� �� ������� �������
                    for (int a = 0; a < engineDesignerWindow.powerPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ���������
                        if (engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item1
                            == engine.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item2
                            == engine.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            engineDesignerWindow.powerPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� �������� �� ������� �������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ���������
                    engineDesignerWindow.DisplayParameters(
                        engine.EngineSize,
                        engine.EngineBoost);
                }
                //�����
                else
                {
                    //���� ������ �� ������ ���������
                    ref readonly WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .engines[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ �������� �� ������� �������
                    for (int a = 0; a < engineDesignerWindow.powerPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ���������
                        if (engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item1
                            == engine.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && engineDesignerWindow.powerPerSizeCoreTechnologiesList[a].Item2
                            == engine.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            engineDesignerWindow.powerPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� �������� �� ������� �������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ���������
                    engineDesignerWindow.DisplayParameters(
                        engine.EngineSize,
                        engine.EngineBoost);
                }

                //������������� � ���������� �������������� ���������
                engineDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� ���������
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //���� ������ �� ���� ��������� ���������
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������ ��������
                    ref readonly DReactor reactor
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .reactors[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ ������� �� ������� �������
                    for (int a = 0; a < reactorDesignerWindow.energyPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ��������
                        if (reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item1
                            == reactor.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item2
                            == reactor.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� ������� �� ������� �������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ��������
                    reactorDesignerWindow.DisplayParameters(
                        reactor.ReactorSize,
                        reactor.ReactorBoost);
                }
                //�����
                else
                {
                    //���� ������ �� ������ ��������
                    ref readonly WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .reactors[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ ������� �� ������� �������
                    for (int a = 0; a < reactorDesignerWindow.energyPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ��������
                        if (reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item1
                            == reactor.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && reactorDesignerWindow.energyPerSizeCoreTechnologiesList[a].Item2
                            == reactor.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� ������� �� ������� �������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ��������
                    reactorDesignerWindow.DisplayParameters(
                        reactor.ReactorSize,
                        reactor.ReactorBoost);
                }

                //������������� � ���������� �������������� ��������
                reactorDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� ��������� �����
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //���� ������ �� ���� ��������� ��������� �����
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������ ���������� ����
                    ref readonly DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .fuelTanks[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ ������
                    for (int a = 0; a < fuelTankDesignerWindow.compressionCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ���������� ����
                        if (fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item1
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item2
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            fuelTankDesignerWindow.compressionCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� ������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ���������� ����
                    fuelTankDesignerWindow.DisplayParameters(
                        fuelTank.Size);
                }
                //�����
                else
                {
                    //���� ������ �� ������ ���������� ����
                    ref readonly WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .fuelTanks[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ ������
                    for (int a = 0; a < fuelTankDesignerWindow.compressionCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ���������� ����
                        if (fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item1
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && fuelTankDesignerWindow.compressionCoreTechnologiesList[a].Item2
                            == fuelTank.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            fuelTankDesignerWindow.compressionCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� ������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ���������� ����
                    fuelTankDesignerWindow.DisplayParameters(
                        fuelTank.Size);
                }

                //������������� � ���������� �������������� ���������� ����
                fuelTankDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� ������������ ��� ������ ������
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //���� ������ �� ���� ��������� ����������� ������������
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������ ������������ ��� ������ ������
                    ref readonly DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .solidExtractionEquipments[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ �������� �� ������� �������
                    for (int a = 0; a < extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ������������ ��� ������ ������
                        if (extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item1
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item2
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� �������� �� ������� �������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ������������ ��� ������ ������
                    extractionEquipmentDesignerWindow.DisplayParameters(
                        extractionEquipmentSolid.Size);
                }
                //�����
                else
                {
                    //���� ������ �� ������ ������������ ��� ������ ������
                    ref readonly WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .solidExtractionEquipments[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ �������� �� ������� �������
                    for (int a = 0; a < extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ������������ ��� ������ ������
                        if (extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item1
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[a].Item2
                            == extractionEquipmentSolid.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� �������� �� ������� �������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ������������ ��� ������ ������
                    extractionEquipmentDesignerWindow.DisplayParameters(
                        extractionEquipmentSolid.Size);
                }

                //������������� � ���������� �������������� ������������ ��� ������ ������
                extractionEquipmentDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� �������������� ������
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //���� ������ �� ���� ��������� �������������� ������
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������ ��������������� ������
                    ref readonly DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[contentSetIndex]
                        .energyGuns[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ �����������
                    for (int a = 0; a < energyGunDesignerWindow.rechargeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ��������������� ������
                        if (energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item1
                            == energyGun.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item2
                            == energyGun.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            energyGunDesignerWindow.rechargeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� �����������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ��������������� ������
                    energyGunDesignerWindow.DisplayParameters(
                        energyGun.GunCaliber,
                        energyGun.GunBarrelLength);
                }
                //�����
                else
                {
                    //���� ������ �� ������ ��������������� ������
                    ref readonly WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[contentSetIndex]
                        .energyGuns[objectIndex];

                    //����������� �������� ����������

                    //��� ������ ���������� � ������ �������� ����������, ������������ �����������
                    for (int a = 0; a < energyGunDesignerWindow.rechargeCoreTechnologiesList.Count; a++)
                    {
                        //���� ������ ������ �������� � ������ ���������� ��������� � ����������� ��������������� ������
                        if (energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item1
                            == energyGun.coreTechnologies[0].ContentObjectLink.ContentSetIndex
                            && energyGunDesignerWindow.rechargeCoreTechnologiesList[a].Item2
                            == energyGun.coreTechnologies[0].ContentObjectLink.ObjectIndex)
                        {
                            //�������� ��������� �������� ���������� � ���������� ������
                            energyGunDesignerWindow.rechargeCoreTechnologyPanel.technologiesDropdown.value
                                = a;

                            //��������� ����������� �����������
                            DesignerComponentChangeCoreTechnology(
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                a);

                            //������� �� �����
                            break;
                        }
                    }

                    //���������� ��������� ��������������� ������
                    energyGunDesignerWindow.DisplayParameters(
                        energyGun.GunCaliber,
                        energyGun.GunBarrelLength);
                }

                //������������� � ���������� �������������� ��������������� ������
                energyGunDesignerWindow.CalculateCharacteristics();
            }

            //��������� ������������� � ����� ������� ������ ��������
            designerWindow.otherContentSetsList.toggleGroup.SetAllTogglesOff();
            designerWindow.currentContentSetList.toggleGroup.SetAllTogglesOff();
        }

        void DesignerDeleteContentObject(
            int contentSetIndex,
            int objectIndex)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������� �������� ��������
            if (designerWindow.designerType
                == DesignerType.ShipClass)
            {
                //���� ������ �� ���� ��������� ��������
                UIShipClassDesignerWindow shipDesignerWindow
                    = designerWindow.shipClassDesigner;

                //������� ��� ������ �� ������ �������
                DesignerShipClassDeleteAllComponentsRefs(
                    contentSetIndex,
                    objectIndex);

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //�������� ������ �������� �� ��������� ������
                    List<DShipClass> shipClasses
                        = new(
                            contentData.Value.contentSets[contentSetIndex].shipClasses);

                    //������� ������ �� ���������� �������
                    shipClasses.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.contentSets[contentSetIndex].shipClasses
                        = shipClasses.ToArray();
                }
                //�����
                else
                {
                    //�������� ������ �������� �� ��������� ������
                    List<WDShipClass> shipClasses
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].shipClasses);

                    //������� ������ �� ���������� �������
                    shipClasses.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.wDContentSets[contentSetIndex].shipClasses
                        = shipClasses.ToArray();
                }

                //���������� ������ �������� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ����������
            else if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //���� ������ �� ���� ��������� ����������
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //������� ��� ������ �� ������ ���������
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.Engine,
                    contentSetIndex,
                    objectIndex);

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //�������� ������ �������� �� ��������� ������
                    List<DEngine> engines
                        = new(
                            contentData.Value.contentSets[contentSetIndex].engines);

                    //������� ������ �� ���������� �������
                    engines.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.contentSets[contentSetIndex].engines
                        = engines.ToArray();
                }
                //�����
                else
                {
                    //�������� ������ �������� �� ��������� ������
                    List<WDEngine> engines
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].engines);

                    //������� ������ �� ���������� �������
                    engines.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.wDContentSets[contentSetIndex].engines
                        = engines.ToArray();
                }

                //���������� ������ ���������� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ���������
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //���� ������ �� ���� ��������� ���������
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //������� ��� ������ �� ������ �������
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.Reactor,
                    contentSetIndex,
                    objectIndex);

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //�������� ������ ��������� �� ��������� ������
                    List<DReactor> reactors
                        = new(
                            contentData.Value.contentSets[contentSetIndex].reactors);

                    //������� ������ �� ���������� �������
                    reactors.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.contentSets[contentSetIndex].reactors
                        = reactors.ToArray();
                }
                //�����
                else
                {
                    //�������� ������ ��������� �� ��������� ������
                    List<WDReactor> reactors
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].reactors);

                    //������� ������ �� ���������� �������
                    reactors.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.wDContentSets[contentSetIndex].reactors
                        = reactors.ToArray();
                }

                //���������� ������ ��������� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ��������� �����
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //���� ������ �� ���� ��������� ��������� �����
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //������� ��� ������ �� ������ ��������� ���
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.HoldFuelTank,
                    contentSetIndex,
                    objectIndex);

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //�������� ������ ��������� ����� �� ��������� ������
                    List<DHoldFuelTank> fuelTanks
                        = new(
                            contentData.Value.contentSets[contentSetIndex].fuelTanks);

                    //������� ������ �� ���������� �������
                    fuelTanks.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.contentSets[contentSetIndex].fuelTanks
                        = fuelTanks.ToArray();
                }
                //�����
                else
                {
                    //�������� ������ ��������� ����� �� ��������� ������
                    List<WDHoldFuelTank> fuelTanks
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].fuelTanks);

                    //������� ������ �� ���������� �������
                    fuelTanks.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.wDContentSets[contentSetIndex].fuelTanks
                        = fuelTanks.ToArray();
                }

                //���������� ������ ��������� ����� �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� ������������ ��� ������ ������
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //���� ������ �� ���� ��������� ����������� ������������
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //������� ��� ������ �� ������ ������������ ��� ������ ������
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.ExtractionEquipmentSolid,
                    contentSetIndex,
                    objectIndex);

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //�������� ������ ������������ ��� ������ ������ �� ��������� ������
                    List<DExtractionEquipment> extractionEquipmentSolids
                        = new(
                            contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments);

                    //������� ������ �� ���������� �������
                    extractionEquipmentSolids.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.contentSets[contentSetIndex].solidExtractionEquipments
                        = extractionEquipmentSolids.ToArray();
                }
                //�����
                else
                {
                    //�������� ������ ������������ ��� ������ ������ �� ��������� ������
                    List<WDExtractionEquipment> extractionEquipmentSolids
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments);

                    //������� ������ �� ���������� �������
                    extractionEquipmentSolids.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.wDContentSets[contentSetIndex].solidExtractionEquipments
                        = extractionEquipmentSolids.ToArray();
                }

                //���������� ������ ������������ ��� ������ ������ �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }
            //�����, ���� ������� �������� �������������� ������
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //���� ������ �� ���� ��������� �������������� ������
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //������� ��� ������ �� ������ �������������� ������
                DesignerComponentDeleteAllTechnologiesRefs(
                    ShipComponentType.GunEnergy,
                    contentSetIndex,
                    objectIndex);

                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //�������� ������ �������� �� ��������� ������
                    List<DGunEnergy> energyGuns
                        = new(
                            contentData.Value.contentSets[contentSetIndex].energyGuns);

                    //������� ������ �� ���������� �������
                    energyGuns.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.contentSets[contentSetIndex].energyGuns
                        = energyGuns.ToArray();
                }
                //�����
                else
                {
                    //�������� ������ �������� �� ��������� ������
                    List<WDGunEnergy> energyGuns
                        = new(
                            contentData.Value.wDContentSets[contentSetIndex].energyGuns);

                    //������� ������ �� ���������� �������
                    energyGuns.RemoveAt(
                        objectIndex);

                    //�������������� ������
                    contentData.Value.wDContentSets[contentSetIndex].energyGuns
                        = energyGuns.ToArray();
                }

                //���������� ������ �������������� ������ �� �������� ������ ��������
                DesignerDisplayContentSetPanelList(
                    true);
            }

            //��������� ������������� � ����� ������� ������ ��������
            designerWindow.otherContentSetsList.toggleGroup.SetAllTogglesOff();
            designerWindow.currentContentSetList.toggleGroup.SetAllTogglesOff();
        }

        //�������� ��������
        void DesignerOpenShipClassWindow(
            int baseContentSetIndex)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������ �� ���� ��������� ��������
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;


            //��������� ���������� ������ ������ ������� ��������
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);


            //������� ������ ������������� �����������
            for (int a = 0; a < shipDesignerWindow.installedComponentsPanelsList.Count; a++)
            {
                GameObject.Destroy(
                    shipDesignerWindow.installedComponentsPanelsList[a]);
            }
            shipDesignerWindow.installedComponentsPanelsList.Clear();


            //������� ������ ��������� ���������� � ����������
            DesignerShipClassDisplayComponentDetailedInfo(
                false);

            //���������� ������ ��������� ��� ��������� ����������
            DesignerShipClassDisplayAvailableComponentsType(
                0);


            //���������� ������ �������� �� �������� ������ ��������
            DesignerDisplayContentSetPanelList(
                true);


            //���� ������� ������������� ��������
            if (designerWindow.isInGameDesigner
                == true)
            {
                //������� ������������� ����� ������� �� ������ ����
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
            //�����
            else
            {
                //���� ������ �������� ������ �������� �� ����� ���� � �� ����� ������� �������� ������ ��������
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //���������� ������ �������� �� �������� ������ ��������
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }

                //������� ������������� ����� ������� �� ������ ����������
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

            //������������� � ���������� �������������� �������
            shipDesignerWindow.CalculateCharacteristics(
                contentData.Value,
                designerWindow.isInGameDesigner);
        }

        void DesignerShipClassDisplayInstalledComponents(
            ShipComponentType componentType)
        {
            //���� ������ �� ���� ��������� ��������
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //���� ��������� ��� ����������� �� ������
            if (componentType
                == ShipComponentType.None)
            {
                //���������� ������������� ���������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentEngine,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);

                //���������� ������������� ��������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentReactor,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //���������� ������������� ��������� ����
                DesignerDisplayContentPanels(
                    DesignerType.ComponentHoldFuelTank,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //���������� ������������� ������������ ��� ������ ������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentExtractionEquipmentSolid,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //���������� ������������� �������������� ������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentGunEnergy,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    false);

                //���������� ������������� ������ ����������
            }
            //�����, ���� ��������� ���������� ������ ����������
            else if (componentType
                == ShipComponentType.Engine)
            {
                //���������� ������������� ���������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentEngine,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //�����, ���� ��������� ���������� ������ ���������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //���������� ������������� ��������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentReactor,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //�����, ���� ��������� ���������� ������ ��������� �����
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //���������� ������������� ��������� ����
                DesignerDisplayContentPanels(
                    DesignerType.ComponentHoldFuelTank,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //�����, ���� ��������� ���������� ������ ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //���������� ������������� ������������ ��� ������ ������
                DesignerDisplayContentPanels(
                    DesignerType.ComponentExtractionEquipmentSolid,
                    DesignerDisplayContentType.ShipComponents,
                    shipDesignerWindow.installedComponentsListToggleGroup,
                    shipDesignerWindow.installedComponentsListLayoutGroup,
                    shipDesignerWindow.installedComponentsPanelsList,
                    -2,
                    true);
            }
            //�����, ���� ��������� ���������� ������ �������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //���������� ������������� �������������� ������
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
            //���� ������ �� ���� ��������� ��������
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //������ ���������� ��� ������������, ��� �� ��������� ���������� �����
            bool isOldComponent
                = false;

            //������ ���������� ��� ������������, ������� �� ������������� � ������ ������������� �����������
            bool isInstalledActive
                = false;

            //���� �����-���� ������������� � ������ ������������� �������
            if (shipDesignerWindow.installedComponentsListToggleGroup.AnyTogglesOn()
                == true)
            {
                isInstalledActive
                    = true;
            }

            //���� ������� ����������� ���������� ���������
            if (componentType
                == ShipComponentType.Engine)
            {
                //��������� ���������
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.engines,
                    ref shipDesignerWindow.currentGameShipClass.engines,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� ���������� ��������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //��������� �������
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.reactors,
                    ref shipDesignerWindow.currentGameShipClass.reactors,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� ���������� ���������� ����
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //��������� ��������� ���
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.fuelTanks,
                    ref shipDesignerWindow.currentGameShipClass.fuelTanks,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� ���������� ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //��������� ������������ ��� ������ ������
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids,
                    ref shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� ���������� ��������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //��������� �������������� ������
                DesignerShipClassAddComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.energyGuns,
                    ref shipDesignerWindow.currentGameShipClass.energyGuns,
                    componentType,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }

            //��������� ������ ������������� �����������
            DesignerShipClassDisplayInstalledComponents(
                ShipComponentType.None);

            //������������� � ���������� �������������� �������
            shipDesignerWindow.CalculateCharacteristics(
                contentData.Value,
                eUI.Value.designerWindow.isInGameDesigner);

            //���� ��������� ��� ���������� �����
            if (isOldComponent
                == true)
            {
                //� �����-���� ������������� � ������ ������������� �������
                if (isInstalledActive
                    == true)
                {
                    //��� ������ ������ � ������ ������������� �����������
                    for (int a = 0; a < shipDesignerWindow.installedComponentsPanelsList.Count; a++)
                    {
                        //���� ��������� �������� ������ �������������� ����������
                        if (shipDesignerWindow.installedComponentsPanelsList[a].TryGetComponent(
                            out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                        {
                            //���� � ������ ������������ ������������ ����������
                            if (installedComponentBriefInfoPanel.componentType
                                == componentType
                                && installedComponentBriefInfoPanel.contentSetIndex
                                == contentSetIndex
                                && installedComponentBriefInfoPanel.componentIndex
                                == componentIndex)
                            {
                                //���������� ������������� ������
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
            //��������, ��� ��������� �� ��� ���������� �����
            isOldComponent
                = false;

            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //��� ������� ���������� � �������
                for (int a = 0; a < dShipClassComponents.Length; a++)
                {
                    //���� ������ ������ �������� � ������ ��������� ������������� ������������ ����������
                    if (dShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && dShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //��������� ����� �����������
                        dShipClassComponents[a].numberOfComponents
                            += numberOfComponents;

                        //��������, ��� ��� ��� ������ ���������
                        isOldComponent
                            = true;

                        //������� �� �������
                        return;
                    }
                }

                //���������� ������ ����������
                DShipClassComponent newComponent
                    = new(
                        contentSetIndex,
                        componentIndex,
                        numberOfComponents);

                //�������� ������ ������� �����������
                Array.Resize(
                    ref dShipClassComponents,
                    dShipClassComponents.Length + 1);

                //������� ����� ��������� � ������ ��� ��������� �������
                dShipClassComponents[dShipClassComponents.Length - 1]
                    = newComponent;
            }
            //�����
            else
            {
                //��� ������� ���������� � �������
                for (int a = 0; a < wDShipClassComponents.Length; a++)
                {
                    //���� ������ ������ �������� � ������ ��������� ������������� ������������ ����������
                    if (wDShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && wDShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //��������� ����� �����������
                        wDShipClassComponents[a].numberOfComponents
                            += numberOfComponents;

                        //��������, ��� ��� ��� ������ ���������
                        isOldComponent
                            = true;

                        //������� �� �������
                        return;
                    }
                }

                //���������� �������� ����������
                string componentName
                    = "";

                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //���� �������� ���������
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .engines[componentIndex].ObjectName;
                }
                //�����, ���� ��� ���������� - �������
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //���� �������� ���������� ����
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .reactors[componentIndex].ObjectName;
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //���� �������� ���������� ����
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .fuelTanks[componentIndex].ObjectName;
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //���� �������� ������������ ��� ������ ������
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .solidExtractionEquipments[componentIndex].ObjectName;
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //���� �������� ��������������� ������
                    componentName
                        = contentData.Value
                        .wDContentSets[contentSetIndex]
                        .energyGuns[componentIndex].ObjectName;
                }

                //���������� ������ ����������
                WDShipClassComponent newComponent
                    = new(
                        contentData.Value.wDContentSets[contentSetIndex].ContentSetName,
                        componentName,
                        numberOfComponents,
                        contentSetIndex,
                        componentIndex,
                        true);

                //�������� ������ ������� �����������
                Array.Resize(
                    ref wDShipClassComponents,
                    wDShipClassComponents.Length + 1);

                //������� ����� ��������� � ������ ��� ��������� �������
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
            //���� ������ �� ���� ��������� ��������
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //������ ���������� ��� ������������, ��� �� ��������� ���������� �����
            bool isOldComponent
                = false;

            //������ ���������� ��� ������������, ������� �� ������������� � ������ ������������� �����������
            bool isInstalledActive
                = false;

            //���� �����-���� ������������� � ������ ������������� �������
            if (shipDesignerWindow.installedComponentsListToggleGroup.AnyTogglesOn()
                == true)
            {
                isInstalledActive
                    = true;
            }

            //���� ������� ����������� �������� ���������
            if (componentType
                == ShipComponentType.Engine)
            {
                //������� ���������
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.engines,
                    ref shipDesignerWindow.currentGameShipClass.engines,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� �������� ��������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //������� �������
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.reactors,
                    ref shipDesignerWindow.currentGameShipClass.reactors,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� �������� ���������� ����
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //������� ��������� ���
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.fuelTanks,
                    ref shipDesignerWindow.currentGameShipClass.fuelTanks,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� �������� ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //������� ������������ ��� ������ ������
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.extractionEquipmentSolids,
                    ref shipDesignerWindow.currentGameShipClass.extractionEquipmentSolids,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }
            //�����, ���� ������� ����������� �������� ��������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //������� �������������� ������
                DesignerShipClassDeleteComponentSecond(
                    ref shipDesignerWindow.currentWorkshopShipClass.energyGuns,
                    ref shipDesignerWindow.currentGameShipClass.energyGuns,
                    contentSetIndex,
                    componentIndex,
                    numberOfComponents,
                    out isOldComponent);
            }

            //��������� ������ ������������� �����������
            DesignerShipClassDisplayInstalledComponents(
                ShipComponentType.None);

            //������������� � ���������� �������������� �������
            shipDesignerWindow.CalculateCharacteristics(
                contentData.Value,
                eUI.Value.designerWindow.isInGameDesigner);

            //���� ��������� ��� ���������� �����
            if (isOldComponent
                == true)
            {
                //� �����-���� ������������� � ������ ������������� �������
                if (isInstalledActive
                    == true)
                {
                    //��� ������ ������ � ������ ������������� �����������
                    for (int a = 0; a < shipDesignerWindow.installedComponentsPanelsList.Count; a++)
                    {
                        //���� ��������� �������� ������ �������������� ����������
                        if (shipDesignerWindow.installedComponentsPanelsList[a].TryGetComponent(
                            out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                        {
                            //���� � ������ ������������ ����������
                            if (installedComponentBriefInfoPanel.componentType
                                == componentType
                                && installedComponentBriefInfoPanel.contentSetIndex
                                == contentSetIndex
                                && installedComponentBriefInfoPanel.componentIndex
                                == componentIndex)
                            {
                                //���������� ������������� ������
                                installedComponentBriefInfoPanel.panelToggle.isOn
                                    = true;
                            }
                        }
                    }
                }
            }
            //�����
            else
            {
                //������� ������ ��������� ���������� � ����������
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
            //��������, ��� ��������� �� ��� ���������� �����
            isOldComponent
                = false;

            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //��� ������� ���������� � �������
                for (int a = 0; a < dShipClassComponents.Length; a++)
                {
                    //���� ������ ������ �������� � ������ ��������� ������������� ������������ ����������
                    if (dShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && dShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //���� ��������� ������� ������ �����������, ��� �����������
                        if (dShipClassComponents[a].numberOfComponents
                            > numberOfComponents)
                        {
                            //��������� ����� ����������
                            dShipClassComponents[a].numberOfComponents
                                -= numberOfComponents;

                            //��������, ��� ��� ��� ������ ���������
                            isOldComponent
                                = true;
                        }
                        //�����
                        else
                        {
                            //������� ��������� ���������

                            //�������� ������ � ������
                            List<DShipClassComponent> shipClassComponents
                                = new(dShipClassComponents);

                            //������� ��������� � ��������� ��������
                            shipClassComponents.RemoveAt(
                                a);

                            //�������������� ������
                            dShipClassComponents
                                = shipClassComponents.ToArray();
                        }

                        //������� �� �����
                        break;
                    }
                }
            }
            //�����
            else
            {
                //��� ������� ���������� � �������
                for (int a = 0; a < wDShipClassComponents.Length; a++)
                {
                    //���� ������ ������ �������� � ������ ��������� ������������� ������������ ����������
                    if (wDShipClassComponents[a].ContentSetIndex
                        == contentSetIndex
                        && wDShipClassComponents[a].ObjectIndex
                        == componentIndex)
                    {
                        //���� ��������� ������� ������ �����������, ��� �����������
                        if (wDShipClassComponents[a].numberOfComponents
                            > numberOfComponents)
                        {
                            //��������� ����� ����������
                            wDShipClassComponents[a].numberOfComponents
                                -= numberOfComponents;

                            //��������, ��� ��� ��� ������ ���������
                            isOldComponent
                                = true;
                        }
                        //�����
                        else
                        {
                            //������� ��������� ���������

                            //�������� ������ � ������
                            List<WDShipClassComponent> shipClassComponents
                                = new(wDShipClassComponents);

                            //������� ��������� � ��������� ��������
                            shipClassComponents.RemoveAt(
                                a);

                            //�������������� ������
                            wDShipClassComponents
                                = shipClassComponents.ToArray();
                        }

                        //������� �� �����
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
            //���� ������ �� ���� ��������� ��������
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //���� �����-�� ������ ��������� ���������� � ���������� ���� �������
            if (shipDesignerWindow.activeComponentDetailedInfoPanel
                != null)
            {
                //������ � ����������
                shipDesignerWindow.activeComponentDetailedInfoPanel.gameObject.SetActive(
                    false);
            }

            //���� ��������� ���������� ������ ��������� ����������
            if (isDisplay
                == true)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ��� ���������� - ���������
                    if (componentType
                        == ShipComponentType.Engine)
                    {
                        //���� ������ �� ���������
                        ref readonly DEngine engine
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .engines[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.EngineDetailedInfoPanelControl(
                            engine,
                            engine);
                    }
                    //�����, ���� ��� ���������� - �������
                    else if (componentType
                        == ShipComponentType.Reactor)
                    {
                        //���� ������ �� �������
                        ref readonly DReactor reactor
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .reactors[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.ReactorDetailedInfoPanelControl(
                            reactor,
                            reactor);
                    }
                    //�����, ���� ��� ���������� - ��������� ���
                    else if (componentType
                        == ShipComponentType.HoldFuelTank)
                    {
                        //���� ������ �� ��������� ���
                        ref readonly DHoldFuelTank fuelTank
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .fuelTanks[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.FuelTankDetailedInfoPanelControl(
                            fuelTank,
                            fuelTank,
                            fuelTank);
                    }
                    //�����, ���� ��� ���������� - ������������ ��� ������ ������
                    else if (componentType
                        == ShipComponentType.ExtractionEquipmentSolid)
                    {
                        //���� ������ �� ������������ ��� ������ ������
                        ref readonly DExtractionEquipment extractionEquipmentSolid
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .solidExtractionEquipments[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.ExtractionEquipmentDetailedInfoPanelControl(
                            extractionEquipmentSolid,
                            extractionEquipmentSolid);
                    }
                    //�����, ���� ��� ���������� - �������������� ������
                    else if (componentType
                        == ShipComponentType.GunEnergy)
                    {
                        //���� ������ �� �������������� ������
                        ref readonly DGunEnergy energyGun
                            = ref contentData.Value
                            .contentSets[contentSetIndex]
                            .energyGuns[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.EnergyGunDetailedInfoPanelControl(
                            energyGun,
                            energyGun,
                            energyGun);
                    }
                }
                //�����
                else
                {
                    //���� ��� ���������� - ���������
                    if (componentType
                        == ShipComponentType.Engine)
                    {
                        //���� ������ �� ���������
                        ref readonly WDEngine engine
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .engines[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.EngineDetailedInfoPanelControl(
                            engine,
                            engine);
                    }
                    //�����, ���� ��� ���������� - �������
                    else if (componentType
                        == ShipComponentType.Reactor)
                    {
                        //���� ������ �� �������
                        ref readonly WDReactor reactor
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .reactors[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.ReactorDetailedInfoPanelControl(
                            reactor,
                            reactor);
                    }
                    //�����, ���� ��� ���������� - ��������� ���
                    else if (componentType
                        == ShipComponentType.HoldFuelTank)
                    {
                        //���� ������ �� �������
                        ref readonly WDHoldFuelTank fuelTank
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .fuelTanks[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.FuelTankDetailedInfoPanelControl(
                            fuelTank,
                            fuelTank,
                            fuelTank);
                    }
                    //�����, ���� ��� ���������� - ������������ ��� ������ ������
                    else if (componentType
                        == ShipComponentType.ExtractionEquipmentSolid)
                    {
                        //���� ������ �� ������������ ��� ������ ������
                        ref readonly WDExtractionEquipment extractionEquipmentSolid
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .solidExtractionEquipments[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.ExtractionEquipmentDetailedInfoPanelControl(
                            extractionEquipmentSolid,
                            extractionEquipmentSolid);
                    }
                    //�����, ���� ��� ���������� - �������������� ������
                    else if (componentType
                        == ShipComponentType.GunEnergy)
                    {
                        //���� ������ �� �������������� ������
                        ref readonly WDGunEnergy energyGun
                            = ref contentData.Value
                            .wDContentSets[contentSetIndex]
                            .energyGuns[componentIndex];

                        //���������� ��������� ����������
                        shipDesignerWindow.EnergyGunDetailedInfoPanelControl(
                            energyGun,
                            energyGun,
                            energyGun);
                    }
                }
            }
            //�����
            else
            {
                //������� �������� ����������
                shipDesignerWindow.currentComponentName.text
                    = "";

                //� ��� �������� ����������
                shipDesignerWindow.currentComponentType.text
                    = "";
            }
        }

        void DesignerShipClassDisplayAvailableComponentsType(
            ShipComponentType componentType)
        {
            //���� ������ �� ���� ��������� ��������
            UIShipClassDesignerWindow shipDesignerWindow
                = eUI.Value.designerWindow.shipClassDesigner;

            //��������� ������� ��� ��������� ����������� �������
            shipDesignerWindow.currentAvailableComponentsType
                = componentType;

            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //����������� ����������� ���������������� ���� �����������
                DesignerDisplayContentPanels(
                    designerData.Value.ShipComponentToDesignerType(
                        componentType),
                    DesignerDisplayContentType.ContentSet,
                    shipDesignerWindow.availableComponentsListToggleGroup,
                    shipDesignerWindow.availableComponentsListLayoutGroup,
                    shipDesignerWindow.availableComponentsPanelsList,
                    eUI.Value.designerWindow.currentContentSetIndex);
            }
            //�����
            else
            {
                //����������� ����������� ���������������� ���� �����������
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
            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //���� ������ �� ����� �������
                ref DShipClass shipClass
                    = ref contentData.Value
                    .contentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //��� ������� ���������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ���������� ����, �� ������� ��������� �������
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ������������ ��� ������ ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������������� ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.energyGuns[a],
                        ShipComponentType.GunEnergy,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }
            }
            //�����
            else
            {
                //���� ������ �� ����� �������
                ref WDShipClass shipClass
                    = ref contentData.Value
                    .wDContentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //��� ������� ���������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ���������� ����, �� ������� ��������� �������
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ������������ ��� ������ ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //������� ������ �� �������
                    DesignerShipClassDeleteComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������������� ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //������� ������ �� �������
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
            //���� ��� ���������� - ���������
            if (componentType
                == ShipComponentType.Engine)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ���������
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ ���������
                    for (int a = 0; a < engine.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (engine.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && engine.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            engine.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ���������
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ ���������
                    for (int a = 0; a < engine.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (engine.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && engine.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            engine.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
            }
            //�����, ���� ��� ���������� - �������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ �������
                    for (int a = 0; a < reactor.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (reactor.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && reactor.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            reactor.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����
                else
                {
                    //���� ������ �� �������
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ �������
                    for (int a = 0; a < reactor.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (reactor.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && reactor.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            reactor.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
            }
            //�����, ���� ��� ���������� - ��������� ���
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ��������� ���
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ ��������� ���
                    for (int a = 0; a < fuelTank.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (fuelTank.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && fuelTank.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            fuelTank.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ��������� ���
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ ��������� ���
                    for (int a = 0; a < fuelTank.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (fuelTank.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && fuelTank.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            fuelTank.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
            }
            //�����, ���� ��� ���������� - ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ ������������ ��� ������ ������
                    for (int a = 0; a < extractionEquipmentSolid.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (extractionEquipmentSolid.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && extractionEquipmentSolid.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            extractionEquipmentSolid.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� ������ ������������ ��� ������ ������
                    for (int a = 0; a < extractionEquipmentSolid.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (extractionEquipmentSolid.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && extractionEquipmentSolid.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            extractionEquipmentSolid.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
            }
            //�����, ���� ��� ���������� - �������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������������� ������
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� �������������� ������
                    for (int a = 0; a < energyGun.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (energyGun.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && energyGun.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            energyGun.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����
                else
                {
                    //���� ������ �� �������������� ������
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //��� ������� �������, ������������ �� �������������� ������
                    for (int a = 0; a < energyGun.ShipClasses.Count; a++)
                    {
                        //���� ������� ������ �������� � ������ ������� ���������
                        if (energyGun.ShipClasses[a].ContentSetIndex
                            == shipClassContentSetIndex
                            && energyGun.ShipClasses[a].ObjectIndex
                            == shipClassIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            energyGun.ShipClasses.RemoveAt(
                                a);

                            //������� �� �����
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
            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //���� ������ �� ����� �������
                ref DShipClass shipClass
                    = ref contentData.Value
                    .contentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //��� ������� ���������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ���������� ����, �� ������� ��������� �������
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ������������ ��� ������ ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������������� ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.energyGuns[a],
                        ShipComponentType.GunEnergy,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }
            }
            //�����
            else
            {
                //���� ������ �� ����� �������
                ref WDShipClass shipClass
                    = ref contentData.Value
                    .wDContentSets[shipClassContentSetIndex]
                    .shipClasses[shipClassIndex];

                //��� ������� ���������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.engines.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.engines[a],
                        ShipComponentType.Engine,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.reactors.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.reactors[a],
                        ShipComponentType.Reactor,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ���������� ����, �� ������� ��������� �������
                for (int a = 0; a < shipClass.fuelTanks.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.fuelTanks[a],
                        ShipComponentType.HoldFuelTank,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������������� ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.extractionEquipmentSolids.Length; a++)
                {
                    //��������� ������ �� �������
                    DesignerShipClassAddComponentRef(
                        shipClass.extractionEquipmentSolids[a],
                        ShipComponentType.ExtractionEquipmentSolid,
                        shipClassContentSetIndex,
                        shipClassIndex);
                }

                //��� ������� ��������������� ������, �� ������� ��������� �������
                for (int a = 0; a < shipClass.energyGuns.Length; a++)
                {
                    //��������� ������ �� �������
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
            //���� ��� ���������� - ���������
            if (componentType
                == ShipComponentType.Engine)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ���������
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ ���������
                    engine.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����
                else
                {
                    //���� ������ �� ���������
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .engines[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ ���������
                    engine.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //�����, ���� ��� ���������� - �������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ �������
                    reactor.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����
                else
                {
                    //���� ������ �� �������
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .reactors[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ �������
                    reactor.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //�����, ���� ��� ���������� - ��������� ���
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ��������� ���
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ ��������� ���
                    fuelTank.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����
                else
                {
                    //���� ������ �� ��������� ���
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .fuelTanks[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ ��������� ���
                    fuelTank.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //�����, ���� ��� ���������� - ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ ������������ ��� ������ ������
                    extractionEquipmentSolid.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����
                else
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .solidExtractionEquipments[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ ������������ ��� ������ ������
                    extractionEquipmentSolid.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
            //�����, ���� ��� ���������� - �������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������������� ������
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ �������������� ������
                    energyGun.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
                //�����
                else
                {
                    //���� ������ �� �������������� ������
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[shipClassComponentRef.ContentSetIndex]
                        .energyGuns[shipClassComponentRef.ObjectIndex];

                    //��������� ������� ������� � ������ ����������� �� ������ �������������� ������
                    energyGun.ShipClasses.Add(
                        new(
                            shipClassContentSetIndex,
                            shipClassIndex));
                }
            }
        }

        //�������� �����������
        void DesignerComponentChangeCoreTechnology(
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyDropdownIndex)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������� �������� ����������
            if (designerWindow.designerType
                == DesignerType.ComponentEngine)
            {
                //���� ������ �� ���� ��������� ����������
                UIEngineDesignerWindow engineDesignerWindow
                    = designerWindow.engineDesigner;

                //���� ��� ������������ ���������� - �������� �� ������� �������
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.EnginePowerPerSize)
                {
                    //�������� �������� ������������ �� ��������������� ������

                    //��������� ������� �������� ������������
                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentParameterText.text
                        = engineDesignerWindow.powerPerSizeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //��������� ������ ������� ����������
                    engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //������������� � ���������� �������������� ���������
                engineDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� ���������
            else if (designerWindow.designerType
                == DesignerType.ComponentReactor)
            {
                //���� ������ �� ���� ��������� ���������
                UIReactorDesignerWindow reactorDesignerWindow
                    = designerWindow.reactorDesigner;

                //���� ��� ������������ ���������� - ������� �� ������� �������
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.ReactorEnergyPerSize)
                {
                    //�������� �������� ������������ �� ��������������� ������

                    //��������� ������� �������� ������������
                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentParameterText.text
                        = reactorDesignerWindow.energyPerSizeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //��������� ������ ������� ����������
                    reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //������������� � ���������� �������������� ��������
                reactorDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� ��������� �����
            else if (designerWindow.designerType
                == DesignerType.ComponentHoldFuelTank)
            {
                //���� ������ �� ���� ��������� ��������� �����
                UIFuelTankDesignerWindow fuelTankDesignerWindow
                    = designerWindow.fuelTankDesigner;

                //���� ��� ������������ ���������� - ������
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.FuelTankCompression)
                {
                    //�������� �������� ������������ �� ��������������� ������

                    //��������� ������� �������� ������������
                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentParameterText.text
                        = fuelTankDesignerWindow.compressionCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //��������� ������ ������� ����������
                    fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //������������� � ���������� �������������� ���������� ����
                fuelTankDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� ������������ ��� ������ ������
            else if (designerWindow.designerType
                == DesignerType.ComponentExtractionEquipmentSolid)
            {
                //���� ������ �� ���� ��������� ����������� ������������
                UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                    = designerWindow.extractionEquipmentDesigner;

                //���� ��� ������������ ���������� - �������� �� ������� �������
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize)
                {
                    //�������� �������� ������������ �� ��������������� ������

                    //��������� ������� �������� ������������
                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentParameterText.text
                        = extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //��������� ������ ������� ����������
                    extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //������������� � ���������� �������������� ������������ ��� ������ ������
                extractionEquipmentDesignerWindow.CalculateCharacteristics();
            }
            //�����, ���� ������� �������� �������������� ������
            else if (designerWindow.designerType
                == DesignerType.ComponentGunEnergy)
            {
                //���� ������ �� ���� ��������� �������������� ������
                UIGunEnergyDesignerWindow energyGunDesignerWindow
                    = designerWindow.energyGunDesigner;

                //���� ��� ������������ ���������� - �����������
                if (componentCoreModifierType
                    == TechnologyComponentCoreModifierType.GunEnergyRecharge)
                {
                    //�������� �������� ������������ �� ��������������� ������

                    //��������� ������� �������� ������������
                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentParameterText.text
                        = energyGunDesignerWindow.rechargeCoreTechnologiesList[technologyDropdownIndex].Item3.ToString();

                    //��������� ������ ������� ����������
                    energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex
                        = technologyDropdownIndex;
                }

                //������������� � ���������� �������������� ��������������� ������
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
            //������ ��������� ������ �������� �������� ����������
            List<string> coreComponentTechnologyNames
                = new();

            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //��� ������ ���������� � ���������� ���������� ������� ����������
                for (int a = 0; a < globalTechnologiesArray.Length; a++)
                {
                    //���� ������ �� ������ ����������
                    ref readonly DTechnologyModifierGlobalSort technologyGlobalSortData
                        = ref globalTechnologiesArray[a];

                    //���� ������ � ������ ���������� ���� � ���������� ������� �������
                    if (factionTechnologiesArray[technologyGlobalSortData.contentSetIndex].TryGetValue(
                        technologyGlobalSortData.technologyIndex,
                        out DOrganizationTechnology tempFactionTechnology))
                    {
                        //���� ������ ���������� �����������
                        if (tempFactionTechnology.isResearched
                            == true)
                        {
                            //���� ������ �� ������ ����������
                            ref readonly DTechnology technology
                                = ref contentData.Value
                                .contentSets[technologyGlobalSortData.contentSetIndex]
                                .technologies[technologyGlobalSortData.technologyIndex];

                            //������� � �������� � ������ �������� ����������
                            coreComponentTechnologyNames.Add(
                                technology.ObjectName);

                            //��� ������� ������������ �������� ���������� ����������
                            for (int b = 0; b < technology.technologyComponentCoreModifiers.Length; b++)
                            {
                                //���� ��� ������������ ������������� ������������
                                if (technology.technologyComponentCoreModifiers[b].ModifierType
                                    == technologyComponentCoreModifierType)
                                {
                                    //������� ������ ���������� � ���������� ������ ����������
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
            //�����
            else
            {
                //��� ������ ���������� � ���������� ���������� ������� ����������
                for (int a = 0; a < globalTechnologiesArray.Length; a++)
                {
                    //���� ������ �� ������ ����������
                    ref readonly DTechnologyModifierGlobalSort technologyGlobalSortData
                        = ref globalTechnologiesArray[a];

                    //���� ������ � ������ ���������� ���� � ���������� ������� �������
                    if (factionTechnologiesArray[technologyGlobalSortData.contentSetIndex].TryGetValue(
                        technologyGlobalSortData.technologyIndex,
                        out DOrganizationTechnology tempFactionTechnology))
                    {
                        //���� ������ ���������� �����������
                        if (tempFactionTechnology.isResearched
                            == true)
                        {
                            //���� ������ �� ������ ����������
                            ref readonly WDTechnology technology
                                = ref contentData.Value
                                .wDContentSets[technologyGlobalSortData.contentSetIndex]
                                .technologies[technologyGlobalSortData.technologyIndex];

                            //������� � �������� � ������ �������� ����������
                            coreComponentTechnologyNames.Add(
                                technology.ObjectName);

                            //��� ������� ������������ �������� ���������� ����������
                            for (int b = 0; b < technology.technologyComponentCoreModifiers.Length; b++)
                            {
                                //���� ��� ������������ ������������� ������������
                                if (technology.technologyComponentCoreModifiers[b].ModifierType
                                    == technologyComponentCoreModifierType)
                                {
                                    //������� ������ ���������� � ���������� ������ ����������
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

            //����������� ���������� ������ �������� ����������
            eUI.Value.designerWindow.ComponentCoreTechnologyPanelControl(
                coreComponentTechnologyNames,
                componentCoreTechnologyPanel);

            //���� ����� �������� � ������ ������ ����
            if (coreComponentTechnologyNames.Count
                > 0
                //� ����� ���������� � ������ ������ ����
                && technologyModifiersList.Count
                > 0)
            {
                //��������� �������� ������������
                componentCoreTechnologyPanel.currentParameterText.text
                    = technologyModifiersList[technologyModifiersList.Count - 1].Item3.ToString();

                //��������� ������ ������� ����������
                componentCoreTechnologyPanel.currentTechnologyIndex
                    = technologyModifiersList.Count - 1;

                //�������� ��������� ���������� � ���������� ������
                componentCoreTechnologyPanel.technologiesDropdown.SetValueWithoutNotify(
                    technologyModifiersList.Count - 1);
            }
            //�����
            else
            {
                //������� ������ �������� ������������
                componentCoreTechnologyPanel.currentParameterText.text
                    = "";

                //���������, ��� ������ ������� ���������� - -1
                componentCoreTechnologyPanel.currentTechnologyIndex
                    = -1;
            }
        }

        void DesignerComponentDeleteAllTechnologiesRefs(
            ShipComponentType componentType,
            int componentContentSetIndex,
            int componentIndex)
        {
            //���� ��� ���������� - ���������
            if (componentType
                == ShipComponentType.Engine)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ���������
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ���������
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� ���������
                        DesignerComponentDeleteTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ���������
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ���������
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� ���������
                        DesignerComponentDeleteTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - �������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� �������
                        DesignerComponentDeleteTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� �������
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� �������
                        DesignerComponentDeleteTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - ��������� ���
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ��������� ���
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ��������� ���
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� ��������� ���
                        DesignerComponentDeleteTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ��������� ���
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ��������� ���
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� ��������� ���
                        DesignerComponentDeleteTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ������������ ��� ������ ������
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� ������������ ��� ������ ������
                        DesignerComponentDeleteTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ������������ ��� ������ ������
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� ������������ ��� ������ ������
                        DesignerComponentDeleteTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - �������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������������� ������
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������������� ������
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� �������������� ������
                        DesignerComponentDeleteTechnologyRef(
                            energyGun.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.GunEnergy,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� �������������� ������
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������������� ������
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //������� ������ �� �������������� ������
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
            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //���� ������ �� ����������
                ref DTechnology technology
                    = ref contentData.Value
                    .contentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //��� ������� ���������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.engines.Count; a++)
                    {
                        //���� ������� ������ �������� � ��������� ���������
                        if (technology.engines[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.engines[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ��������� �� ������ �����������
                            technology.engines.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - �������
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //��� ������� ��������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.reactors.Count; a++)
                    {
                        //���� ������� ������ �������� � �������� ���������
                        if (technology.reactors[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.reactors[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            technology.reactors.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //��� ������� ���������� ����, ������������ �� ������ ����������
                    for (int a = 0; a < technology.fuelTanks.Count; a++)
                    {
                        //���� ������� ������ �������� � ���������� ���� ���������
                        if (technology.fuelTanks[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.fuelTanks[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ��������� ��� �� ������ �����������
                            technology.fuelTanks.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //��� ������� ������������ ��� ������ ������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.extractionEquipmentSolids.Count; a++)
                    {
                        //���� ������� ������ �������� � ������������ ��� ������ ������ ���������
                        if (technology.extractionEquipmentSolids[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.extractionEquipmentSolids[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ������������ ��� ������ ������ �� ������ �����������
                            technology.extractionEquipmentSolids.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //��� ������� ��������������� ������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.energyGuns.Count; a++)
                    {
                        //���� ������� ������ �������� � ��������������� ������
                        if (technology.energyGuns[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.energyGuns[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ��������������� ������ �� ������ �����������
                            technology.energyGuns.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
            }
            //�����
            else
            {
                //���� ������ �� ����������
                ref WDTechnology technology
                    = ref contentData.Value
                    .wDContentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //��� ������� ���������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.engines.Count; a++)
                    {
                        //���� ������� ������ �������� � ��������� ���������
                        if (technology.engines[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.engines[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ��������� �� ������ �����������
                            technology.engines.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - �������
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //��� ������� ��������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.reactors.Count; a++)
                    {
                        //���� ������� ������ �������� � �������� ���������
                        if (technology.reactors[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.reactors[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ������� �� ������ �����������
                            technology.reactors.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //��� ������� ���������� ����, ������������ �� ������ ����������
                    for (int a = 0; a < technology.fuelTanks.Count; a++)
                    {
                        //���� ������� ������ �������� � ���������� ���� ���������
                        if (technology.fuelTanks[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.fuelTanks[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ��������� ��� �� ������ �����������
                            technology.fuelTanks.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //��� ������� ������������ ��� ������ ������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.extractionEquipmentSolids.Count; a++)
                    {
                        //���� ������� ������ �������� � ������������ ��� ������ ������ ���������
                        if (technology.extractionEquipmentSolids[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.extractionEquipmentSolids[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ������������ ��� ������ ������ �� ������ �����������
                            technology.extractionEquipmentSolids.RemoveAt(
                                a);

                            //������� �� �����
                            break;
                        }
                    }
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //��� ������� ��������������� ������, ������������ �� ������ ����������
                    for (int a = 0; a < technology.energyGuns.Count; a++)
                    {
                        //���� ������� ������ �������� � ��������������� ������
                        if (technology.energyGuns[a].ContentSetIndex
                            == componentContentSetIndex
                            && technology.energyGuns[a].ObjectIndex
                            == componentIndex)
                        {
                            //������� ������ ��������������� ������ �� ������ �����������
                            technology.energyGuns.RemoveAt(
                                a);

                            //������� �� �����
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
            //���� ��� ���������� - ���������
            if (componentType
                == ShipComponentType.Engine)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ���������
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ���������
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ���������
                        DesignerComponentAddTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ���������
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .engines[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ���������
                    for (int a = 0; a < engine.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ���������
                        DesignerComponentAddTechnologyRef(
                            engine.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Engine,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - �������
            else if (componentType
                == ShipComponentType.Reactor)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� �������
                        DesignerComponentAddTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� �������
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .reactors[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������
                    for (int a = 0; a < reactor.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� �������
                        DesignerComponentAddTechnologyRef(
                            reactor.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.Reactor,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - ��������� ���
            else if (componentType
                == ShipComponentType.HoldFuelTank)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ��������� ���
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ��������� ���
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ��������� ���
                        DesignerComponentAddTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ��������� ���
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .fuelTanks[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ��������� ���
                    for (int a = 0; a < fuelTank.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ��������� ���
                        DesignerComponentAddTechnologyRef(
                            fuelTank.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.HoldFuelTank,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - ������������ ��� ������ ������
            else if (componentType
                == ShipComponentType.ExtractionEquipmentSolid)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ������������ ��� ������ ������
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ������������ ��� ������ ������
                        DesignerComponentAddTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� ������������ ��� ������ ������
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .solidExtractionEquipments[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� ������������ ��� ������ ������
                    for (int a = 0; a < extractionEquipmentSolid.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ������������ ��� ������ ������
                        DesignerComponentAddTechnologyRef(
                            extractionEquipmentSolid.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.ExtractionEquipmentSolid,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
            }
            //�����, ���� ��� ���������� - �������������� ������
            else if (componentType
                == ShipComponentType.GunEnergy)
            {
                //���� ������� ������������� ��������
                if (eUI.Value.designerWindow.isInGameDesigner
                    == true)
                {
                    //���� ������ �� �������������� ������
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������������� ������
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ������������ ��� ������ ������
                        DesignerComponentAddTechnologyRef(
                            energyGun.coreTechnologies[a].ContentObjectLink,
                            ShipComponentType.GunEnergy,
                            componentContentSetIndex,
                            componentIndex);
                    }
                }
                //�����
                else
                {
                    //���� ������ �� �������������� ������
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[componentContentSetIndex]
                        .energyGuns[componentIndex];

                    //��� ������ �������� ����������, �� ������� ��������� �������������� ������
                    for (int a = 0; a < energyGun.coreTechnologies.Length; a++)
                    {
                        //��������� ������ �� ������������ ��� ������ ������
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
            //���� ������� ������������� ��������
            if (eUI.Value.designerWindow.isInGameDesigner
                == true)
            {
                //���� ������ �� ����������
                ref DTechnology technology
                    = ref contentData.Value
                    .contentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //��������� ������� ��������� � ������ ����������� �� ������ ����������
                    technology.engines.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - �������
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //��������� ������� ������� � ������ ����������� �� ������ ����������
                    technology.reactors.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //��������� ������� ��������� ��� � ������ ����������� �� ������ ����������
                    technology.fuelTanks.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //��������� ������� ������������ ��� ������ ������ � ������ ����������� �� ������ ����������
                    technology.extractionEquipmentSolids.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //��������� ������� �������������� ������ � ������ ����������� �� ������ ����������
                    technology.energyGuns.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
            }
            //�����
            else
            {
                //���� ������ �� ����������
                ref WDTechnology technology
                    = ref contentData.Value
                    .wDContentSets[coreTechnologyRef.ContentSetIndex]
                    .technologies[coreTechnologyRef.ObjectIndex];

                //���� ��� ���������� - ���������
                if (componentType
                    == ShipComponentType.Engine)
                {
                    //��������� ������� ��������� � ������ ����������� �� ������ ����������
                    technology.engines.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - �������
                else if (componentType
                    == ShipComponentType.Reactor)
                {
                    //��������� ������� ������� � ������ ����������� �� ������ ����������
                    technology.reactors.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - ��������� ���
                else if (componentType
                    == ShipComponentType.HoldFuelTank)
                {
                    //��������� ������� ��������� ��� � ������ ����������� �� ������ ����������
                    technology.fuelTanks.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - ������������ ��� ������ ������
                else if (componentType
                    == ShipComponentType.ExtractionEquipmentSolid)
                {
                    //��������� ������� ������������ ��� ������ ������ � ������ ����������� �� ������ ����������
                    technology.extractionEquipmentSolids.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
                //�����, ���� ��� ���������� - �������������� ������
                else if (componentType
                    == ShipComponentType.GunEnergy)
                {
                    //��������� ������� �������������� ������ � ������ ����������� �� ������ ����������
                    technology.energyGuns.Add(
                        new(
                            componentContentSetIndex,
                            componentIndex));
                }
            }
        }


        //�������� ����������
        void DesignerOpenEngineComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������ �� ���� ��������� ����������
            UIEngineDesignerWindow engineDesignerWindow
                = eUI.Value.designerWindow.engineDesigner;


            //���������� ������ ���� ��������� ����������
            int engineDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.Engine);

            //���� ������ �� ������ ���� ���������
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[engineDesignerIndex];


            //��������� ���������� ������ ������ ������� ��������
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //���� ������� �� ������������� ��������
            if (designerWindow.isInGameDesigner
                == false)
            {
                //���� ������ �������� ������ �������� �� ����� ���� � �� ����� ������� �������� ������ ��������
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //���������� ������ ���������� �� �������� ������ ��������
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //���������� ������ ���������� �� �������� ������ ��������
            DesignerDisplayContentSetPanelList(
                true);


            //���������� �������� ����������

            //���������� ���������� �������� ��������� �� ������� �������
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesEnginePowerPerSize,
                in factionTechnologiesArray,
                engineDesignerWindow.powerPerSizeCoreTechnologiesList,
                engineDesignerWindow.powerPerSizeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.EnginePowerPerSize);

            //���������� ���������

            //����������� ������ ������� ���������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinEngineSize,
                technologyModifiers.designerMaxEngineSize,
                engineDesignerWindow.sizeParameterPanel);

            //����������� ������ ������� ���������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[1],
                technologyModifiers.designerMinEngineBoost,
                technologyModifiers.designerMaxEngineBoost,
                engineDesignerWindow.boostParameterPanel);


            //������������� � ���������� �������������� ���������
            engineDesignerWindow.CalculateCharacteristics();
        }
        //�������� ���������
        void DesignerOpenReactorComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������ �� ���� ��������� ���������
            UIReactorDesignerWindow reactorDesignerWindow
                = eUI.Value.designerWindow.reactorDesigner;


            //���������� ������ ���� ��������� ���������
            int reactorDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.Reactor);

            //���� ������ �� ������ ���� ���������
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[reactorDesignerIndex];


            //��������� ���������� ������ ������ ������� ��������
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //���� ������� �� ������������� ��������
            if (designerWindow.isInGameDesigner
                == false)
            {
                //���� ������ �������� ������ �������� �� ����� ���� � �� ����� ������� �������� ������ ��������
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //���������� ������ ��������� �� �������� ������ ��������
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //���������� ������ ��������� �� �������� ������ ��������
            DesignerDisplayContentSetPanelList(
                true);


            //���������� �������� ����������

            //���������� ���������� ������� �������� �� ������� �������
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesReactorEnergyPerSize,
                in factionTechnologiesArray,
                reactorDesignerWindow.energyPerSizeCoreTechnologiesList,
                reactorDesignerWindow.energyPerSizeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.ReactorEnergyPerSize);

            //���������� ���������

            //����������� ������ ������� ��������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinReactorSize,
                technologyModifiers.designerMaxReactorSize,
                reactorDesignerWindow.sizeParameterPanel);

            //����������� ������ ������� ��������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[1],
                technologyModifiers.designerMinReactorBoost,
                technologyModifiers.designerMaxReactorBoost,
                reactorDesignerWindow.boostParameterPanel);


            //������������� � ���������� �������������� ��������
            reactorDesignerWindow.CalculateCharacteristics();
        }
        //�������� ��������� �����
        void DesignerOpenFuelTankComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������ �� ���� ��������� ��������� �����
            UIFuelTankDesignerWindow fuelTankDesignerWindow
                = eUI.Value.designerWindow.fuelTankDesigner;


            //���������� ������ ���� ��������� ��������� �����
            int fuelTankDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.HoldFuelTank);

            //���� ������ �� ������ ���� ���������
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[fuelTankDesignerIndex];


            //��������� ���������� ������ ������ ������� ��������
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //���� ������� �� ������������� ��������
            if (designerWindow.isInGameDesigner
                == false)
            {
                //���� ������ �������� ������ �������� �� ����� ���� � �� ����� ������� �������� ������ ��������
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //���������� ������ ��������� ����� �� �������� ������ ��������
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //���������� ������ ��������� ����� �� �������� ������ ��������
            DesignerDisplayContentSetPanelList(
                true);


            //���������� �������� ����������

            //���������� ���������� ������ ���������� ����
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesReactorEnergyPerSize,
                in factionTechnologiesArray,
                fuelTankDesignerWindow.compressionCoreTechnologiesList,
                fuelTankDesignerWindow.compressionCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.FuelTankCompression);

            //���������� ���������

            //����������� ������ ������� ���������� ����
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinFuelTankSize,
                technologyModifiers.designerMaxFuelTankSize,
                fuelTankDesignerWindow.sizeParameterPanel);


            //������������� � ���������� �������������� ���������� ����
            fuelTankDesignerWindow.CalculateCharacteristics();
        }
        //�������� ����������� ������������
        void DesignerOpenExtractionEquipmentComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������ �� ���� ��������� ������������ ��� ������
            UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                = eUI.Value.designerWindow.extractionEquipmentDesigner;


            //���������� ������ ���� ��������� ������������ ��� ������
            int extractionEquipmentDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.ExtractionEquipmentSolid);

            //���� ������ �� ������ ���� ���������
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[extractionEquipmentDesignerIndex];


            //��������� ���������� ������ ������ ������� ��������
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //���� ������� �� ������������� ��������
            if (designerWindow.isInGameDesigner
                == false)
            {
                //���� ������ �������� ������ �������� �� ����� ���� � �� ����� ������� �������� ������ ��������
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //���������� ������ ������������ ��� ������ �� �������� ������ ��������
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //���������� ������ ������������ ��� ������ �� �������� ������ ��������
            DesignerDisplayContentSetPanelList(
                true);


            //���������� �������� ����������

            //���������� ���������� �������� ������ �� ������� �������
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesSolidExtractionEquipmentSpeedPerSize,
                in factionTechnologiesArray,
                extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologiesList,
                extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize);

            //���������� ���������

            //����������� ������ ������� ������������ ��� ������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinSolidExtractionEquipmentSize,
                technologyModifiers.designerMaxSolidExtractionEquipmentSize,
                extractionEquipmentDesignerWindow.sizeParameterPanel);


            //������������� � ���������� �������������� ������������ ��� ������
            extractionEquipmentDesignerWindow.CalculateCharacteristics();
        }
        //�������� �������������� ������
        void DesignerOpenEnergyGunComponentWindow(
            int baseContentSetIndex,
            in DTechnologyModifiers technologyModifiers,
            in Dictionary<int, DOrganizationTechnology>[] factionTechnologiesArray)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������ �� ���� ��������� �������������� ������
            UIGunEnergyDesignerWindow energyGunDesignerWindow
                = eUI.Value.designerWindow.energyGunDesigner;


            //���������� ������ ���� ��������� �������������� ������
            int engineDesignerIndex
                = Array.FindIndex(designerData.Value.componentDesignerTypes,
                x => x.type == ShipComponentType.GunEnergy);

            //���� ������ �� ������ ���� ���������
            ref readonly DComponentDesignerType designerType
                = ref designerData.Value.componentDesignerTypes[engineDesignerIndex];


            //��������� ���������� ������ ������ ������� ��������
            DesignerDisplayOtherContentSetDropdown(
                designerWindow.currentContentSetIndex);

            //���� ������� �� ������������� ��������
            if (designerWindow.isInGameDesigner
                == false)
            {
                //���� ������ �������� ������ �������� �� ����� ���� � �� ����� ������� �������� ������ ��������
                if ((designerWindow.currentContentSetIndex == 0
                    && designerWindow.currentContentSetIndex == baseContentSetIndex)
                    != true)
                {
                    //���������� ������ �������������� ������ �� �������� ������ ��������
                    DesignerDisplayContentSetPanelList(
                        false,
                        baseContentSetIndex);
                }
            }

            //���������� ������ �������������� ������ �� �������� ������ ��������
            DesignerDisplayContentSetPanelList(
                true);


            //���������� �������� ����������

            //���������� ���������� ����������� ��������������� ������ �� ������� �������
            DesignerComponentDisplayFactionCoreTechnologies(
                in contentData.Value.technologiesEnergyGunRecharge,
                in factionTechnologiesArray,
                energyGunDesignerWindow.rechargeCoreTechnologiesList,
                energyGunDesignerWindow.rechargeCoreTechnologyPanel,
                TechnologyComponentCoreModifierType.GunEnergyRecharge);

            //���������� ���������

            //����������� ������ ������� ��������������� ������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[0],
                technologyModifiers.designerMinEnergyGunCaliber,
                technologyModifiers.designerMaxEnergyGunCaliber,
                energyGunDesignerWindow.gunCaliberParameterPanel);

            //����������� ������ ������� ��������������� ������
            designerWindow.ComponentParameterPanelControl(
                in designerType.typeParameters[1],
                technologyModifiers.designerMinEnergyGunBarrelLength,
                technologyModifiers.designerMaxEnergyGunBarrelLength,
                energyGunDesignerWindow.gunBarrelLengthParameterPanel);


            //������������� � ���������� �������������� ��������������� ������
            energyGunDesignerWindow.CalculateCharacteristics();
        }
        #endregion

        #region Game
        void EventCheckGame()
        {
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //��������� ������� �������� �������
            GameCreatePanelRequest();

            //��������� ������� �������� � ����
            GameActionRequest();

            //��������� ������� �������� ��������� � ����
            GameOpenDesignerRequest();

            //��������� ����������� ���������� ������ RAEO
            GameRefreshRAEOObjectPanelSelfRequest();

            //��������� ������� �������� � ������ �������
            GameObjectActionRequest();

            //��������� ������� �������� � ��������� ������
            FleetManagerActionRequest();

            //��������� ����������� ���������� ����������
            GameRefreshUISelfRequest();
        }

        readonly EcsFilterInject<Inc<RGameCreatePanel>> gameCreatePanelRequestFilter = default;
        readonly EcsPoolInject<RGameCreatePanel> gameCreatePanelRequestPool = default;
        void GameCreatePanelRequest()
        {
            //��� ������� ������� �������� ������
            foreach (int requestEntity in gameCreatePanelRequestFilter.Value)
            {
                //���� ������
                ref RGameCreatePanel requestComp = ref gameCreatePanelRequestPool.Value.Get(requestEntity);

                //���� ������������� �������� �������� ������ ORAEO
                if (requestComp.creatingPanelType == CreatingPanelType.ORAEOBriefInfoPanel)
                {
                    //������ �������� ������ ORAEO
                    RegionORAEOsCreateORAEOSummaryPanel(ref requestComp);
                }
                //�����, ���� ������������� �������� �������� ������ �����
                else if (requestComp.creatingPanelType == CreatingPanelType.FleetOverviewPanel)
                {
                    //���� ����
                    requestComp.objectPE.Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //������ �������� ������ �����
                    FleetManagerFleetsCreateFleetSummaryPanel(ref fleet);
                }
                //�����, ���� ������������� �������� �������� ������ ����������� ������
                else if (requestComp.creatingPanelType == CreatingPanelType.TaskForceOverviewPanel)
                {
                    //���� ������
                    requestComp.objectPE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //������ �������� ������ ������
                    FleetManagerFleetsCreateTaskForceSummaryPanel(ref taskForce);
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RGameAction>> gameActionRequestFilter = default;
        readonly EcsPoolInject<RGameAction> gameActionRequestPool = default;
        void GameActionRequest()
        {
            //��� ������� ������� �������� � ����
            foreach (int requestEntity in gameActionRequestFilter.Value)
            {
                //���� ������
                ref RGameAction requestComp = ref gameActionRequestPool.Value.Get(requestEntity);

                //�������� ��������� �����
                GamePause(requestComp.actionType);

                world.Value.DelEntity(requestEntity);
            }
        }

        readonly EcsFilterInject<Inc<RGameOpenDesigner>> gameOpenDesignerRequestFilter = default;
        readonly EcsPoolInject<RGameOpenDesigner> gameOpenDesignerEventPool = default;
        void GameOpenDesignerRequest()
        {
            //��� ������� ������� �������� ���� ���������
            foreach (int requestEntity in gameOpenDesignerRequestFilter.Value)
            {
                //���� ������
                ref RGameOpenDesigner requestComp = ref gameOpenDesignerEventPool.Value.Get(requestEntity);

                //���������� �����
                GamePause(GameActionType.PauseOn);

                //���������� ������ ���� ���������
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
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //��� ������� ����������� ���������� ������ ������� RAEO
            foreach (int selfRequestEntity in refreshRAEOObjectPanelSelfRequestFilter.Value)
            {
                //���� ������� ��������� RAEO
                if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                {
                    //���� ������ � RAEO
                    ref CHexRegion region = ref regionPool.Value.Get(selfRequestEntity);
                    ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(selfRequestEntity);

                    //���� ������� ������ ���� �� RAEO, ��� ����� ����������
                    if (gameWindow.objectPanel.activeObjectPE.EqualsTo(rAEO.selfPE))
                    {
                        //���� ������ RAEO
                        UIRegionSubpanel RAEOSubpanel = gameWindow.objectPanel.regionSubpanel;

                        //���� ������� �������� �������
                        if (RAEOSubpanel.tabGroup.selectedTab == RAEOSubpanel.overviewTab.selfTabButton)
                        {
                            //���������� �������� ������� RAEO
                            RegionShowOverview(
                                ref region,
                                ref rAEO,
                                false);
                        }
                        //�����, ���� ������� ������� ORAEO
                        else if (RAEOSubpanel.tabGroup.selectedTab == RAEOSubpanel.oRAEOsTab.selfTabButton)
                        {
                            //���������� ������� ����������� RAEO
                            RegionShowORAEOs(
                                ref rAEO,
                                false);
                        }
                    }
                }

                //������� � �������� RAEO ����������
                refreshRAEOObjectPanelSelfRequestPool.Value.Del(selfRequestEntity);
            }
        }

        readonly EcsFilterInject<Inc<CBuildingDisplayedSummaryPanel, SRObjectRefreshUI>> buildingRefreshUISelfRequestFilter = default;
        readonly EcsFilterInject<Inc<CTaskForceDisplayedSummaryPanel, SRObjectRefreshUI>> taskForceRefreshUISelfRequestFilter = default;
        readonly EcsFilterInject<Inc<CTFTemplateDisplayedSummaryPanel, SRObjectRefreshUI>> tFTemplateRefreshUISelfRequestFilter = default;
        readonly EcsPoolInject<SRObjectRefreshUI> objectRefreshUISelfRequestPool = default;
        void GameRefreshUISelfRequest()
        {
            //��������� ��������� ����������
            GameRefreshUIBuiliding();

            //��������� ��������� ����������� �����
            GameRefreshUITaskForce();

            //��������� ���������� �������� ����������� �����
            GameRefreshUITFTemplate();
        }

        void GameRefreshUIBuiliding()
        {
            //��� ������� ����������, ������� ������������ ������ � ���������� ���������� ����������
            foreach (int buildingEntity in buildingRefreshUISelfRequestFilter.Value)
            {
                //���� ��������� ������������ ������ � ����������
                ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel = ref buildingDisplayedSummaryPanelPool.Value.Get(buildingEntity);
                ref SRObjectRefreshUI selfRequestComp = ref objectRefreshUISelfRequestPool.Value.Get(buildingEntity);

                //���� ������������� ���������� ����������
                if (selfRequestComp.requestType == RefreshUIType.Refresh)
                {
                    //���� ����������
                    ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

                    //���� ���������� ����� ������������ �������� ������
                    if (buildingDisplayedSummaryPanel.buildingSummaryPanel != null)
                    {
                        //��������� �
                        buildingDisplayedSummaryPanel.buildingSummaryPanel.parentBuildingCategoryPanel.parentBuildingsTab.RefreshBuildingSummaryPanel(
                            ref building, buildingDisplayedSummaryPanel.buildingSummaryPanel);
                    }
                }
                //�����, ���� ������������� �������� ����������
                else if (selfRequestComp.requestType == RefreshUIType.Delete)
                {
                    //���� ���������� ����� ������������ �������� ������
                    if (buildingDisplayedSummaryPanel.buildingSummaryPanel != null)
                    {
                        //������� �
                        buildingDisplayedSummaryPanel.buildingSummaryPanel.parentBuildingCategoryPanel.parentBuildingsTab.CacheBuildingSummaryPanel(
                            ref buildingDisplayedSummaryPanel);
                    }

                    //������� ��������� ������������ �������� ������
                    buildingDisplayedSummaryPanelPool.Value.Del(buildingEntity);
                }

                //������� ��������� ���������� ����������
                objectRefreshUISelfRequestPool.Value.Del(buildingEntity);
            }
        }

        void GameRefreshUITaskForce()
        {
            //��� ������ ����������� ������, ������� ������������ ������ � ���������� ���������� ����������
            foreach (int taskForceEntity in taskForceRefreshUISelfRequestFilter.Value)
            {
                //���� ��������� ������������ ������ � ����������
                ref CTaskForceDisplayedSummaryPanel tFDisplayedSummaryPanel = ref tFDisplayedSummaryPanelPool.Value.Get(taskForceEntity);
                ref SRObjectRefreshUI selfRequestComp = ref objectRefreshUISelfRequestPool.Value.Get(taskForceEntity);

                //���� ������������� ���������� ����������
                if (selfRequestComp.requestType == RefreshUIType.Refresh)
                {
                    //���� ������
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //���� ������ ����� ������������ �������� ������
                    if (tFDisplayedSummaryPanel.taskForceSummaryPanel != null)
                    {
                        //��������� �
                        tFDisplayedSummaryPanel.taskForceSummaryPanel.parentFleetSummaryPanel.parentFleetsTab.RefreshTaskForceSummaryPanel(
                            ref taskForce, tFDisplayedSummaryPanel.taskForceSummaryPanel);
                    }
                }
                //�����, ���� ������������� �������� ����������
                else if(selfRequestComp.requestType == RefreshUIType.Delete)
                {
                    //���� ������ ����� ������������ �������� ������
                    if (tFDisplayedSummaryPanel.taskForceSummaryPanel != null)
                    {
                        //������� �
                        tFDisplayedSummaryPanel.taskForceSummaryPanel.parentFleetSummaryPanel.parentFleetsTab.CacheTaskForceSummaryPanel(
                            ref tFDisplayedSummaryPanel);
                    }

                    //������� ��������� ������������ �������� ������
                    tFDisplayedSummaryPanelPool.Value.Del(taskForceEntity);
                }

                //������� ���������� ���������� ����������
                objectRefreshUISelfRequestPool.Value.Del(taskForceEntity);
            }
        }

        void GameRefreshUITFTemplate()
        {
            //��� ������� ������� ����������� ������, �������� ������������ ������ � ���������� ���������� ����������
            foreach (int templateEntity in tFTemplateRefreshUISelfRequestFilter.Value)
            {
                //���� ��������� ������������ ������ � ����������
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);
                ref SRObjectRefreshUI selfRequestComp = ref objectRefreshUISelfRequestPool.Value.Get(templateEntity);

                //���� ������������� �������� ����������
                if (selfRequestComp.requestType == RefreshUIType.Delete)
                {
                    //���� ������ ������� ������ �� �����
                    if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel != null)
                    {
                        //�������� �
                        eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab
                            .CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                    }

                    //���� ������ ������� �������� �� �����
                    if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel != null)
                    {
                        //�������� �
                        eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab.listSubtab
                            .CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                    }

                    //������� ��������� ������������ �������� ������
                    tFTemplateDisplayedSummaryPanelPool.Value.Del(templateEntity);
                }

                //������� ���������� ���������� ����������
                objectRefreshUISelfRequestPool.Value.Del(templateEntity);
            }
        }

        #region GameObject
        readonly EcsFilterInject<Inc<RGameObjectPanelAction>> gameObjectPanelActionRequestFilter = default;
        readonly EcsPoolInject<RGameObjectPanelAction> gameObjectPanelActionRequestPool = default;
        void GameObjectActionRequest()
        {
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //��� ������� ������� �������� ������ �������
            foreach (int requestEntity in gameObjectPanelActionRequestFilter.Value)
            {
                //���� ������
                ref RGameObjectPanelAction requestComp = ref gameObjectPanelActionRequestPool.Value.Get(requestEntity);

                //���������� ������ �������
                GameShowObjectPanel();

                //���� ������������� ����������� ��������� ��������� ������
                if (requestComp.requestType == ObjectPanelActionRequestType.FleetManager)
                {
                    //���������� ��������� ��������� ������
                    FleetManagerShow(ref requestComp);
                }
                //�����, ���� ������������� ����������� ��������� �����������
                else if (requestComp.requestType == ObjectPanelActionRequestType.Organization)
                {
                    //���������� ��������� �����������
                    OrganizationShow(ref requestComp);
                }
                //�����, ���� ������������� ����������� ��������� �������
                else if (requestComp.requestType == ObjectPanelActionRequestType.Region)
                {
                    //���������� ��������� �������
                    RegionShow(ref requestComp);
                }
                //�����, ���� ������������� ����������� ��������� ORAEO
                else if (requestComp.requestType == ObjectPanelActionRequestType.ORAEO)
                {
                    //���������� ��������� ORAEO
                    ORAEOShow(ref requestComp);
                }
                //�����, ���� ������������� ����������� ��������� ����������
                else if (requestComp.requestType == ObjectPanelActionRequestType.Building)
                {
                    //���������� ������ ����������
                    BuildingShow(ref requestComp);
                }

                //�����, ���� ������� ��������� ��������� ������
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                {
                    //���� ��������� ��������� ������
                    UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

                    //���� ������������� ����������� ������� ������
                    if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerFleets)
                    {
                        //���� �����������
                        requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //���������� ������� ������
                        FleetManagerShowFleets(
                            ref organization,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� ����������� ������� �������� ����������� �����
                    else if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerTaskForceTemplates)
                    {
                        //���� �����������
                        requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //���������� ������� �������� �����
                        FleetManagerShowTFTemplates(
                            ref organization,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� �������� ������ �������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //�������� ��������� ��������� ������
                        FleetManagerHide();

                        //�������� ��������� �������
                        GameHideObjectSubpanel();

                        //�������� ������ �������
                        GameHideObjectPanel();
                    }

                    //�����, ���� ������� ������� ������
                    else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.fleetsTab.selfTabButton)
                    {
                        //���� ������� ������ 
                        UIFleetsTab fleetsTab = fleetManagerSubpanel.fleetsTab;


                    }
                    //�����, ���� ������� ������� �������� �����
                    else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.taskForceTemplatesTab.selfTabButton)
                    {
                        //���� ������� �������� �����
                        UITFTemplatesTab taskForceTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

                        //���� ������������� ����������� ���������� ������ �������� �����
                        if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesList)
                        {
                            //���� �����������
                            requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                            //���������� ���������� ������ �������� �����
                            FleetManagerTFTemplatesShowList(
                                ref organization,
                                requestComp.isRefresh);
                        }
                        //�����, ���� ������������� ����������� ���������� ��������� �������� �����
                        else if (requestComp.requestType == ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner)
                        {
                            //���� �����������
                            requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                            //���������� ���������� ��������� �������� �����
                            FleetManagerTFTemplatesShowDesigner(
                                ref organization,
                                requestComp.isRefresh);
                        }

                        //�����, ���� ������� ���������� ������ �������� �����
                        else if (taskForceTemplatesTab.listSubtab.isActiveAndEnabled == true)
                        {

                        }
                        //�����, ���� ������� ���������� ��������� �������� �����
                        else if (taskForceTemplatesTab.designerSubtab.isActiveAndEnabled == true)
                        {

                        }
                    }
                }
                //�����, ���� ������� ��������� �����������
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
                {
                    //���� ������������� ����������� �������� �������
                    if (requestComp.requestType == ObjectPanelActionRequestType.OrganizationOverview)
                    {
                        //���� �����������
                        requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //���������� �������� ������� �����������

                    }
                    //�����, ���� ������������� �������� ������ �������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //�������� ��������� �����������
                        OrganizationHide();

                        //�������� ��������� �������
                        GameHideObjectSubpanel();

                        //�������� ������ �������
                        GameHideObjectPanel();
                    }
                }
                //�����, ���� ������� ��������� �������
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                {
                    //���� ��������������� ����������� �������� ������� 
                    if (requestComp.requestType == ObjectPanelActionRequestType.RegionOverview)
                    {
                        //���� ������ � RAEO
                        requestComp.objectPE.Unpack(world.Value, out int regionEntity);
                        ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
                        ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

                        //���������� �������� ������� �������
                        RegionShowOverview(
                            ref region,
                            ref rAEO,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� ����������� ������� �����������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.RegionOrganizations)
                    {
                        //���� RAEO
                        requestComp.objectPE.Unpack(world.Value, out int regionEntity);
                        ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

                        //���������� ������� ����������� RAEO
                        RegionShowORAEOs(
                            ref rAEO,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� �������� ������ �������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //�������� ��������� �������
                        RegionHide();

                        //�������� ��������� �������
                        GameHideObjectSubpanel();

                        //�������� ������ �������
                        GameHideObjectPanel();
                    }
                }
                //�����, ���� ������� ��������� ORAEO
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
                {
                    //���� ������������� ����������� �������� �������
                    if (requestComp.requestType == ObjectPanelActionRequestType.ORAEOOverview)
                    {
                        //���� ExORAEO � EcORAEO
                        requestComp.objectPE.Unpack(world.Value, out int oRAEOEntity);
                        ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                        ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                        //���������� �������� ������� ORAEO
                        ORAEOShowOverview(
                            ref exORAEO,
                            ref ecORAEO,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� ����������� ������� ����������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.ORAEOBuildings)
                    {
                        //���� EcORAEO
                        requestComp.objectPE.Unpack(world.Value, out int oRAEOEntity);
                        ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                        //���������� ������� ���������� ORAEO
                        ORAEOShowBuildings(
                            ref ecORAEO,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� �������� ������ �������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //�������� ��������� ORAEO
                        ORAEOHide();

                        //�������� ��������� �������
                        GameHideObjectSubpanel();

                        //�������� ������ �������
                        GameHideObjectPanel();
                    }
                }
                //�����, ���� ������� ��������� ����������
                else if (gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Building)
                {
                    //���� ������������� ����������� �������� �������
                    if (requestComp.requestType == ObjectPanelActionRequestType.BuildingOverview)
                    {
                        //���� ����������
                        requestComp.objectPE.Unpack(world.Value, out int buildingEntity);
                        ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

                        //���������� �������� ������� ����������
                        BuildingShowOverview(
                            ref building,
                            requestComp.isRefresh);
                    }
                    //�����, ���� ������������� �������� ������ �������
                    else if (requestComp.requestType == ObjectPanelActionRequestType.CloseObjectPanel)
                    {
                        //�������� ��������� ����������
                        BuildingHide();

                        //�������� ��������� �������
                        GameHideObjectSubpanel();

                        //�������� ������ �������
                        GameHideObjectPanel();
                    }
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        void GameShowObjectPanel()
        {
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //���� �����-���� ������� ������ �������
            if (gameWindow.activeMainPanelType != MainPanelType.None)
            {
                //�������� �
                gameWindow.activeMainPanel.gameObject.SetActive(false);
            }

            //������ ������ ������� ��������
            gameWindow.objectPanel.gameObject.SetActive(true);

            //��������� � ��� �������� ������� ������
            gameWindow.activeMainPanelType = MainPanelType.Object;
            gameWindow.activeMainPanel = gameWindow.objectPanel.gameObject;
        }

        void GameHideObjectPanel()
        {
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //�������� ������ �������
            gameWindow.objectPanel.gameObject.SetActive(false);

            //���������, ��� ��� �������� ������� ������
            gameWindow.activeMainPanelType = MainPanelType.None;
            gameWindow.activeMainPanel = null;
        }

        void GameHideObjectSubpanel()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //�������� �������� ���������
            objectPanel.activeObjectSubpanel.gameObject.SetActive(false);

            //���������, ��� ��� �������� ���������
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.None;
            objectPanel.activeObjectSubpanel = null;
            //������� PE ��������� �������
            objectPanel.activeObjectPE = new();
        }

        #region GameObjectFleetManager
        readonly EcsFilterInject<Inc<RGameFleetManagerSubpanelAction>> gameFleetManagerSubpanelActionRequestFilter = default;
        readonly EcsPoolInject<RGameFleetManagerSubpanelAction> gameFleetManagerSubpanelActionRequestPool = default;
        void FleetManagerActionRequest()
        {
            //��� ������� ������� �������� ��������� ������
            foreach (int requestEntity in gameFleetManagerSubpanelActionRequestFilter.Value)
            {
                //���� ��������� ��������� ������
                UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

                //���� ������
                ref RGameFleetManagerSubpanelAction requestComp = ref gameFleetManagerSubpanelActionRequestPool.Value.Get(requestEntity);

                //���� �����������
                requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                //���� ������������� ���������� ������ �������� ��� ����� ������� ������
                if (requestComp.requestType == FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesChangeList)
                {
                    //���� ������
                    requestComp.taskForcePE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //��������� ������ �������� ��� ����� ������� ������
                    FleetManagerFleetsFillTFTemplatesChangingList(
                        ref organization,
                        ref taskForce);
                }
                //�����, ���� ������������� ���������� ������ �������� ��� �������� ����� ������
                else if(requestComp.requestType == FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesNewList)
                {
                    //��������� ������ �������� ��� �������� ����� ������
                    FleetManagerFleetsFillTFTemplatesCreatingList(ref organization);
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        void FleetManagerShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //���� ��������� �����������
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� �����-���� ��������� �������, �������� �
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //������ ��������� ��������� ������ ��������
            objectPanel.fleetManagerSubpanel.gameObject.SetActive(true);

            //��������� � ��� �������� ���������
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.FleetManager;
            objectPanel.activeObjectSubpanel = objectPanel.fleetManagerSubpanel;
            //���������, �������� ������ ����� ����������� ���������� ���������
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;


            //����������, ��� ��� �������� ������
            objectPanel.objectName.text = "Fleet Manager";

            //���������� ������� ������
            FleetManagerShowFleets(
                ref organization,
                false);
        }

        void FleetManagerHide()
        {
            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

            //������� PE ��������� �����
            InputData.activeFleetPE = new();

            //������� PE �������� ����������� �����
            InputData.activeTaskForcePEs.Clear();

            //�������� ������ �������� ����� �� ������� ������
            fleetManagerSubpanel.fleetsTab.HideTFTemplatesChangingList();
        }

        void FleetManagerShowFleets(
            ref COrganization organization,
            bool isRefresh)
        {
            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

            //���� ������� ������
            UIFleetsTab fleetsTab = fleetManagerSubpanel.fleetsTab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� ������� ������
                fleetManagerSubpanel.tabGroup.OnTabSelected(fleetsTab.selfTabButton);

                //��� ������ ����������� ������, ������� �������� ������
                foreach (int taskForceEntity in tFDisplayedSummaryPanelFilter.Value)
                {
                    //���� ��������� ������������ �������� ������
                    ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel = ref tFDisplayedSummaryPanelPool.Value.Get(taskForceEntity);

                    //�������� �������� ������
                    fleetsTab.CacheTaskForceSummaryPanel(ref taskForceDisplayedSummaryPanel);

                    //������� ��������� � ��������
                    tFDisplayedSummaryPanelPool.Value.Del(taskForceEntity);
                }

                //��� ������� �����, �������� �������� ������
                foreach (int fleetEntity in fleetDisplayedSummaryPanelFilter.Value)
                {
                    //���� ��������� ������������ �������� ������
                    ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Get(fleetEntity);

                    //�������� �������� ������
                    fleetsTab.CacheFleetSummaryPanel(ref fleetDisplayedSummaryPanel);

                    //������� ��������� � ��������
                    fleetDisplayedSummaryPanelPool.Value.Del(fleetEntity);
                }

                //��� ������� ����� �����������
                for (int a = 0; a < organization.ownedFleets.Count; a++)
                {
                    //���� ����
                    organization.ownedFleets[a].Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //������ �������� ������ �����
                    FleetManagerFleetsCreateFleetSummaryPanel(ref fleet);
                }

                //�������� ������ ��� �������� ������
                fleetsTab.HideTFTemplatesCreatingList();

                //�������� ������ ��� ����� �������
                fleetsTab.HideTFTemplatesChangingList();
            }
        }

        void FleetManagerFleetsCreateFleetSummaryPanel(
            ref CFleet fleet)
        {
            //���� �������� �����
            fleet.selfPE.Unpack(world.Value, out int fleetEntity);

            //��������� ����� ��������� ������������ �������� ������
            ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Add(fleetEntity);

            //���� ������� ������
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //������ ������
            fleetsTab.InstantiateFleetSummaryPanel(ref fleet, ref fleetDisplayedSummaryPanel);

            //��� ������ ����������� ������ �����
            for (int a = 0; a < fleet.ownedTaskForcePEs.Count; a++)
            {
                //���� ������
                fleet.ownedTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                //������ �������� ������ ������
                FleetManagerFleetsCreateTaskForceSummaryPanel(ref taskForce);
            }
        }

        void FleetManagerFleetsCreateTaskForceSummaryPanel(
            ref CTaskForce taskForce)
        {
            //���� ��������� ������������ �������� ������ �����-���������
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Get(fleetEntity);

            //���� �������� ����������� ������
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);

            //��������� ������ ��������� ������������ �������� ������
            ref CTaskForceDisplayedSummaryPanel taskForceDisplayedOverviewPanel = ref tFDisplayedSummaryPanelPool.Value.Add(taskForceEntity);

            //���� ������� ������
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //������ ������
            fleetsTab.InstantiateTaskForceSummaryPanel(
                fleetDisplayedSummaryPanel.fleetSummaryPanel,
                ref taskForce, ref taskForceDisplayedOverviewPanel);
        }

        void FleetManagerFleetsFillTFTemplatesChangingList(
            ref COrganization organization,
            ref CTaskForce taskForce)
        {
            //���� ������� ������ ��������� ������
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //���� ������ ��� ����� �������
            UITFTemplateSummaryPanelsList templatesList = fleetsTab.templatesChangingList;

            //������� ������ �������
            templatesList.templatePanels.Clear();

            //��� ������� ������� ������, �������� �������� ������
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //���� ��������� ������������ ������
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //���� ������ ������� ������� �� �����
                if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel != null)
                {
                    //�������� �������� ������ ������� �������
                    fleetsTab.CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                }

                //���� ������ ������ ������� �����
                if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel == null)
                {
                    //������� ���������
                    world.Value.DelEntity(templateEntity);
                }
            }

            //��� ������� ������� ������ �����������
            for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
            {
                //���� ������
                DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                //���� ��� �� ��� ������, ��� ������ ��� �����
                if(taskForce.template != template)
                {
                    //������ �������� ������ ������� ������
                    FleetManagerFleetsCreateTFTemplateSummaryPanel(
                        template,
                        templatesList);
                }
            }

            //���� ���������� �������� ������� �������� � ������ �������� ������ ����
            if (templatesList.templatePanels.Count < 5)
            {
                //������������� ������ ������ �������������� ���������� �������
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    templatesList.templatePanels.Count * 40);
            }
            //�����, ���� ���������� ������ ��� ����� ����
            else if (templatesList.templatePanels.Count >= 5)
            {
                //������������� ������ ������ ��� ��� ���� ���������
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    5 * 40);
            }
            //�����
            else
            {
                //������������� ������ ������ ��� ��� ����� ��������
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    1 * 40);
            }
        }

        void FleetManagerFleetsFillTFTemplatesCreatingList(
            ref COrganization organization)
        {
            //���� ������� ������ ��������� ������
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //���� ������ ��� �������� ������
            UITFTemplateSummaryPanelsList templatesList = fleetsTab.templatesCreatingList;

            //������� ������ �������
            templatesList.templatePanels.Clear();

            //��� ������� ������� ������, �������� �������� ������
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //���� ��������� ������������ ������
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //���� ������ ������� ������� �� �����
                if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel != null)
                {
                    //�������� �������� ������ ������� �������
                    fleetsTab.CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                }

                //���� ������ ������ ������� �����
                if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel == null)
                {
                    //������� ���������
                    world.Value.DelEntity(templateEntity);
                }
            }

            //��� ������� ������� ������ �����������
            for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
            {
                //���� ������
                DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                //������ �������� ������ ������� ������
                FleetManagerFleetsCreateTFTemplateSummaryPanel(
                    template,
                    templatesList);
            }

            //���� ���������� �������� ������� �������� � ������ �������� ������ ����
            if (templatesList.templatePanels.Count < 5)
            {
                //������������� ������ ������ �������������� ���������� �������
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    templatesList.templatePanels.Count * 40);
            }
            //�����, ���� ���������� ������ ��� ����� ����
            else if (templatesList.templatePanels.Count >= 5)
            {
                //������������� ������ ������ ��� ��� ���� ���������
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    5 * 40);
            }
            //�����
            else
            {
                //������������� ������ ������ ��� ��� ����� ��������
                templatesList.selfRectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    1 * 40);
            }
        }

        void FleetManagerFleetsCreateTFTemplateSummaryPanel(
            DTFTemplate template,
            UITFTemplateSummaryPanelsList templatesList)
        {
            //����������, �� ���������� �� ��� ������ ��� ������� �������
            bool isPanelExist = false;
            int existTemplateEntity = -1;

            //��� ������ ������������ �������� ������ �������
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //���� ���������
                ref CTFTemplateDisplayedSummaryPanel existTemplateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //���� ������ ��������� ���������� ����������� ������
                if (existTemplateDisplayedSummaryPanel.CheckTemplate(template))
                {
                    //��������, ��� ������ ����������
                    isPanelExist = true;

                    //��������� �������� 
                    existTemplateEntity = templateEntity;

                    break;
                }
            }

            //���� ������� ������
            UIFleetsTab fleetsTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.fleetsTab;

            //���� ������ ����������
            if (isPanelExist == true)
            {
                //���� ��������� ������������ ������
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(existTemplateEntity);

                //������ ������
                fleetsTab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel,
                    templatesList);
            }
            //�����
            else
            {
                //������ ����� �������� � ��������� �� ��������� ������������ �������� ������ ������� ����������� ������ �� ������� ������
                int templateEntity = world.Value.NewEntity();
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Add(templateEntity);

                //��������� ������ ����������
                templateDisplayedSummaryPanel = new(world.Value.PackEntity(templateEntity));

                //������ ������
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
            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel;

            //���� ������� �������� ����������� �����
            UITFTemplatesTab taskForceTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� ������� �������� �����
                fleetManagerSubpanel.tabGroup.OnTabSelected(taskForceTemplatesTab.selfTabButton);

                //���������� ������ �����
                FleetManagerTFTemplatesShowList(
                    ref organization,
                    isRefresh);
            }
        }

        void FleetManagerTFTemplatesShowList(
            ref COrganization organization,
            bool isRefresh)
        {
            //���� ������� �������� ����������� �����
            UITFTemplatesTab templatesTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab;

            //���� ���������� ������ �������� �����
            UIListSubtab listSubtab = templatesTab.listSubtab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���� �������� �������� ����� ������������, �������� ���
                if (templatesTab.designerSubtab.isActiveAndEnabled == true)
                {
                    templatesTab.designerSubtab.gameObject.SetActive(false);
                }

                //���������� ���������� ������ �������� �����
                listSubtab.gameObject.SetActive(true);

                //��� ������� ������� ������, �������� �������� ������
                foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
                {
                    //���� ��������� ������������ �������� ������
                    ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                    //���� ������ ������� ������� �� �����
                    if (templateDisplayedSummaryPanel.tFTemplatesTemplateSummaryPanel != null)
                    {
                        //�������� �������� ������� �������
                        listSubtab.CacheTFTemplateSummaryPanel(ref templateDisplayedSummaryPanel);
                    }

                    //���� ������ ������ ������� �����
                    if (templateDisplayedSummaryPanel.fleetsTemplateSummaryPanel == null)
                    {
                        //������� ���������
                        world.Value.DelEntity(templateEntity);
                    }
                }

                //��� ������� ������� ������ �����������
                for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
                {
                    //���� ������
                    DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                    //������ �������� ������ ������� ������
                    FleetManagerTFTemplatesListCreateTFTemplateSummaryPanel(
                        template);
                }
            }
        }

        void FleetManagerTFTemplatesListCreateTFTemplateSummaryPanel(
            DTFTemplate template)
        {
            //����������, �� ���������� �� ��� ������ ��� ������� �������
            bool isPanelExist = false;
            int existTemplateEntity = -1;

            //��� ������ ������������ �������� ������ �������
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //���� ���������
                ref CTFTemplateDisplayedSummaryPanel existTemplateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //���� ������ ��������� ���������� ����������� ������
                if (existTemplateDisplayedSummaryPanel.CheckTemplate(template))
                {
                    //��������, ��� ������ ����������
                    isPanelExist = true;

                    //��������� �������� 
                    existTemplateEntity = templateEntity;

                    break;
                }
            }

            //���� ���������� ������ �������� �����
            UIListSubtab listSubtab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab.listSubtab;

            //���� ������ ����������
            if (isPanelExist == true)
            {
                //���� ��������� ������������ ������
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(existTemplateEntity);

                //������ ������
                listSubtab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel);
            }
            //�����
            else
            {
                //������ ����� �������� � ��������� �� ��������� ������������ �������� ������ ������� ����������� ������ �� ������� ������
                int templateEntity = world.Value.NewEntity();
                ref CTFTemplateDisplayedSummaryPanel templateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Add(templateEntity);


                //��������� ������ ����������
                templateDisplayedSummaryPanel = new(world.Value.PackEntity(templateEntity));

                //������ ������
                listSubtab.InstantiateTFTemplateSummaryPanel(
                    template,
                    ref templateDisplayedSummaryPanel);
            }
        }

        void FleetManagerTFTemplatesShowDesigner(
            ref COrganization organization,
            bool isRefresh)
        {
            //���� ������� �������� ����������� �����
            UITFTemplatesTab taskForceTemplatesTab = eUI.Value.gameWindow.objectPanel.fleetManagerSubpanel.taskForceTemplatesTab;

            //���� ���������� ��������� �������� �����
            UIDesignerSubtab designerSubtab = taskForceTemplatesTab.designerSubtab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���� ������ �������� ����� ������������, �������� ���
                if (taskForceTemplatesTab.listSubtab.isActiveAndEnabled == true)
                {
                    taskForceTemplatesTab.listSubtab.gameObject.SetActive(false);
                }

                //����
                //���� ������ ������ �� ����������
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
                //����

                //���������� ���������� ��������� �������� �����
                designerSubtab.gameObject.SetActive(true);

                //������� ����������, ��������� ������ ������
                designerSubtab.ClearTaskForceTemplate();

                //���� ������������� ����������� ������� �������
                if (designerSubtab.template == null)
                {
                    //���������� �������� ������� ������ �� ��������� - "Task Force Template + organization.taskForceCount"
                    designerSubtab.tFTemplateName.text = "Task Force Template";
                }
                //�����
                else
                {
                    //���� ����������� ������ �� ������ �����������
                    DTFTemplate template = designerSubtab.template;

                    //���������� �������� ������� ������
                    designerSubtab.tFTemplateName.text = template.selfName;

                    //��� ������� ���� ������� � �������
                    for (int a = 0; a < template.shipTypes.Length; a++)
                    {
                        //���� ��� �������
                        ref DShipType shipType = ref template.shipTypes[a].shipType;

                        //���� ��� ������� ��������� � ������ ����� ���������
                        if (shipType.BattleGroup == TaskForceBattleGroup.ShortRange)
                        {
                            //��������� ��� ������� 
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
            //���� ��������� ������� � RAEO
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);
            ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� �����-���� ��������� �������, �������� �
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //������ ��������� ������� �������� 
            objectPanel.regionSubpanel.gameObject.SetActive(true);

            //��������� � ��� �������� ���������
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.Region;
            objectPanel.activeObjectSubpanel = objectPanel.regionSubpanel;
            //���������, ����� ������ ���������� ���������
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //���� ��������� �������
            UIRegionSubpanel regionSubpanel = objectPanel.regionSubpanel;


            //���������� �������� �������
            objectPanel.objectName.text = region.centerPoint.ToString();

            //���������� �������� ������� �������
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
            //���� ��������� �������
            UIRegionSubpanel regionSubpanel = eUI.Value.gameWindow.objectPanel.regionSubpanel;

            //���� �������� �������
            GameWindow.Object.Region.UIOverviewTab overviewTab = regionSubpanel.overviewTab;

            //���� ��������� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //���� ��������� ORAEO �����������-���������
            rAEO.organizationRAEOs[playerOrganization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int ownerORAEOEntity);
            ref CExplorationORAEO ownerExORAEO = ref explorationORAEOPool.Value.Get(ownerORAEOEntity);

            //���������� ������� ������������ ������� ������������-����������
            overviewTab.explorationLevel.text = region.Index.ToString();//= ownerExORAEO.explorationLevel.ToString();

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� �������� �������
                regionSubpanel.tabGroup.OnTabSelected(overviewTab.selfTabButton);

                //���� ����������� ������ ����� EcORAEO
                if (rAEO.organizationRAEOs[playerOrganization.selfIndex].organizationRAEOType == ORAEOType.Economic)
                {
                    //�������� ������ �����������
                    overviewTab.colonizeButtonTest.gameObject.SetActive(false);
                }
                //�����
                else
                {
                    //���������� ������ �����������
                    overviewTab.colonizeButtonTest.gameObject.SetActive(true);
                }

            }
        }

        void RegionShowORAEOs(
            ref CRegionAEO regionRAEO,
            bool isRefresh)
        {
            //���� ��������� �������
            UIRegionSubpanel regionSubpanel = eUI.Value.gameWindow.objectPanel.regionSubpanel;

            //���� ������� ORAEO
            UIORAEOsTab oRAEOsTab = regionSubpanel.oRAEOsTab;

            //��� ������� ORAEO � RAEO
            for (int a = 0; a < regionRAEO.organizationRAEOs.Length; a++)
            {
                //���� ������������ �� ����������
                if (isRefresh == false)
                {
                    //���� ������ ORAEO 
                    UIORAEOSummaryPanel briefInfoPanel = oRAEOsTab.panelsList[a];

                    //���� � ����������� ���� EcORAEO
                    if (regionRAEO.organizationRAEOs[a].organizationRAEOType == ORAEOType.Economic)
                    {
                        //���������� ������
                        briefInfoPanel.gameObject.SetActive(true);

                        //���� ��������� ExORAEO
                        regionRAEO.organizationRAEOs[a].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                        ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                        ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

                        //���� ��������� �����������
                        exORAEO.organizationPE.Unpack(world.Value, out int organizationEntity);
                        ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                        //��������� PE ORAEO
                        briefInfoPanel.selfPE = exORAEO.selfPE;
                    }
                    //�����
                    else
                    {
                        //�������� ������
                        briefInfoPanel.gameObject.SetActive(false);
                    }
                }
            }

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� ������� �����������
                regionSubpanel.tabGroup.OnTabSelected(oRAEOsTab.selfTabButton);
            }
        }

        void RegionORAEOsCreateORAEOSummaryPanel(
            ref RGameCreatePanel requestComp)
        {
            //���� �����������
            requestComp.objectPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //���� ������� ORAEO ��������� �������
            UIORAEOsTab organizationsTab = eUI.Value.gameWindow.objectPanel.regionSubpanel.oRAEOsTab;

            //������ �������� ������
            UIORAEOSummaryPanel oRAEOSummaryPanel = organizationsTab.InstantiateORAEOSummaryPanel(ref organization);

            //�������� ������
            oRAEOSummaryPanel.gameObject.SetActive(false);
        }
        #endregion

        #region GameObjectORAEO
        void ORAEOShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //���� ExORAEO � EcORAEO
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int oRAEOEntity);
            ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
            ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� �����-���� ��������� �������, �������� �
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //������ ��������� ORAEO �������� 
            objectPanel.oRAEOSubpanel.gameObject.SetActive(true);

            //��������� � ��� �������� ���������
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.ORAEO;
            objectPanel.activeObjectSubpanel = objectPanel.oRAEOSubpanel;
            //���������, ����� ORAEO ���������� ���������
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //���� ��������� ORAEO
            UIORAEOSubpanel oRAEOSubpanel = objectPanel.oRAEOSubpanel;


            //���������� �������� ORAEO
            objectPanel.objectName.text = exORAEO.selfPE.ToString();

            //���������� �������� ������� ORAEO
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
            //���� ��������� ORAEO
            UIORAEOSubpanel oRAEOSubpanel = eUI.Value.gameWindow.objectPanel.oRAEOSubpanel;

            //���� �������� �������
            GameWindow.Object.ORAEO.UIOverviewTab overviewTab = oRAEOSubpanel.overviewTab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� �������� �������
                oRAEOSubpanel.tabGroup.OnTabSelected(overviewTab.selfTabButton);
            }

            overviewTab.testText.text = "! ! !";
        }

        void ORAEOShowBuildings(
            ref CEconomicORAEO ecORAEO,
            bool isRefresh)
        {
            //���� ��������� ORAEO
            UIORAEOSubpanel oRAEOSubpanel = eUI.Value.gameWindow.objectPanel.oRAEOSubpanel;

            //���� ������� ����������
            UIBuildingsTab buildingsTab = oRAEOSubpanel.buildingsTab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� ������� ����������
                oRAEOSubpanel.tabGroup.OnTabSelected(buildingsTab.selfTabButton);

                //��� ������� ����������, �������� �������� ������
                foreach (int buildingEntity in buildingDisplayedSummaryPanelFilter.Value)
                {
                    //���� ��������� ������������ �������� ������
                    ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel = ref buildingDisplayedSummaryPanelPool.Value.Get(buildingEntity);

                    //�������� �������� ������
                    buildingsTab.CacheBuildingSummaryPanel(ref buildingDisplayedSummaryPanel);

                    //������� ��������� � ��������
                    buildingDisplayedSummaryPanelPool.Value.Del(buildingEntity);
                }

                //��� ������� ���������� EcORAEO
                for (int a = 0; a < ecORAEO.ownedBuildings.Count; a++)
                {
                    //���� ����������
                    ecORAEO.ownedBuildings[a].buildingPE.Unpack(world.Value, out int buildingEntity);
                    ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

                    //������ �������� ������ ����������
                    ORAEOBuildingsCreateBuildingSummaryPanel(ref building);
                }
            }
        }

        void ORAEOBuildingsCreateBuildingSummaryPanel(
            ref CBuilding building)
        {
            //���� �������� ����������
            building.selfPE.Unpack(world.Value, out int buildingEntity);

            //��������� ���������� ��������� ������������ �������� ������
            ref CBuildingDisplayedSummaryPanel buildingDisplayedSummaryPanel = ref buildingDisplayedSummaryPanelPool.Value.Add(buildingEntity);

            //���� ������� ����������
            UIBuildingsTab buildingsTab = eUI.Value.gameWindow.objectPanel.oRAEOSubpanel.buildingsTab;

            //������ ������
            buildingsTab.InstantiateBuildingSummaryPanel(ref building, ref buildingDisplayedSummaryPanel);
        }
        #endregion

        #region GameObjectBuilding
        void BuildingShow(
            ref RGameObjectPanelAction gameObjectPanelActionRequest)
        {
            //���� ����������
            gameObjectPanelActionRequest.objectPE.Unpack(world.Value, out int buildingEntity);
            ref CBuilding building = ref buildingPool.Value.Get(buildingEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� �����-���� ��������� �������, �������� �
            if (objectPanel.activeObjectSubpanelType != ObjectSubpanelType.None)
            {
                objectPanel.activeObjectSubpanel.gameObject.SetActive(false);
            }

            //������ ��������� ���������� ��������
            objectPanel.buildingSubpanel.gameObject.SetActive(true);

            //��������� � ��� �������� ���������
            objectPanel.activeObjectSubpanelType = ObjectSubpanelType.Building;
            objectPanel.activeObjectSubpanel = objectPanel.buildingSubpanel;
            //���������, ����� ���������� ���������� ������
            objectPanel.activeObjectPE = gameObjectPanelActionRequest.objectPE;

            //���� ��������� ����������
            UIBuildingSubpanel buildingSubpanel = objectPanel.buildingSubpanel;


            //���������� �������� ����������
            objectPanel.objectName.text = building.buildingType.ObjectName;

            //���������� �������� ������� ����������
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
            //���� ��������� ����������
            UIBuildingSubpanel buildingSubpanel = eUI.Value.gameWindow.objectPanel.buildingSubpanel;

            //���� �������� �������
            GameWindow.Object.Building.UIOverviewTab overviewTab = buildingSubpanel.overviewTab;

            //���� ������������ �� ����������
            if (isRefresh == false)
            {
                //���������� �������� �������
                buildingSubpanel.tabGroup.OnTabSelected(overviewTab.selfTabButton);
            }

            
        }
        #endregion
        #endregion

        void GameOpenWindow()
        {
            //��������� �������� ������� ����
            CloseMainWindow();

            //���� ������ �� ���� ����
            UIGameWindow gameWindow
                = eUI.Value.gameWindow;

            //������ ���� ���� ��������
            gameWindow.gameObject.SetActive(true);
            //� ��������� ��� ��� �������� ���� � EUI
            eUI.Value.activeMainWindow
                = eUI.Value.gameWindow.gameObject;

            //���������, ��� ������� ���� ����
            eUI.Value.activeMainWindowType
                = MainWindowType.Game;
        }

        void GamePause(
            GameActionType pauseMode)
        {
            //���� ��������� �������� �����
            if (pauseMode
                == GameActionType.PauseOn)
            {
                //���������, ��� ���� ���������
                runtimeData.Value.isGameActive
                    = false;
            }
            //�����
            else if (pauseMode
                == GameActionType.PauseOff)
            {
                //���������, ��� ���� �������
                runtimeData.Value.isGameActive
                    = true;
            }
        }
        #endregion

        void SaveContentSetRequest(
            int contentSetIndex)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ���������� ������ ��������
            int requestEntity = world.Value.NewEntity();
            ref RSaveContentSet saveContentSetRequest = ref saveContentSetRequestPool.Value.Add(requestEntity);

            //��������� ������ ������ ��������
            saveContentSetRequest.contentSetIndex = contentSetIndex;
        }

        void EcsGroupSystemStateEvent(
            string systemGroupName,
            bool systemGroupState)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ��������� ������ ������
            int eventEntity = world.Value.NewEntity();
            ref EcsGroupSystemState groupSystemStateEvent = ref ecsGroupSystemStatePool.Value.Add(eventEntity);

            //��������� �������� ������ ������
            groupSystemStateEvent.Name = systemGroupName;
            //��������� ��������� ������ ������
            groupSystemStateEvent.State = systemGroupState;
        }
    }
}