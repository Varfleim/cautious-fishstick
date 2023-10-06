
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
        //����
        readonly EcsWorldInject world = default;

        //������� ����
        readonly EcsPoolInject<RGameObjectPanelAction> gameObjectPanelActionRequestPool = default;

        //������
        readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //���� ������� ���� ����
            if (eUI.Value.activeMainWindowType == MainWindowType.Game)
            {
                //���������, ��������� �� ���������� � ���� ����
                RefreshCheckGame();
            }
        }

        #region Game
        void RefreshCheckGame()
        {
            //���� ���� ����
            UIGameWindow gameWindow = eUI.Value.gameWindow;

            //���� ������� ������ �������
            if (gameWindow.activeMainPanelType == MainPanelType.Object)
            {
                //��������, ��������� �� ���������� � ������ �������
                RefreshCheckGameObject();
            }
        }

        #region GameObject
        void RefreshCheckGameObject()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ������� ��������� ��������� ������
            if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
            {
                //���������, ��������� �� ���������� � ��������� ��������� ������
                RefreshCheckGameObjectFleetManager();
            }
            //���� ������� ��������� �����������
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
            {
                //���������, ��������� �� ���������� � ��������� �����������
                RefreshCheckGameObjectOrganization();
            }
            //�����, ���� ������� ��������� �������
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
            {
                //���������, ��������� �� ���������� � ��������� �������
                RefreshCheckGameObjectRegion();
            }
            //�����, ���� ������� ��������� ORAEO
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
            {
                //���������, ��������� �� ���������� � ��������� ORAEO
                RefreshCheckGameObjectORAEO();
            }
            //�����, ���� ������� ��������� ����������
            else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Building)
            {
                //���������, ��������� �� ���������� � ��������� ����������
                RefreshCheckGameObjectBuilding();
            }
        }

        void RefreshCheckGameObjectFleetManager()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ��������� ������
            UIFleetManagerSubpanel fleetManagerSubpanel = objectPanel.fleetManagerSubpanel;

            //���� ������� ������� ������
            if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.fleetsTab.selfTabButton)
            {
                //����������� ���������� ������� ������
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.FleetManagerFleets,
                    objectPanel.activeObjectPE);
            }
            //�����, ���� ������� ������� �������� ����������� �����
            else if (fleetManagerSubpanel.tabGroup.selectedTab == fleetManagerSubpanel.taskForceTemplatesTab.selfTabButton)
            {
                //���� ������� �������� �����
                UITFTemplatesTab taskForceTemplatesTab = fleetManagerSubpanel.taskForceTemplatesTab;

                //���� ������� ���������� ������ �������� �����
                if (taskForceTemplatesTab.listSubtab.isActiveAndEnabled == true)
                {
                    //����������� ���������� ���������� ������ �������� �����
                    GameObjectPanelControlRequest(
                        ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesList,
                        objectPanel.activeObjectPE);
                }
                //�����, ���� ������� ���������� ��������� �������� �����
                else if (taskForceTemplatesTab.designerSubtab.isActiveAndEnabled == true)
                {
                    //����������� ���������� ���������� ��������� �������� �����
                    GameObjectPanelControlRequest(
                        ObjectPanelActionRequestType.FleetManagerTaskForceTemplatesDesigner,
                        objectPanel.activeObjectPE);
                }
            }
        }

        void RefreshCheckGameObjectOrganization()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� �����������
            UIOrganizationSubpanel organizationSubpanel = objectPanel.organizationSubpanel;

            //���� ������� �������� �������
            if (organizationSubpanel.tabGroup.selectedTab == organizationSubpanel.overviewTab.selfTabButton)
            {
                //����������� ���������� �������� �������
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.OrganizationOverview,
                    objectPanel.activeObjectPE);
            }
        }

        void RefreshCheckGameObjectRegion()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� �������
            UIRegionSubpanel islandSubpanel = objectPanel.regionSubpanel;

            //���� ������� �������� �������
            if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.overviewTab.selfTabButton)
            {
                //����������� ���������� �������� �������
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.RegionOverview,
                    objectPanel.activeObjectPE);
            }
            //�����, ���� ������� ������� �����������
            else if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.oRAEOsTab.selfTabButton)
            {
                //����������� ���������� ������� �����������
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.RegionOrganizations,
                    objectPanel.activeObjectPE);
            }
        }

        void RefreshCheckGameObjectORAEO()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ORAEO
            UIORAEOSubpanel oRAEOSubpanel = objectPanel.oRAEOSubpanel;

            //���� ������� �������� �������
            if (oRAEOSubpanel.tabGroup.selectedTab == oRAEOSubpanel.overviewTab.selfTabButton)
            {
                //����������� ���������� �������� �������
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.ORAEOOverview,
                    objectPanel.activeObjectPE);
            }
            //�����, ���� ������� ������� ����������
            else if (oRAEOSubpanel.tabGroup.selectedTab == oRAEOSubpanel.buildingsTab.selfTabButton)
            {
                //����������� ���������� ������� ����������
                GameObjectPanelControlRequest(
                    ObjectPanelActionRequestType.ORAEOBuildings,
                    objectPanel.activeObjectPE);
            }
        }

        void RefreshCheckGameObjectBuilding()
        {
            //���� ������ �������
            UIObjectPanel objectPanel = eUI.Value.gameWindow.objectPanel;

            //���� ��������� ����������
            UIBuildingSubpanel buildingSubpanel = objectPanel.buildingSubpanel;

            //���� ������� �������� �������
            if (buildingSubpanel.tabGroup.selectedTab == buildingSubpanel.overviewTab.selfTabButton)
            {
                //����������� ���������� �������� �������
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
        #endregion
    }
}