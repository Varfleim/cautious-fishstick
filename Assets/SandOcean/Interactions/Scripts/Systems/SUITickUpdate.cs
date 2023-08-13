
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;

namespace SandOcean.UI
{
    public class SUITickUpdate : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������� ����
        readonly EcsPoolInject<RGameDisplayObjectPanel> gameDisplayObjectPanelRequestPool = default;

        //������
        readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //���� ������� ���� ����
            if (eUI.Value.activeMainWindowType == MainWindowType.Game)
            {
                //���� ���� ����
                UIGameWindow gameWindow = eUI.Value.gameWindow;

                //���� ������� ������ �������
                if (gameWindow.activeMainPanelType == MainPanelType.Object)
                {
                    //���� ������ �������
                    UIObjectPanel objectPanel = gameWindow.objectPanel;

                    //���� ������� ��������� �����������
                    if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Organization)
                    {
                        //���� ��������� �����������
                        UIOrganizationObjectSubpanel organizationSubpanel = objectPanel.organizationObjectSubpanel;

                        //���� ������� �������� �������
                        if (organizationSubpanel.tabGroup.selectedTab == organizationSubpanel.overviewTab.selfTabButton)
                        {
                            //����������� ���������� �������� �������
                            GameDisplayObjectPanelRequest(
                                DisplayObjectPanelRequestType.OrganizationOverview,
                                objectPanel.activeObjectPE);
                        }
                    }
                    //�����, ���� ������� ��������� �������
                    else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.Region)
                    {
                        //���� ��������� �������
                        UIRegionObjectSubpanel islandSubpanel = objectPanel.regionObjectSubpanel;

                        //���� ������� �������� �������
                        if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.overviewTab.selfTabButton)
                        {
                            //����������� ���������� �������� �������
                            GameDisplayObjectPanelRequest(
                                DisplayObjectPanelRequestType.RegionOverview,
                                objectPanel.activeObjectPE);
                        }
                        //�����, ���� ������� ������� �����������
                        else if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.organizationsTab.selfTabButton)
                        {
                            //����������� ���������� ������� �����������
                            GameDisplayObjectPanelRequest(
                                DisplayObjectPanelRequestType.RegionOrganizations,
                                objectPanel.activeObjectPE);
                        }
                    }
                    //�����, ���� ������� ��������� ORAEO
                    else if (objectPanel.activeObjectSubpanelType == ObjectSubpanelType.ORAEO)
                    {
                        //���� ��������� ORAEO
                        UIORAEOObjectSubpanel oRAEOSubpanel = objectPanel.oRAEOObjectSubpanel;

                        //���� ������� �������� �������
                        if (oRAEOSubpanel.tabGroup.selectedTab == oRAEOSubpanel.overviewTab.selfTabButton)
                        {
                            //����������� ���������� �������� �������
                            GameDisplayObjectPanelRequest(
                                DisplayObjectPanelRequestType.ORAEOOverview,
                                objectPanel.activeObjectPE);
                        }
                    }
                }
            }
        }

        void GameDisplayObjectPanelRequest(
            DisplayObjectPanelRequestType requestType,
            EcsPackedEntity objectPE,
            bool isRefresh = true)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����������� ������ �������
            int requestEntity = world.Value.NewEntity();
            ref RGameDisplayObjectPanel gameDisplayObjectPanelRequest = ref gameDisplayObjectPanelRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
            gameDisplayObjectPanelRequest = new(
                requestType,
                objectPE,
                isRefresh);
        }
    }
}