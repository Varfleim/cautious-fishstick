
using System;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct TFleetPMissionSearchTargetAssign : IEcsThread<
        CFleetPMissionSearch, CFleet,
        CTaskForce, CTaskForcePatrolMission>
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

        public void Init(
            int[] entities,
            CFleetPMissionSearch[] pool1, int[] indices1,
            CFleet[] pool2, int[] indices2,
            CTaskForce[] pool3, int[] indices3,
            CTaskForcePatrolMission[] pool4, int[] indices4)
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

                //���� � ����� ���� �������
                if (fleet.fleetRegions.Count > 0)
                {
                    //���������, ���� �� � ����� ��������� ������
                    bool hasFreeForce = false;

                    //��� ������ �������� ������
                    for (int b = 0; b < fleetPMissionSearch.activeTaskForcePEs.Count; b++)
                    {
                        //���� ���������� ������ ������
                        fleetPMissionSearch.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                        ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool[taskForcePatrolMissionIndices[taskForceEntity]];

                        //���� ������ �� ��������� �� ������
                        if (tFPatrolMission.missionStatus == TaskForceMissionStatus.None)
                        {
                            //��������, ��� ��������� ������ ����, � ������� �� �����
                            hasFreeForce = true;

                            break;
                        }
                    }

                    //���� � ����� ���� ��������� ������
                    if (hasFreeForce == true)
                    {
                        //������ ������ ��� ������������ ���������� ������ ��������
                        DFleetRegionPriority[] regionSearchPriorities = new DFleetRegionPriority[fleet.fleetRegions.Count];

                        //��� ������� ������� �����
                        for (int b = 0; b < fleet.fleetRegions.Count; b++)
                        {
                            //������������ ����� ����� � ���������� ������ �������
                            float neighbourAverageLastTime = 0;

                            //��� ������� ������
                            for (int c = 0; c < fleet.fleetRegions[b].neighbourRegions.Count; c++)
                            {
                                //���������� ����� � ���������� ������
                                neighbourAverageLastTime += fleet.fleetRegions[b].neighbourRegions[c].searchMissionLastTime;
                            }

                            //����� ���������� ����� �� ���������� �������
                            neighbourAverageLastTime /= fleet.fleetRegions[b].neighbourRegions.Count;

                            //������� ���������� ����� � ����� � ���������� ������ ������ ������� � ������
                            regionSearchPriorities[b] = new(b, neighbourAverageLastTime + fleet.fleetRegions[b].searchMissionLastTime);
                        }

                        //��������� ������ �����������
                        Array.Sort(regionSearchPriorities);

                        //������ ����������, ������������, � ����� ������ ����� ���������� ������
                        int forceRegionIndex = regionSearchPriorities.Length - 1;

                        //��� ������ ������ ������
                        for (int b = 0; b < fleetPMissionSearch.activeTaskForcePEs.Count; b++)
                        {
                            //���� ������ ������� ������ ����
                            if (forceRegionIndex < 0)
                            {
                                //���������� ��� �� ������������
                                forceRegionIndex = regionSearchPriorities.Length - 1;
                            }

                            //���� ���������� ������ ������
                            fleetPMissionSearch.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                            ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool[taskForcePatrolMissionIndices[taskForceEntity]];

                            //���� ������ �� ��������� �� � ������ ��������, �� ��������
                            if (tFPatrolMission.missionStatus == TaskForceMissionStatus.None)
                            {
                                //���� ������
                                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                                //���� ������ � ���������� ����������� - ��������� ����� ���������� ������������ �����
                                DFleetRegion fleetRegion = fleet.fleetRegions[regionSearchPriorities[forceRegionIndex].regionIndex];

                                //��������� ������ ������� ������
                                taskForce.movementTargetPE = fleetRegion.regionPE;
                                taskForce.movementTargetType = TaskForceMovementTargetType.Region;

                                //��������� ������ � ����� ��������
                                tFPatrolMission.missionStatus = TaskForceMissionStatus.Movement;

                                //��������� ������ �������, ��������� ���� ������ � ������� ��������
                                forceRegionIndex--;
                            }
                        }
                    }
                }
            }
        }
    }
}