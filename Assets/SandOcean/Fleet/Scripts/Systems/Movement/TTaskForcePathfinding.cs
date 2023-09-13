
using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.Map.Pathfinding;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.Warfare.Fleet.Moving
{
    public struct TTaskForcePathfinding : IEcsThread<
        SRTaskForceFindPath, 
        CTaskForce, CTaskForceMovement,
        CHexRegion>
    {
        public EcsWorld world;

        int[] taskForceEntities;

        SRTaskForceFindPath[] taskForceFindPathSelfRequestPool;
        int[] taskForceFindPathSelfRequestIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        CTaskForceMovement[] taskForceMovementPool;
        int[] taskForceMovementIndices;

        CHexRegion[] regionPool;
        int[] regionIndices;

        public void Init(
            int[] entities,
            SRTaskForceFindPath[] pool1, int[] indices1,
            CTaskForce[] pool2, int[] indices2,
            CTaskForceMovement[] pool3, int[] indices3,
            CHexRegion[] pool4, int[] indices4)
        {
            taskForceEntities = entities;

            taskForceFindPathSelfRequestPool = pool1;
            taskForceFindPathSelfRequestIndices = indices1;

            taskForcePool = pool2;
            taskForceIndices = indices2;

            taskForceMovementPool = pool3;
            taskForceMovementIndices = indices3;

            regionPool = pool4;
            regionIndices = indices4;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //���� ������� ���� ������� ����������
            if (RegionsData.needRefreshPathMatrix[threadId] == true)
            {
                //��������� ������� ����
                PathMatrixRefresh(threadId);
            }

            //��� ������ ����������� ������ � ������������ ������ ����
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� �����������, ������ � ��������� ��������
                int taskForceEntity = taskForceEntities[a];
                ref SRTaskForceFindPath requestComp = ref taskForceFindPathSelfRequestPool[taskForceFindPathSelfRequestIndices[taskForceEntity]];
                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool[taskForceMovementIndices[taskForceEntity]];

                //������� ������ ��������
                tFMovement.pathRegionPEs.Clear();

                //������������ ���� �� ����
                PathFind(
                    threadId,
                    ref requestComp,
                    ref taskForce, ref tFMovement);
            }
        }

        void PathMatrixRefresh(
            int threadId)
        {
            //��������, ��� ������� ���� �� ������� ����������
            RegionsData.needRefreshPathMatrix[threadId] = false;

            //���� ������ ��� ������ ���� ����
            if (RegionsData.pathfindingArray[threadId] == null)
            {
                //����� ��������� �������� � �������� ����
                RegionsData.openRegionValues[threadId] = 1;
                RegionsData.closeRegionValues[threadId] = 2;

                //������ ������
                RegionsData.pathfindingArray[threadId] = new DPathFindingNodeFast[RegionsData.regionPEs.Length];

                //������ �������
                RegionsData.pathFindingQueue[threadId] = new(
                    new PathFindingNodesComparer(RegionsData.pathfindingArray[threadId]),
                    RegionsData.regionPEs.Length);

                //������ ������ ��������� ����
                RegionsData.closedNodes[threadId] = new();
            }
            //�����
            else
            {
                //������� ������� � ������
                RegionsData.pathFindingQueue[threadId].Clear();
                Array.Clear(RegionsData.pathfindingArray, 0, RegionsData.pathfindingArray[threadId].Length);

                //��������� ���������� �������� � �������
                PathFindingNodesComparer comparer = (PathFindingNodesComparer)RegionsData.pathFindingQueue[threadId].Comparer;
                comparer.SetMatrix(RegionsData.pathfindingArray[threadId]);
            }
        }

        void PathFind(
            int threadId,
            ref SRTaskForceFindPath requestComp,
            ref CTaskForce taskForce, ref CTaskForceMovement tFMovement)
        {
            //���� ��������� ������ �� ����� ���������
            if (taskForce.currentRegionPE.EqualsTo(requestComp.targetRegionPE) == false)
            {
                //���� ������� ������ ������
                taskForce.currentRegionPE.Unpack(world, out int fromRegionEntity);
                ref CHexRegion fromRegion = ref regionPool[regionIndices[fromRegionEntity]];

                //���� ������� ������
                requestComp.targetRegionPE.Unpack(world, out int toRegionEntity);
                ref CHexRegion toRegion = ref regionPool[regionIndices[toRegionEntity]];

                //���������� ������������ ���������� ����� ��� ������

                //������� ����
                List<DPathFindingClosedNode> path = PathFindFast(
                    threadId,
                    ref fromRegion, ref toRegion);

                //���� ���� �� ����
                if (path != null)
                {
                    //��� ������� ������� � ����
                    for (int a = 0; a < path.Count - 1; a++)
                    {
                        //������� ��� � ������ PE
                        tFMovement.pathRegionPEs.Add(RegionsData.regionPEs[path[a].index]);
                    }
                }
            }
        }

        List<DPathFindingClosedNode> PathFindFast(
            int threadId,
            ref CHexRegion fromRegion, ref CHexRegion toRegion)
        {
            //������ ���������� ��� ������������ ������� ����
            bool found = false;

            //������ ������� �����
            int stepsCount = 0;

            //���� ���� ������ ������ 250
            if (RegionsData.openRegionValues[threadId] > 250)
            {
                //�������� ����
                RegionsData.openRegionValues[threadId] = 1;
                RegionsData.closeRegionValues[threadId] = 2;
            }
            //�����
            else
            {
                //��������� ����
                RegionsData.openRegionValues[threadId] += 2;
                RegionsData.closeRegionValues[threadId] += 2;
            }
            //������� ������� � ����
            RegionsData.pathFindingQueue[threadId].Clear();
            RegionsData.closedNodes[threadId].Clear();

            //���� ����� ��������� �������
            Vector3 destinationCenter = toRegion.center;

            //������ ���������� ��� ���������� �������
            int nextRegionIndex;

            //�������� ������ ���������� ������� � �������
            RegionsData.pathfindingArray[threadId][fromRegion.Index].distance = 0;
            RegionsData.pathfindingArray[threadId][fromRegion.Index].priority = 2;
            RegionsData.pathfindingArray[threadId][fromRegion.Index].prevIndex = fromRegion.Index;
            RegionsData.pathfindingArray[threadId][fromRegion.Index].status = RegionsData.openRegionValues[threadId];

            //������� ��������� ������ � �������
            RegionsData.pathFindingQueue[threadId].Push(fromRegion.Index);

            //���� � ������� ���� �������
            while (RegionsData.pathFindingQueue[threadId].regionsCount > 0)
            {
                //���� ������ ������ � ������� ��� �������
                int currentRegionIndex = RegionsData.pathFindingQueue[threadId].Pop();

                //���� ������ ������ ��� ����� �� ������� ������, �� ��������� � ����������
                if (RegionsData.pathfindingArray[threadId][currentRegionIndex].status == RegionsData.closeRegionValues[threadId])
                {
                    continue;
                }

                //���� ������ ������� ����� ������� ��������� �������
                if (currentRegionIndex == toRegion.Index)
                {
                    //������� ������ �� ������� ������
                    RegionsData.pathfindingArray[threadId][currentRegionIndex].status = RegionsData.closeRegionValues[threadId];

                    //��������, ��� ���� ������, � ������� �� �����
                    found = true;
                    break;
                }

                //���� ������� ����� ������ �������
                if (stepsCount >= RegionsData.pathfindingSearchLimit)
                {
                    return null;
                }

                //���� ������� ������
                RegionsData.regionPEs[currentRegionIndex].Unpack(world, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool[regionIndices[currentRegionEntity]];

                //��� ������� ������ �������� �������
                for (int a = 0; a < currentRegion.neighbourRegionPEs.Length; a++)
                {
                    //���� ������
                    currentRegion.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool[regionIndices[neighbourRegionEntity]];
                    nextRegionIndex = neighbourRegion.Index;

                    //������������ ���������� �� ������
                    float newDistance = RegionsData.pathfindingArray[threadId][currentRegionIndex].distance + neighbourRegion.crossCost;

                    //���� ������ ��������� � ������� ������ ��� ��� ������� �� �������
                    if (RegionsData.pathfindingArray[threadId][nextRegionIndex].status == RegionsData.openRegionValues[threadId]
                        || RegionsData.pathfindingArray[threadId][nextRegionIndex].status == RegionsData.closeRegionValues[threadId])
                    {
                        //���� ���������� �� ������� ������ ��� ����� ������, �� ��������� � ���������� ������
                        if (RegionsData.pathfindingArray[threadId][nextRegionIndex].distance <= newDistance)
                        {
                            continue;
                        }
                    }
                    //����� ��������� ����������

                    //��������� ������ ����������� ������� � ����������
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].prevIndex = currentRegionIndex;
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].distance = newDistance;

                    //������������ ��������� ������
                    //������������ ����, ������������ � �������� ���������
                    float angle = Vector3.Angle(destinationCenter, neighbourRegion.center);

                    //��������� ��������� ������� � ������
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].priority = newDistance + 2f * angle;
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].status = RegionsData.openRegionValues[threadId];

                    //������� ������ � �������
                    RegionsData.pathFindingQueue[threadId].Push(nextRegionIndex);
                }

                //��������� ������� �����
                stepsCount++;

                //������� ������� ������ �� ������� ������
                RegionsData.pathfindingArray[threadId][currentRegionIndex].status = RegionsData.closeRegionValues[threadId];
            }

            //���� ���� ������
            if (found == true)
            {
                //������� ������ ����
                RegionsData.closedNodes[threadId].Clear();

                //���� �������� ������
                int pos = toRegion.Index;

                //������ ��������� ��������� � ������� � �� ������ ��������� �������
                DPathFindingNodeFast tempRegion = RegionsData.pathfindingArray[threadId][toRegion.Index];
                DPathFindingClosedNode stepRegion;

                //��������� ������ �� �������� � ��������
                stepRegion.priority = tempRegion.priority;
                stepRegion.distance = tempRegion.distance;
                stepRegion.prevIndex = tempRegion.prevIndex;
                stepRegion.index = toRegion.Index;

                //���� ������ ������� �� ����� ������� �����������,
                //�� ���� ���� �� ��������� ��������� ������
                while (stepRegion.index != stepRegion.prevIndex)
                {
                    //������� ������ � ������ ����
                    RegionsData.closedNodes[threadId].Add(stepRegion);

                    //���� �������� ������ ����������� �������
                    pos = stepRegion.prevIndex;
                    tempRegion = RegionsData.pathfindingArray[threadId][pos];

                    //��������� ������ �� �������� � ��������
                    stepRegion.priority = tempRegion.priority;
                    stepRegion.distance = tempRegion.distance;
                    stepRegion.prevIndex = tempRegion.prevIndex;
                    stepRegion.index = pos;
                }
                //������� ��������� ������ � ������ ����
                RegionsData.closedNodes[threadId].Add(stepRegion);

                //���������� ������ ����
                return RegionsData.closedNodes[threadId];
            }
            return null;
        }
    }
}