
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
        readonly EcsPoolInject<EGameDisplayObjectPanel> gameDisplayObjectPanelEventPool = default;

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
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.OrganizationOverview,
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
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.RegionOverview,
                                objectPanel.activeObjectPE);
                        }
                        //�����, ���� ������� ������� �����������
                        else if (islandSubpanel.tabGroup.selectedTab == islandSubpanel.organizationsTab.selfTabButton)
                        {
                            //����������� ���������� ������� �����������
                            GameDisplayObjectPanelEvent(
                                DisplayObjectPanelEventType.RegionOrganizations,
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
            //������ ����� �������� � ��������� �� ��������� ������� ����������� ������ �������
            int eventEntity = world.Value.NewEntity();
            ref EGameDisplayObjectPanel gameDisplayObjectPanelEvent = ref gameDisplayObjectPanelEventPool.Value.Add(eventEntity);

            //��������� ������ �������
            gameDisplayObjectPanelEvent = new(
                eventType,
                objectPE,
                isRefresh);
        }
    }
}