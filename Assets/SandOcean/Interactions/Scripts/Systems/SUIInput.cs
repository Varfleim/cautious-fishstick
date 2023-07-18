
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
        //����
        readonly EcsWorldInject world = default;
        EcsWorld uguiSOWorld;
        EcsWorld uguiUIWorld;


        //������
        //readonly EcsPoolInject<CPlayer> playerPool = default;

        //������� �����
        readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //�����
        readonly EcsPoolInject<CHexChunk> chunkPool = default;

        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //���������������-������������� �������
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        //�������
        readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

        //readonly EcsPoolInject<CSGMoving> sGMovingPool = default;


        //������� �������� ����
        readonly EcsPoolInject<EMainMenuAction> mainMenuActionEventPool = default;

        //������� ���� ����� ����
        readonly EcsPoolInject<ENewGameMenuAction> newGameMenuActionEventPool = default;

        //������� ���� ��������

        //������� ����������
        readonly EcsPoolInject<EWorkshopAction> workshopActionEventPool = default;

        //������� ���������
        readonly EcsPoolInject<EDesignerAction> designerActionEventPool = default;

        //������� ��������� ��������
        readonly EcsPoolInject<EDesignerShipClassAction> designerShipClassActionEventPool = default;
        //������� ��������� �����������
        readonly EcsPoolInject<EDesignerComponentAction> designerComponentActionEventPool = default;

        //������� ����
        readonly EcsPoolInject<EGameAction> gameActionEventPool = default;

        readonly EcsPoolInject<EGameOpenDesigner> gameOpenDesignerEventPool = default;

        readonly EcsPoolInject<EGameDisplayObjectPanel> gameDisplayObjectPanelEventPool = default;

        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //������� ���������������-������������� ��������
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        //������� �����
        readonly EcsPoolInject<SRMapChunkRefresh> mapChunkRefreshSelfRequestPool = default;

        //����� �������
        EcsFilter clickEventSpaceFilter;
        EcsPool<EcsUguiClickEvent> clickEventSpacePool;
        
        EcsFilter clickEventUIFilter;
        EcsPool<EcsUguiClickEvent> clickEventUIPool;

        EcsFilter dropdownEventUIFilter;
        EcsPool<EcsUguiTmpDropdownChangeEvent> dropdownEventUIPool;

        EcsFilter sliderEventUIFilter;
        EcsPool<EcsUguiSliderChangeEvent> sliderEventUIPool;

        readonly EcsPoolInject<EQuitGame> quitGameEventPool = default;

        //������
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

            mapGenerationData.Value.terrainMaterial.EnableKeyword("_SHOW_GRID");

            Shader.EnableKeyword("_HEX_MAP_EDIT_MODE");
            //Shader.DisableKeyword("_HEX_MAP_EDIT_MODE");
        }

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ����� �� ����������
            foreach (int clickEventUIEntity in clickEventUIFilter)
            {
                //���� ��������� �������
                ref EcsUguiClickEvent clickEvent = ref clickEventUIPool.Get(clickEventUIEntity);

                Debug.LogWarning(
                    "Click! "
                    + clickEvent.WidgetName);

                //���� ������� ���� ����
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.Game)
                {
                    //���� ������ �� ���� ����
                    UIGameWindow gameWindow
                        = eUI.Value.gameWindow;

                    //���� ��������� ������� ������
                    inputData.Value.playerOrganizationPE.Unpack(world.Value, out int playerFactionEntity);
                    ref COrganization playerFaction
                        = ref organizationPool.Value.Get(playerFactionEntity);

                    //���� ������� ����������� �������� ��������� ��������
                    if (clickEvent.WidgetName
                        == "OpenShipClassDesigner")
                    {
                        //����������� �������� ��������� ��������
                        GameOpenDesignerEvent(
                            DesignerType.ShipClass,
                            playerFaction.contentSetIndex);
                    }
                    //�����, ���� ������� ����������� �������� ��������� ����������
                    else if (clickEvent.WidgetName
                        == "OpenEngineDesigner")
                    {
                        //����������� �������� ��������� ����������
                        GameOpenDesignerEvent(
                            DesignerType.ComponentEngine,
                            playerFaction.contentSetIndex);
                    }
                    //�����, ���� ������� ����������� �������� ��������� ���������
                    else if (clickEvent.WidgetName
                        == "OpenReactorDesigner")
                    {
                        //����������� �������� ��������� ���������
                        GameOpenDesignerEvent(
                            DesignerType.ComponentReactor,
                            playerFaction.contentSetIndex);
                    }
                    //�����, ���� ������� ����������� �������� ��������� ��������� �����
                    else if (clickEvent.WidgetName
                        == "OpenFuelTankDesigner")
                    {
                        //����������� �������� ��������� ��������� �����
                        GameOpenDesignerEvent(
                            DesignerType.ComponentHoldFuelTank,
                            playerFaction.contentSetIndex);
                    }
                    //�����, ���� ������� ����������� �������� ��������� ������������ ��� ������ ������
                    else if (clickEvent.WidgetName
                        == "OpenExtractionEquipmentSolidDesigner")
                    {
                        //����������� �������� ��������� ������������ ��� ������ ������
                        GameOpenDesignerEvent(
                            DesignerType.ComponentExtractionEquipmentSolid,
                            playerFaction.contentSetIndex);
                    }
                    //�����, ���� ������� ����������� �������� ��������� �������������� ������
                    else if (clickEvent.WidgetName
                        == "OpenEnergyGunDesigner")
                    {
                        //����������� �������� ��������� �������������� ������
                        GameOpenDesignerEvent(
                            DesignerType.ComponentGunEnergy,
                            playerFaction.contentSetIndex);
                    }

                    //���� ������� ������ �������
                    if (gameWindow.activeMainPanelType == MainPanelType.Object)
                    {
                        //���� ������ �������
                        UIObjectPanel objectPanel = gameWindow.objectPanel;

                        //���� ������� ��������� �����������
                        if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
                        {

                        }
                        //�����, ���� ������� ��������� �������
                        else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                        {
                            //���� ��������� �������
                            UIRegionObjectSubpanel regionSubpanel = objectPanel.regionObjectSubpanel;

                            //���� ������ ������ �������� �������
                            if (clickEvent.WidgetName == "RegionOverviewTab")
                            {
                                //����������� ����������� �������� �������
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.RegionOverview,
                                    objectPanel.activeObjectPE);
                            }
                            //�����, ���� ������ ������ ������� �����������
                            else if (clickEvent.WidgetName == "RegionOrganizationsTab")
                            {
                                //����������� ����������� ������� �����������
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.RegionOrganizations,
                                    objectPanel.activeObjectPE);
                            }

                            //�����, ���� ������� �������� �������
                            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.overviewTab.selfTabButton)
                            {
                                //���� ������ ������ �����������
                                if (clickEvent.WidgetName == "RAEOColonize")
                                {
                                    //���� ��������� ����������� ������
                                    inputData.Value.playerOrganizationPE.Unpack(world.Value, out int organizationEntity);
                                    ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                                    //���� ��������� RAEO
                                    objectPanel.activeObjectPE.Unpack(world.Value, out int rAEOEntity);
                                    ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                                    //���� ��������� ExORAEO ������ �����������
                                    rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                                    ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);

                                    //��������� �������� ORAEO ���������� ��������
                                    ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Add(oRAEOEntity);

                                    //��������� ������ �����������
                                    oRAEOActionSR = new(ORAEOActionType.Colonization);
                                }
                            }
                            //�����, ���� ������� ������� �����������
                            else if (regionSubpanel.tabGroup.selectedTab == regionSubpanel.organizationsTab.selfTabButton)
                            {
                                //���� �������� ������� ������ ��������� ORAEOBriefInfoPanel
                                if (clickEvent.Sender.TryGetComponent(out UIORAEOBriefInfoPanel briefInfoPanel))
                                {
                                    //����������� ����������� ��������� ORAEO
                                    GameDisplayObjectPanelEvent(
                                        DisplayObjectPanelEventType.ORAEO,
                                        briefInfoPanel.organizationRAEOPE);
                                }
                            }
                        }
                        //�����, ���� ������� ��������� ORAEO
                        else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
                        {
                            //���� ��������� ORAEO
                            UIORAEOObjectSubpanel oRAEOSubpanel = objectPanel.oRAEOObjectSubpanel;

                            //���� ������ ������ �������� �������
                            if (clickEvent.WidgetName == "ORAEOOverviewTab")
                            {
                                //����������� ����������� �������� �������
                                GameDisplayObjectPanelEvent(
                                    DisplayObjectPanelEventType.ORAEOOverview,
                                    objectPanel.activeObjectPE);
                            }
                        }
                    }
                }
                //���� ������� ���� �������� ����
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.MainMenu)
                {
                    //���� ������� ����������� �������� ���� ����� ����
                    if (clickEvent.WidgetName
                        == "OpenNewGameMenu")
                    {
                        //������ �������, ������������� �������� ���� ����� ����
                        MainMenuActionEvent(
                            MainMenuActionType.OpenNewGameMenu);
                    }
                    //�����, ���� ������� ����������� �������� ���� �������� ����
                    else if (clickEvent.WidgetName
                        == "OpenLoadGameMenu")
                    {
                        //������ �������, ������������� �������� ���� �������� ����
                        MainMenuActionEvent(
                            MainMenuActionType.OpenLoadGameMenu);
                    }
                    //�����, ���� ������� ����������� �������� ���� ����������
                    else if (clickEvent.WidgetName
                        == "OpenWorkshop")
                    {
                        //������ �������, ������������� �������� ���� ����������
                        MainMenuActionEvent(
                            MainMenuActionType.OpenWorkshop);
                    }
                    //�����, ���� ������� ����������� �������� ���� ������� ��������
                    else if (clickEvent.WidgetName
                        == "OpenSettings")
                    {
                        //������ �������, ������������� �������� ���� ������� ��������
                        MainMenuActionEvent(
                            MainMenuActionType.OpenMainSettings);
                    }
                    //�����, ���� ������� ����������� ����� �� ����
                    else if (clickEvent.WidgetName
                        == "QuitGame")
                    {
                        //������ �������, ������������� ����� �� ����
                        QuitGameEvent();
                    }
                }
                //���� ������� ���� ����� ����
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {
                    //���� ������ �� ���� ���� ����� ����
                    UINewGameMenuWindow newGameMenuWindow
                        = eUI.Value.newGameMenuWindow;

                    //���� ������� ����������� ������ ����� ����
                    if (clickEvent.WidgetName
                        == "StartNewGame")
                    {
                        //������ �������, ������������� ������ ����� ����
                        NewGameMenuActionEvent(
                            NewGameMenuActionType.StartNewGame);
                    }
                }
                //���� ������� ����������
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Workshop)
                {
                    //���� ������ �� ���� ����������
                    UIWorkshopWindow workshopWindow
                        = eUI.Value.workshopWindow;

                    //���� �������� ������� �����������
                    if (clickEvent.WidgetName
                        == "")
                    {
                        //�������� �������� ������ ������ ��������
                        if (clickEvent.Sender.TryGetComponent(
                            out UIWorkshopContentSetPanel workshopContentSetPanel))
                        {
                            //����������� ����������� ���������� ������ ��������
                            WorkshopActionEvent(
                                WorkshopActionType.DisplayContentSet,
                                workshopContentSetPanel.contentSetIndex);
                        }
                    }
                    //�����, ���� ������� ����������� �������� ��������� ��������
                    else if (clickEvent.WidgetName
                        == "WorkshopOpenDesigner")
                    {
                        //���� �������� ������������� � ������ ��������
                        Toggle activeToggle
                            = workshopWindow.contentInfoToggleGroup.GetFirstActiveToggle();

                        //���� �� �� ����
                        if (activeToggle != null)
                        {
                            //�������� �������� ������ ���������� ���� ��������
                            if (activeToggle.TryGetComponent(
                                out UIWorkshopContentInfoPanel workshopContentInfoPanel))
                            {
                                //����������� ����������� ���������� ��������� � ������� ������ ��������
                                WorkshopActionEvent(
                                    WorkshopActionType.OpenDesigner,
                                    workshopWindow.currentContentSetIndex,
                                    workshopContentInfoPanel.designerType);
                            }
                        }
                    }
                }
                //���� ������� ��������
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Designer)
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
                            DesignerActionEvent(
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
                                DesignerActionEvent(
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
                                DesignerActionEvent(
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
                                    DesignerActionEvent(
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
                        DesignerActionEvent(
                            DesignerActionType.DisplayContentSetPanel,
                            false);
                    }
                    //�����, ���� ������� ����������� �������� ������ ������ ������� ��������
                    else if (clickEvent.WidgetName
                        == "HideOtherContentSets")
                    {
                        //����������� �������� ������ ������ ������� ��������
                        DesignerActionEvent(
                            DesignerActionType.HideContentSetPanel,
                            false);
                    }
                    //�����, ���� ������� ����������� ����������� ������ �������� ������ ��������
                    else if (clickEvent.WidgetName
                        == "DisplayCurrentContentSet")
                    {
                        //����������� ����������� ������ �������� ������ ��������
                        DesignerActionEvent(
                            DesignerActionType.DisplayContentSetPanel,
                            true);
                    }
                    //�����, ���� ������� ����������� �������� ������ ������ ������� ��������
                    else if (clickEvent.WidgetName
                        == "HideCurrentContentSet")
                    {
                        //����������� �������� ������ �������� ������ ��������
                        DesignerActionEvent(
                            DesignerActionType.HideContentSetPanel,
                            true);
                    }
                    //�����, ���� ������� �������� ��������
                    else if (designerWindow.designerType
                        == DesignerType.ShipClass)
                    {
                        //���� ������ �� ���� ��������� ��������
                        UIShipDesignerWindow shipClassDesignerWindow
                            = eUI.Value.designerWindow.shipDesigner;

                        //���� ������� �� ����� �������
                        if (clickEvent.WidgetName
                            == "")
                        {
                            //���� �������� ������������� �� ������ ��������� �����������
                            Toggle activeToggleAvailableComponent
                                = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                            //���� �������� ������������� �� ������ ������������� �����������
                            Toggle activeToggleInstalledComponent
                                = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                            //���� ������������� � ������ ��������� ����������� �� ����
                            if (activeToggleAvailableComponent
                                != null
                                //� �������� ������������ �������� �������
                                && activeToggleAvailableComponent.gameObject
                                == clickEvent.Sender)
                            {
                                //�������� �������� ������ ���������� ����������
                                if (activeToggleAvailableComponent.TryGetComponent(
                                    out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                                {
                                    //����������� ����������� ��������� ���������� � ����������
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                        shipClassDesignerWindow.currentAvailableComponentsType,
                                        contentObjectBriefInfoPanel.contentSetIndex,
                                        contentObjectBriefInfoPanel.objectIndex);

                                    //���� ������������� � ������ ������������� ����������� �������
                                    if (activeToggleInstalledComponent
                                        != null)
                                    {
                                        activeToggleInstalledComponent.isOn
                                            = false;
                                    }
                                }
                            }
                            //�����, ���� ������������� � ������ ������������� ����������� �� ����
                            else if (activeToggleInstalledComponent
                                != null
                                //� �������� ������������ �������� �������
                                && activeToggleInstalledComponent.gameObject
                                == clickEvent.Sender)
                            {
                                //�������� �������� ������ ���������� ����������
                                if (activeToggleInstalledComponent.TryGetComponent(
                                    out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                                {
                                    //����������� ����������� ��������� ���������� � ����������
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.DisplayComponentDetailedInfo,
                                        componentBriefInfoPanel.componentType,
                                        componentBriefInfoPanel.contentSetIndex,
                                        componentBriefInfoPanel.componentIndex);

                                    //���� ������������� � ������ ��������� ����������� �������
                                    if (activeToggleAvailableComponent
                                        != null)
                                    {
                                        activeToggleAvailableComponent.isOn
                                            = false;
                                    }
                                }
                            }
                            //�����, ���� ������������ ������ ������� ����� ��������� Toggle
                            else if (clickEvent.Sender.TryGetComponent(
                                out Toggle eventSenderToggle))
                            {
                                //���� ������������ ToggleGroup �������� ������ ��������� �����������
                                if (eventSenderToggle.group
                                    == shipClassDesignerWindow.availableComponentsListToggleGroup
                                    //��� ���� ������������ ToggleGroup �������� ������ ������������� �����������
                                    || eventSenderToggle.group
                                    == shipClassDesignerWindow.installedComponentsListToggleGroup)
                                {
                                    //����������� �������� ��������� ���������� � ����������
                                    DesignerShipClassActionEvent(
                                        DesignerShipClassActionType.HideComponentDetailedInfo);
                                }
                            }
                        }
                        //���� ������� ����������� ���������� ���������� ���������� � ������������� �������
                        if (clickEvent.WidgetName
                            == "AddComponentToShipClass")
                        {
                            //���� �������� ������������� �� ������ ��������� �����������
                            Toggle activeToggleAvailableComponent
                                = shipClassDesignerWindow.availableComponentsListToggleGroup.GetFirstActiveToggle();

                            //���� ������������� �� ����
                            if (activeToggleAvailableComponent
                                != null)
                            {
                                //�������� �������� ������ ���������� ����������
                                if (activeToggleAvailableComponent.TryGetComponent(
                                    out UIContentObjectBriefInfoPanel contentObjectBriefInfoPanel))
                                {
                                    //������ ������� ���������� ����������
                                    DesignerShipClassActionEvent(
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
                                Toggle activeToggleInstalledComponent
                                    = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                                //���� ������������� �� ����
                                if (activeToggleInstalledComponent
                                    != null)
                                {
                                    //�������� �������� ������ ���������� ����������
                                    if (activeToggleInstalledComponent.TryGetComponent(
                                        out UIInstalledComponentBriefInfoPanel installedComponentBriefInfoPanel))
                                    {
                                        //������ ������� ���������� ����������
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
                        //�����, ���� ������� ����������� �������� ���������� ���������� �� �������������� �������
                        else if (clickEvent.WidgetName
                            == "DeleteComponentFromShipClass")
                        {
                            //���� �������� ������������� �� ������ ������������� �����������
                            Toggle activeToggleInstalledComponent
                                = shipClassDesignerWindow.installedComponentsListToggleGroup.GetFirstActiveToggle();

                            //���� ������������� �� ����
                            if (activeToggleInstalledComponent
                                != null)
                            {
                                //�������� �������� ������ ���������� ����������
                                if (activeToggleInstalledComponent.TryGetComponent(
                                    out UIInstalledComponentBriefInfoPanel componentBriefInfoPanel))
                                {
                                    //������ ������� �������� ����������
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
                    //�����, ���� ������� �������� ����������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentEngine)
                    {

                    }
                    //�����, ���� ������� �������� ���������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentReactor)
                    {

                    }
                    //�����, ���� ������� �������� ��������� �����
                    else if (designerWindow.designerType
                        == DesignerType.ComponentHoldFuelTank)
                    {

                    }
                    //�����, ���� ������� �������� ������������ ��� ������ ������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentExtractionEquipmentSolid)
                    {

                    }
                    //�����, ���� ������� �������� �������������� ������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentGunEnergy)
                    {

                    }
                }

                uguiUIWorld.DelEntity(clickEventUIEntity);
            }

            //��� ������� ������� ��������� �������� ����������� ������
            foreach (int dropdownEventUIEntity in dropdownEventUIFilter)
            {
                //���� ��������� �������
                ref EcsUguiTmpDropdownChangeEvent dropdownEvent = ref dropdownEventUIPool.Get(dropdownEventUIEntity);

                Debug.LogWarning(
                    "Dropdown change! " + dropdownEvent.WidgetName
                    + " ! " + dropdownEvent.Value);

                //���� ������� ��������
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.Designer)
                {
                    //���� ������ �� ���� ���������
                    UIDesignerWindow designerWindow
                        = eUI.Value.designerWindow;

                    //���� ������� ����������� ����� ������ �������� �� ������ ������ ������� ��������
                    if (dropdownEvent.WidgetName
                        == "ChangeOtherContentSet")
                    {
                        //���������� �������� ������ ������ ��������
                        int contentSetIndex
                            = -1;

                        //���� ������ ����������� ������ ������, ��� ������ �������� ������ ��������
                        if (dropdownEvent.Value
                            < designerWindow.currentContentSetIndex)
                        {
                            //�� ��������� ������ ������������� ������� �� ������
                            contentSetIndex
                                = dropdownEvent.Value;
                        }
                        //�����
                        else
                        {
                            //��������� ������ ������ �� 1
                            contentSetIndex
                                = dropdownEvent.Value + 1;
                        }

                        //����������� ����� ������ �������� �� ������ ������ ������� ��������
                        DesignerActionEvent(
                            DesignerActionType.DisplayContentSetPanelList,
                            false,
                            contentSetIndex);
                    }
                    //�����, ���� ������� �������� ��������
                    else if (designerWindow.designerType
                        == DesignerType.ShipClass)
                    {
                        //���� ������ �� ���� ��������� ��������
                        UIShipDesignerWindow shipDesignerWindow
                            = designerWindow.shipDesigner;

                        //���� ������� ����������� ����� ���� ��������� �����������
                        if (dropdownEvent.WidgetName
                            == "ChangeAvailableComponentsType")
                        {
                            //����������� ��������� ���� ��������� �����������
                            DesignerShipClassActionEvent(
                                DesignerShipClassActionType.ChangeAvailableComponentsType,
                                (ShipComponentType)shipDesignerWindow.availableComponentTypeDropdown.value);
                        }
                    }
                    //�����, ���� ������� �������� ����������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentEngine)
                    {
                        //���� ������ �� ���� ��������� ����������
                        UIEngineDesignerWindow engineDesignerWindow
                            = designerWindow.engineDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ �������� ��������� �� ������� �������
                        if (dropdownEvent.WidgetName
                            == "ChangeEnginePowerPerSizeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ����������
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.EnginePowerPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� ���������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentReactor)
                    {
                        //���� ������ �� ���� ��������� ���������
                        UIReactorDesignerWindow reactorDesignerWindow
                            = designerWindow.reactorDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ ������� �������� �� ������� �������
                        if (dropdownEvent.WidgetName
                            == "ChangeReactorEnergyPerSizeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ���������
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ReactorEnergyPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� ��������� �����
                    else if (designerWindow.designerType
                        == DesignerType.ComponentHoldFuelTank)
                    {
                        //���� ������ �� ���� ��������� ���������
                        UIFuelTankDesignerWindow fuelTankDesignerWindow
                            = designerWindow.fuelTankDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ ������ ���������� ����
                        if (dropdownEvent.WidgetName
                            == "ChangeFuelTankCompressionTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ��������� �����
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.FuelTankCompression,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� ������������ ��� ������ ������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentExtractionEquipmentSolid)
                    {
                        //���� ������ �� ���� ��������� ����������� ������������
                        UIExtractionEquipmentDesignerWindow extractionEquipmentDesignerWindow
                            = designerWindow.extractionEquipmentDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ �������� �� ������� �������
                        if (dropdownEvent.WidgetName
                            == "ChangeExtractionEquipmentSolidSpeedPerSizeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� ������������ ��� ������ ������
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.ExtractionEquipmentSolidSpeedPerSize,
                                dropdownEvent.Value);
                        }
                    }
                    //�����, ���� ������� �������� �������������� ������
                    else if (designerWindow.designerType
                        == DesignerType.ComponentGunEnergy)
                    {
                        //���� ������ �� ���� ��������� �������������� ������
                        UIGunEnergyDesignerWindow energyGunDesignerWindow
                            = designerWindow.energyGunDesigner;

                        //���� ������� ����������� ����� �������� ����������, ������������ �����������
                        if (dropdownEvent.WidgetName
                            == "ChangeEnergyGunRechargeTechnology")
                        {
                            //����������� ��������� �������� ���������� � ��������� �������������� ������
                            DesignerComponentActionEvent(
                                DesignerComponentActionType.ChangeCoreTechnology,
                                TechnologyComponentCoreModifierType.GunEnergyRecharge,
                                dropdownEvent.Value);
                        }
                    }
                }

                uguiUIWorld.DelEntity(dropdownEventUIEntity);
            }

            //��� ������� ������� ��������� �������� ��������
            foreach (int sliderEventUIEntity
                in sliderEventUIFilter)
            {
                //���� ��������� �������
                ref EcsUguiSliderChangeEvent
                    sliderEvent
                    = ref sliderEventUIPool.Get(sliderEventUIEntity);

                Debug.LogWarning(
                    "Slider change! " + sliderEvent.WidgetName
                    + " ! " + sliderEvent.Value);

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

                Debug.LogWarning(
                    "Click! "
                    + clickEvent.WidgetName);

                //���� ������� ����� ����� ���������
                /*if (inputData.Value.activeMapMode == MapMode.Galaxy)
                {
                    //��� ������� SO � ������� �������������� ������
                    foreach (int spaceObjectEntity in galaxySpaceObjectFilter.Value)
                    {
                        //���� ��������� SO
                        ref CSpaceObject spaceObject = ref spaceObjectPool.Value.Get(spaceObjectEntity);

                        //���� ������� SO - �������
                        if (spaceObject.Transform == clickEvent.Sender.transform.parent)
                        {
                            //����������� ����� ������ ����� �� ����� �������
                            MapChangeModeEvent(
                                ChangeMapModeRequestType.Sector,
                                spaceObject.selfPE);
                        }
                    }
                }*/

                uguiSOWorld.DelEntity(clickEventSpaceEntity);
            }

            //���� ������� �����-���� ����� �����
            if (inputData.Value.mapMode != MapMode.None)
            {
                //���� ����������� ������
                InputCameraMoving();

                //���� �������� ������
                InputCameraRotating();

                //���� ����������� ������
                InputCameraZoom();
            }

            //������ ����
            InputOther();

            //���� ����
            MouseInput();

            if (Input.GetMouseButton(0)
                && UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject() == false)
            {
                HandleInput2();
            }
            else
            {
                //������� ���������� ������
                inputData.Value.previousRegionPE = new();
            }
        }

        //������� ����
        void MainMenuActionEvent(
            MainMenuActionType actionType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ������� ����
            int eventEntity = world.Value.NewEntity();
            ref EMainMenuAction mainMenuActionEvent
                = ref mainMenuActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ������� ���� �������������
            mainMenuActionEvent.actionType 
                = actionType;
        }

        //���� ����� ����
        void NewGameMenuActionEvent(
            NewGameMenuActionType actionType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ���� ����� ����
            int eventEntity = world.Value.NewEntity();
            ref ENewGameMenuAction newGameMenuActionEvent
                = ref newGameMenuActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ���� ����� ���� �������������
            newGameMenuActionEvent.actionType
                = actionType;
        }

        //���� ��������
        //

        //����������
        void WorkshopActionEvent(
            WorkshopActionType actionType,
            int contentSetIndex = -1,
            DesignerType designerType = DesignerType.None)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ����������
            int eventEntity = world.Value.NewEntity();
            ref EWorkshopAction workshopActionEvent
                = ref workshopActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ���������� �������������
            workshopActionEvent.actionType
                = actionType;

            //��������� ������ ������ ��������, ������� ��������� ����������
            workshopActionEvent.contentSetIndex
                = contentSetIndex;

            //���������, ����� ��� ��������� ��������� �������
            workshopActionEvent.designerType
                = designerType;
        }

        //��������
        void DesignerActionEvent(
            DesignerActionType actionType,
            bool isCurrentContentSet = true,
            int contentSetIndex = -1,
            int objectIndex = -1)
        {
            //������� ����� �������� � ��������� �� ��������� ������� �������� � ���������
            int eventEntity = world.Value.NewEntity();
            ref EDesignerAction designerActionEvent
                = ref designerActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ��������� �������������
            designerActionEvent.actionType
                = actionType;

            //���������, � ������� �� ������� �������� ��������� ��������� ��������
            designerActionEvent.isCurrentContentSet
                = isCurrentContentSet;

            //��������� ������ ������ ��������
            designerActionEvent.contentSetIndex
                = contentSetIndex;

            //��������� ������ �������
            designerActionEvent.objectIndex
                = objectIndex;
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
                else if(designerWindow.designerType
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

        //�������� ��������
        void DesignerShipClassActionEvent(
            DesignerShipClassActionType actionType,
            ShipComponentType componentType = ShipComponentType.None,
            int contentSetIndex = -1,
            int modelIndex = -1,
            int numberOfComponents = -1)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ��������� ������ ������ ����������
            int eventEntity = world.Value.NewEntity();
            ref EDesignerShipClassAction designerShipClassActionEvent
                = ref designerShipClassActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ��������� �������� �������������
            designerShipClassActionEvent.actionType
                = actionType;


            //��������� ��� ����������
            designerShipClassActionEvent.componentType
                = componentType;


            //��������� ������ ������ ��������
            designerShipClassActionEvent.contentSetIndex
                = contentSetIndex;

            //��������� ������ ������
            designerShipClassActionEvent.modelIndex
                = modelIndex;


            //��������� ����� �����������, ������� ��������� ����������/�������
            designerShipClassActionEvent.numberOfComponents
                = numberOfComponents;
        }


        void DesignerComponentActionEvent(
            DesignerComponentActionType actionType,
            TechnologyComponentCoreModifierType componentCoreModifierType,
            int technologyIndex)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� �������� ����������
            int eventEntity = world.Value.NewEntity();
            ref EDesignerComponentAction designerComponentActionEvent
                = ref designerComponentActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ��������� ����������� �������������
            designerComponentActionEvent.actionType
                = actionType;

            //��������� ��� ������������
            designerComponentActionEvent.componentCoreModifierType
                = componentCoreModifierType;

            //��������� ������ ��������� ����������
            designerComponentActionEvent.technologyDropdownIndex
                = technologyIndex;
        }

        //����
        void GameActionEvent(
            GameActionType actionType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� � ����
            int eventEntity = world.Value.NewEntity();
            ref EGameAction gameActionEvent = ref gameActionEventPool.Value.Add(eventEntity);

            //���������, ����� �������� � ���� �������������
            gameActionEvent.actionType = actionType;
        }

        void GameOpenDesignerEvent(
            DesignerType designerType,
            int contentSetIndex)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ��������� � ����
            int eventEntity = world.Value.NewEntity();
            ref EGameOpenDesigner gameOpenDesignerEvent = ref gameOpenDesignerEventPool.Value.Add(eventEntity);

            //���������, ����� �������� ��������� �������
            gameOpenDesignerEvent.designerType = designerType;

            //���������, ����� ����� �������� ��������� �������
            gameOpenDesignerEvent.contentSetIndex = contentSetIndex;
        }

        void GameDisplayObjectPanelEvent(
            DisplayObjectPanelEventType eventType,
            EcsPackedEntity objectPE,
            bool isRefresh = false)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����������� ������ �������
            int eventEntity = world.Value.NewEntity();
            ref EGameDisplayObjectPanel gameDisplayObjectPanelEvent = ref gameDisplayObjectPanelEventPool.Value.Add(eventEntity);

            //��������� ������ �������
            gameDisplayObjectPanelEvent = new(
                eventType,
                objectPE,
                isRefresh);
        }

        void MapChangeModeEvent(
            ChangeMapModeRequestType requestType,
            EcsPackedEntity objectPE)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ������ �����
            int changeMapModeRequestEntity = world.Value.NewEntity();
            ref RChangeMapMode changeMapModeRequest = ref changeMapModeRequestPool.Value.Add(changeMapModeRequestEntity);

            //��������� ��������� ����� �����
            changeMapModeRequest.requestType = requestType;

            changeMapModeRequest.mapMode = MapMode.Distance;

            //���������, ����� ������ ������� �����������
            changeMapModeRequest.activeObject = objectPE;
        }


        void InputCameraMoving()
        {
            //���� �������� �� ����
            float xDelta = Input.GetAxis("Horizontal");
            float zDelta = Input.GetAxis("Vertical");
            //���� �������� �� ����� ����
            if (xDelta != 0f || zDelta != 0f)
            {
                //��������� ��������
                CameraAdjustPosition(xDelta, zDelta);
            }
        }

        void CameraAdjustPosition(
            float xDelta, float zDelta)
        {
            //������������ ��������� ��������
            Vector3 direction = inputData.Value.mapCamera.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
            float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
            float distance = Mathf.Lerp(inputData.Value.movementSpeedMinZoom, inputData.Value.movementSpeedMaxZoom, inputData.Value.zoom)
                * damping * UnityEngine.Time.deltaTime;

            //������������ �������� ������
            Vector3 position = inputData.Value.mapCamera.localPosition;
            position += direction * distance;
            position = CameraWrapPosition(position);

            //��������� �������� ������
            inputData.Value.mapCamera.localPosition = position;
        }

        Vector3 CameraWrapPosition(
            Vector3 position)
        {
            //���������� ������� �������� ������
            float width = mapGenerationData.Value.regionCountX * MapGenerationData.innerDiameter;
            //���� ���������� X ������ ����
            while (position.x < 0f)
            {
                //���������� ������
                position.x += width;
            }
            //���� ���������� X ������ ������
            while (position.x > width)
            {
                //�������� ������
                position.x -= width;
            }

            //������������ �������� �� ��� Z
            float zMax = (mapGenerationData.Value.regionCountZ - 1) * (1.5f * MapGenerationData.outerRadius);
            position.z = Mathf.Clamp(position.z, 0f, zMax);

            //��������� ��������� �������� ������
            MapCenter(position.x);

            //���������� �������
            return position;
        }

        void MapCenter(float xPosition)
        {
            //���������� ������ �������, ������� ��������� � ������
            int centerColumnIndex = (int)(xPosition / (MapGenerationData.innerDiameter * MapGenerationData.chunkSizeX));

            //���� ������� ����������� ������� ����� ������������
            if (inputData.Value.currentCenterColumnIndex == centerColumnIndex)
            {
                //������� �� �������
                return;
            }
            //�����
            else
            {
                //��������� ������� ����������� �������
                inputData.Value.currentCenterColumnIndex = centerColumnIndex;
            }

            //���������� ����������� � ������������ ������� ��������
            int minColumnIndex = centerColumnIndex - mapGenerationData.Value.chunkCountX / 2;
            int maxColumnIndex = centerColumnIndex + mapGenerationData.Value.chunkCountX / 2;

            //���������� ������� ��������
            Vector3 position;
            position.y = position.z = 0;
            //��� ������� �������
            for (int a = 0; a < mapGenerationData.Value.columns.Length; a++)
            {
                //���� ������ ������ ������������
                if (a < minColumnIndex)
                {
                    //���������� ������ �� ������ ����
                    position.x = mapGenerationData.Value.chunkCountX * (MapGenerationData.innerDiameter * MapGenerationData.chunkSizeX);
                }
                //�����, ���� ������ ������ �������������
                else if (a > maxColumnIndex)
                {
                    //���������� ������� �� ����� ����
                    position.x = mapGenerationData.Value.chunkCountX * -(MapGenerationData.innerDiameter * MapGenerationData.chunkSizeX);
                }
                //�����
                else
                {
                    //�������� ������� ������
                    position.x = 0f;
                }

                //��������� ������� ������
                mapGenerationData.Value.columns[a].localPosition = position;
            }
        }

        void InputCameraRotating()
        {
            //���� ��������
            float rotationDelta = Input.GetAxis("Rotation");
            //���� �������� �� ����� ����
            if (rotationDelta != 0f)
            {
                //��������� ��������
                CameraAdjustRotation(rotationDelta);
            }
        }

        void CameraAdjustRotation(
            float rotationDelta)
        {
            //������������ ���� ��������
            inputData.Value.rotationAngle += rotationDelta * inputData.Value.rotationSpeed * UnityEngine.Time.deltaTime;

            //����������� ����
            if (inputData.Value.rotationAngle < 0f)
            {
                inputData.Value.rotationAngle += 360f;
            }
            else if (inputData.Value.rotationAngle >= 360f)
            {
                inputData.Value.rotationAngle -= 360f;
            }

            //��������� ��������
            inputData.Value.mapCamera.localRotation = Quaternion.Euler(0f, inputData.Value.rotationAngle, 0f);
        }

        void InputCameraZoom()
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
            float zoomAngle = Mathf.Lerp(inputData.Value.swiwelMinZoom, inputData.Value.swiwelMaxZoom, inputData.Value.zoom);
            inputData.Value.swiwel.localRotation = Quaternion.Euler(zoomAngle, 0f, 0f);
        }

        void CameraValidatePosition()
        {
            //��������� ������� �������� ������
            CameraAdjustPosition(0f, 0f);
        }

        void InputOther()
        {
            //����� �����
            if (Input.GetKeyUp(KeyCode.Space))
            {
                //���� ���� �������
                if (runtimeData.Value.isGameActive
                    == true)
                {
                    //����������� ��������� �����
                    GameActionEvent(
                        GameActionType.PauseOn);
                }
                //�����
                else
                {
                    //����������� ���������� �����
                    GameActionEvent(
                        GameActionType.PauseOff);
                }
            }

            if (Input.GetKeyUp(KeyCode.Escape))
            {
                //���� ������� ���� ����
                if (eUI.Value.activeMainWindowType
                    == MainWindowType.Game)
                {
                    //������ �������, ������������� ����� �� ����
                    QuitGameEvent();
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
                        DesignerActionEvent(
                            DesignerActionType.OpenGame);
                    }
                    //�����
                    else
                    {
                        //����������� �������� ���� ����������
                        DesignerActionEvent(
                            DesignerActionType.OpenWorkshop);
                    }
                }
                //�����, ���� ������� ���� ����������
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.Workshop)
                {
                    //����������� �������� ���� �������� ����
                    WorkshopActionEvent(
                        WorkshopActionType.OpenMainMenu);
                }
                //�����, ���� ������� ���� ���� ����� ����
                else if (eUI.Value.activeMainWindowType
                    == MainWindowType.NewGameMenu)
                {
                    //����������� �������� ���� �������� ����
                    NewGameMenuActionEvent(
                        NewGameMenuActionType.OpenMainMenu);
                }
            }

            if (Input.GetKeyUp(KeyCode.Y))
            {
                Debug.LogWarning("Y!");

                //����������� ����� ������ ����� �� ����� ����������
                MapChangeModeEvent(
                    ChangeMapModeRequestType.Distance,
                    new EcsPackedEntity());
            }
        }

        void MouseInput()
        {
            //���� ��� �� ��������� ����
            Ray inputRay = inputData.Value.camera.ScreenPointToRay(Input.mousePosition);

            //���� ��� �������� �������
            if (Physics.Raycast(inputRay, out RaycastHit hit))
            {
                //�������� �������� ��������� GO MO
                if (hit.transform.gameObject.TryGetComponent(
                    out GOMapObject gOMapObject))
                {
                    //���� ��������� ������������� MO
                    gOMapObject.mapObjectPE.Unpack(world.Value, out int mapObjectEntity);
                    ref CMapObject mapObject = ref mapObjectPool.Value.Get(mapObjectEntity);

                    //���� ��� MO - ������ ��������
                    if (mapObject.objectType == MapObjectType.ShipGroup)
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
                    }
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

                    Debug.LogWarning(mapObject.objectType);
                }
            }
        }

        void HandleInput2()
        {
            //���� ������ ��� ��������
            if (GetRegionUnderCursor().Unpack(world.Value, out int regionEntity))
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

                        //��������� ��������� ������
                        inputData.Value.searchFromRegion = currentRegion.selfPE;
                        currentRegion.EnableHighlight(Color.blue);

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
                    GameDisplayObjectPanelEvent(
                        DisplayObjectPanelEventType.Region,
                        currentRegion.selfPE);
                }

                //���� �����-���� ������� �������
                /*if (inputData.Value.activeShipPE.Unpack(world.Value, out int activeShipEntity))
                {
                    //���� ��������� �������
                    ref CShip activeShip
                        = ref shipPool.Value.Get(activeShipEntity);

                    //���� ������� ��������� � ������ ��������
                    if (activeShip.shipMode
                        == ShipMode.Idle)
                    {
                        //��������� ������� � ����� ������ ����
                        activeShip.shipMode
                            = ShipMode.Pathfinding;

                        //��������� ������� ������
                        activeShip.targetPE
                            = currentCell.selfPE;

                        //��������� ��� �������� �������
                        activeShip.targetType
                            = MovementTargetType.Cell;
                    }
                }*/

                //��������� ������ ��� ���������� ��� ���������� �����
                inputData.Value.previousRegionPE = currentRegion.selfPE;
            }
            else
            {
                //������� ���������� ������
                inputData.Value.previousRegionPE = new();
            }
        }

        void OnDraw()
        {
            List<EcsPackedEntity> path = GetPath();

            if (path == null || path.Count == 0)
            {
                return;
            }

            //���� ��������� ������� �������
            path[0].Unpack(world.Value, out int firstRegionEntity);
            ref CHexRegion firstRegion = ref regionPool.Value.Get(firstRegionEntity);

            Vector3 a, b, c = firstRegion.Position;

            for (int i = 1; i < path.Count; i++)
            {
                //���� ��������� ����������� �������
                path[i - 1].Unpack(world.Value, out int previousRegionEntity);
                ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                //���� ��������� �������� �������
                path[i].Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                a = c;
                b = previousRegion.Position;
                c = (b + currentRegion.Position) * 0.5f;

                for (float t = 0f; t < 1f; t += 0.1f)
                {
                    GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position
                        = Bezier.GetPoint(a, b, c, t);
                }
            }

            //���� ��������� ���������� �������
            path[path.Count - 1].Unpack(world.Value, out int lastRegionEntity);
            ref CHexRegion lastRegion = ref regionPool.Value.Get(lastRegionEntity);

            a = c;
            b = lastRegion.Position;
            c = b;

            for (float t = 0f; t < 1f; t += 0.1f)
            {
                GameObject.CreatePrimitive(PrimitiveType.Sphere).transform.position
                    = Bezier.GetPoint(a, b, c, t);
            }
        }

        EcsPackedEntity GetRegionUnderCursor()
        {
            //���� ��� �� ��������� ����
            Ray inputRay = inputData.Value.camera.ScreenPointToRay(Input.mousePosition);

            //���������� PE �������  �� ������� �����������
            return GetRegionPE(inputRay);
        }

        EcsPackedEntity GetRegionPE(
            Ray ray)
        {
            //���� ��� ���������� ���������
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                //���������� PE ������� �� ����� �����������
                return GetRegionPE(hit.point);
            }
            //�����
            else
            {
                //���������� ������ PE
                return new();
            }
        }

        EcsPackedEntity GetRegionPE(
            Vector3 position)
        {
            //���������� ������� �����
            position = sceneData.Value.coreObject.transform.InverseTransformPoint(position);

            //��������� ���������� �������
            DHexCoordinates coordinates = DHexCoordinates.FromPosition(position);

            return GetRegionPE(coordinates);
        }

        EcsPackedEntity GetRegionPE(
            DHexCoordinates coordinates)
        {
            int z = coordinates.Z;

            //���� ���������� ������� �� ������� �����
            if (z < 0 || z >= mapGenerationData.Value.regionCountZ)
            {
                return new();
            }

            int x = coordinates.X + z / 2;

            //���� ���������� ������� �� ������� �����
            if (x < 0 || x >= mapGenerationData.Value.regionCountX)
            {
                return new();
            }

            return mapGenerationData.Value.regionPEs[x + z * mapGenerationData.Value.regionCountX];
        }

        void RegionsEdit(
            ref CHexRegion centerRegion)
        {
            //���� ���������� ������������ �������
            int centerX = centerRegion.coordinates.X;
            int centerZ = centerRegion.coordinates.Z;

            //�������� ���������� ������ �����
            int brushSize = 0;

            //��� ������� ������� � ������ �������� �����
            for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
            {
                for (int x = centerX - r; x <= centerX + brushSize; x++)
                {
                    //���� ���������� ������ � ������ ������������
                    if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int regionEntity))
                    {
                        //���� ��������� �������
                        ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                        //����������� ������
                        RegionEdit(ref region);
                    }
                }
            }
            //��� ������� ������� � ������� �������� �����
            for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
            {
                for (int x = centerX - brushSize; x <= centerX + r; x++)
                {
                    //���� ���������� ������ � ������ ������������
                    if (GetRegionPE(new DHexCoordinates(x, z)).Unpack(world.Value, out int regionEntity))
                    {
                        //���� ��������� �������
                        ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                        //����������� ������
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
            //���� ������� ����� �������� ���
            if (eUI.Value.gameWindow.riverMode == OptionalToggle.No)
            {
                //������� ����
                RegionRemoveRiver(
                    ref region);
            }
            //���� ������� ����� �������� �����
            if (eUI.Value.gameWindow.roadMode == OptionalToggle.No)
            {
                //������� ������
                RegionRemoveRoads(
                    ref region);
            }
            //���� ����� ���� �� ���������
            if (eUI.Value.gameWindow.walledMode != OptionalToggle.Ignore)
            {
                //������������� ����� �������� �������������
                region.Walled = eUI.Value.gameWindow.walledMode == OptionalToggle.Yes;
            }
            //���� �������������� �������
            if (inputData.Value.isDrag == true)
            {
                //���� ���������� ������ ����������
                if (region.GetNeighbour(inputData.Value.dragDirection.Opposite()).Unpack(world.Value, out int previousRegionEntity))
                {
                    //���� ��������� ����������� �������
                    ref CHexRegion previousRegion
                        = ref regionPool.Value.Get(previousRegionEntity);

                    //���� ������� ����� �������� ���
                    if (eUI.Value.gameWindow.riverMode == OptionalToggle.Yes)
                    {
                        //������ ����
                        RegionSetOutgoingRiver(
                            ref previousRegion,
                            inputData.Value.dragDirection);
                    }
                    //�����, ���� ������� ����� �������� �����
                    else if ( eUI.Value.gameWindow.roadMode == OptionalToggle.Yes)
                    {
                        //������ ������
                        RegionAddRoad(
                            ref previousRegion,
                            inputData.Value.dragDirection);
                    }
                }
            }

            //����������� ������������ �����
            ChunkRefreshSelfRequest(
                ref region);
        }

        void ValidateDrag(
            ref CHexRegion region)
        {
            //���� ��������� ����������� �������
            if (inputData.Value.previousRegionPE.Unpack(world.Value, out int previousRegionEntity))
            {
                ref CHexRegion previousRegion
                    = ref regionPool.Value.Get(previousRegionEntity);

                //��� ������� PE ������
                for (inputData.Value.dragDirection = HexDirection.NE; inputData.Value.dragDirection <= HexDirection.NW; inputData.Value.dragDirection++)
                {
                    //���� ����� - ��� ������� ������
                    if (previousRegion.GetNeighbour(inputData.Value.dragDirection).EqualsTo(region.selfPE) == true)
                    {
                        //��������, ��� �������������� �������
                        inputData.Value.isDrag = true;

                        //������� �� �������
                        return;
                    }
                }
            }

            //��������, ��� �������������� ���������
            inputData.Value.isDrag = false;
        }

        void RegionSetOutgoingRiver(
            ref CHexRegion region,
            HexDirection direction)
        {
            //���� ���� ��� ����������
            if (region.HasOutgoingRiver == true
                && region.OutgoingRiver == direction)
            {
                //������� �� �������
                return;
            }

            //���� ����� � ����� ����������� ����������
            if (region.GetNeighbour(direction).Unpack(world.Value, out int neighbourRegionEntity))
            {
                //���� ��������� ������
                ref CHexRegion neighbourRegion
                    = ref regionPool.Value.Get(neighbourRegionEntity);

                //���� ������ ������ ������
                if (region.IsValidRiverDestination(ref neighbourRegion) == false)
                {
                    //������� �� �������
                    return;
                }

                //������� ��������� ����
                RegionRemoveOutgoingRiver(
                    ref region);
                //���� ����������� ������ �������� �����
                if (region.HasIncomingRiver == true
                    && region.IncomingRiver == direction)
                {
                    //������� �������� ����
                    RegionRemoveIncomingRiver(
                        ref region);
                }

                //������ ��������� ����
                region.HasOutgoingRiver = true;
                region.OutgoingRiver = direction;
                region.SpecialIndex = 0;

                //������ �������� ����
                RegionRemoveIncomingRiver(
                    ref neighbourRegion);
                neighbourRegion.HasIncomingRiver = true;
                neighbourRegion.IncomingRiver = direction.Opposite();
                neighbourRegion.SpecialIndex = 0;

                //������� ������
                RegionSetRoad(
                    ref region,
                    (int)direction,
                    false);
            }
        }

        void RegionRemoveOutgoingRiver(
            ref CHexRegion region)
        {
            //���� ������ ����� ��������� ����
            if (region.HasOutgoingRiver == true)
            {
                //������� ��������� ����
                region.HasOutgoingRiver = false;

                //���� ��������� ������, ���� ��� ����
                region.GetNeighbour(region.OutgoingRiver).Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion
                    = ref regionPool.Value.Get(neighbourRegionEntity);

                //������� �������� ����
                neighbourRegion.HasIncomingRiver = false;

                //����������� ������������ �����
                ChunkRefreshSelfRequest(
                    ref region);
                //���� �������� ������ ��������� � ������� �����
                if (region.parentChunkPE.EqualsTo(in neighbourRegion.parentChunkPE) == false)
                {
                    //����������� ������������ �����
                    ChunkRefreshSelfRequest(
                        ref neighbourRegion);
                }
            }
        }

        void RegionRemoveIncomingRiver(
            ref CHexRegion region)
        {
            //���� ������ ����� ��������� ����
            if (region.HasIncomingRiver == true)
            {
                //������� ��������� ����
                region.HasIncomingRiver = false;

                //���� ��������� ������, ���� ��� ����
                region.GetNeighbour(region.IncomingRiver).Unpack(world.Value, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion
                    = ref regionPool.Value.Get(neighbourRegionEntity);

                //������� �������� ����
                neighbourRegion.HasOutgoingRiver = false;

                //����������� ������������ �����
                ChunkRefreshSelfRequest(
                    ref region);
                //���� �������� ������ ��������� � ������� �����
                if (region.parentChunkPE.EqualsTo(in neighbourRegion.parentChunkPE) == false)
                {
                    //����������� ������������ �����
                    ChunkRefreshSelfRequest(
                        ref neighbourRegion);
                }
            }
        }

        void RegionRemoveRiver(
            ref CHexRegion region)
        {
            //������� ��������� � �������� ����
            RegionRemoveOutgoingRiver(
                ref region);
            RegionRemoveIncomingRiver(
                ref region);
        }

        void RegionAddRoad(
            ref CHexRegion region,
            HexDirection direction)
        {
            //���� ������ � ������ ����������� �����������
            if (region.roads[(int)direction] == false
                && region.IsSpecial == false
                //� ���� ����� ������ ����� �� ����������
                && region.HasRiverThroughEdge(direction) == false)
            {
                //������ ������
                RegionSetRoad(
                    ref region,
                    (int)direction,
                    true);
            }
        }

        void RegionRemoveRoads(
            ref CHexRegion region)
        {
            //��� ������ �������� ������ � �������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� � ������ ����������� ���� ������
                if (region.roads[a] == true)
                {
                    //������� ������
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
            //����� ������ ������ ���������
            region.roads[index] = state;

            //���� ��������� ������ � ������� �����������
            region.neighbourRegionPEs[index].Unpack(world.Value, out int neighbourRegionEntity);
            ref CHexRegion neighbourRegion
                = ref regionPool.Value.Get(neighbourRegionEntity);

            //����� ������ ������ ���������
            neighbourRegion.roads[(int)((HexDirection)index).Opposite()] = state;

            //����������� ������������ �����
            ChunkRefreshSelfRequest(
                ref region);
            //���� �������� ������ ��������� � ������� �����
            if (region.parentChunkPE.EqualsTo(in neighbourRegion.parentChunkPE) == false)
            {
                //����������� ������������ �����
                ChunkRefreshSelfRequest(
                    ref neighbourRegion);
            }
        }

        void ChunkRefreshSelfRequest(
            ref CHexRegion region)
        {
            //���� ��������� ������������� ����� �������
            region.parentChunkPE.Unpack(world.Value, out int parentChunkEntity);
            ref CHexChunk parentChunk = ref chunkPool.Value.Get(parentChunkEntity);

            //���� ��� �� ���������� ����������� ���������� �����
            if (mapChunkRefreshSelfRequestPool.Value.Has(parentChunkEntity) == false)
            {
                //��������� �������� ���������� ���������� �����
                mapChunkRefreshSelfRequestPool.Value.Add(parentChunkEntity);
            }

            //��� ������� ������ �������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ����� ���������� 
                if (region.neighbourRegionPEs[a].Unpack(world.Value, out int neighbourRegionEntity))
                {
                    //���� ��������� ������
                    ref CHexRegion neighbourRegion = ref regionPool.Value.Get(neighbourRegionEntity);

                    //���� �������� ������������� ����� �������
                    neighbourRegion.parentChunkPE.Unpack(world.Value, out int neighbourParentChunkEntity);

                    //���� ��� �� ���������� ����������� ���������� �����
                    if (mapChunkRefreshSelfRequestPool.Value.Has(neighbourParentChunkEntity) == false)
                    {
                        //��������� �������� ���������� ���������� �����
                        mapChunkRefreshSelfRequestPool.Value.Add(neighbourParentChunkEntity);
                    }
                }
            }
        }

        void QuitGameEvent()
        {
            //������ ����� �������� � ��������� �� ��������� ������� ������ �� ����
            int eventEntity = world.Value.NewEntity();
            ref EQuitGame quitGameEvent
                = ref quitGameEventPool.Value.Add(eventEntity);
        }

        void FindPath(
            ref CHexRegion fromRegion, ref CHexRegion toRegion)
        {
            //������� ����
            ClearPath();

            //��������� ��������� � �������� �������
            inputData.Value.currentPathFrom = fromRegion.selfPE;
            inputData.Value.currentPathTo = toRegion.selfPE;
            
            //����������, ���������� �� ����
            inputData.Value.currentPathExists = Search(ref fromRegion, ref toRegion);

            //���������� ����
            ShowPath();
        }

        void ShowPath()
        {
            //���� ���������� ���������� � ��������� ��������
            inputData.Value.currentPathFrom.Unpack(world.Value, out int fromRegionEntity);
            ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

            inputData.Value.currentPathTo.Unpack(world.Value, out int toRegionEntity);
            ref CHexRegion toRegion = ref regionPool.Value.Get(toRegionEntity);

            //���� ���� ���������� 
            if (inputData.Value.currentPathExists == true)
            {
                //���� �������� ������ ��� �������
                ref CHexRegion currentRegion = ref regionPool.Value.Get(toRegionEntity);

                //���� ������� ������ - �� ���������
                while (currentRegion.selfPE.EqualsTo(inputData.Value.currentPathFrom) == false)
                {
                    //���������� ����������� � ��������� �������
                    currentRegion.SetLabel(currentRegion.Distance.ToString());
                    currentRegion.EnableHighlight(Color.white);

                    //���� ��������� ����������� �������
                    currentRegion.PathFromPE.Unpack(world.Value, out int currentRegionEntity);
                    currentRegion = ref regionPool.Value.Get(currentRegionEntity);
                }
            }

            //���������� ��������� ���������� � ��������� ��������
            fromRegion.EnableHighlight(Color.blue);
            toRegion.EnableHighlight(Color.red);
        }

        void ClearPath()
        {
            //���� �������� ������ ����������
            if (inputData.Value.currentPathTo.Unpack(world.Value, out int toRegionEntity))
            {
                //���� ��������� ��������� �������
                ref CHexRegion toRegion = ref regionPool.Value.Get(toRegionEntity);

                //���� ���� ���������� 
                if (inputData.Value.currentPathExists == true)
                {
                    //���� �������� ������ ��� �������
                    ref CHexRegion currentRegion = ref regionPool.Value.Get(toRegionEntity);

                    //���� ������� ������ - �� ���������
                    while (currentRegion.selfPE.EqualsTo(inputData.Value.currentPathFrom) == false)
                    {
                        //������� ����������� � ��������� �������
                        currentRegion.SetLabel(null);
                        currentRegion.DisableHighlight();

                        //���� ��������� ����������� �������
                        currentRegion.PathFromPE.Unpack(world.Value, out int currentRegionEntity);
                        currentRegion = ref regionPool.Value.Get(currentRegionEntity);
                    }
                }
                //�����, ���� ���� ����������
                else if (inputData.Value.currentPathFrom.Unpack(world.Value, out int fromRegionEntity))
                {
                    //���� ��������� ���������� �������
                    ref CHexRegion fromRegion = ref regionPool.Value.Get(fromRegionEntity);

                    //��������� ��������� ���������� � ��������� ��������
                    fromRegion.DisableHighlight();
                    toRegion.DisableHighlight();
                }
            }

            //������� PE ���������� � ��������� ��������
            inputData.Value.currentPathFrom = inputData.Value.currentPathTo = new();
        }

        bool Search(
            ref CHexRegion fromRegion, ref CHexRegion toRegion)
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
                //���� ��������� ������
                ref CHexRegion currentCell
                    = ref regionPool.Value.Get(cellEntity);

                //������� ������
                currentCell.SearchPhase = 0;
                //currentCell.SetLabel(null);
                //currentCell.DisableHighlight();
            }

            //������� ��������� ������ �� ������� ������
            fromRegion.SearchPhase = 2;
            fromRegion.Distance = 0;
            fromRegion.EnableHighlight(
                Color.blue);

            //���� �������� ������� ��������� ������
            fromRegion.selfPE.Unpack(world.Value, out int fromCellEntity);

            //������� � ������ � �������
            inputData.Value.searchFrontier.Enqueue(
                fromRegion.selfPE,
                0);

            while (inputData.Value.searchFrontier.Count > 0)
            {
                //���� ��������� ������� ������
                inputData.Value.searchFrontier.Dequeue().cellPE.Unpack(world.Value, out int currentCellEntity);
                ref CHexRegion currentCell
                    = ref regionPool.Value.Get(currentCellEntity);

                //����������� ���� ������ ������
                currentCell.SearchPhase += 1;

                //���� ������� ������ ��������� � �������
                if (currentCell.selfPE.EqualsTo(toRegion.selfPE) == true)
                {
                    //�� ���� ������
                    return true;
                }

                //��� ������ �������� ������
                for(HexDirection a = HexDirection.NE; a <= HexDirection.NW; a++)
                {
                    //���� ������ ����������
                    if (currentCell.GetNeighbour(a).Unpack(world.Value, out int neighbourCellEntity))
                    {
                        //���� ��������� �������� ������
                        ref CHexRegion neighbourCell
                            = ref regionPool.Value.Get(neighbourCellEntity);

                        //���� ������ ��������� �� �������� ������
                        if (neighbourCell.SearchPhase > 2)
                        {
                            //��������� � ���������
                            continue;
                        }

                        //���������� ��� �����
                        HexEdgeType edgeType = MapGenerationData.GetEdgeType(currentCell.Elevation, neighbourCell.Elevation);

                        //���� ������ ��������� ��� �����
                        if (neighbourCell.IsUnderwater == true
                            //��� ��� ����� - �����
                            || edgeType == HexEdgeType.Cliff
                            //��� ������ ����������� �����
                            || currentCell.Walled != neighbourCell.Walled)
                        {
                            //��������� � ��������� ������
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

                        //���� ���������� �� ������ ��� �� ���� �������
                        if (neighbourCell.SearchPhase < 2)
                        {
                            //������� ������ �� ������� ������
                            neighbourCell.SearchPhase = 2;

                            //��������� ���������� �� ������
                            neighbourCell.Distance = distance;

                            neighbourCell.PathFromPE = currentCell.selfPE;

                            neighbourCell.SearchHeuristic = neighbourCell.coordinates.DistanceTo(
                                toRegion.coordinates);

                            //������� ��� �������� � �������
                            inputData.Value.searchFrontier.Enqueue(
                                neighbourCell.selfPE,
                                neighbourCell.SearchPriority);
                        }
                        //�����, ���� ���������� �� ������ ������ ��������
                        else if(distance < neighbourCell.Distance)
                        {
                            //��������� ���������
                            int oldPriority = neighbourCell.SearchPriority;

                            //��������� ���������� �� ����
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

            //����������, ��� ���� �� ��� ������
            return false;
        }

        List<EcsPackedEntity> GetPath()
        {
            //���� ������� ���� �� ����������
            if (inputData.Value.currentPathExists == false)
            {
                //���������� null
                return null;
            }
            //�����
            else
            {
                //������ ������ �� ����
                List<EcsPackedEntity> path = ListPool<EcsPackedEntity>.Get();

                //���� ��������� ��������� �������
                inputData.Value.currentPathTo.Unpack(world.Value, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                //������� �������� ������ � ������
                path.Add(currentRegion.selfPE);

                //���� ���������� ������ �� ����� ����������
                while (currentRegion.PathFromPE.EqualsTo(inputData.Value.currentPathFrom) == false)
                {
                    //���� ��������� ����������� �������
                    currentRegion.PathFromPE.Unpack(world.Value, out currentRegionEntity);
                    currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                    //������� ������� ������ � ������
                    path.Add(currentRegion.selfPE);
                }

                //������� ��������� ������ � ������
                path.Add(inputData.Value.currentPathFrom);

                //������������� ������ � �������� �������
                path.Reverse();

                //���������� ������
                return path;
            }
        }
    }
}