
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.Organization;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public class STaskForceReinforcementCreating : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //�����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //�����
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsPoolInject<CReserveFleet> reserveFleetPool = default;
        readonly EcsPoolInject<CFleetTMissionReinforcement> fleetTargetMissionReinforcementPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;
        readonly EcsPoolInject<CTaskForceTMissionReinforcement> taskForceTargetMissionReinforcementPool = default;

        //������� ������
        readonly EcsFilterInject<Inc<CFleet, CReserveFleet, SRReserveFleetReinforcementCheck>> reserveFleetReinforcementCheckSelfRequestFilter = default;
        readonly EcsPoolInject<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, SRTaskForceReinforcementCheck>> taskForceReinforcementCheckSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool = default;

        //����� �������
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������ � ������������ �������� ����������
            foreach (int taskForceEntity in taskForceReinforcementCheckSelfRequestFilter.Value)
            {
                //������� ����������
                taskForceReinforcementCheckSelfRequestPool.Value.Del(taskForceEntity);
            }

            //��� ������� ���������� ����� � ������������ �������� ����������
            foreach (int fleetEntity in reserveFleetReinforcementCheckSelfRequestFilter.Value)
            {
                //���� ��������� ���� � ���������� �������� ����������
                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);
                ref CReserveFleet reserveFleet = ref reserveFleetPool.Value.Get(fleetEntity);
                ref SRReserveFleetReinforcementCheck selfRequestComp = ref reserveFleetReinforcementCheckSelfRequestPool.Value.Get(fleetEntity);

                //��� ������ ����������� ������ ���������� �� ����� � �������� �������
                for (int a = reserveFleet.requestedTaskForceReinforcements.Count - 1; a >= 0; a--)
                {
                    UnityEngine.Debug.LogWarning("!");

                    //������ ������ ����������
                    TaskForceReinforcementCreating(
                        reserveFleet.requestedTaskForceReinforcements[a]);

                    //������� ������ ������ �� ������
                    reserveFleet.requestedTaskForceReinforcements.RemoveAt(a);
                }

                //���������, ��������� �� ����������
                bool isReinforcementComplete = true;

                //��� ������� ��������� �����
                for (int a = 0; a < reserveFleet.ownedFleetPEs.Count; a++)
                {
                    //���� �������� ����
                    reserveFleet.ownedFleetPEs[a].Unpack(world.Value, out int ownedFleetEntity);
                    ref CFleet ownedFleet = ref fleetPool.Value.Get(ownedFleetEntity);

                    //��� ������� ������� ����������
                    for (int b = 0; b < ownedFleet.tFReinforcementRequests.Count; b++)
                    {
                        //���� ���������� ����������� ����� �������� ������ ����
                        if (fleet.tFReinforcementRequests[b].shipTypes.Count > 0)
                        {
                            //��������, ��� ���������� �� ���������
                            isReinforcementComplete = false;

                            break;
                        }
                    }
                }

                //���� ���������� ���������
                if (isReinforcementComplete == true)
                {
                    //������� ���������� �������� ���������� � �������� �����
                    reserveFleetReinforcementCheckSelfRequestPool.Value.Del(fleetEntity);
                }
            }
        }

        void TaskForceReinforcementCreating(
            DTaskForceReinforcementCreating taskForceReinforcementCreating)
        {
            //���� ������� ����������� ������
            taskForceReinforcementCreating.targetTaskForcePE.Unpack(world.Value, out int targetTaskForceEntity);
            ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

            //���� ����������� ���� ������� ������
            targetTaskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //���� ������������ ����������� ������� �����
            fleet.parentOrganizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);


            //������ ����� �������� � ��������� �� ��������� ����������� ������
            int taskForceEntity = world.Value.NewEntity();
            ref CTaskForce taskForce = ref taskForcePool.Value.Add(taskForceEntity);

            //��������� �������� ������ ������ ����������
            taskForce = new(
                world.Value.PackEntity(taskForceEntity),
                fleet.selfPE);

            taskForce.rand = UnityEngine.Random.Range(0, 50);

            //������� ������ � ������ �����
            fleet.ownedTaskForcePEs.Add(taskForce.selfPE);

            //�������� ������ ������ �� ����������
            TaskForceChangeMission(
                ref taskForce,
                TaskForceActionType.ChangeMissionReinforcement,
                taskForceReinforcementCreating);

            //������ ������� ������
            TaskForceCreateShips(
                ref taskForce,
                ref taskForceReinforcementCreating);

            //������ �������, ���������� � �������� ����� ������
            ObjectNewCreatedEvent(taskForce.selfPE, ObjectNewCreatedType.TaskForce);
        }

        void TaskForceChangeMission(
            ref CTaskForce taskForce,
            TaskForceActionType actionType,
            DTaskForceReinforcementCreating taskForceReinforcementCreating = new())
        {
            //���� ������������� ��������� ������ ����������
            if (actionType == TaskForceActionType.ChangeMissionReinforcement)
            {
                //������ ������ ������ �� ����������
                TaskForceChangeMissionReinforcement(
                    ref taskForce,
                    ref taskForceReinforcementCreating);

                taskForce.activeMissionType = TaskForceMissionType.TargetMissionReinforcement;
            }

            UnityEngine.Debug.LogWarning(taskForce.rand + " ! " + taskForce.activeMissionType);
        }

        void TaskForceChangeMissionReinforcement(
            ref CTaskForce taskForce,
            ref DTaskForceReinforcementCreating taskForceReinforcementCreating)
        {
            //���� ������������ ���� ����������� ������
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //���� ���� �� ����� ���������� ����������
            if (fleetTargetMissionReinforcementPool.Value.Has(fleetEntity) == false)
            {
                //��������� ����� ��������� ������ ����������
                ref CFleetTMissionReinforcement tMissionReinforcement = ref fleetTargetMissionReinforcementPool.Value.Add(fleetEntity);

                //��������� �������� ������ ���������� ������
                tMissionReinforcement = new(0);
            }

            //���� ��������� ������ ����������
            ref CFleetTMissionReinforcement fleetTMissionReinforcement = ref fleetTargetMissionReinforcementPool.Value.Get(fleetEntity);

            //���� �������� ������ � ��������� �� �������� ������� ������ � ������ ����������
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);
            ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool.Value.Add(taskForceEntity);
            ref CTaskForceTMissionReinforcement tFMissionReinforcement = ref taskForceTargetMissionReinforcementPool.Value.Add(taskForceEntity);

            //��������� �������� ������ ���������� ������� ������
            tFTargetMission = new(0);

            //��������� �������� ������ ���������� ������ ����������
            tFMissionReinforcement = new(0);

            //������� ������ � ������ �����, ����������� ������ ����������
            fleetTMissionReinforcement.activeTaskForcePEs.Add(taskForce.selfPE);

            //���� ������� ������
            taskForceReinforcementCreating.targetTaskForcePE.Unpack(world.Value, out int targetTaskForceEntity);
            ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

            //��������� ������ ������� ������
            taskForce.movementTargetPE = targetTaskForce.selfPE;
            taskForce.movementTargetType = TaskForceMovementTargetType.TaskForce;

            if (fleet.fleetRegions.Count > 0)
            {
                taskForce.currentRegionPE = fleet.fleetRegions[UnityEngine.Random.Range(0, fleet.fleetRegions.Count)].regionPE;
            }


            UnityEngine.Debug.LogWarning("!1!");

            //��������� ������ � ����� ��������
            tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;

            //���� ������� ������ ��������� �� � ��� �� �������, ��� �������
            /*if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.currentRegionPE) == false)
            {
                UnityEngine.Debug.LogWarning("!1!");

                //��������� ������ � ����� ��������
                tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;
            }
            //�����
            else
            {
                UnityEngine.Debug.LogWarning("!2!");

                //����
                //��������� ������ � ������ �����, ��� ����� ���������� �
                tFTargetMission.missionStatus = TaskForceMissionStatus.None;
                //����
            }*/
        }

        void TaskForceCreateShips(
            ref CTaskForce taskForce,
            ref DTaskForceReinforcementCreating taskForceReinforcementCreating)
        {
            //����
            //��� ������� ���� ������� � �������
            for (int a = 0; a < taskForceReinforcementCreating.shipTypes.Count; a++)
            {
                //��� ������� ������������ �������
                for (int b = 0; b < taskForceReinforcementCreating.shipTypes[a].shipCount; b++)
                {
                    //������ �������
                    ShipCreate(
                        ref taskForce,
                        taskForceReinforcementCreating.shipTypes[a].shipType);
                }
            }
            //����
        }

        void ShipCreate(
            ref CTaskForce taskForce,
            DShipType shipType)
        {
            //������ ������� � ��������� ��� ������
            DShip ship = new(
                RuntimeData.shipCount, shipType);

            //��������� ������� ��������
            RuntimeData.shipCount++;

            //������� ������� � ������ �������� ������
            taskForce.ships.Add(ship);
        }

        void ObjectNewCreatedEvent(
            EcsPackedEntity objectPE, ObjectNewCreatedType objectNewCreatedType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ �������
            int eventEntity = world.Value.NewEntity();
            ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Add(eventEntity);

            //��������� ������ �������
            eventComp = new(objectPE, objectNewCreatedType);
        }
    }
}