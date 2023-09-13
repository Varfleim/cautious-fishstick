
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.Warfare.Fleet.Moving
{
    public struct TTaskForceMovement : IEcsThread<
        CTaskForceMovement, CTaskForce,
        CHexRegion>
    {
        public EcsWorld world;

        int[] taskForceEntities;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        CTaskForceMovement[] taskForceMovementPool;
        int[] taskForceMovementIndices;

        CHexRegion[] regionPool;
        int[] regionIndices;

        public void Init(
            int[] entities,
            CTaskForceMovement[] pool1, int[] indices1,
            CTaskForce[] pool2, int[] indices2,
            CHexRegion[] pool3, int[] indices3)
        {
            taskForceEntities = entities;

            taskForceMovementPool = pool1;
            taskForceMovementIndices = indices1;

            taskForcePool = pool2;
            taskForceIndices = indices2;

            regionPool = pool3;
            regionIndices = indices3;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //��� ������ ����������� ������ � ����������� ��������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� �������� � ������
                int taskForceEntity = taskForceEntities[a];
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool[taskForceMovementIndices[taskForceEntity]];
                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                //���� ���������� ������ �� ����
                if (taskForce.previousRegionPE.Unpack(world, out int previousRegionEntity))
                {
                    //�������� ���������� ������
                    taskForce.previousRegionPE = new();
                }

                //���� ������� ������ �� ����
                if (tFMovement.pathRegionPEs.Count > 0)
                {
                    //���� ��������� ������ � ��������, �� ���� ��������� ������ ����
                    tFMovement.pathRegionPEs[tFMovement.pathRegionPEs.Count - 1].Unpack(world, out int nextRegionEntity);
                    ref CHexRegion nextRegion = ref regionPool[regionIndices[nextRegionEntity]];

                    //������������ �������� � ������ ������� ����������� ������ � ������������ ���������� �������
                    float movementSpeed = 50;

                    //���������� �������� � ����������� ����������
                    tFMovement.traveledDistance += movementSpeed;

                    //���� ���������� ���������� ������ ��� ����� ���������� ����� ���������
                    if (tFMovement.traveledDistance >= RegionsData.regionDistance)
                    {
                        //�� ������ ��������� � ��������� ������

                        //��������, ��� ������ ��������� �����������
                        tFMovement.isTraveled = true;

                        //�������� ���������� ����������
                        tFMovement.traveledDistance = 0;

                        //UnityEngine.Debug.LogWarning("Finish 1! " + taskForce.rand);
                    }
                }
                //�����
                else
                {
                    //������ ��� ��������� � ������� ������� (��� �������� ������ ��� ���������� ������� ����)

                    //��������, ��� ������ ��������� �����������
                    tFMovement.isTraveled = true;

                    //�������� ���������� ����������
                    tFMovement.traveledDistance = 0;

                    //UnityEngine.Debug.LogWarning("Finish 2! " + taskForce.rand);
                }
            }
        }
    }
}