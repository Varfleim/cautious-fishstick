
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.Organization;
using SandOcean.Map;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce
{
    public class STaskForceEmptyCheck : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //�����
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //�����
        readonly EcsPoolInject<CFleet> fleetPool = default;
        //readonly EcsPoolInject<CFleetPMissionSearch> fleetPatrolMissionSearchPool = default;
        //readonly EcsPoolInject<CFleetTMissionStrikeGroup> fleetTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CFleetTMissionReinforcement> fleetTMissionReinforcementPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        //readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        //readonly EcsPoolInject<CTaskForcePMissionSearch> taskForcePatrolMissionSearchPool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;
        //readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> taskForceTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CTaskForceTMissionReinforcement> taskForceTargetMissionReinforcementPool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;


        //������� ������
        readonly EcsPoolInject<SRTaskForceRefreshUI> tFRefreshUISelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, SRTaskForceEmptyCheck>> tFEmptyCheckSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceEmptyCheck> tFEmptyCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceTMissionReinforcement, SRTaskForceDelete>> tFTMissionReinforcementDeleteSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceDelete> tFDeleteSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������ � ������������ �������� �������
            foreach (int taskForceEntity in tFEmptyCheckSelfRequestFilter.Value)
            {
                //���� ������ � ����������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceEmptyCheck requestComp = ref tFEmptyCheckSelfRequestPool.Value.Get(taskForceEntity);

                //���� ������ �����
                if (taskForce.ships.Count == 0)
                {
                    //���� ������ ����� ������ ����������
                    if (taskForceTargetMissionReinforcementPool.Value.Has(taskForceEntity) == true)
                    {
                        //�� ������ ������� �������, ��������� ����� ������ �� ����� ������������ �������

                        //����������� �������� ������
                        TaskForceDeleteSelfRequest(taskForceEntity);
                    }
                }

                tFEmptyCheckSelfRequestPool.Value.Del(taskForceEntity);
            }

            //��� ������ ����������� ������ � ������� ������ � ������������ ��������
            /*foreach (int taskForceEntity in tFPatrolMissionSearchDeleteSelfRequestFilter.Value)
            {
                //���� ������ � ����������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceDelete requestComp = ref taskForceDeleteSelfRequestPool.Value.Get(taskForceEntity);
            }

            //��� ������ ����������� ������ � ������� ������� ������ � ������������ ��������
            foreach (int taskForceEntity in tFPatrolMissionSearchDeleteSelfRequestFilter.Value)
            {
                //���� ������ � ����������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceDelete requestComp = ref taskForceDeleteSelfRequestPool.Value.Get(taskForceEntity);
            }*/

            //������� ������ � ������� ����������
            TaskForceTMissionReinforcementDelete();

            //��� ������ ����������� ������ � ������������ ��������
            /*foreach (int taskForceEntity in taskForceDeleteSelfRequestFilter.Value)
            {
                //���� ������ � ����������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
            }*/
        }

        void TaskForceTMissionReinforcementDelete()
        {
            //��� ������ ����������� ������ � ������� ���������� � ������������ ��������
            foreach (int taskForceEntity in tFTMissionReinforcementDeleteSelfRequestFilter.Value)
            {
                //���� ������ � ����������
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceDelete requestComp = ref tFDeleteSelfRequestPool.Value.Get(taskForceEntity);

                //���� ������������ ���� � ��� ������ ����������
                taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);
                ref CFleetTMissionReinforcement fleetTMissionReinforcement = ref fleetTMissionReinforcementPool.Value.Get(fleetEntity);

                //������� ������ �� ������ �������� �����
                fleetTMissionReinforcement.activeTaskForcePEs.Remove(taskForce.selfPE);

                //���� ������ �������� ����� ����, �� ������� ������ ���������� �����
                if (fleetTMissionReinforcement.activeTaskForcePEs.Count == 0)
                {
                    fleetTMissionReinforcementPool.Value.Del(fleetEntity);
                }

                //������� ��������� ������� ������ � ������ ����������
                taskForceTargetMissionPool.Value.Del(taskForceEntity);
                taskForceTargetMissionReinforcementPool.Value.Del(taskForceEntity);

                //������� ������
                TaskForceDelete(taskForceEntity, ref taskForce);

                tFDeleteSelfRequestPool.Value.Del(taskForceEntity);
            }
        }

        void TaskForceDelete(
            int taskForceEntity, ref CTaskForce taskForce)
        {
            //���� ������� ������ ������
            taskForce.currentRegionPE.Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

            //������� ������ �� ������ ����� � �������
            region.taskForcePEs.Remove(taskForce.selfPE);

            //���� ������������ ����
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //������� ������ �� ������ ����� �����
            fleet.ownedTaskForcePEs.Remove(taskForce.selfPE);
            //���� � ������ ���� ������ ����������
            if (taskForce.reinforcementRequest != null)
            {
                //������� ��� �� ������ �����
                fleet.tFReinforcementRequests.Remove(taskForce.reinforcementRequest);
            }

            //������� ������� ������
            TaskForceShipDelete(ref taskForce);

            //���� � ������ ���� ������
            if (taskForce.template != null)
            {
                //������� ������ �� ������ ����� �������
                taskForce.template.taskForces.Remove(taskForce.selfPE);
            }

            UnityEngine.Debug.LogWarning("Task Force Deleted! " + taskForce.rand);

            //����������� �������� ���������� ������
            TaskForceFunctions.RefreshUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                tFRefreshUISelfRequestPool.Value,
                ref taskForce,
                UI.RefresUIType.Delete);

            //������� ��������� ������
            taskForcePool.Value.Del(taskForceEntity);
        }

        void TaskForceShipDelete(
            ref CTaskForce taskForce)
        {
            //��� ������� ������� � ����������� ������
            for (int a = 0; a < taskForce.ships.Count; a++)
            {
                //���� �������
                DShip ship = taskForce.ships[a];

            }

            //������� ������ ��������
            taskForce.ships.Clear();
        }

        void TaskForceDeleteSelfRequest(
            int taskForceEntity)
        {
            //��������� �������� ����������� ������ ���������� ��������
            ref SRTaskForceDelete selfRequestComp = ref tFDeleteSelfRequestPool.Value.Add(taskForceEntity);
        }
    }
}