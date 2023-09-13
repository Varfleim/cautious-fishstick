
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct TFleetPMissionSearchRegionUpdate : IEcsThread<
        CFleetPMissionSearch, CFleet,
        CTaskForce, CTaskForcePatrolMission, CTaskForceWaiting>
    {
        public EcsWorld world;

        int[] fleetEntities;

        CFleetPMissionSearch[] fleetPatrolMissionSearchPool;
        int[] fleetPatrolMissionSearchIndices;

        CFleet[] fleetPool;
        int[] fleetIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        CTaskForcePatrolMission[] taskForcePatrolMissionPool;
        int[] taskForcePatrolMissionIndices;

        CTaskForceWaiting[] taskForceWaitingPool;
        int[] taskForceWaitingIndices;

        public void Init(
            int[] entities,
            CFleetPMissionSearch[] pool1, int[] indices1,
            CFleet[] pool2, int[] indices2,
            CTaskForce[] pool3, int[] indices3,
            CTaskForcePatrolMission[] pool4, int[] indices4,
            CTaskForceWaiting[] pool5, int[] indices5)
        {
            fleetEntities = entities;

            fleetPatrolMissionSearchPool = pool1;
            fleetPatrolMissionSearchIndices = indices1;

            fleetPool = pool2;
            fleetIndices = indices2;

            taskForcePool = pool3;
            taskForceIndices = indices3;

            taskForcePatrolMissionPool = pool4;
            taskForcePatrolMissionIndices = indices4;

            taskForceWaitingPool = pool5;
            taskForceWaitingIndices = indices5;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //��� ������� ����� � ������� ������ � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ����
                int fleetEntity = fleetEntities[a];
                ref CFleetPMissionSearch fleetPMissionSearch = ref fleetPatrolMissionSearchPool[fleetPatrolMissionSearchIndices[fleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[fleetEntity]];

                //��� ������ ������ ����������� ������
                for (int b = 0; b < fleetPMissionSearch.activeTaskForcePEs.Count; b++)
                {
                    //���� � ���������� ������
                    fleetPMissionSearch.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                    ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool[taskForcePatrolMissionIndices[taskForceEntity]];

                    //���� ������ ��������� � ������ ��������
                    if (tFPatrolMission.missionStatus == TaskForceMissionStatus.Waiting)
                    {
                        //���� ��������� ��������
                        ref CTaskForceWaiting tFWaiting = ref taskForceWaitingPool[taskForceWaitingIndices[taskForceEntity]];

                        //���������� 1 � �����, ���������� � ��������
                        tFWaiting.waitingTime++;

                        //���� ����� �������� ������ ��� ����� ������������� ������������
                        if (tFWaiting.waitingTime >= 3)
                        {
                            //��������� ������ � ������ �����, ��� ����� ���������� � � ���������� �������� ���������� ��������
                            tFPatrolMission.missionStatus = TaskForceMissionStatus.None;
                        }
                    }
                }

                //��� ������� ������� �����
                for (int b = 0; b < fleet.fleetRegions.Count; b++)
                {
                    //���������� 1 � ����� � ���������� ������
                    fleet.fleetRegions[b].searchMissionLastTime++;

                    //��� ������ ������ ������
                    for (int c = 0; c < fleetPMissionSearch.activeTaskForcePEs.Count; c++)
                    {
                        //���� ������ � � ������
                        fleetPMissionSearch.activeTaskForcePEs[c].Unpack(world, out int taskForceEntity);
                        ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                        //���� ������ ��������� � ������ �������
                        if (taskForce.currentRegionPE.EqualsTo(fleet.fleetRegions[b].regionPE) == true)
                        {
                            //�������� ���� � ���������� ������
                            fleet.fleetRegions[b].searchMissionLastTime = 0;
                        }
                    }
                }
            }
        }
    }
}