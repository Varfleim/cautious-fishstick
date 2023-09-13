using Leopotam.EcsLite;

using SandOcean.UI;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce
{
    public static class TaskForceFunctions
    {
        public static void ReinforcementCheckSelfRequest(
            EcsWorld world,
            EcsPool<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool,
            EcsPool<CFleet> fleetPool,
            EcsPool<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool,
            ref CTaskForce taskForce)
        {
            //���� ������ ������ �� ����
            if (taskForce.template != null)
            {
                //���� ������������ ���� ����������� ������
                taskForce.parentFleetPE.Unpack(world, out int fleetEntity);
                ref CFleet fleet = ref fleetPool.Get(fleetEntity);

                //���� �������� ���������� �����
                fleet.reserveFleetPE.Unpack(world, out int reserveFleetEntity);

                //���� ��������� ���� �� ����� ����������� �������� ����������
                if (reserveFleetReinforcementCheckSelfRequestPool.Has(reserveFleetEntity) == false)
                {
                    //��������� ���������� ����� ���������� �������� ����������
                    ref SRReserveFleetReinforcementCheck selfRequestComp = ref reserveFleetReinforcementCheckSelfRequestPool.Add(reserveFleetEntity);
                }

                //���� �������� ������ � ��������� �� ���������� �������� ����������
                taskForce.selfPE.Unpack(world, out int taskForceEntity);

                //���� ������ �� ����� ����������� �������� ����������
                if (taskForceReinforcementCheckSelfRequestPool.Has(taskForceEntity) == false)
                {
                    //��������� ������ ���������� �������� ����������
                    ref SRTaskForceReinforcementCheck selfRequestComp = ref taskForceReinforcementCheckSelfRequestPool.Add(taskForceEntity);
                }
            }
        }

        public static void RefreshUISelfRequest(
            EcsWorld world,
            EcsPool<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool, EcsPool<SRTaskForceRefreshUI> taskForceRefreshUISelfRequestPool,
            ref CTaskForce taskForce,
            RefresUIType requestType = RefresUIType.Refresh)
        {
            //���� �������� ����������� ������
            taskForce.selfPE.Unpack(world, out int taskForceEntity);

            //���� ������ ����� ������������ �������� ������
            if (taskForceDisplayedSummaryPanelPool.Has(taskForceEntity) == true)
            {
                //���� ������ �� ����� ����������� ���������� ����������
                if (taskForceRefreshUISelfRequestPool.Has(taskForceEntity) == false)
                {
                    //�� ��������� �� ���������� ���������� ����������
                    ref SRTaskForceRefreshUI selfRequestComp = ref taskForceRefreshUISelfRequestPool.Add(taskForceEntity);

                    //��������� ������ �����������
                    selfRequestComp = new(requestType);
                }
                //�����
                else
                {
                    //���� ���������� ���������� ����������
                    ref SRTaskForceRefreshUI selfRequestComp = ref taskForceRefreshUISelfRequestPool.Add(taskForceEntity);

                    //���� ������������� �� ��������
                    if (selfRequestComp.requestType != RefresUIType.Delete)
                    {
                        //�� ��������� ������
                        selfRequestComp.requestType = requestType;
                    }
                }
            }
        }

        public static void ShipTransfer(
            ref CTaskForce fromTaskForce, ref CTaskForce toTaskForce,
            DShip ship)
        {
            //������� ������� �� ������ �������� ����������� ������
            fromTaskForce.ships.Remove(ship);

            //������� ������� � ������ ������� ������
            toTaskForce.ships.Add(ship);
        }
    }
}