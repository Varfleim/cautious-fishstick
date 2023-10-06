using Leopotam.EcsLite;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;
using SandOcean.UI.Events;

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

        public static void RefreshTaskForceUISelfRequest(
            EcsWorld world,
            EcsPool<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool, EcsPool<SRObjectRefreshUI> refreshUISelfRequestPool,
            ref CTaskForce taskForce,
            RefreshUIType requestType = RefreshUIType.Refresh)
        {
            //���� �������� ����������� ������
            taskForce.selfPE.Unpack(world, out int taskForceEntity);

            //���� ������ ����� ������������ �������� ������
            if (taskForceDisplayedSummaryPanelPool.Has(taskForceEntity) == true)
            {
                //���� ������ �� ����� ����������� ���������� ����������
                if (refreshUISelfRequestPool.Has(taskForceEntity) == false)
                {
                    //�� ��������� �� ���������� ���������� ����������
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(taskForceEntity);

                    //��������� ������ �����������
                    selfRequestComp = new(requestType);
                }
                //�����
                else
                {
                    //���� ���������� ���������� ����������
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(taskForceEntity);

                    //���� ������������� �� ��������
                    if (selfRequestComp.requestType != RefreshUIType.Delete)
                    {
                        //�� ��������� ����������
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


        public static void RefreshTFTemplateUISelfRequest(
            EcsWorld world,
            EcsPool<CTFTemplateDisplayedSummaryPanel> tFTemplateDisplayedSummaryPanelPool, EcsPool<SRObjectRefreshUI> refreshUISelfRequestPool,
            ref CTFTemplateDisplayedSummaryPanel tfTemplateDisplayedSummaryPanel,
            RefreshUIType requestType = RefreshUIType.Refresh)
        {
            //���� �������� ������� ����������� ������
            tfTemplateDisplayedSummaryPanel.selfPE.Unpack(world, out int tFTemplateEntity);

            //���� ������ ����� ������������ �������� ������
            //if (tFTemplateDisplayedSummaryPanelPool.Has(tFTemplateEntity) == true)
            //{
                //���� ������ �� ����� ����������� ���������� ����������
                if (refreshUISelfRequestPool.Has(tFTemplateEntity) == false)
                {
                    //�� ��������� ��� ���������� ���������� ����������
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(tFTemplateEntity);

                    //��������� ������ �����������
                    selfRequestComp = new(requestType);
                }
                //�����
                else
                {
                    //���� ���������� ���������� ����������
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(tFTemplateEntity);

                    //���� ������������� �� ��������
                    if (selfRequestComp.requestType != RefreshUIType.Delete)
                    {
                        //�� ��������� ����������
                        selfRequestComp.requestType = requestType;
                    }
                }
            //}
        }
    }
}