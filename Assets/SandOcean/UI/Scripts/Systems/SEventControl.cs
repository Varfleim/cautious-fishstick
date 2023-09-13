
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;
using SandOcean.Organization;
using SandOcean.Warfare.Fleet;
using SandOcean.UI.GameWindow.Object;
using SandOcean.Warfare.TaskForce;

namespace SandOcean
{
    public class SEventControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //�����
        readonly EcsPoolInject<CFleet> fleetPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;

        //������� ����
        readonly EcsPoolInject<RGameCreatePanel> gameCreatePanelRequestPool = default;

        //������� �����
        //readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //������� ����������

        //������� ���������������-������������� ��������

        //������� ������

        //����� �������
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameRequestFilter = default;

        readonly EcsFilterInject<Inc<EObjectNewCreated>> objectNewCreatedEventFilter = default;
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        readonly EcsPoolInject<EcsGroupSystemState> ecsGroupSystemStatePool = default;


        //������
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int eventEntity in startNewGameRequestFilter.Value)
            {
                //����������� ���������� ������ ������ "NewGame"
                EcsGroupSystemStateEvent("NewGame", false);

                world.Value.DelEntity(eventEntity);
            }

            //��� ������� ������� �������� ������ �������
            foreach (int eventEntity in objectNewCreatedEventFilter.Value)
            {
                //���� �������
                ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Get(eventEntity);

                //���� ������� �������� � �������� ����� �����������
                if (eventComp.objectNewCreatedType == ObjectNewCreatedType.Organization)
                {
                    //����������� �������� �������� ������ ORAEO
                    GameCreatePanelRequest(
                        CreatingPanelType.ORAEOBriefInfoPanel,
                        eventComp.objectPE);
                }
                //�����, ���� ������� �������� � �������� ������ EcORAEO
                else if (eventComp.objectNewCreatedType == ObjectNewCreatedType.EcORAEO)
                {
                    //���� ������� ����� ����������
                    if (inputData.Value.mapMode == MapMode.Distance)
                    {
                        //���� �����������-�������� ORAEO - ����������� ������
                        /*if (eventComp.organizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                        {
                            //���� �������� ����� ������ ����� ���
                            if (changeMapModeRequestFilter.Value.GetEntitiesCount() == 0)
                            {
                                //�� ����������� ���������� ������ ����� ����������
                                MapChangeModeRequest(MapMode.Distance);
                            }
                        }*/
                    }
                }
                //�����, ���� ������� �������� � �������� ������ �����
                else if (eventComp.objectNewCreatedType == ObjectNewCreatedType.Fleet)
                {
                    //���� ����
                    eventComp.objectPE.Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //���� ������� �������� ������
                    if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                    {
                        //���� �����������-�������� ����� - ����������� ������
                        if (fleet.parentOrganizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                        {
                            //����������� �������� �������� ������ �����
                            GameCreatePanelRequest(
                                CreatingPanelType.FleetOverviewPanel,
                                fleet.selfPE);
                        }
                    }
                }
                //�����, ���� ������� �������� � �������� ����� ����������� ������
                else if (eventComp.objectNewCreatedType == ObjectNewCreatedType.TaskForce)
                {
                    //���� ����������� ������
                    eventComp.objectPE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //���� ����-��������
                    taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //���� ������� �������� ������
                    if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                    {
                        //���� �����������-�������� ����� - ����������� ������
                        if (fleet.parentOrganizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                        {
                            //����������� �������� �������� ������ ����������� ������
                            GameCreatePanelRequest(
                                CreatingPanelType.TaskForceOverviewPanel,
                                taskForce.selfPE);
                        }
                    }
                }
            }
        }

        void GameCreatePanelRequest(
            CreatingPanelType creatingPanelType,
            EcsPackedEntity objectPE)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ � ����
            int requestEntity = world.Value.NewEntity();
            ref RGameCreatePanel requestComp = ref gameCreatePanelRequestPool.Value.Add(requestEntity);

            //��������� �������� ������ �������
            requestComp = new(
                creatingPanelType,
                objectPE);
        }

        void MapChangeModeRequest(
            MapMode mapMode)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ������ �����
            int requestEntity = world.Value.NewEntity();
            ref RChangeMapMode requestComp = ref changeMapModeRequestPool.Value.Add(requestEntity);

            //��������� ��������� ����� �����
            requestComp.mapMode = mapMode;
        }

        void EcsGroupSystemStateEvent(
            string systemGroupName,
            bool systemGroupState)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ��������� ������ ������
            int eventEntity = world.Value.NewEntity();
            ref EcsGroupSystemState eventComp = ref ecsGroupSystemStatePool.Value.Add(eventEntity);

            //��������� �������� ������ ������
            eventComp.Name = systemGroupName;
            //��������� ��������� ������ ������
            eventComp.State = systemGroupState;
        }
    }
}