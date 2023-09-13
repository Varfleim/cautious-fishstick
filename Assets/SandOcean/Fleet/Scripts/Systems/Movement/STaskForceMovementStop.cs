
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce;
using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class STaskForceMovementStop : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //�����
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //�����
        readonly EcsPoolInject<CTaskForce> taskForcePool = default;

        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceMovement>> taskForceMovingFilter = default;
        readonly EcsPoolInject<CTaskForceMovement> taskForceMovementPool = default;
        readonly EcsPoolInject<CTaskForceMovementToMoving> taskForceMovementToMovingPool = default;


        //������� ������
        readonly EcsPoolInject<SRTaskForceTargetCheck> taskForceTargetCheckSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������ ����������� ������ � ����������� ��������
            foreach (int taskForceEntity in taskForceMovingFilter.Value)
            {
                //���� ��������� ��������
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool.Value.Get(taskForceEntity);

                //���� ������ ��������� ��������
                if (tFMovement.isTraveled == true)
                {
                    //��������, ��� ������ ����� ���������� ��������
                    tFMovement.isTraveled = false;

                    //���� ������
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //���� ������ ����� ��������� ������ � ��������
                    if (tFMovement.pathRegionPEs.Count > 0)
                    {
                        //���� ������� ������ ������
                        taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                        ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                        //������� ������ �� ������ ����� � �������
                        currentRegion.taskForcePEs.Remove(taskForce.selfPE);

                        //���� ��������� ������ � �������� ������
                        tFMovement.pathRegionPEs[tFMovement.pathRegionPEs.Count - 1].Unpack(world.Value, out int nextRegionEntity);
                        ref CHexRegion nextRegion = ref regionPool.Value.Get(nextRegionEntity);

                        //������� ������ � ������ ����� � �������
                        nextRegion.taskForcePEs.Add(taskForce.selfPE);

                        //������ ������� ������ ������
                        taskForce.currentRegionPE = nextRegion.selfPE;

                        //������� ��������� (��� �������) ������ �� ��������
                        tFMovement.pathRegionPEs.RemoveAt(tFMovement.pathRegionPEs.Count - 1);

                        //��������� ���������� ������ ������
                        taskForce.previousRegionPE = currentRegion.selfPE;
                    }

                    //���� � �������� ������ ������ �� �������� ��������
                    if (tFMovement.pathRegionPEs.Count == 0)
                    {
                        //����������� �������� ����
                        TaskForceAssignTargetCheck(taskForceEntity);

                        //������� ��������� ��������
                        taskForceMovementPool.Value.Del(taskForceEntity);

                        //���� ������ ����� ��������� �������� � ���������� ����, ������� ���
                        if (taskForceMovementToMovingPool.Value.Has(taskForceEntity) == true)
                        {
                            taskForceMovementToMovingPool.Value.Del(taskForceEntity);
                        }
                    }
                }
            }
        }

        void TaskForceAssignTargetCheck(
            int taskForceEntity)
        {
            //��������� ����������� ������ ���������� �������� ����
            ref SRTaskForceTargetCheck tfTargetCheck = ref taskForceTargetCheckSelfRequestPool.Value.Add(taskForceEntity);

            //��������� �������� ������ �����������
            tfTargetCheck = new(0);
        }
    }
}