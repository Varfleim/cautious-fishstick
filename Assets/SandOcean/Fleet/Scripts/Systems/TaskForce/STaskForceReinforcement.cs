
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce
{
    public class STaskForceReinforcement : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //�����
        readonly EcsPoolInject<CFleet> fleetPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;


        //������� ������
        readonly EcsPoolInject<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, SRTaskForceReinforcement>> tFReinforcementSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceReinforcement> tFReinforcementSelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceRefreshUI> taskForceRefreshUISelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceEmptyCheck> taskForceEmptyCheckSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������, ������� ���������� ����������
            foreach (int taskForceEntity in tFReinforcementSelfRequestFilter.Value)
            {
                //���� ������ � ����������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceReinforcement selfRequestComp = ref tFReinforcementSelfRequestPool.Value.Get(taskForceEntity);

                //��������� �������
                TaskForceReinforcement(ref taskForce, ref selfRequestComp);

                //������� ���������� � �������� ������
                tFReinforcementSelfRequestPool.Value.Del(taskForceEntity);
            }
        }

        void TaskForceReinforcement(
            ref CTaskForce fromTaskForce, ref SRTaskForceReinforcement selfRequestComp)
        {
            //���� ������� ������
            selfRequestComp.targetTaskForcePE.Unpack(world.Value, out int toTaskForceEntity);
            ref CTaskForce toTaskForce = ref taskForcePool.Value.Get(toTaskForceEntity);

            //��� ������� ������� � �������� ������ � �������� �������
            for (int a = fromTaskForce.ships.Count - 1; a >= 0; a--)
            {
                //���� �������
                DShip ship = fromTaskForce.ships[a];

                //��������� �������
                TaskForceFunctions.ShipTransfer(
                    ref fromTaskForce, ref toTaskForce,
                    ship);

                //��� ������� ���� ������� � ������� ������� ������
                for (int b = 0; b < toTaskForce.reinforcementRequest.shipTypes.Count; b++)
                {
                    //���� ��� ��� �������
                    if (toTaskForce.reinforcementRequest.shipTypes[b].shipType == ship.shipType)
                    {
                        //��������� ���������� ����������� � ������������ ��������
                        toTaskForce.reinforcementRequest.shipTypes[b].requestedShipCount--;
                        toTaskForce.reinforcementRequest.shipTypes[b].sentedShipCount--;

                        //������� �� �����, ��������� ��� ������
                        break;
                    }
                }
            }
            //������� ������ ��������
            fromTaskForce.ships.Clear();

            //����������� �������� ���������� ��� ������� ������
            TaskForceFunctions.ReinforcementCheckSelfRequest(
                world.Value,
                reserveFleetReinforcementCheckSelfRequestPool.Value,
                fleetPool.Value,
                taskForceReinforcementCheckSelfRequestPool.Value,
                ref toTaskForce);

            //����������� ���������� ���������� ������� ������
            TaskForceFunctions.RefreshUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                taskForceRefreshUISelfRequestPool.Value,
                ref toTaskForce);


            //���� �������� �������� ������
            fromTaskForce.selfPE.Unpack(world.Value, out int taskForceEntity);

            //���� �������� ������ �����
            if (fromTaskForce.ships.Count == 0)
            {
                //����������� �������� ������ ������
                TaskForceDeleteSelfRequest(taskForceEntity);
            }
        }

        void TaskForceDeleteSelfRequest(
            int taskForceEntity)
        {
            //��������� �������� ����������� ������ ���������� ��������
            ref SRTaskForceEmptyCheck requestComp = ref taskForceEmptyCheckSelfRequestPool.Value.Add(taskForceEntity);
        }
    }
}