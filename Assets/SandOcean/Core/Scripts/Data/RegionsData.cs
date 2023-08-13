
using System;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

using SandOcean.Map.Pathfinding;

namespace SandOcean.Map
{
    public class RegionsData : MonoBehaviour
    {
        //������ ��������
        public static EcsPackedEntity[] regionPEs;

        //������ ������ ����
        public static bool needRefreshRouteMatrix;
        public static DPathFindingNodeFast[] pfCalc;
        public static PathFindingQueueInt open;
        public static List<DPathFindingClosedNode> close = new();
        public static byte openRegionValue = 1;
        public static byte closeRegionValue = 2;
        public const int pathFindingSearchLimitBase = 30000;
        public static int pathFindingSearchLimit;

        public static EcsPackedEntity GetRegion(
            int regionIndex)
        {
            //���������� PE ������������ �������
            return regionPEs[regionIndex];
        }
        public static EcsPackedEntity GetRegionRandom()
        {
            //���������� PE ���������� �������
            return GetRegion(UnityEngine.Random.Range(0, regionPEs.Length));
        }

        public static List<int> GetRegionIndicesWithinSteps(
            EcsWorld world,
            EcsFilter regionFilter, EcsPool<CHexRegion> regionPool,
            ref CHexRegion region,
            int maxSteps)
        {
            //������ ������������� ������
            List<int> candidates = new();

            //��� ������� ������ �������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ������
                region.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Get(neighbourRegionEntity);

                //������� ������ ������ � ������ ����������
                candidates.Add(neighbourRegion.Index);
            }

            //������ �������������� �������
            Dictionary<int, bool> processed = new();

            //������� ����������� ������ � �������
            processed.Add(region.Index, true);

            //������ �������� ������
            List<int> results = new();

            //������ �������� ������� ��� �������������� ��������
            int candidatesLast = candidates.Count - 1;

            //���� �� ��������� ��������� ������ � ������
            while (candidatesLast >= 0)
            {
                //���� ���������� ���������
                int candidateIndex = candidates[candidatesLast];
                candidates.RemoveAt(candidatesLast);
                candidatesLast--;
                RegionsData.regionPEs[candidateIndex].Unpack(world, out int regionEntity);
                ref CHexRegion candidateRegion = ref regionPool.Get(regionEntity);

                //������� ���� �� ����
                List<int> pathRegions = RegionsData.FindPath(
                    world,
                    regionFilter, regionPool,
                    ref region, ref candidateRegion,
                    maxSteps);

                //���� ������� ��� �� �������� ��� � ���������� ���� 
                if (processed.ContainsKey(candidateIndex) == false && pathRegions != null)
                {
                    //������� ��������� � �������� ������ � �������
                    results.Add(candidateRegion.Index);
                    processed.Add(candidateRegion.Index, true);

                    //��� ������� ��������� �������
                    for (int a = 0; a < candidateRegion.neighbourRegionPEs.Length; a++)
                    {
                        //���� ������
                        candidateRegion.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                        ref CHexRegion neighbourRegion = ref regionPool.Get(neighbourRegionEntity);

                        //���� ������� �� �������� ���
                        if (processed.ContainsKey(neighbourRegion.Index) == false)
                        {
                            //������� ��� � ������ � ����������� �������
                            candidates.Add(neighbourRegion.Index);
                            candidatesLast++;
                        }
                    }
                }
            }

            return results;
        }

        public static List<int> GetRegionIndicesWithinSteps(
            EcsWorld world,
            EcsFilter regionFilter, EcsPool<CHexRegion> regionPool,
            ref CHexRegion region,
            int minSteps, int maxSteps)
        {
            //������ ������������� ������
            List<int> candidates = new();

            //��� ������� ������ �������
            for (int a = 0; a < region.neighbourRegionPEs.Length; a++)
            {
                //���� ������
                region.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                ref CHexRegion neighbourRegion = ref regionPool.Get(neighbourRegionEntity);

                //������� ������ ������ � ������ ����������
                candidates.Add(neighbourRegion.Index);
            }

            //������ �������������� �������
            Dictionary<int, bool> processed = new();

            //������� ����������� ������ � �������
            processed.Add(region.Index, true);

            //������ �������� ������
            List<int> results = new();

            //������ �������� ������� ��� �������������� ��������
            int candidatesLast = candidates.Count - 1;

            //���� �� ��������� ��������� ������ � ������
            while (candidatesLast >= 0)
            {
                //���� ���������� ���������
                int candidateIndex = candidates[candidatesLast];
                candidates.RemoveAt(candidatesLast);
                candidatesLast--;
                RegionsData.regionPEs[candidateIndex].Unpack(world, out int regionEntity);
                ref CHexRegion candidateRegion = ref regionPool.Get(regionEntity);

                //������� ���� �� ����
                List<int> pathRegions = RegionsData.FindPath(
                    world,
                    regionFilter, regionPool,
                    ref region, ref candidateRegion,
                    maxSteps);

                //���� ������� ��� �� �������� ��� � ���������� ���� 
                if (processed.ContainsKey(candidateIndex) == false && pathRegions != null)
                {
                    //���� ����� ���� ������ ��� ����� ����������� � ������ ��� ����� ������������
                    if (pathRegions.Count >= minSteps && pathRegions.Count <= maxSteps)
                    {
                        //������� ��������� � �������� ������
                        results.Add(candidateRegion.Index);
                    }

                    //������� ��������� � ������� ������������
                    processed.Add(candidateRegion.Index, true);

                    //��� ������� ��������� �������
                    for (int a = 0; a < candidateRegion.neighbourRegionPEs.Length; a++)
                    {
                        //���� ������
                        candidateRegion.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                        ref CHexRegion neighbourRegion = ref regionPool.Get(neighbourRegionEntity);

                        //���� ������� �� �������� ���
                        if (processed.ContainsKey(neighbourRegion.Index) == false)
                        {
                            //������� ��� � ������ � ����������� �������
                            candidates.Add(neighbourRegion.Index);
                            candidatesLast++;
                        }
                    }
                }
            }

            return results;
        }

        public static List<int> FindPath(
            EcsWorld world,
            EcsFilter regionFilter, EcsPool<CHexRegion> regionPool,
            ref CHexRegion startRegion, ref CHexRegion endRegion,
            int searchLimit = 0)
        {
            //������ ������ ��� �������� �������� ����
            List<int> results = new();

            //������� ���� � ���������� ���������� �������� � ����
            int count = FindPath(
                world,
                regionFilter, regionPool,
                ref startRegion, ref endRegion,
                results,
                searchLimit);

            //���� ���������� ����� ����, �� ���������� ������ ������
            return count == 0 ? null : results;
        }

        public static int FindPath(
            EcsWorld world,
            EcsFilter regionFilter, EcsPool<CHexRegion> regionPool,
            ref CHexRegion fromRegion, ref CHexRegion toRegion,
            List<int> results,
            int searchLimit = 0)
        {
            //������� ������
            results.Clear();

            //���� ��������� ������ �� ����� ���������
            if (fromRegion.Index != toRegion.Index)
            {
                //������������ ������� ����
                CalculateRouteMatrix(regionFilter, regionPool);

                //���������� ������������ ���������� ����� ��� ������
                pathFindingSearchLimit = searchLimit == 0 ? pathFindingSearchLimitBase : searchLimit;

                //������� ����
                List<DPathFindingClosedNode> path = FindPathFast(
                    world,
                    regionFilter, regionPool,
                    ref fromRegion, ref toRegion);

                //���� ���� �� ����
                if (path != null)
                {
                    //���� ���������� �������� � ����
                    int routeCount = path.Count;

                    //��� ������� ������� � ����, ����� ���� ���������, � �������� �������
                    for (int r = routeCount - 2; r > 0; r--)
                    {
                        //������� ��� � ������ ��������
                        results.Add(path[r].index);
                    }
                    //������� � ������ �������� ������ ���������� �������
                    results.Add(toRegion.Index);
                }
                //����� ���������� 0, ���������, ��� ���� ����
                else
                {
                    return 0;
                }
            }

            //���������� ���������� �������� � ����
            return results.Count;
        }

        static void CalculateRouteMatrix(
            EcsFilter regionFilter, EcsPool<CHexRegion> regionPool)
        {
            //���� ������� ���� �� ������� ����������, �� ������� �� �������
            if (needRefreshRouteMatrix == false)
            {
                return;
            }

            //��������, ��� ������� ���� �� ������� ����������
            needRefreshRouteMatrix = false;

            //��� ������� �������
            foreach (int regionEntity in regionFilter)
            {
                //���� ������
                ref CHexRegion region = ref regionPool.Get(regionEntity);

                //������������ ��������� ������� �� �������
                float cost = region.crossCost;

                //��������� ��������� ������� �� �������
                region.crossCost = cost;
            }

            //���� ������ ��� ������ ����
            if (pfCalc == null)
            {
                //������ ������
                pfCalc = new DPathFindingNodeFast[RegionsData.regionPEs.Length];

                //������ ������� 
                open = new(
                    new PathFindingNodesComparer(pfCalc),
                    RegionsData.regionPEs.Length);
            }
            //�����
            else
            {
                //������� ������� � ������
                open.Clear();
                Array.Clear(pfCalc, 0, pfCalc.Length);

                //��������� ���������� �������� � �������
                PathFindingNodesComparer comparer = (PathFindingNodesComparer)open.Comparer;
                comparer.SetMatrix(pfCalc);
            }
        }

        static List<DPathFindingClosedNode> FindPathFast(
            EcsWorld world,
            EcsFilter regionFilter, EcsPool<CHexRegion> regionPool,
            ref CHexRegion fromRegion, ref CHexRegion toRegion)
        {
            //������ ���������� ��� ������������ ������� ����
            bool found = false;

            //������ ������� �����
            int stepsCounter = 0;

            //���� ���� ������ ������ 250
            if (openRegionValue > 250)
            {
                //�������� ����
                openRegionValue = 1;
                closeRegionValue = 2;
            }
            //�����
            else
            {
                //��������� ����
                openRegionValue += 2;
                closeRegionValue += 2;
            }
            //������� ������� � ����
            open.Clear();
            close.Clear();

            //���� �������� ������
            Vector3 destinationCenter = toRegion.center;

            //������ ���������� ��� ���������� �������
            int nextRegionIndex;

            //�������� ������ ���������� ������� � �������
            pfCalc[fromRegion.Index].distance = 0;
            pfCalc[fromRegion.Index].priority = 2;
            pfCalc[fromRegion.Index].prevIndex = fromRegion.Index;
            pfCalc[fromRegion.Index].status = openRegionValue;

            //������� ��������� ������ � �������
            open.Push(fromRegion.Index);

            //���� � ������� ���� �������
            while (open.regionsCount > 0)
            {
                //���� ������ ������ � ������� ��� �������
                int currentRegionIndex = open.Pop();

                //���� ������ ������ ��� ����� �� ������� ������, �� ��������� � ����������
                if (pfCalc[currentRegionIndex].status == closeRegionValue)
                {
                    continue;
                }

                //���� ������ ������� ����� ������� ��������� �������
                if (currentRegionIndex == toRegion.Index)
                {
                    //������� ������ �� ������� ������
                    pfCalc[currentRegionIndex].status = closeRegionValue;

                    //��������, ��� ���� ������, � ������� �� �����
                    found = true;
                    break;
                }

                //���� ������� ����� ������ �������
                if (stepsCounter >= pathFindingSearchLimit)
                {
                    return null;
                }

                //���� ������� ������
                RegionsData.regionPEs[currentRegionIndex].Unpack(world, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool.Get(currentRegionEntity);

                //��� ������� ������ �������� �������
                for (int a = 0; a < currentRegion.neighbourRegionPEs.Length; a++)
                {
                    //���� ������
                    currentRegion.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool.Get(neighbourRegionEntity);
                    nextRegionIndex = neighbourRegion.Index;

                    //������������ ���������� �� ������
                    float newDistance = pfCalc[currentRegionIndex].distance + neighbourRegion.crossCost;

                    //���� ������ ��������� � ������� ������ ��� ��� ������� �� �������
                    if (pfCalc[nextRegionIndex].status == openRegionValue
                        || pfCalc[nextRegionIndex].status == closeRegionValue)
                    {
                        //���� ���������� �� ������� ������ ��� ����� ������, �� ��������� � ���������� ������
                        if (pfCalc[nextRegionIndex].distance <= newDistance)
                        {
                            continue;
                        }
                    }
                    //����� ��������� ����������

                    //��������� ������ ����������� ������� � ����������
                    pfCalc[nextRegionIndex].prevIndex = currentRegionIndex;
                    pfCalc[nextRegionIndex].distance = newDistance;

                    //������������ ��������� ������
                    //������������ ����, ������������ � �������� ���������
                    float angle = Vector3.Angle(destinationCenter, neighbourRegion.center);

                    //��������� ��������� ������� � ������
                    pfCalc[nextRegionIndex].priority = newDistance + 2f * angle;
                    pfCalc[nextRegionIndex].status = openRegionValue;

                    //������� ������ � �������
                    open.Push(nextRegionIndex);
                }

                //��������� ������� �����
                stepsCounter++;

                //������� ������� ������ �� ������� ������
                pfCalc[currentRegionIndex].status = closeRegionValue;
            }

            //���� ���� ������
            if (found == true)
            {
                //������� ������ ����
                close.Clear();

                //���� �������� ������
                int pos = toRegion.Index;

                //������ ��������� ��������� � ������� � �� ������ ��������� �������
                DPathFindingNodeFast tempRegion = pfCalc[toRegion.Index];
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
                    close.Add(stepRegion);

                    //���� �������� ������ ����������� �������
                    pos = stepRegion.prevIndex;
                    tempRegion = pfCalc[pos];

                    //��������� ������ �� �������� � ��������
                    stepRegion.priority = tempRegion.priority;
                    stepRegion.distance = tempRegion.distance;
                    stepRegion.prevIndex = tempRegion.prevIndex;
                    stepRegion.index = pos;
                }
                //������� ��������� ������ � ������ ����
                close.Add(stepRegion);

                //���������� ������ ����
                return close;
            }
            return null;
        }
    }
}