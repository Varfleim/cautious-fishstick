
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct TFleetTMissionStrikeGroupTargetAssign : IEcsThread<
        CFleetTMissionStrikeGroup, CFleet,
        CTaskForce, CTaskForceTargetMission>
    {
        public EcsWorld world;

        int[] fleetEntities;

        CFleetTMissionStrikeGroup[] fleetTargetMissionStrikeGroupPool;
        int[] fleetTargetMissionStrikeGroupIndices;

        CFleet[] fleetPool;
        int[] fleetIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        CTaskForceTargetMission[] taskForceTargetMissionPool;
        int[] taskForceTargetMissionIndices;

        public void Init(
            int[] entities,
            CFleetTMissionStrikeGroup[] pool1, int[] indices1,
            CFleet[] pool2, int[] indices2,
            CTaskForce[] pool3, int[] indices3,
            CTaskForceTargetMission[] pool4, int[] indices4)
        {
            fleetEntities = entities;

            fleetTargetMissionStrikeGroupPool = pool1;
            fleetTargetMissionStrikeGroupIndices = indices1;

            fleetPool = pool2;
            fleetIndices = indices2;

            taskForcePool = pool3;
            taskForceIndices = indices3;

            taskForceTargetMissionPool = pool4;
            taskForceTargetMissionIndices = indices4;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //������ ������ ��� ������������ ���������� �����
            List<DFleetTargetPriority> fleetTargetPriorities = new();

            //��� ������� ����� � ������� ������� ������ � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ����
                int fleetEntity = fleetEntities[a];
                ref CFleetTMissionStrikeGroup fleetTMissionStrikeGroup = ref fleetTargetMissionStrikeGroupPool[fleetTargetMissionStrikeGroupIndices[fleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[fleetEntity]];

                //���� � ����� ���� �������
                if (fleet.fleetRegions.Count > 0)
                {
                    //���������, ���� �� � ����� ��������� ������
                    bool hasFreeForce = false;

                    //��� ������ �������� ������
                    for (int b = 0; b < fleetTMissionStrikeGroup.activeTaskForcePEs.Count; b++)
                    {
                        //���� ������� ������ ������
                        fleetTMissionStrikeGroup.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                        ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool[taskForceTargetMissionIndices[taskForceEntity]];

                        //���� ������ �� ������
                        if (tFTargetMission.missionStatus == TaskForceMissionStatus.None)
                        {
                            //��������, ��� ��������� ������ ����, � ������� �� �����
                            hasFreeForce = true;

                            break;
                        }
                    }

                    //���� � ����� ���� ��������� ������
                    if (hasFreeForce == true)
                    {
                        //������� ������ �����������
                        fleetTargetPriorities.Clear();

                        //���������� ������������ ������ ����������, �������� ���������� � ���������� ������ �� ��������

                        //����
                        //��� ������ ������ ������� �����
                        for (int b = 0; b < fleet.ownedTaskForcePEs.Count; b++)
                        {
                            //���� �������� ������
                            fleet.ownedTaskForcePEs[b].Unpack(world, out int tFEntity);

                            //������� ������ �� ��������� ������
                            fleetTargetPriorities.Add(new(tFEntity, 0));
                        }

                        //��������� ������
                        fleetTargetPriorities.Sort();

                        //��� ������ �������� ������
                        for (int b = 0; b < fleetTMissionStrikeGroup.activeTaskForcePEs.Count; b++)
                        {
                            //���� ������� ������ ������
                            fleetTMissionStrikeGroup.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                            ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool[taskForceTargetMissionIndices[taskForceEntity]];

                            //���� ������ �� ������
                            if (tFTargetMission.missionStatus == TaskForceMissionStatus.None)
                            {
                                //���� ������
                                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                                //��� ������ ����������������� ����
                                for (int c = 0; c < fleetTargetPriorities.Count; c++)
                                {
                                    if (b + c > 2)
                                    {

                                        //���� ��� �� ������� ������
                                        if (fleetTargetPriorities[c].targetEntity != taskForceEntity)
                                        {
                                            //���� ������� ������
                                            ref CTaskForce targetTaskForce = ref taskForcePool[taskForceIndices[fleetTargetPriorities[c].targetEntity]];

                                            //��������� ������� ������ ������� ������
                                            taskForce.movementTargetPE = targetTaskForce.selfPE;
                                            taskForce.movementTargetType = TaskForceMovementTargetType.TaskForce;

                                            //���� ������� ������ ��������� �� � ��� �� �������, ��� �������
                                            if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.currentRegionPE) == false)
                                            {
                                                //��������� ������ � ����� ��������
                                                tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;
                                            }
                                            //�����
                                            else
                                            {
                                                //��������� ������ � ����� ���


                                                //����
                                                //��������� ������ � ������ �����, ��� ����� ���������� �
                                                tFTargetMission.missionStatus = TaskForceMissionStatus.None;
                                                //����
                                            }

                                            //������� �� �����
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        //����
                    }
                }
            }
        }
    }
}