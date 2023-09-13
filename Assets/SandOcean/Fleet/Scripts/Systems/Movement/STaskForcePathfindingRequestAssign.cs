
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Warfare.TaskForce;
using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class STaskForcePathfindingRequestAssign : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //�����
        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForcePatrolMission>, Exc<CTaskForceMovement>> taskForcePatrolMissionFilter = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceTargetMission>, Exc<CTaskForceMovement>> taskForceTargetMissionFilter = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;

        readonly EcsPoolInject<CTaskForceMovement> taskForceMovementPool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceMovementToMoving>> taskForceMovementToMovingFilter = default;
        readonly EcsPoolInject<CTaskForceMovementToMoving> taskForceMovementToMovingPool = default;

        //������� ������
        readonly EcsPoolInject<SRTaskForceFindPath> taskForceFindPathSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������, ������� ���������� ������, �� �� ������� ���������� ��������
            foreach (int taskForceEntity in taskForcePatrolMissionFilter.Value)
            {
                //���� ���������� ������ ������
                ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool.Value.Get(taskForceEntity);

                //���� ������ ��������� � ������ ��������
                if (tFPatrolMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //��������� ���������� �������� ������
                    TaskForceAssignMovement(taskForceEntity);
                }
            }

            //��� ������ ����������� ������, ������� ������� ������, �� �� ������� ���������� ��������
            foreach (int taskForceEntity in taskForceTargetMissionFilter.Value)
            {
                //���� ������� ������ ������
                ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool.Value.Get(taskForceEntity);

                //���� ������ ��������� � ������ ��������
                if (tFTargetMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //��������� ���������� �������� ������
                    TaskForceAssignMovement(taskForceEntity);
                }
            }

            //��� ������ ����������� ������, ���������� � ���������� ����
            foreach (int taskForceEntity in taskForceMovementToMovingFilter.Value)
            {
                //���� ������ � ��������� �������� � ���������� ����
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForceMovementToMoving tFMovementToMoving = ref taskForceMovementToMovingPool.Value.Get(taskForceEntity);

                //���� ���� �������� - ����������� ������
                if (taskForce.movementTargetType == TaskForceMovementTargetType.TaskForce)
                {
                    //���� ������� ������
                    taskForce.movementTargetPE.Unpack(world.Value, out int targetTaskForceEntity);
                    ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

                    //���� ��� ��������� �� � ��� �������, ��� ��� ���������� ��������
                    if (targetTaskForce.currentRegionPE.EqualsTo(tFMovementToMoving.movingTargetLastRegionPE) == false)
                    {
                        //����������� ����� ���� �� �������� ������� ������� ������
                        TaskForceFindPathSelfRequest(
                            taskForceEntity,
                            targetTaskForce.currentRegionPE);

                        //��������� ��������� ������ ���� ��������
                        tFMovementToMoving.movingTargetLastRegionPE = targetTaskForce.currentRegionPE;
                    }
                }
            }
        }

        void TaskForceAssignMovement(
            int taskForceEntity)
        {
            //���� ������
            ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

            //���� ������ ����� ���� ��������
            if (taskForce.movementTargetPE.Unpack(world.Value, out int movingTargetEntity))
            {
                //��������� ������ ��������� ��������
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool.Value.Add(taskForceEntity);

                //��������� ������ ����������
                tFMovement = new(0);

                //���� ���� �������� - ��� ������
                if (taskForce.movementTargetType == TaskForceMovementTargetType.Region)
                {
                    //�� ����� ����� ����������� ����� ���� �� �������� �������
                    TaskForceFindPathSelfRequest(
                        taskForceEntity,
                        taskForce.movementTargetPE);
                }
                //�����, ���� ���� �������� - ����������� ������
                else if (taskForce.movementTargetType == TaskForceMovementTargetType.TaskForce)
                {
                    //���� ������� ������
                    ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(movingTargetEntity);

                    //����������� ����� ���� �� �������� ������� ������� ������
                    TaskForceFindPathSelfRequest(
                        taskForceEntity,
                        targetTaskForce.currentRegionPE);

                    //��������� ����������� ������ ��������� �������� � ���������� ����
                    ref CTaskForceMovementToMoving tFMovementToMoving = ref taskForceMovementToMovingPool.Value.Add(taskForceEntity);

                    //��������� ������ ����������
                    tFMovementToMoving = new(targetTaskForce.currentRegionPE);
                }
                //�����, ���� ���� �������� - 

            }
        }

        void TaskForceFindPathSelfRequest(
            int taskForceEntity,
            EcsPackedEntity targetRegionPE)
        {
            //��������� �������� ����������� ������ ���������� ������ ���� 
            ref SRTaskForceFindPath selfRequestComp = ref taskForceFindPathSelfRequestPool.Value.Add(taskForceEntity);

            //��������� ������ �����������
            selfRequestComp = new(targetRegionPE);
        }
    }
}