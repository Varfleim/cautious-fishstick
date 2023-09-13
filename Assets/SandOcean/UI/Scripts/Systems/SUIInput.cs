
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
        //����
        readonly EcsWorldInject world = default;
        EcsWorld uguiSOWorld;
        EcsWorld uguiUIWorld;


        //������
        //readonly EcsPoolInject<CPlayer> playerPool = default;

        //�����
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        //�����
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsPoolInject<CFleetDisplayedSummaryPanel> fleetDisplayedSummaryPanelPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;

        readonly EcsPoolInject<CTaskForcePMissionSearch> taskForcePatrolMissionSearchPool = default;
        readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> taskForceTargetMissionStrikeGroupPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceMovement>> taskForceMovementFilter = default;
        readonly EcsPoolInject<CTaskForceMovement> taskForceMovementPool = default;


        //������� ���������������-������������� ��������
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        //������� ������
        readonly EcsPoolInject<RFleetCreating> fleetCreatingRequestPool = default;

        readonly EcsPoolInject<RTaskForceCreating> taskForceCreatingRequestPool = default;
        readonly EcsPoolInject<RTaskForceAction> taskForceActionRequestPool = default;

        readonly EcsPoolInject<RTFTemplateCreating> tFTemplateCreatingRequestPool = default;

        readonly EcsPoolInject<RTFTemplateAction> tFTemplateActionRequestPool = default;

        //������� �����


        //����� �������
        EcsFilter clickEventSpaceFilter;
        EcsPool<EcsUguiClickEvent> clickEventSpacePool;
        
        EcsFilter clickEventUIFilter;
        EcsPool<EcsUguiClickEvent> clickEventUIPool;

        EcsFilter dropdownEventUIFilter;
        EcsPool<EcsUguiTmpDropdownChangeEvent> dropdownEventUIPool;

        EcsFilter sliderEventUIFilter;
        EcsPool<EcsUguiSliderChangeEvent> sliderEventUIPool;

        readonly EcsPoolInject<RQuitGame> quitGameRequestPool = default;


        //������
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
            //���������� ��������� ������
            CheckButtons();

            //��� ������� ������� ����� �� ����������
            foreach (int clickEventUIEntity in clickEventUIFilter)
            {
                //���� �������
                ref EcsUguiClickEvent clickEvent = ref clickEventUIPool.Get(clickEventUIEntity);

                Debug.LogWarning("Click! " + clickEvent.WidgetName);

                //���� ������� ���� ����
                if (eUI.Value.activeMainWindowType == MainWindowType.Game)
                {
                    //��������� ����� � ���� ����
                    ClickActionGame(ref clickEvent);
                }
                //�����, ���� ������� ���� �������� ����
                else if (eUI.Value.activeMainWindowType == MainWindowType.MainMenu)
                {
                    //��������� ����� � ���� �������� ����
                    ClickActionMainMenu(ref clickEvent);
                }
                //���� ������� ���� ����� ����
                else if (eUI.Value.activeMainWindowType == MainWindowType.NewGameMenu)
                {
                    //��������� ����� � ���� ���� ����� ����
                    ClickActionNewGameMenu(ref clickEvent);
                }
                //���� ������� ����������
                else if (eUI.Value.activeMainWindowType == MainWindowType.Workshop)
                {
                    //��������� ����� � ���� ����������
                    ClickActionWorkshop(ref clickEvent);
                }
                //���� ������� ��������
                else if (eUI.Value.activeMainWindowType == MainWindowType.Designer)
                {
                    //��������� ����� � ���� ���������
                    ClickActionDesigner(ref clickEvent);
                }

                uguiUIWorld.DelEntity(clickEventUIEntity);
            }

            //��� ������� ������� ��������� �������� ����������� ������
            foreach (int dropdownEventUIEntity in dropdownEventUIFilter)
            {
                //���� ��������� �������
                ref EcsUguiTmpDropdownChangeEvent dropdownEvent = ref dropdownEventUIPool.Get(dropdownEventUIEntity);

                Debug.LogWarning("Dropdown change! " + dropdownEvent.WidgetName + " ! " + dropdownEvent.Value);

                //���� ������� ��������
                if (eUI.Value.activeMainWindowType == MainWindowType.Designer)
                {
                    //���� ������ �� ���� ���������
                    UIDesignerWindow designerWindow = eUI.Value.designerWindow;

                    //���� ������� ����������� ����� ������ �������� �� ������ ������ ������� ��������
                    if (dropdownEvent.WidgetName == "ChangeOtherContentSet")
                    {
                        //���������� �������� ������ ������ ��������
                        int contentSetIndex = -1;

                        //���� ������ ����������� ������ ������, ��� ������ �������� ������ ��������
                        if (dropdownEvent.Value < designerWindow.currentContentSetIndex)
                        {
                            //�� ��������� ������ ������������� ������� �� ������
                            contentSetIndex = dropdownEvent.Value;
                        }
                        //�����
                        else
                        {
                            //��������� ������ ������ �� 1
                            contentSetIndex = dropdownEvent.Value + 1;
                        }

                        //����������� ����� ������ �������� �� ������ ������ ������� ��������
                        DesignerActionRequest(
                            DesignerActionType.DisplayContentSetPanelList,
                            false,
                            contentSetIndex);
                    }
                    //�����, ���� ������� �������� ��������
                    else if (designerWindow.designerType == DesignerType.ShipClass)
                    {
                        //���� ������ �� ���� ��������� ��������
                        UIShipClassDesignerWindow shipDesignerWindow
                            = designerWindow.shipClassDesigner;

                        //���� ������� ����������� ����� ���� ��������� �����������
                        if (dropdownEvent.WidgetName
                            == "ChangeAvailableComponentsType")
                        {
                            //����������� ��������� ���� ��������� �����������
                            DesignerShipClassActionRequest(
                                DesignerShipClassActionType.ChangeAvailableComponentsType,
                                (ShipComponentType)shipDesignerWindow.availableComponentTypeDropdown.value);
                        }
                    }
                    //�����, ���� ������� �������� ����������
                    else if (designerWindow.designerType == DesignerType.ComponentEngine)
                    {
                        //���� ������ �� ���� ��������� ����������
                        UIEngineDesignerWindow engineDesignerWindow = designerWindow.engineDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ �������� ��������� �� ������� �������
                        if (dropdownEvent.WidgetName == "ChangeEnginePowerPerSizeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ����������
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� ���������
                    else if (designerWindow.designerType == DesignerType.ComponentReactor)
                    {
                        //���� ������ �� ���� ��������� ���������
                        UIReactorDesignerWindow reactorDesignerWindow = designerWindow.reactorDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ ������� �������� �� ������� �������
                        if (dropdownEvent.WidgetName == "ChangeReactorEnergyPerSizeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ���������
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� ��������� �����
                    else if (designerWindow.designerType == DesignerType.ComponentHoldFuelTank)
                    {
                        //���� ������ �� ���� ��������� ���������
                        UIFuelTankDesignerWindow fuelTankDesignerWindow = designerWindow.fuelTankDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ ������ ���������� ����
                        if (dropdownEvent.WidgetName == "ChangeFuelTankCompressionTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ��������� �����
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� ������������ ��� ������ ������
                    else if (designerWindow.designerType == DesignerType.ComponentExtractionEquipmentSolid)
                    {
                        //���� ������ �� ���� ��������� ����������� ������������
                        UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow = designerWindow.extractionEquipmentDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ �������� �� ������� �������
                        if (dropdownEvent.WidgetName == "ChangeExtractionEquipmentSolidSpeedPerSizeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ������������ ��� ������ ������
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� �������������� ������
                    else if (designerWindow.designerType == DesignerType.ComponentGunEnergy)
                    {
                        //���� ������ �� ���� ��������� �������������� ������
                        UIGunEnergyDesignerWindow energyGunDesignerWindow = designerWindow.energyGunDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ �����������
                        if (dropdownEvent.WidgetName == "ChangeEnergyGunRechargeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� �������������� ������
                            DesignerComponentActionRequest(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                dropdownEvent.Value);
                        }
                    }
                }

                uguiUIWorld.DelEntity(dropdownEventUIEntity);
            }

            //��� ������� ������� ��������� �������� ��������
            foreach (int sliderEventUIEntity in sliderEventUIFilter)
            {
                //���� ��������� �������
                ref EcsUguiSliderChangeEvent
                    sliderEvent
                    = ref sliderEventUIPool.Get(sliderEventUIEntity);

                Debug.LogWarning("Slider change! " + sliderEvent.WidgetName + " ! " + sliderEvent.Value);

                //���� ������� ���� ����� ����
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {

                }
            }

            //��� ������� ������� ����� �� ������������ �������
            foreach (int clickEventSpaceEntity in clickEventSpaceFilter)
            {
                //���� ��������� �������
                ref EcsUguiClickEvent clickEvent = ref clickEventSpacePool.Get(clickEventSpaceEntity);

                //��������, ��� ������ ����� ��� �� ������
                bool clickEventComplete = false;

                Debug.LogWarning("Click! " + clickEvent.WidgetName);

                uguiSOWorld.DelEntity(clickEventSpaceEntity);
            }

            //���� ������� �����-���� ����� �����
            if (inputData.Value.mapMode != MapMode.None)
            {
                //���� ����������� ������
                //InputCameraMoving();

                //���� �������� ������
                CameraInputRotating();

                //���� ����������� ������
                CameraInputZoom();
            }

            //������ ����
            InputOther();

            //���� ����
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

                //���� ��������� �������� �������
                taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                RegionSetColor(ref currentRegion, Color.blue);

                //��� ������� ������� � ��������
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
            //���� ������������� �������� ���� ����� ����
            if (clickEvent.WidgetName == "OpenNewGameMenu")
            {
                //������ ������ �������� ���� ����� ����
                MainMenuActionRequest(MainMenuActionType.OpenNewGameMenu);
            }
            //�����, ���� ������������� �������� ���� �������� ����
            else if (clickEvent.WidgetName == "OpenLoadGameMenu")
            {
                //������ ������ �������� ���� �������� ����
                MainMenuActionRequest(MainMenuActionType.OpenLoadGameMenu);
            }
            //�����, ���� ������������� �������� ���� ����������
            else if (clickEvent.WidgetName == "OpenWorkshop")
            {
                //������ ������� �������� ���� ����������
                MainMenuActionRequest(MainMenuActionType.OpenWorkshop);
            }
            //�����, ���� ������������� �������� ���� ������� ��������
            else if (clickEvent.WidgetName == "OpenSettings")
            {
                //������ ������ �������� ���� ������� ��������
                MainMenuActionRequest(MainMenuActionType.OpenMainSettings);
            }
            //�����, ���� ������������� ����� �� ����
            else if (clickEvent.WidgetName == "QuitGame")
            {
                //������ ������ ������ �� ����
                QuitGameRequest();
            }
        }

        readonly EcsPoolInject<RMainMenuAction> mainMenuActionRequestPool = default;
        void MainMenuActionRequest(
            MainMenuActionType actionType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ������� ����
            int requestEntity = world.Value.NewEntity();
            ref RMainMenuAction mainMenuActionRequest = ref mainMenuActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ������� ���� �������������
            mainMenuActionRequest.actionType = actionType;
        }
        #endregion

        #region NewGameMenu
        void ClickActionNewGameMenu(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ������������� ������ ����� ����
            if (clickEvent.WidgetName == "StartNewGame")
            {
                //������ ������ ������ ����� ����
                NewGameMenuActionRequest(NewGameMenuActionType.StartNewGame);
            }
        }

        readonly EcsPoolInject<RNewGameMenuAction> newGameMenuActionRequestPool = default;
        void NewGameMenuActionRequest(
            NewGameMenuActionType actionType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ���� ����� ����
            int requestEntity = world.Value.NewEntity();
            ref RNewGameMenuAction newGameMenuActionRequest = ref newGameMenuActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ���� ����� ���� �������������
            newGameMenuActionRequest.actionType = actionType;
        }
        #endregion

        #region Workshop
        void ClickActionWorkshop(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ������ �� ���� ����������
            UIWorkshopWindow workshopWindow = eUI.Value.workshopWindow;

            //���� �������� ������� �����������
            if (clickEvent.WidgetName == "")
            {
                //�������� �������� ������ ������ ��������
                if (clickEvent.Sender.TryGetComponent(out UIWorkshopContentSetPanel workshopContentSetPanel))
                {
                    //����������� ����������� ���������� ������ ��������
                    WorkshopActionRequest(
                        WorkshopActionType.DisplayContentSet,
                        workshopContentSetPanel.contentSetIndex);
                }
            }
            //�����, ���� ������� ����������� �������� ��������� ��������
            else if (clickEvent.WidgetName == "WorkshopOpenDesigner")
            {
                //���� �������� ������������� � ������ ��������
                Toggle activeToggle = workshopWindow.contentInfoToggleGroup.GetFirstActiveToggle();

                //���� �� �� ����
                if (activeToggle != null)
                {
                    //�������� �������� ������ ���������� ���� ��������
                    if (activeToggle.TryGetComponent(out UIWorkshopContentInfoPanel workshopContentInfoPanel))
                    {
                        //����������� ����������� ���������� ��������� � ������� ������ ��������
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
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ����������
            int requestEntity = world.Value.NewEntity();
            ref RWorkshopAction workshopActionRequest = ref workshopActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ���������� �������������
            workshopActionRequest.actionType = actionType;

            //��������� ������ ������ ��������, ������� ��������� ����������
            workshopActionRequest.contentSetIndex = contentSetIndex;

            //���������, ����� ��� ��������� ��������� �������
            workshopActionRequest.designerType = designerType;
        }
        #endregion

        #region Designer
        void ClickActionDesigner(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������� ����������� ���������� ������� �������� �� �������� ������ ��������
            if (clickEvent.WidgetName
                == "SaveCurrentContentSetPanel")
            {
                //���� ���������� ��������
                if (DesignerContentSavePossible()
                    == true)
                {
                    //����������� ���������� ������� ��������
                    DesignerActionRequest(
                        DesignerActionType.SaveContentObject,
                        true);
                }
            }
            //�����, ���� ������� ����������� �������� ������� �������� �� �������� ������ ��������
            else if (clickEvent.WidgetName
                == "LoadCurrentContentSetPanel")
            {
                //���� �������� �������������
                Toggle activeToggle
                    = designerWindow.currentContentSetList.toggleGroup.GetFirstActiveToggle();

                //���� �� �� ����
                if (activeToggle
                    != null)
                {
                    //�������� �������� �������� ������ ���������� �������
                    if (activeToggle.TryGetComponent(
                        out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                    {
                        //����������� �������� ������� ��������
                        DesignerActionRequest(
                            DesignerActionType.LoadContentSetObject,
                            true,
                            contentObjectBriefInfoPanel.contentSetIndex,
                            contentObjectBriefInfoPanel.objectIndex);
                    }
                }
            }
            //�����, ���� ������� ����������� �������� ������� �������� �� ������� ������ ��������
            else if (clickEvent.WidgetName
                == "LoadOtherContentSetPanel")
            {
                //���� �������� �������������
                Toggle activeToggle
                    = designerWindow.otherContentSetsList.toggleGroup.GetFirstActiveToggle();

                //���� �� �� ����
                if (activeToggle
                    != null)
                {
                    //�������� �������� �������� ������ ���������� �������
                    if (activeToggle.TryGetComponent(
                        out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                    {
                        //����������� �������� ������� ��������
                        DesignerActionRequest(
                            DesignerActionType.LoadContentSetObject,
                            false,
                            contentObjectBriefInfoPanel.contentSetIndex,
                            contentObjectBriefInfoPanel.objectIndex);
                    }
                }
            }
            //�����, ���� ������� ����������� �������� ������� �������� �� �������� ������ ��������
            else if (clickEvent.WidgetName
                == "DeleteCurrentContentSetPanel")
            {
                //���� �������� �������������
                Toggle activeToggle
                    = designerWindow.currentContentSetList.toggleGroup.GetFirstActiveToggle();

                //���� �� �� ����
                if (activeToggle != null)
                {
                    //�������� �������� �������� ������ ���������� �������
                    if (activeToggle.TryGetComponent(
                        out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                    {
                        //���� �������� ��������
                        if (DesignerContentDeletePossible(
                            contentObjectBriefInfoPanel))
                        {
                            //����������� �������� ������� ��������
                            DesignerActionRequest(
                                DesignerActionType.DeleteContentSetObject,
                                true,
                                contentObjectBriefInfoPanel.contentSetIndex,
                                contentObjectBriefInfoPanel.objectIndex);
                        }
                    }
                }
            }
            //�����, ���� ������� ����������� ����������� ������ ������ ������� ��������
            else if (clickEvent.WidgetName
                == "DisplayOtherContentSets")
            {
                //����������� ����������� ������ ������ ������� ��������
                DesignerActionRequest(
                    DesignerActionType.DisplayContentSetPanel,
                    false);
            }
            //�����, ���� ������� ����������� �������� ������ ������ ������� ��������
            else if (clickEvent.WidgetName
                == "HideOtherContentSets")
            {
                //����������� �������� ������ ������ ������� ��������
                DesignerActionRequest(
                    DesignerActionType.HideContentSetPanel,
                    false);
            }
            //�����, ���� ������� ����������� ����������� ������ �������� ������ ��������
            else if (clickEvent.WidgetName
                == "DisplayCurrentContentSet")
            {
                //����������� ����������� ������ �������� ������ ��������
                DesignerActionRequest(
                    DesignerActionType.DisplayContentSetPanel,
                    true);
            }
            //�����, ���� ������� ����������� �������� ������ ������ ������� ��������
            else if (clickEvent.WidgetName
                == "HideCurrentContentSet")
            {
                //����������� �������� ������ �������� ������ ��������
                DesignerActionRequest(
                    DesignerActionType.HideContentSetPanel,
                    true);
            }
            //�����, ���� ������� �������� ��������
            else if (designerWindow.designerType == DesignerType.ShipClass)
            {
                //���� ������ �� ���� ��������� ��������
                UIShipClassDesignerWindow shipClassDesignerWindow = eUI.Value.designerWindow.shipClassDesigner;

                //���� ������� �� ����� �������
                if (clickEvent.WidgetName == "")
                {
                    //���� �������� ������������� �� ������ ��������� �����������
                    Toggle activeToggleAvailableComponent = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                    //���� �������� ������������� �� ������ ������������� �����������
                    Toggle activeToggleInstalledComponent = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                    //���� ������������� � ������ ��������� ����������� �� ����
                    if (activeToggleAvailableComponent != null
                        //� �������� ������������ �������� �������
                        && activeToggleAvailableComponent.gameObject == clickEvent.Sender)
                    {
                        //�������� �������� ������ ���������� ����������
                        if (activeToggleAvailableComponent.TryGetComponent(out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                        {
                            //����������� ����������� ��������� ���������� � ����������
                            DesignerShipClassActionRequest(
                                DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                shipClassDesignerWindow.currentAvailableComponentsType,
                                contentObjectBriefInfoPanel.contentSetIndex,
                                contentObjectBriefInfoPanel.objectIndex);

                            //���� ������������� � ������ ������������� ����������� �������
                            if (activeToggleInstalledComponent != null)
                            {
                                activeToggleInstalledComponent.isOn = false;
                            }
                        }
                    }
                    //�����, ���� ������������� � ������ ������������� ����������� �� ����
                    else if (activeToggleInstalledComponent != null
                        //� �������� ������������ �������� �������
                        && activeToggleInstalledComponent.gameObject == clickEvent.Sender)
                    {
                        //�������� �������� ������ ���������� ����������
                        if (activeToggleInstalledComponent.TryGetComponent(out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                        {
                            //����������� ����������� ��������� ���������� � ����������
                            DesignerShipClassActionRequest(
                                DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                componentBriefInfoPanel.componentType,
                                componentBriefInfoPanel.contentSetIndex,
                                componentBriefInfoPanel.componentIndex);

                            //���� ������������� � ������ ��������� ����������� �������
                            if (activeToggleAvailableComponent != null)
                            {
                                activeToggleAvailableComponent.isOn = false;
                            }
                        }
                    }
                    //�����, ���� ������������ ������ ������� ����� ��������� Toggle
                    else if (clickEvent.Sender.TryGetComponent(out Toggle eventSenderToggle))
                    {
                        //���� ������������ ToggleGroup �������� ������ ��������� �����������
                        if (eventSenderToggle.group == shipClassDesignerWindow.availableComponentsListToggleGroup
                            //��� ���� ������������ ToggleGroup �������� ������ ������������� �����������
                            || eventSenderToggle.group == shipClassDesignerWindow.installedComponentsListToggleGroup)
                        {
                            //����������� �������� ��������� ���������� � ����������
                            DesignerShipClassActionRequest(DesignerShipClassActionType.HideComponentDetailedInfo);
                        }
                    }
                }
                //���� ������� ����������� ���������� ���������� ���������� � ������������� �������
                if (clickEvent.WidgetName == "AddComponentToShipClass")
                {
                    //���� �������� ������������� �� ������ ��������� �����������
                    Toggle activeToggleAvailableComponent = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                    //���� ������������� �� ����
                    if (activeToggleAvailableComponent != null)
                    {
                        //�������� �������� ������ ���������� ����������
                        if (activeToggleAvailableComponent.TryGetComponent(out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                        {
                            //������ ������� ���������� ����������
                            DesignerShipClassActionRequest(
                                DesignerShipClassActionType.AddComponentToClass,
                                shipClassDesignerWindow.currentAvailableComponentsType,
                                contentObjectBriefInfoPanel.contentSetIndex,
                                contentObjectBriefInfoPanel.objectIndex,
                                1);
                        }
                    }
                    //����� 
                    else
                    {
                        //���� �������� ������������� �� ������ ������������� �����������
                        Toggle activeToggleInstalledComponent = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                        //���� ������������� �� ����
                        if (activeToggleInstalledComponent != null)
                        {
                            //�������� �������� ������ ���������� ����������
                            if (activeToggleInstalledComponent.TryGetComponent(out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                            {
                                //������ ������� ���������� ����������
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
                //�����, ���� ������� ����������� �������� ���������� ���������� �� �������������� �������
                else if (clickEvent.WidgetName == "DeleteComponentFromShipClass")
                {
                    //���� �������� ������������� �� ������ ������������� �����������
                    Toggle activeToggleInstalledComponent = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                    //���� ������������� �� ����
                    if (activeToggleInstalledComponent != null)
                    {
                        //�������� �������� ������ ���������� ����������
                        if (activeToggleInstalledComponent.TryGetComponent(out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                        {
                            //������ ������� �������� ����������
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
            //�����, ���� ������� �������� ����������
            else if (designerWindow.designerType == DesignerType.ComponentEngine)
            {

            }
            //�����, ���� ������� �������� ���������
            else if (designerWindow.designerType == DesignerType.ComponentReactor)
            {

            }
            //�����, ���� ������� �������� ��������� �����
            else if (designerWindow.designerType == DesignerType.ComponentHoldFuelTank)
            {

            }
            //�����, ���� ������� �������� ������������ ��� ������ ������
            else if (designerWindow.designerType == DesignerType.ComponentExtractionEquipmentSolid)
            {

            }
            //�����, ���� ������� �������� �������������� ������
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
            //������� ����� �������� � ��������� �� ��������� ������� �������� � ���������
            int requestEntity = world.Value.NewEntity();
            ref RDesignerAction designerActionRequest = ref designerActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ��������� �������������
            designerActionRequest.actionType = actionType;

            //���������, � ������� �� ������� �������� ��������� ��������� ��������
            designerActionRequest.isCurrentContentSet = isCurrentContentSet;

            //��������� ������ ������ ��������
            designerActionRequest.contentSetIndex = contentSetIndex;

            //��������� ������ �������
            designerActionRequest.objectIndex = objectIndex;
        }

        bool DesignerContentSavePossible()
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ����� �������� ������� �� �����
            if (designerWindow.currentContentSetList.objectName.text
                != "")
            {
                //���� ������� ������������� ��������
                if (designerWindow.isInGameDesigner
                    == true)
                {
                    //��� ������ �������� ������ � ������ �������� ������ ��������
                    for (int a = 0; a < designerWindow.currentContentSetList.panelsList.Count; a++)
                    {
                        //�������� �������� ������
                        if (designerWindow.currentContentSetList.panelsList[a].TryGetComponent(
                            out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                        {
                            //���� �������� ������� ��������� � ��������� � �����
                            if (contentObjectBriefInfoPanel.objectName.text
                                == designerWindow.currentContentSetList.objectName.text)
                            {
                                //���������� ����������
                                return false;
                            }
                        }
                    }
                }

                //���� ������� �������� ����������
                if (designerWindow.designerType
                    == DesignerType.ComponentEngine)
                {
                    //���� ������ �� ���� ��������� ����������
                    UIEngineDesignerWindow engineDesignerWindow
                        = designerWindow.engineDesigner;

                    //���� � ��������� ���������� ������� �������� ���������� ������� ����
                    if (engineDesignerWindow.powerPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //���������� ��������
                        return true;
                    }
                    //�����
                    else
                    {
                        //���������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ���������
                else if (designerWindow.designerType
                    == DesignerType.ComponentReactor)
                {
                    //���� ������ �� ���� ��������� ���������
                    UIReactorDesignerWindow reactorDesignerWindow
                        = designerWindow.reactorDesigner;

                    //���� � ��������� ��������� ������� �������� ���������� ������� ����
                    if (reactorDesignerWindow.energyPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //���������� ��������
                        return true;
                    }
                    //�����
                    else
                    {
                        //���������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ��������� �����
                else if (designerWindow.designerType
                    == DesignerType.ComponentHoldFuelTank)
                {
                    //���� ������ �� ���� ��������� ��������� �����
                    UIFuelTankDesignerWindow fuelTankDesignerWindow
                        = designerWindow.fuelTankDesigner;

                    //���� � ��������� ��������� ����� ������� �������� ���������� ������� ����
                    if (fuelTankDesignerWindow.compressionCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //���������� ��������
                        return true;
                    }
                    //�����
                    else
                    {
                        //���������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ������������ ��� ������ ������
                else if (designerWindow.designerType
                    == DesignerType.ComponentExtractionEquipmentSolid)
                {
                    //���� ������ �� ���� ��������� ����������� ������������
                    UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                        = designerWindow.extractionEquipmentDesigner;

                    //���� � ��������� ����������� ������������ ������� �������� ���������� ������� ����
                    if (extractionEquipmentDesignerWindow.speedPerSizeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //���������� ��������
                        return true;
                    }
                    //�����
                    else
                    {
                        //���������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� �������������� ������
                else if (designerWindow.designerType
                    == DesignerType.ComponentGunEnergy)
                {
                    //���� ������ �� ���� ��������� �������������� ������
                    UIGunEnergyDesignerWindow energyGunDesignerWindow
                        = designerWindow.energyGunDesigner;

                    //���� � ��������� �������������� ������ ������� �������� ���������� ������� ����
                    if (energyGunDesignerWindow.rechargeCoreTechnologyPanel.currentTechnologyIndex
                        >= 0)
                    //&&engineDesignerWindow.Xtechnology.currentTechnologyIndex
                    //>= 0
                    {
                        //���������� ��������
                        return true;
                    }
                    //�����
                    else
                    {
                        //���������� ����������
                        return false;
                    }
                }

                //�����
                else
                {
                    //���������� ��������
                    return true;
                }
            }
            //�����
            else
            {
                //���������� ����������
                return false;
            }
        }

        bool DesignerContentDeletePossible(
            UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel)
        {
            //���� ������ �� ���� ���������
            UIDesignerWindow designerWindow
                = eUI.Value.designerWindow;

            //���� ������� ������������� ��������
            if (designerWindow.isInGameDesigner
                == true)
            {
                //���� ������� �������� ����������
                if (designerWindow.designerType
                    == DesignerType.ComponentEngine)
                {
                    //���� ������ �� ���������, ������� ��������� �������
                    ref DEngine engine
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .engines[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ��������� ������� �������� ����
                    if (engine.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ���������
                else if (designerWindow.designerType
                    == DesignerType.ComponentReactor)
                {
                    //���� ������ �� �������, ������� ��������� �������
                    ref DReactor reactor
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .reactors[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ������� ������� �������� ����
                    if (reactor.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ��������� �����
                else if (designerWindow.designerType
                    == DesignerType.ComponentHoldFuelTank)
                {
                    //���� ������ �� ��������� ���, ������� ��������� �������
                    ref DHoldFuelTank fuelTank
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .fuelTanks[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ��������� ��� ������� �������� ����
                    if (fuelTank.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ������������ ��� ������ ������
                else if (designerWindow.designerType
                    == DesignerType.ComponentExtractionEquipmentSolid)
                {
                    //���� ������ �� ������������ ��� ������ ������, ������� ��������� �������
                    ref DExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .solidExtractionEquipments[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ������������ ��� ������ ������ ������� �������� ����
                    if (extractionEquipmentSolid.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� �������������� ������
                else if (designerWindow.designerType
                    == DesignerType.ComponentGunEnergy)
                {
                    //���� ������ �� �������������� ������, ������� ��������� �������
                    ref DGunEnergy energyGun
                        = ref contentData.Value
                        .contentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .energyGuns[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� �������������� ������ ������� �������� ����
                    if (energyGun.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }

                //�������� ����������
                return false;
            }
            //�����
            else
            {
                //���� ������� �������� ����������
                if (designerWindow.designerType
                    == DesignerType.ComponentEngine)
                {
                    //���� ������ �� ���������, ������� ��������� �������
                    ref WDEngine engine
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .engines[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ��������� ������� �������� ����
                    if (engine.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ���������
                else if (designerWindow.designerType
                    == DesignerType.ComponentReactor)
                {
                    //���� ������ �� �������, ������� ��������� �������
                    ref WDReactor reactor
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .reactors[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ������� ������� �������� ����
                    if (reactor.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ��������� �����
                else if (designerWindow.designerType
                    == DesignerType.ComponentHoldFuelTank)
                {
                    //���� ������ �� ��������� ���, ������� ��������� �������
                    ref WDHoldFuelTank fuelTank
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .fuelTanks[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ��������� ��� ������� �������� ����
                    if (fuelTank.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� ������������ ��� ������ ������
                else if (designerWindow.designerType
                    == DesignerType.ComponentExtractionEquipmentSolid)
                {
                    //���� ������ �� ������������ ��� ������ ������, ������� ��������� �������
                    ref WDExtractionEquipment extractionEquipmentSolid
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .solidExtractionEquipments[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� ������������ ��� ������ ������ ������� �������� ����
                    if (extractionEquipmentSolid.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }
                //�����, ���� ������� �������� �������������� ������
                else if (designerWindow.designerType
                    == DesignerType.ComponentGunEnergy)
                {
                    //���� ������ �� �������������� ������, ������� ��������� �������
                    ref WDGunEnergy energyGun
                        = ref contentData.Value
                        .wDContentSets[contentObjectBriefInfoPanel.contentSetIndex]
                        .energyGuns[contentObjectBriefInfoPanel.objectIndex];

                    //���� ������ ����������� �� �������������� ������ ������� �������� ����
                    if (energyGun.ShipClasses.Count
                        > 0)
                    {
                        //�������� ����������
                        return false;
                    }
                }

                //�������� ����������
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
            //������ ����� �������� � ��������� �� ��������� ������� ��������� ������ ������ ����������
            int requestEntity = world.Value.NewEntity();
            ref RDesignerShipClassAction requestComp = ref designerShipClassActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ��������� �������� �������������
            requestComp.actionType = actionType;


            //��������� ��� ����������
            requestComp.componentType = componentType;


            //��������� ������ ������ ��������
            requestComp.contentSetIndex = contentSetIndex;

            //��������� ������ ������
            requestComp.modelIndex = modelIndex;


            //��������� ����� �����������, ������� ��������� ����������/�������
            requestComp.numberOfComponents = numberOfComponents;
        }

        readonly EcsPoolInject<RDesignerComponentAction> designerComponentActionRequestPool = default;
        void DesignerComponentActionRequest(
            DesignerComponentActionType actionType,
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyIndex)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� �������� ����������
            int requestEntity = world.Value.NewEntity();
            ref RDesignerComponentAction requestComp = ref designerComponentActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ��������� ����������� �������������
            requestComp.actionType = actionType;

            //��������� ��� ������������
            requestComp.componentCoreModifierType = componentCoreModifierType;

            //��������� ������ ��������� ����������
            requestComp.technologyDropdownIndex = technologyIndex;
        }
        #endregion

        #region Game
        void ClickActionGame(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //���� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //����
            //���� ������������� �������� ��������� ��������
            if (clickEvent.WidgetName == "OpenShipClassDesigner")
            {
                //����������� �������� ��������� ��������
                GameOpenDesignerRequest(
                    DesignerType.ShipClass,
                    playerOrganization.contentSetIndex);
            }
            //�����, ���� ������������� �������� ��������� ����������
            else if (clickEvent.WidgetName == "OpenEngineDesigner")
            {
                //����������� �������� ��������� ����������
                GameOpenDesignerRequest(
                    DesignerType.ComponentEngine,
                    playerOrganization.contentSetIndex);
            }
            //�����, ���� ������������� �������� ��������� ���������
            else if (clickEvent.WidgetName == "OpenReactorDesigner")
            {
                //����������� �������� ��������� ���������
                GameOpenDesignerRequest(
                    DesignerType.ComponentReactor,
                    playerOrganization.contentSetIndex);
            }
            //�����, ���� ������������� �������� ��������� ��������� �����
            else if (clickEvent.WidgetName == "OpenFuelTankDesigner")
            {
                //����������� �������� ��������� ��������� �����
                GameOpenDesignerRequest(
                    DesignerType.ComponentHoldFuelTank,
                    playerOrganization.contentSetIndex);
            }
            //�����, ���� ������������� �������� ��������� ������������ ��� ������ ������
            else if (clickEvent.WidgetName == "OpenExtractionEquipmentSolidDesigner")
            {
                //����������� �������� ��������� ������������ ��� ������ ������
                GameOpenDesignerRequest(
                    DesignerType.ComponentExtractionEquipmentSolid,
                    playerOrganization.contentSetIndex);
            }
            //�����, ���� ������������� �������� ��������� �������������� ������
            else if (clickEvent.WidgetName == "OpenEnergyGunDesigner")
            {
                //����������� �������� ��������� �������������� ������
                GameOpenDesignerRequest(
                    DesignerType.ComponentGunEnergy,
                    playerOrganization.contentSetIndex);
            }
            //�����, ���� ������������� �������� ��������� ������
            else if (clickEvent.WidgetName == "OpenFleetManager")
            {
                //����������� �������� ��������� ��������� ������
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.FleetManager,
                    playerOrganization.selfPE);
            }
            //����

            //���� ������� ������ �������
            if (gameWindow.activeMainPanelType == MainPanelType.Object)
            {
                //��������� ����� � ������ �������
                ClickActionGameObject(ref clickEvent);
            }
        }

        #region GameObject
        void ClickActionGameObject(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //����
            //���� ������ ������ �������� ������ �������
            if (clickEvent.WidgetName == "CloseObjectPanel")
            {
                //����������� �������� ������ �������
                GameObjectPanelActionRequest(ObjectPanelActionRequestType.CloseObjectPanel);
            }
            //����

            //�����, ���� ������� ��������� ��������� ������
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
            {
                //��������� ����� � ��������� ��������� ������
                ClickActionGameObjectFleetManager(ref clickEvent);
            }
            //�����, ���� ������� ��������� �����������
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
            {
                //��������� ����� � ��������� �����������
                ClickActionGameObjectOrganization(ref clickEvent);
            }
            //�����, ���� ������� ��������� �������
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
            {
                //��������� ����� � ��������� �������
                ClickActionGameObjectRegion(ref clickEvent);
            }
            //�����, ���� ������� ��������� ORAEO
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
            {
                //��������� ����� � ��������� ORAEO
                ClickActionGameObjectORAEO(ref clickEvent);
            }
        }

        #region GameObjectFleetManager
        void ClickActionGameObjectFleetManager(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //���� ������ ������ ������� ������
            if (clickEvent.WidgetName == "FleetManagerFleetsTab")
            {
                //����������� ����������� ������� ������
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.FleetManagerFleets,
                    objectPanel.activeObjectPE);
            }
            //�����, ���� ������ ������ ������� �������� ����������� �����
            else if (clickEvent.WidgetName == "FleetManagerTaskForceTemplatesTab")
            {
                //����������� ����������� ������� �������� �����
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.FleetManagerTaskForceTemplates,
                    objectPanel.activeObjectPE);
            }

            //�����, ���� ������� ������� ������
            else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.fleetsTab.selfTabButton)
            {
                //��������� ����� �� ������� ������
                ClickActionGameObjectFleetManagerFleets(ref clickEvent);
            }
            //�����, ���� ������� ������� �������� �����
            else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.taskForceTemplatesTab.selfTabButton)
            {
                //��������� ����� �� ������� �������� �����
                ClickActionGameObjectFleetManagerTaskForceTemplates(ref clickEvent);
            }
        }

        void ClickActionGameObjectFleetManagerFleets(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //���� ������� ������
            UIFleetsTab fleetsTab = fleetManagerSubpanel.fleetsTab;

            //���������, �� ���� �� ������ �������� ����������� �����
            bool isActiveTaskForces = InputData.activeTaskForcePEs.Count > 0;

            //���� ������ �������� ����������� ����� �� ����
            //� ������ ������ ������ ������
            if (isActiveTaskForces == true
                && clickEvent.WidgetName == "SearchMissionFM")
            {
                //��� ������ �������� ����������� ������
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //���� ����������� ������
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //���� ������ �� ����� ���������� ������ ������
                    if (taskForcePatrolMissionSearchPool.Value.Has(taskForceEntity) == false)
                    {
                        //����������� ��������� ������ ������ �� ������ ������
                        TaskForceActionRequest(
                            TaskForceActionType.ChangeMissionSearch,
                            ref taskForce);
                    }
                }
            }
            //�����, ���� ������ �������� ����������� ����� �� ����
            //� ������ ������ ������ ������
            else if (isActiveTaskForces == true
                && clickEvent.WidgetName == "StrikeGroupMissionFM")
            {
                //��� ������ �������� ����������� ������
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //���� ����������� ������
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //���� ������ �� ����� ���������� ������ ������� ������
                    if (taskForceTargetMissionStrikeGroupPool.Value.Has(taskForceEntity) == false)
                    {
                        //����������� ��������� ������ ������ �� ������ ������
                        TaskForceActionRequest(
                            TaskForceActionType.ChangeMissionStrikeGroup,
                            ref taskForce);
                    }
                }
            }
            //�����, ���� ������ �������� ����������� ����� �� ����
            //� ������ ������ ���������
            else if (isActiveTaskForces == true
                && clickEvent.WidgetName == "HoldMissionFM")
            {
                //��� ������ �������� ����������� ������
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //���� ����������� ������
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //����������� ��������� ������ ������ �� ������ ���������
                    TaskForceActionRequest(
                        TaskForceActionType.ChangeMissionHold,
                        ref taskForce);
                }
            }

            //�����, ���� �������� ������� ����� ��������� FleetSummaryPanel
            else if (clickEvent.Sender.TryGetComponent(out UIFleetSummaryPanel fleetSummaryPanel))
            {
                //��� ������ ��������� ����������� ������
                for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                {
                    //���� ��������� ������������ �������� ������
                    InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel = ref taskForceDisplayedSummaryPanelPool.Value.Get(taskForceEntity);

                    //������ � ����������
                    taskForceDisplayedSummaryPanel.taskForceSummaryPanel.toggle.isOn = false;
                }
                //������� ������ ��������� ����������� �����
                InputData.activeTaskForcePEs.Clear();

                //���� ������ ���������
                if (fleetSummaryPanel.toggle.isOn == false)
                {
                    //����� � �������� � �������� ��� ��������
                    fleetSummaryPanel.toggle.isOn = true;
                    InputData.activeFleetPE = fleetSummaryPanel.selfPE;

                    //����������� ����������� �������� �������� �����

                }
                //�����
                else
                {
                    //������ � ���������� � ��������, ��� ��� ��������� �����
                    fleetSummaryPanel.toggle.isOn = false;
                    InputData.activeFleetPE = new();

                    //����������� �������� �������� �������� �����

                }
            }
            //�����, ���� �������� ������� ����� ��������� TaskForceSummaryPanel
            else if (clickEvent.Sender.TryGetComponent(out UITaskForceSummaryPanel taskForceSummaryPanel))
            {
                //���� ���� �������� ����
                if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                {
                    //���� ��������� ������������ �������� ������
                    ref CFleetDisplayedSummaryPanel fleetDisplayedSummaryPanel = ref fleetDisplayedSummaryPanelPool.Value.Get(fleetEntity);

                    //������ � ����������
                    fleetDisplayedSummaryPanel.fleetSummaryPanel.toggle.isOn = false;

                    //��������, ��� ��� ��������� �����
                    InputData.activeFleetPE = new();
                }

                //���� ����� LeftShift
                if (InputData.leftShiftKeyPressed)
                {
                    //�� ��� ����������� ������ ����� ��������� � ��������� ��� �������

                    //���� ������ ���������
                    if (taskForceSummaryPanel.toggle.isOn == false)
                    {
                        //������ � �������� � ������� � ������ ��������
                        taskForceSummaryPanel.toggle.isOn = true;
                        InputData.activeTaskForcePEs.Add(taskForceSummaryPanel.selfPE);
                    }
                    //�����
                    else
                    {
                        //������ � ���������� � ������� �� ������ ��������
                        taskForceSummaryPanel.toggle.isOn = false;
                        InputData.activeTaskForcePEs.Remove(taskForceSummaryPanel.selfPE);
                    }
                }
                //����� ������ ������ ��� 
                else
                {
                    //�� ������ ��� ����������� ������ ����� ������� ��� �������

                    //���� ������ ���� ������� � � ������ ���� ������ ����� ������, �� ������� ������ � ���������� ������
                    //����� ��������� ������
                    bool isActivation
                        = taskForceSummaryPanel.toggle.isOn && (InputData.activeTaskForcePEs.Count > 1)
                        || (taskForceSummaryPanel.toggle.isOn == false);

                    //��� ������ ��������� ����������� ������
                    for (int a = 0; a < InputData.activeTaskForcePEs.Count; a++)
                    {
                        //���� ��������� ������������ �������� ������
                        InputData.activeTaskForcePEs[a].Unpack(world.Value, out int taskForceEntity);
                        ref CTaskForceDisplayedSummaryPanel taskForceDisplayedSummaryPanel = ref taskForceDisplayedSummaryPanelPool.Value.Get(taskForceEntity);

                        //������ � ����������
                        taskForceDisplayedSummaryPanel.taskForceSummaryPanel.toggle.isOn = false;
                    }
                    //������� ������ ��������� ����������� �����
                    InputData.activeTaskForcePEs.Clear();

                    //���� ��������� ������������ ������
                    if (isActivation == true)
                    {
                        //������ � �������� � ������� � ������ ��������
                        taskForceSummaryPanel.toggle.isOn = true;
                        InputData.activeTaskForcePEs.Add(taskForceSummaryPanel.selfPE);
                    }
                }
            }

            //�����, ���� ������ �������� ��������� ������� ����� ��������� TaskForceSummaryPanel
            else if (clickEvent.Sender.transform.parent.parent.TryGetComponent(
                out UITaskForceSummaryPanel taskForceSummaryPanel2))
            {
                //���� �������� ������� - ��� ������ ������� ������
                if (clickEvent.Sender == taskForceSummaryPanel2.templateSummaryButton.gameObject)
                {
                    //���� ������ ��� ����� ������� ������ �������
                    if (fleetsTab.templatesChangingList.isActiveAndEnabled == true)
                    {
                        //����������� ���������� ������ ��� ����� ������� ������
                        GameFleetManagerSubpanelActionRequest(
                            FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesChangeList,
                            objectPanel.activeObjectPE,
                            taskForceSummaryPanel2.selfPE);
                    }
                }
            }
            //�����, ���� �������� ������� ����� ��������� TemplateSummaryPanel
            else if (clickEvent.Sender.TryGetComponent(
                out UITFTemplateSummaryPanel templateSummaryPanel))
            {
                //���� ������������ ������� ������ �������� ������ ��� ����� ������� ������
                if(templateSummaryPanel.parentList == fleetsTab.templatesChangingList)
                {
                    //���� ������, ������ ������� ����������
                    fleetsTab.templateChangingTaskForcePanel.selfPE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce parentTaskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //����������� ����� ������� ��� ������ ������
                    TaskForceActionRequest(
                        TaskForceActionType.ChangeTemplate,
                        ref parentTaskForce,
                        templateSummaryPanel.template);
                }
                //�����, ���� ������������ ������� ������ �������� ������ ��� �������� ������
                else if(templateSummaryPanel.parentList == fleetsTab.templatesCreatingList)
                {
                    //���� ���� �������� ����
                    if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                    {
                        //���� ����
                        ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                        //����������� �������� ����� ����������� ������
                        TaskForceCreatingRequest(
                            ref fleet,
                            templateSummaryPanel.template);
                    }
                }
            }

            //�����, ���� ������ ������ �������� ����� ����������� ������
            else if (clickEvent.WidgetName == "CreateNewTaskForceFM")
            {
                //���� ������ ��� �������� ������ �������
                if (fleetsTab.templatesCreatingList.isActiveAndEnabled == true)
                {
                    //����������� ���������� ������ ��� �������� ������
                    GameFleetManagerSubpanelActionRequest(
                        FleetManagerSubpanelActionRequestType.FleetsTabFillTemplatesNewList,
                        objectPanel.activeObjectPE);
                }
            }
            //�����, ���� ������ ������ �������� ������ �����
            else if (clickEvent.WidgetName == "CreateNewFleetFM")
            {
                //����������� �������� ������ �����
                FleetCreatingRequest(ref playerOrganization);
            }
        }

        void ClickActionGameObjectFleetManagerTaskForceTemplates(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //���� ������� �������� �����
            UITFTemplatesTab tFTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

            //���� �����������
            objectPanel.activeObjectPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //���� ������� ���������� ������ �������� �����
            if (tFTemplatesTab.listSubtab.isActiveAndEnabled == true)
            {
                //���� ������ ������ �������� ������ ������� ������
                if (clickEvent.WidgetName == "CreateNewTaskForceTemplateFM")
                {
                    //�������� ������ �� ������ � ���������
                    tFTemplatesTab.designerSubtab.template = null;

                    //����������� ����������� ���������� ��������� ������� ������
                    GameObjectPanelActionRequest(
                        ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner,
                        objectPanel.activeObjectPE);
                }
                //�����, ���� ������ �������� ��������� ������� ����� ��������� UITFTemplateSummaryPanel
                else if (clickEvent.Sender.transform.parent.parent.TryGetComponent(
                    out GameWindow.Object.FleetManager.TaskForceTemplates.List.UITFTemplateSummaryPanel tFTemplateSummaryPanel))
                {
                    //���� �������� ������� - ��� ������ ��������������
                    if (clickEvent.Sender == tFTemplateSummaryPanel.editButton.gameObject)
                    {
                        //���� ������ �������
                        if (tFTemplateSummaryPanel.editButton.interactable == true)
                        {
                            //��������� � ���������� ��������� ������ �������
                            tFTemplatesTab.designerSubtab.template = tFTemplateSummaryPanel.template;

                            //����������� ����������� ���������� ��������� ������� ������
                            GameObjectPanelActionRequest(
                                ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner,
                                objectPanel.activeObjectPE);
                        }
                    }
                    //�����, ���� �������� ������� - ��� ������ ��������
                    else if (clickEvent.Sender == tFTemplateSummaryPanel.deleteButton.gameObject)
                    {
                        //���� ������ �������
                        if (tFTemplateSummaryPanel.deleteButton.interactable == true)
                        {
                            //����������� �������� ������� ������
                            TaskForceTemplateActionRequest(
                                objectPanel.activeObjectPE,
                                tFTemplateSummaryPanel.template,
                                TFTemplateActionType.Delete);
                        }
                    }
                }

            }
            //�����, ���� ������� ���������� ��������� �������� �����
            else if (tFTemplatesTab.designerSubtab.isActiveAndEnabled == true)
            {
                //���� ���������� ��������� �������� �����
                UIDesignerSubtab taskForceTemplatesDesignerSubtab = tFTemplatesTab.designerSubtab;

                //���� ������ ������ ���������� ������� ������
                if (clickEvent.WidgetName == "SaveTaskForceTemplateFM")
                {
                    //���������, �������� �� ���������� �������
                    bool isSavePossible = true;

                    //��� ������� ������� ������ � ������ �����������
                    for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
                    {
                        //���� �������� ������� ��������� � �������, �� ��������, ��� ���������� ����������
                        if (OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a].selfName
                            == tFTemplatesTab.designerSubtab.tFTemplateName.text)
                        {
                            isSavePossible = false;

                            break;
                        }
                    }

                    //���� ���������� ��������
                    if (isSavePossible == true)
                    {
                        //���� ������ � ��������� �� ����
                        if (taskForceTemplatesDesignerSubtab.template != null)
                        {
                            //����������� ���������� ������� ������
                            TaskForceTemplateCreatingRequest(
                                taskForceTemplatesDesignerSubtab,
                                objectPanel.activeObjectPE,
                                tFTemplatesTab.designerSubtab.tFTemplateName.text,
                                true,
                                tFTemplatesTab.designerSubtab.template);
                        }
                        //�����
                        else
                        {
                            //����������� ���������� ������� ������
                            TaskForceTemplateCreatingRequest(
                                taskForceTemplatesDesignerSubtab,
                                objectPanel.activeObjectPE,
                                tFTemplatesTab.designerSubtab.tFTemplateName.text);
                        }

                        //����������� ����������� ���������� ������ �������� �����
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
            //������ ����� �������� � ��������� �� ��������� ������� �������� ��������� ������
            int requestEntity = world.Value.NewEntity();
            ref RGameFleetManagerSubpanelAction requestComp = ref gameFleetManagerSubpanelActionRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
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
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� �����������
            UIOrganizationSubpanel organizationSubpanel = objectPanel.organizationSubpanel;

            //���� ������ ������ �������� �������
            if (clickEvent.WidgetName == "OrganizationOverviewTab")
            {
                //����������� ����������� �������� �������
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
            //���� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� �������
            UIRegionSubpanel regionSubpanel = objectPanel.regionSubpanel;

            //���� ������ ������ �������� �������
            if (clickEvent.WidgetName == "RegionOverviewTab")
            {
                //����������� ����������� �������� �������
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.RegionOverview,
                    objectPanel.activeObjectPE);
            }
            //�����, ���� ������ ������ ������� �����������
            else if (clickEvent.WidgetName == "RegionORAEOsTab")
            {
                //����������� ����������� ������� �����������
                GameObjectPanelActionRequest(
                    ObjectPanelActionRequestType.RegionOrganizations,
                    objectPanel.activeObjectPE);
            }

            //�����, ���� ������� �������� �������
            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.overviewTab.selfTabButton)
            {
                //��������� ����� � �������� �������
                ClickActionGameObjectRegionOverview(ref clickEvent);
            }
            //�����, ���� ������� ������� �����������
            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.oRAEOsTab.selfTabButton)
            {
                //��������� ����� �� ������� ORAEO
                ClickActionGameObjectRegionORAEOs(ref clickEvent);
            }
        }

        void ClickActionGameObjectRegionOverview(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� ����������� ������
            inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerOrganizationEntity);
            ref COrganization playerOrganization = ref organizationPool.Value.Get(playerOrganizationEntity);

            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ������ ������ �����������
            if (clickEvent.WidgetName == "RAEOColonize")
            {
                //���� RAEO
                objectPanel.activeObjectPE.Unpack(world.Value, out int rAEOEntity);
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //���� ExORAEO ������ �����������
                rAEO.organizationRAEOs[playerOrganization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);

                //��������� �������� ORAEO ���������� ��������
                ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Add(oRAEOEntity);

                //��������� ������ �����������
                oRAEOActionSR = new(ORAEOActionType.Colonization);
            }
        }

        void ClickActionGameObjectRegionORAEOs(
            ref EcsUguiClickEvent clickEvent)
        {
            //���� �������� ������� ������ ��������� ORAEOBriefInfoPanel
            if (clickEvent.Sender.TryGetComponent(out UIORAEOSummaryPanel briefInfoPanel))
            {
                //����������� ����������� ��������� ORAEO
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
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ORAEO
            UIORAEOSubpanel oRAEOSubpanel = objectPanel.oRAEOSubpanel;

            //���� ������ ������ �������� �������
            if (clickEvent.WidgetName == "ORAEOOverviewTab")
            {
                //����������� ����������� �������� �������
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
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ �������
            int requestEntity = world.Value.NewEntity();
            ref RGameObjectPanelAction requestComp = ref gameObjectPanelActionRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
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
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ����
            int requestEntity = world.Value.NewEntity();
            ref RGameAction requestComp = ref gameActionRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� � ���� �������������
            requestComp.actionType = actionType;
        }

        readonly EcsPoolInject<RGameOpenDesigner> gameOpenDesignerRequestPool = default;
        void GameOpenDesignerRequest(
            DesignerType designerType,
            int contentSetIndex)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ��������� � ����
            int requestEntity = world.Value.NewEntity();
            ref RGameOpenDesigner requestComp = ref gameOpenDesignerRequestPool.Value.Add(requestEntity);

            //���������, ����� �������� ��������� �������
            requestComp.designerType = designerType;

            //���������, ����� ����� �������� ��������� �������
            requestComp.contentSetIndex = contentSetIndex;
        }

        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;
        void MapChangeModeRequest(
            ChangeMapModeRequestType requestType,
            EcsPackedEntity objectPE)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ������ �����
            int requestEntity = world.Value.NewEntity();
            ref RChangeMapMode requestComp = ref changeMapModeRequestPool.Value.Add(requestEntity);

            //��������� ��������� ����� �����
            requestComp.requestType = requestType;

            requestComp.mapMode = MapMode.Distance;

            //���������, ����� ������ ������� �����������
            requestComp.activeObject = objectPE;
        }

        void FleetCreatingRequest(
            ref COrganization organization,
            bool isReserve = false)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� �����
            int requestEntity = world.Value.NewEntity();
            ref RFleetCreating requestComp = ref fleetCreatingRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
            requestComp = new(
                organization.selfPE,
                isReserve);
        }

        void TaskForceCreatingRequest(
            ref CFleet fleet,
            DTFTemplate template = null)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ����������� ������
            int requestEntity = world.Value.NewEntity();
            ref RTaskForceCreating requestComp = ref taskForceCreatingRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
            requestComp = new(
                fleet.selfPE,
                template);
        }

        void TaskForceActionRequest(
            TaskForceActionType requestType,
            ref CTaskForce taskForce,
            DTFTemplate template = null)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ����������� ������
            int requestEntity = world.Value.NewEntity();
            ref RTaskForceAction requestComp = ref taskForceActionRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
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
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ ������� ����������� ������
            int requestEntity = world.Value.NewEntity();
            ref RTFTemplateCreating requestComp = ref tFTemplateCreatingRequestPool.Value.Add(requestEntity);

            //������ ������ ����� ��������
            List<DCountedShipType> shipTypes = new();

            //���� ������ ������ ����� ���������
            UIBattleGroupPanel shortRangeBattleGroupPanel = taskForceTemplatesDesignerSubtab.shortRangeGroup;

            //��� ������� ���������� ���� ������� � ������ ����� ���������
            for (int a = 0; a < shortRangeBattleGroupPanel.currentShipTypePanels.Count; a++)
            {
                //���� ���������� �������� ������� ���� ������ ����
                if (shortRangeBattleGroupPanel.currentShipTypePanels[a].CurrentShipCountValue > 0)
                {
                    //������� ��� ������� � ������
                    shipTypes.Add(new(
                        shortRangeBattleGroupPanel.currentShipTypePanels[a].shipType,
                        shortRangeBattleGroupPanel.currentShipTypePanels[a].CurrentShipCountValue));
                }
            }

            //��������� ������ �������
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
            //������ ����� �������� � ��������� �� ��������� ������ �������� ������� ����������� ������
            int requestEntity = world.Value.NewEntity();
            ref RTFTemplateAction requestComp = ref tFTemplateActionRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
            requestComp = new(
                organizationPE,
                template,
                requestType);
        }
        #endregion
        
        void CameraInputRotating()
        {
            //���� Y-��������
            float yRotationDelta = Input.GetAxis("Horizontal");
            //���� ��� �� ����� ����, �� ���������
            if (yRotationDelta != 0f)
            {
                CameraAdjustYRotation(yRotationDelta);
            }

            //���� X-��������
            float xRotationDelta = Input.GetAxis("Vertical");
            //���� ��� �� ����� ����, �� ���������
            if (xRotationDelta != 0f)
            {
                CameraAdjustXRotation(xRotationDelta);
            }
        }

        void CameraAdjustYRotation(
            float rotationDelta)
        {
            //������������ ���� ��������
            inputData.Value.rotationAngleY -= rotationDelta * inputData.Value.rotationSpeed * UnityEngine.Time.deltaTime;

            //����������� ����
            if (inputData.Value.rotationAngleY < 0f)
            {
                inputData.Value.rotationAngleY += 360f;
            }
            else if (inputData.Value.rotationAngleY >= 360f)
            {
                inputData.Value.rotationAngleY -= 360f;
            }

            //��������� ��������
            inputData.Value.mapCamera.localRotation = Quaternion.Euler(
                0f, inputData.Value.rotationAngleY, 0f);
        }
        
        void CameraAdjustXRotation(
            float rotationDelta)
        {
            //������������ ���� ��������
            inputData.Value.rotationAngleX += rotationDelta * inputData.Value.rotationSpeed * UnityEngine.Time.deltaTime;

            //����������� ����
            inputData.Value.rotationAngleX = Mathf.Clamp(
                inputData.Value.rotationAngleX, inputData.Value.minAngleX, inputData.Value.maxAngleX);

            //��������� ��������
            inputData.Value.swiwel.localRotation = Quaternion.Euler(
                inputData.Value.rotationAngleX, 0f, 0f);
        }

        void CameraInputZoom()
        {
            //���� �������� ������� ����
            float zoomDelta = Input.GetAxis("Mouse ScrollWheel");

            //���� ��� �� ����� ����
            if (zoomDelta != 0)
            {
                //��������� �����������
                CameraAdjustZoom(zoomDelta);
            }
        }

        void CameraAdjustZoom(
            float zoomDelta)
        {
            //������������ ����������� ������
            inputData.Value.zoom = Mathf.Clamp01(inputData.Value.zoom + zoomDelta);

            //������������ ���������� ����������� � ��������� ���
            float zoomDistance = Mathf.Lerp(inputData.Value.stickMinZoom, inputData.Value.stickMaxZoom, inputData.Value.zoom);
            inputData.Value.stick.localPosition = new(0f, 0f, zoomDistance);

            //������������ ������� ����������� � ��������� ���
            /*float zoomAngle = Mathf.Lerp(inputData.Value.swiwelMinZoom, inputData.Value.swiwelMaxZoom, inputData.Value.zoom);
            inputData.Value.swiwel.localRotation = Quaternion.Euler(zoomAngle, 0f, 0f);*/
        }

        void InputOther()
        {
            //����� �����
            if (Input.GetKeyUp(KeyCode.Space))
            {
                //���� ���� �������
                if (runtimeData.Value.isGameActive == true)
                {
                    //����������� ��������� �����
                    GameActionRequest(GameActionType.PauseOn);
                }
                //�����
                else
                {
                    //����������� ���������� �����
                    GameActionRequest(GameActionType.PauseOff);
                }
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                //���� ������� ���� ����
                if (eUI.Value.activeMainWindowType == MainWindowType.Game)
                {
                    //������ ������ ������ �� ����
                    QuitGameRequest();
                }
                //�����, ���� ������� ���� ���������
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Designer)
                {
                    //���� ������� ������������� ��������
                    if (eUI.Value.designerWindow.isInGameDesigner
                        == true)
                    {
                        //����������� �������� ���� ����
                        DesignerActionRequest(
                            DesignerActionType.OpenGame);
                    }
                    //�����
                    else
                    {
                        //����������� �������� ���� ����������
                        DesignerActionRequest(
                            DesignerActionType.OpenWorkshop);
                    }
                }
                //�����, ���� ������� ���� ����������
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Workshop)
                {
                    //����������� �������� ���� �������� ����
                    WorkshopActionRequest(
                        WorkshopActionType.OpenMainMenu);
                }
                //�����, ���� ������� ���� ���� ����� ����
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {
                    //����������� �������� ���� �������� ����
                    NewGameMenuActionRequest(
                        NewGameMenuActionType.OpenMainMenu);
                }
            }

            if (Input.GetKeyUp(KeyCode.Y))
            {
                Debug.LogWarning("Y!");

                //����������� ����� ������ ����� �� ����� ����������
                MapChangeModeRequest(
                    ChangeMapModeRequestType.Distance,
                    new EcsPackedEntity());
            }
        }

        void CheckButtons()
        {
            //���������� ��������� ���
            InputData.leftMouseButtonClick = Input.GetMouseButtonDown(0);
            InputData.leftMouseButtonPressed = InputData.leftMouseButtonClick || Input.GetMouseButton(0);
            InputData.leftMouseButtonRelease = Input.GetMouseButtonUp(0);

            //���������� ��������� ���
            InputData.rightMouseButtonClick = Input.GetMouseButtonDown(1);
            InputData.rightMouseButtonPressed = InputData.leftMouseButtonClick || Input.GetMouseButton(1);
            InputData.rightMouseButtonRelease = Input.GetMouseButtonUp(1);

            //���������� ��������� LeftShift
            InputData.leftShiftKeyPressed = Input.GetKey(KeyCode.LeftShift);
        }

        void MouseInput()
        {
            //���� ������ �� ��������� ��� �������� ����������
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
            {
                //��������� �������������� ���� �� ������
                HexasphereCheckMouseInteraction();

                //���� ��� �� ��������� ����
                Ray inputRay = inputData.Value.camera.ScreenPointToRay(Input.mousePosition);

                //���� ��� �������� �������
                if (Physics.Raycast(inputRay, out RaycastHit hit))
                {
                    //�������� �������� ��������� GO MO
                    //if (hit.transform.gameObject.TryGetComponent(
                    //    out GOMapObject gOMapObject))
                    //{
                        //���� ��������� ������������� MO
                        //gOMapObject.mapObjectPE.Unpack(world.Value, out int mapObjectEntity);
                        //ref CMapObject mapObject = ref mapObjectPool.Value.Get(mapObjectEntity);

                        //���� ��� MO - ������ ��������
                        /*if (mapObject.objectType == MapObjectType.ShipGroup)
                        {
                            //���� ��������� ������ ��������
                            ref CShipGroup shipGroup = ref shipGroupPool.Value.Get(mapObjectEntity);

                            //���� ��� �������� ������ ��������
                            if (inputData.Value.activeShipGroupPE.EqualsTo(mapObject.selfPE) == true)
                            {
                                //������ � ����������
                                inputData.Value.activeShipGroupPE = new();
                            }
                            //�����
                            else
                            {
                                //������ � ��������
                                inputData.Value.activeShipGroupPE = mapObject.selfPE;
                            }
                        }*/
                        //�����, ���� ��� MO - ������
                        /*else if (mapObject.objectType == MapObjectType.Island)
                        {
                            //���� ��������� �������
                            ref CIsland island = ref islandPool.Value.Get(mapObjectEntity);

                            //���� ������ ���
                            if (Input.GetMouseButtonDown(0))
                            {
                                //����������� ����������� ��������� ������� �������
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.Island,
                                    island.selfPE);
                            }

                            //���� ���� �������� ������ ��������
                            if (inputData.Value.activeShipGroupPE.Unpack(world.Value, out int activeShipGroupEntity))
                            {
                                //���� ��������� ������ ��������
                                ref CShipGroup activeShipGroup = ref shipGroupPool.Value.Get(activeShipGroupEntity);

                                //���� ������ �������� ��������� � ������ ��������
                                if (activeShipGroup.movingMode == Ship.ShipGroupMovingMode.Idle)
                                {
                                    //��������� �������� ��������� ��������
                                    ref CSGMoving activeSGMoving = ref sGMovingPool.Value.Add(activeShipGroupEntity);

                                    //��������� �������� ������ ���������� ��������
                                    activeSGMoving = new(0);

                                    //��������� ������ �������� � ����� ��������
                                    activeShipGroup.movingMode = Ship.ShipGroupMovingMode.Moving;

                                    //��������� ������� ������ ��� ������ ����� ����
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
            //�����
            else
            {
                //������ ����� �� ��������� ��� �����������
                InputData.isMouseOver = false;
            }
        }

        void HandleInput()
        {
            //���� ������ ��� ��������
            /*if (GetRegionUnderCursor().Unpack(world.Value, out int regionEntity))
            {
                //���� ��������� �������
                ref CHexRegion currentRegion = ref regionPool.Value.Get(regionEntity);

                //���� ������� ������ �� ��������� � ����������
                if (inputData.Value.previousRegionPE.EqualsTo(currentRegion.selfPE) == false)
                {
                    //��������� �������������� � ������� ������
                    ValidateDrag(ref currentRegion);
                }
                //�����
                else
                {
                    //��������, ��� �������������� ���������
                    inputData.Value.isDrag = false;
                }

                //���� ����� �������������� �������
                if (false)
                {
                    //����������� ������
                    RegionsEdit(ref currentRegion);
                }
                //�����, ���� ������ ������
                else if (Input.GetKey(KeyCode.LeftShift)
                    //� �������� ������ �� ��������� � �������
                    && inputData.Value.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                {
                    //���� ��������� ������ �� ��������� � �������
                    if (inputData.Value.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //���� ��������� ������ �����
                        if (inputData.Value.searchFromRegion.Unpack(world.Value, out int fromRegionEntity))
                        {
                            //���� ��������� ���������� �������
                            ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                            //��������� ��������� ���������� �������
                            fromRegion.DisableHighlight();
                        }

                        //���� �������� ������ �����
                        if (inputData.Value.searchToRegion.Unpack(world.Value, out int toRegionEntity))
                        {
                            //���� ��������� ��������� �������
                            ref CHexRegion toRegion = ref regionPool.Value.Get(toRegionEntity);

                            //���� ����
                            FindPath(ref currentRegion, ref toRegion);
                        }
                    }
                }
                //�����, ���� ��������� ������ �����
                else if (inputData.Value.searchFromRegion.Unpack(world.Value, out int fromRegionEntity)
                    //� ��������� ������ �� ��������� � ��������
                    && inputData.Value.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                {
                    //���� �������� ������ �� ��������� � �������
                    if (inputData.Value.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //��������� �������� ������
                        inputData.Value.searchToRegion = currentRegion.selfPE;

                        //���� ��������� ���������� �������
                        ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                        FindPath(ref fromRegion, ref currentRegion);
                    }
                }
                //�����
                else
                {
                    //����������� ����������� ��������� ������� �������
                    GameDisplayObjectPanelRequest(
                        DisplayObjectPanelRequestType.Region,
                        currentRegion.selfPE);
                }

                //���� �����-���� ������� �������
                //if (inputData.Value.activeShipPE.Unpack(world.Value, out int activeShipEntity))
                //{
                //    //���� ��������� �������
                //    ref CShip activeShip
                //        = ref shipPool.Value.Get(activeShipEntity);

                //    //���� ������� ��������� � ������ ��������
                //    if (activeShip.shipMode
                //        == ShipMode.Idle)
                //    {
                //        //��������� ������� � ����� ������ ����
                //        activeShip.shipMode
                //            = ShipMode.Pathfinding;

                //        //��������� ������� ������
                //        activeShip.targetPE
                //            = currentCell.selfPE;

                //        //��������� ��� �������� �������
                //        activeShip.targetType
                //            = MovementTargetType.Cell;
                //    }
                //}

                //��������� ������ ��� ���������� ��� ���������� �����
                inputData.Value.previousRegionPE = currentRegion.selfPE;
            }
            else
            {
                //������� ���������� ������
                inputData.Value.previousRegionPE = new();
            }*/
        }

        void HexasphereCheckMouseInteraction()
        {
            if (HexasphereCheckMousePosition(out Vector3 position, out Ray ray, out EcsPackedEntity regionPE))
            {
                regionPE.Unpack(world.Value, out int regionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(regionEntity);

                //���� ������ ���
                if (InputData.leftMouseButtonClick)
                {
                    //���� ������ ������
                    if (InputData.leftShiftKeyPressed
                        && InputData.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //���� ��������� ������ �� ��������� � �������
                        if (InputData.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                        {
                            //���� ��������� ������ �����
                            if (InputData.searchFromRegion.Unpack(world.Value, out int fromRegionEntity))
                            {
                                //���� ��������� ���������� �������
                                ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                                //��������� ��������� ���������� �������
                                RegionSetColor(ref fromRegion, MapGenerationData.DefaultShadedColor);
                            }

                            //��������� ��������� ������
                            InputData.searchFromRegion = currentRegion.selfPE;

                            //���� �������� ������ �����
                            /*if (inputData.Value.searchToRegion.Unpack(world.Value, out int toRegionEntity))
                            {
                                //���� ��������� ��������� �������
                                ref CHexRegion toRegion = ref regionPool.Value.Get(toRegionEntity);

                                //���� ����
                                FindPath(ref currentRegion, ref toRegion);
                            }*/
                        }
                    }
                    //�����, ���� ��������� ������ �����
                    else if (InputData.searchFromRegion.Unpack(world.Value, out int fromRegionEntity)
                        //� ��������� ������ �� ��������� � ��������
                        && InputData.searchFromRegion.EqualsTo(currentRegion.selfPE) == false)
                    {
                        //���� �������� ������ �� ��������� � �������
                        if (InputData.searchToRegion.EqualsTo(currentRegion.selfPE) == false)
                        {
                            //��������� �������� ������
                            InputData.searchToRegion = currentRegion.selfPE;

                            //���� ��������� ���������� �������
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
                        //����������� ����������� ��������� ������� �������
                        GameObjectPanelActionRequest(
                            ObjectPanelActionRequestType.Region,
                            currentRegion.selfPE);
                    }
                }
                //�����, ���� ������ ���
                else if (InputData.rightMouseButtonClick)
                {
                    //���� ������ ���+LeftShift
                    if (InputData.RMBAndLeftShift)
                    {
                        //���� ������ �������� ������
                        if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                        {
                            //���� ������ ����
                            if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                            {
                                //���� ����
                                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                                int fleetRegionIndex = -1;

                                //��� ������� ������� �����
                                for (int a = 0; a < fleet.fleetRegions.Count; a++)
                                {
                                    //���� ��� ������� ������
                                    if (fleet.fleetRegions[a].regionPE.EqualsTo(currentRegion.selfPE) == true)
                                    {
                                        //������� ������ �� ���� �� ��� �������
                                        fleet.fleetRegions[a].RemoveNeigbours();

                                        //��������� ������ �������
                                        fleetRegionIndex = a;

                                        break;
                                    }
                                }

                                //���� ������ ������ ��� ����� ����
                                if (fleetRegionIndex >= 0)
                                {
                                    //������� PE ������� �� ������
                                    fleet.fleetRegions.RemoveAt(fleetRegionIndex);

                                    //��������� ��������� �������
                                    RegionHideFleetHighlight(ref currentRegion);
                                }
                            }
                        }
                    }
                    //�����
                    else
                    {
                        //���� ������ �������� ������
                        if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                        {
                            //���� ������ ����
                            if (InputData.activeFleetPE.Unpack(world.Value, out int fleetEntity))
                            {
                                //���� ����
                                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                                //������ �����, �������� ������ ������� �����
                                DFleetRegion currentFleetRegion = new(
                                    currentRegion.selfPE);

                                //���� ������ �� �������� ������ ������
                                if (fleet.fleetRegions.Contains(currentFleetRegion) == false)
                                {
                                    //��������� � ������ ������� �������� ������� ����� ��� ������������ �������
                                    currentFleetRegion.AddNeighbours(
                                        ref currentRegion,
                                        fleet.fleetRegions);

                                    //������� PE ������� � ������
                                    fleet.fleetRegions.Add(currentFleetRegion);

                                    //�������� ��������� �������
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
            //���������, ��������� �� ������ ��� �����������
            InputData.isMouseOver = HexasphereGetHitPoint(out position, out ray);

            regionPE = new();

            //���� ������ ��������� ��� �����������
            if (InputData.isMouseOver == true)
            {
                //���������� ������ �������, ��� ������� ��������� ������
                int regionIndex = RegionGetInRayDirection(
                    ray, position,
                    out Vector3 hitPosition);

                //���� ������ ������� ������ ����
                if (regionIndex >= 0)
                {
                    //���� ������
                    RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //���� ������ ������� �� ����� ������� ���������� �������������
                    if (region.Index != InputData.lastHighlightedRegionIndex)
                    {
                        //���� ������ ���������� ������������� ������� ������ ����
                        if (InputData.lastHighlightedRegionIndex > 0)
                        {
                            //�������� ��������� ���������
                            RegionHideHoverHighlight();
                        }

                        //��������� ������������ ������
                        InputData.lastHighlightedRegionPE = region.selfPE;
                        InputData.lastHighlightedRegionIndex = region.Index;

                        //������������ ������
                        RegionShowHoverHighlight(ref region);
                    }

                    regionPE = region.selfPE;

                    return true;
                }

                //���� ������ ������� ������ ���� � ������� ������ - �� ������ ���������� �������������
                /*if (regionIndex >= 0 && regionIndex != InputData.lastHighlightedRegionIndex)
                {
                    //���� ������� ������
                    RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    //��������� ������ ���������� �������, ��� ������� ��������� ������
                    InputData.lastHoverRegionIndex = region.Index;

                    //���� ��������� ������������ ������ �� ����
                    if (InputData.lastHighlightedRegionPE.Unpack(world.Value, out int highlightedRegionEntity))
                    {
                        //���� ������������ ������
                        ref CHexRegion highlightedRegion = ref regionPool.Value.Get(highlightedRegionEntity);

                        //�������� ���������
                        RegionHideHighlighted();
                    }

                    //��������� ������� ������������ ������
                    InputData.lastHighlightedRegionPE = region.selfPE;
                    InputData.lastHighlightedRegionIndex = region.Index;

                    //������������ ������
                    RegionSetMaterial(
                        regionIndex,
                        mapGenerationData.Value.regionHighlightMaterial,
                        true);
                }*/
                //�����, ���� ������ ������� ������ ���� � ������ ���������� ������������� ������� ������ ����
                else if (regionIndex < 0 && InputData.lastHighlightedRegionIndex >= 0)
                {
                    //�������� ��������� ���������
                    RegionHideHoverHighlight();
                }
            }
            
            return false;
        }

        bool HexasphereGetHitPoint(
            out Vector3 position,
            out Ray ray)
        {
            //���� ��� �� ��������� ����
            ray = inputData.Value.camera.ScreenPointToRay(Input.mousePosition);

            //�������� �������
            return HexasphereGetHitPoint(ray, out position);
        }

        bool HexasphereGetHitPoint(
            Ray ray,
            out Vector3 position)
        {
            //���� ��� �������� �������
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //���� ��� �������� ������� ����������
                if (hit.collider.gameObject == SceneData.HexasphereGO)
                {
                    //���������� ����� �������
                    position = hit.point;

                    //� ���������� true
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

            //���������� �������� ����� �������
            Vector3 minPoint = worldPosition;
            Vector3 maxPoint = worldPosition + 0.5f * MapGenerationData.hexasphereScale * ray.direction;

            float rangeMin = MapGenerationData.hexasphereScale * 0.5f;
            rangeMin *= rangeMin;

            float rangeMax = worldPosition.sqrMagnitude;

            float distance;
            Vector3 bestPoint = maxPoint;

            //�������� �����
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

            //���� ������ ������� 
            int nearestRegionIndex = RegionGetAtLocalPosition(SceneData.HexasphereGO.transform.InverseTransformPoint(worldPosition));
            //���� ������ ������ ����, ������� �� �������
            if (nearestRegionIndex < 0)
            {
                return -1;
            }

            //���������� ������ �������
            Vector3 currentPoint = worldPosition;

            //���� ��������� ������
            RegionsData.regionPEs[nearestRegionIndex].Unpack(world.Value, out int nearestRegionEntity);
            ref CHexRegion nearestRegion = ref regionPool.Value.Get(nearestRegionEntity);

            //���������� ������� ����� �������
            Vector3 regionTop = SceneData.HexasphereGO.transform.TransformPoint(
                nearestRegion.center * (1.0f + nearestRegion.ExtrudeAmount * MapGenerationData.extrudeMultiplier));

            //���������� ������ ������� � ������ ����
            float regionHeight = regionTop.sqrMagnitude;
            float rayHeight = currentPoint.sqrMagnitude;

            float minDistance = 1e6f;
            distance = minDistance;

            //���������� ������ �������-���������
            int candidateRegionIndex = -1;

            const int NUM_STEPS = 10;
            //�������� �����
            for (int a = 1; a <= NUM_STEPS; a++)
            {
                distance = Mathf.Abs(rayHeight - regionHeight);

                //���� ���������� ������ ������������
                if (distance < minDistance)
                {
                    //��������� ����������� ���������� � ���������
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

                //��������� ��������� ������
                RegionsData.regionPEs[nearestRegionIndex].Unpack(world.Value, out nearestRegionEntity);
                nearestRegion = ref regionPool.Value.Get(nearestRegionEntity);

                regionTop = SceneData.HexasphereGO.transform.TransformPoint(
                    nearestRegion.center * (1.0f + nearestRegion.ExtrudeAmount * MapGenerationData.extrudeMultiplier));

                //���������� ������ ������� � ������ ����
                regionHeight = regionTop.sqrMagnitude;
                rayHeight = currentPoint.sqrMagnitude;
            }

            //���� ���������� ������ ������������
            if (distance < minDistance)
            {
                //��������� ����������� ���������� � ���������
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
            //���������, �� ��������� �� ��� ������
            if (InputData.lastHitRegionIndex >= 0 && InputData.lastHitRegionIndex < RegionsData.regionPEs.Length)
            {
                //���� ��������� ������
                RegionsData.regionPEs[InputData.lastHitRegionIndex].Unpack(world.Value, out int lastHitRegionEntity);
                ref CHexRegion lastHitRegion = ref regionPool.Value.Get(lastHitRegionEntity);

                //���������� ���������� �� ������ �������
                float distance = Vector3.SqrMagnitude(lastHitRegion.center - localPosition);

                bool isValid = true;

                //��� ������� ��������� �������
                for (int a = 0; a < lastHitRegion.neighbourRegionPEs.Length; a++)
                {
                    //���� �������� ������
                    lastHitRegion.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���������� ���������� �� ������ �������
                    float otherDistance = Vector3.SqrMagnitude(neighbourRegion.center - localPosition);

                    //���� ��� ������ ���������� �� ���������� �������
                    if (otherDistance < distance)
                    {
                        //�������� ��� � ������� �� �����
                        isValid = false;
                        break;
                    }
                }

                //���� ��� ��������� ������
                if (isValid == true)
                {
                    return InputData.lastHitRegionIndex;
                }
            }
            //�����
            else
            {
                //�������� ������ ���������� �������
                InputData.lastHitRegionIndex = 0;
            }

            //������� ����������� ���� � ������������ ����������
            RegionsData.regionPEs[InputData.lastHitRegionIndex].Unpack(world.Value, out int nearestRegionEntity);
            ref CHexRegion nearestRegion = ref regionPool.Value.Get(nearestRegionEntity);

            float minDist = 1e6f;

            //��� ������� �������
            for (int a = 0; a < RegionsData.regionPEs.Length; a++)
            {
                //���� ��������� ������ 
                RegionGetNearestToPosition(
                    ref nearestRegion.neighbourRegionPEs,
                    localPosition,
                    out float regionDistance).Unpack(world.Value, out int newNearestRegionEntity);

                //���� ���������� ������ ������������
                if (regionDistance < minDist)
                {
                    //��������� ������ � ����������� ���������� 
                    nearestRegion = ref regionPool.Value.Get(newNearestRegionEntity);

                    minDist = regionDistance;
                }
                //����� ������� �� �����
                else
                {
                    break;
                }
            }

            //������ ���������� ������� - ��� ������ ����������
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

            //��� ������� ������� � �������
            for (int a = 0; a < regionPEs.Length; a++)
            {
                //���� ������
                regionPEs[a].Unpack(world.Value, out int regionEntity);
                ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                //���� ����� �������
                Vector3 center = region.center;

                //������������ ����������
                float distance 
                    = (center.x - localPosition.x) * (center.x - localPosition.x) 
                    + (center.y - localPosition.y) * (center.y - localPosition.y) 
                    + (center.z - localPosition.z) * (center.z - localPosition.z);

                //���� ���������� ������ ������������
                if (distance < minDistance)
                {
                    //��������� ������ � ����������� ����������
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
            //���� ������
            RegionsData.regionPEs[regionIndex].Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

            //���� ����������� ��������� ��������
            if (temporary == true)
            {
                //���� ���� ��������� �������� ��� �������� �������
                if (region.tempMaterial == material)
                {
                    //�� ������ �� ��������
                    return false;
                }

                //��������� ��������� �������� ��������� �������
                region.hoverRenderer.sharedMaterial = material;
                region.hoverRenderer.enabled = true;
            }
            //�����
            else
            {
                //���� ���� �������� �������� ��� �������� �������
                if (region.customMaterial == material)
                {
                    //�� ������ �� ��������
                    return false;
                }

                //���� ���� ���������
                Color32 materialColor = Color.white;
                if (material.HasProperty(ShaderParameters.Color) == true)
                {
                    materialColor = material.color;
                }
                else if (material.HasProperty(ShaderParameters.BaseColor))
                {
                    materialColor = material.GetColor(ShaderParameters.BaseColor);
                }

                //��������, ��� ��������� ���������� ������
                mapGenerationData.Value.isColorUpdated = true;

                //���� �������� ���������
                Texture materialTexture = null;
                if (material.HasProperty(ShaderParameters.MainTex))
                {
                    materialTexture = material.mainTexture;
                }
                else if (material.HasProperty(ShaderParameters.BaseMap))
                {
                    materialTexture = material.GetTexture(ShaderParameters.BaseMap);
                }

                //���� �������� �� �����
                if (materialTexture != null)
                {
                    //��������, ��� ��������� ���������� ������� �������
                    mapGenerationData.Value.isTextureArrayUpdated = true;
                }
                //�����
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

            //���� �������� - �� �������� ��������� ���������
            if (material != mapGenerationData.Value.hoverRegionHighlightMaterial)
            {
                //���� ��� ��������� ��������
                if (temporary == true)
                {
                    region.tempMaterial = material;
                }
                //�����
                else
                {
                    region.customMaterial = material;
                }
            }

            //���� �������� ��������� �� ���� � ������ - ��� ��������� ������������ ������
            if (mapGenerationData.Value.hoverRegionHighlightMaterial != null && InputData.lastHighlightedRegionIndex == region.Index)
            {
                //����� ��������� �������� ��������� ���������
                region.hoverRenderer.sharedMaterial = mapGenerationData.Value.hoverRegionHighlightMaterial;

                //���� �������� �������� 
                Material sourceMaterial = null;
                if (region.tempMaterial != null)
                {
                    sourceMaterial = region.tempMaterial;
                }
                else if (region.customMaterial != null)
                {
                    sourceMaterial = region.customMaterial;
                }

                //���� �������� �������� �� ����
                if (sourceMaterial != null)
                {
                    //���� ���� ��������� ���������
                    Color32 color = Color.white;
                    if (sourceMaterial.HasProperty(ShaderParameters.Color) == true)
                    {
                        color = sourceMaterial.color;
                    }
                    else if (sourceMaterial.HasProperty(ShaderParameters.BaseColor))
                    {
                        color = sourceMaterial.GetColor(ShaderParameters.BaseColor);
                    }
                    //������������� ��������� ���� ��������� ��������� ���������
                    mapGenerationData.Value.hoverRegionHighlightMaterial.SetColor(ShaderParameters.Color2, color);

                    //���� �������� ��������� ���������
                    Texture tempMaterialTexture = null;
                    if (sourceMaterial.HasProperty(ShaderParameters.MainTex))
                    {
                        tempMaterialTexture = sourceMaterial.mainTexture;
                    }
                    else if (sourceMaterial.HasProperty(ShaderParameters.BaseMap))
                    {
                        tempMaterialTexture = sourceMaterial.GetTexture(ShaderParameters.BaseMap);
                    }

                    //���� �������� �� �����
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
            //���������� �������� ���������
            region.hoverRenderer.enabled = true;
        }

        void RegionHideHoverHighlight()
        {
            //���� ������������ ������ ����������
            if (InputData.lastHighlightedRegionPE.Unpack(world.Value, out int highlightedRegionEntity))
            {
                //���� ������
                ref CHexRegion highlightedRegion = ref regionPool.Value.Get(highlightedRegionEntity);

                //��������� �������� ���������
                highlightedRegion.hoverRenderer.enabled = false;
            }

            //������� ��������� ������������ ������
            InputData.lastHighlightedRegionPE = new();
            InputData.lastHighlightedRegionIndex = -1;
        }

        void RegionShowFleetHighlight(
            ref CHexRegion region)
        {
            //���������� �������� �����
            region.fleetRenderer.enabled = true;
        }

        void RegionHideFleetHighlight(
            ref CHexRegion region)
        {
            //��������� �������� �����
            region.fleetRenderer.enabled = false;
        }

        void RegionSetExtrudeAmount(
            ref CHexRegion region,
            float extrudeAmount)
        {
            //���� ������ ������� ��� ����� ��������, ������� �� �������
            if (region.ExtrudeAmount == extrudeAmount)
            {
                return;
            }

            //������������ ������
            extrudeAmount = Mathf.Clamp01(extrudeAmount);

            //��������� ������ �������
            region.ExtrudeAmount = extrudeAmount;

            //���� ���� �������
            List<Vector4> uvShadedChunk = MapGenerationData.uvShaded[region.uvShadedChunkIndex];

            //��� ������ UV-��������� �������
            for (int a = 0; a < region.uvShadedChunkLength; a++)
            {
                //���� ����������
                Vector4 uv4 = uvShadedChunk[region.uvShadedChunkStart + a];

                //��������� W-��������
                uv4.w = region.ExtrudeAmount;

                //��������� UV-���������� � �����
                uvShadedChunk[region.uvShadedChunkStart + a] = uv4;
            }

            //��������, ��� UV-���������� ������� ����������
            MapGenerationData.uvShadedDirty[region.uvShadedChunkIndex] = true;
            mapGenerationData.Value.isUVUpdatedFast = true;
        }

        void RegionSetColor(
            ref CHexRegion region,
            Color color)
        {
            //���� ������������ ��������
            Material material;

            //���� �������� ������ ����� ��� ���������� � ���� ������� ����������
            if (MapGenerationData.colorCache.ContainsKey(color) == false)
            {
                //�� ������ ����� �������� � �������� ���
                material = GameObject.Instantiate(mapGenerationData.Value.regionColoredMaterial);
                MapGenerationData.colorCache.Add(color, material);

                //��������� �������� ������ ���������
                material.hideFlags = HideFlags.DontSave;
                material.color = color;
                material.SetFloat(ShaderParameters.RegionAlpha, 1f);
            }
            //�����
            else
            {
                //���� �������� �� �������
                material = MapGenerationData.colorCache[color];
            }

            //������������� �������� �������
            RegionSetMaterial(
                region.Index,
                material);
        }

        void RegionAddFeature(
            ref CHexRegion region,
            GameObject featureGO)
        {
            //���� ����� �������
            Vector3 regionCenter = region.GetCenter();

            //���������� GO � ����� �������
            featureGO.transform.position = regionCenter;
            featureGO.transform.SetParent(SceneData.HexasphereGO.transform);
            featureGO.transform.LookAt(SceneData.HexasphereGO.transform);
        }

        void QuitGameRequest()
        {
            //������ ����� �������� � ��������� �� ��������� ������� ������ �� ����
            int requestEntity = world.Value.NewEntity();
            ref RQuitGame quitGameRequest = ref quitGameRequestPool.Value.Add(requestEntity);
        }
    }
}