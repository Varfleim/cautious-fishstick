
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
            //Если матрица пути требует обновления
            if (RegionsData.needRefreshPathMatrix[threadId] == true)
            {
                //Обновляем матрицу пути
                PathMatrixRefresh(threadId);
            }

            //Для каждой оперативной группы с самозапросом поиска пути
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент самозапроса, группу и компонент движения
                int taskForceEntity = taskForceEntities[a];
                ref SRTaskForceFindPath requestComp = ref taskForceFindPathSelfRequestPool[taskForceFindPathSelfRequestIndices[taskForceEntity]];
                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool[taskForceMovementIndices[taskForceEntity]];

                //Очищаем данные движения
                tFMovement.pathRegionPEs.Clear();

                //Рассчитываем путь до него
                PathFind(
                    threadId,
                    ref requestComp,
                    ref taskForce, ref tFMovement);
            }
        }

        void PathMatrixRefresh(
            int threadId)
        {
            //Отмечаем, что матрица пути не требует обновления
            RegionsData.needRefreshPathMatrix[threadId] = false;

            //Если массив для поиска пути пуст
            if (RegionsData.pathfindingArray[threadId] == null)
            {
                //Задаём стартовые открытую и закрытую фазы
                RegionsData.openRegionValues[threadId] = 1;
                RegionsData.closeRegionValues[threadId] = 2;

                //Создаём массив
                RegionsData.pathfindingArray[threadId] = new DPathFindingNodeFast[RegionsData.regionPEs.Length];

                //Создаём очередь
                RegionsData.pathFindingQueue[threadId] = new(
                    new PathFindingNodesComparer(RegionsData.pathfindingArray[threadId]),
                    RegionsData.regionPEs.Length);

                //Создаём список итогового пути
                RegionsData.closedNodes[threadId] = new();
            }
            //Иначе
            else
            {
                //Очищаем очередь и массив
                RegionsData.pathFindingQueue[threadId].Clear();
                Array.Clear(RegionsData.pathfindingArray, 0, RegionsData.pathfindingArray[threadId].Length);

                //Обновляем сравнитель регионов в очереди
                PathFindingNodesComparer comparer = (PathFindingNodesComparer)RegionsData.pathFindingQueue[threadId].Comparer;
                comparer.SetMatrix(RegionsData.pathfindingArray[threadId]);
            }
        }

        void PathFind(
            int threadId,
            ref SRTaskForceFindPath requestComp,
            ref CTaskForce taskForce, ref CTaskForceMovement tFMovement)
        {
            //Если стартовый регион не равен конечному
            if (taskForce.currentRegionPE.EqualsTo(requestComp.targetRegionPE) == false)
            {
                //Берём текущий регион группы
                taskForce.currentRegionPE.Unpack(world, out int fromRegionEntity);
                ref CHexRegion fromRegion = ref regionPool[regionIndices[fromRegionEntity]];

                //Берём целевой регион
                requestComp.targetRegionPE.Unpack(world, out int toRegionEntity);
                ref CHexRegion toRegion = ref regionPool[regionIndices[toRegionEntity]];

                //Определяем максимальное количество шагов при поиске

                //Находим путь
                List<DPathFindingClosedNode> path = PathFindFast(
                    threadId,
                    ref fromRegion, ref toRegion);

                //Если путь не пуст
                if (path != null)
                {
                    //Для каждого региона в пути
                    for (int a = 0; a < path.Count - 1; a++)
                    {
                        //Заносим его в список PE
                        tFMovement.pathRegionPEs.Add(RegionsData.regionPEs[path[a].index]);
                    }
                }
            }
        }

        List<DPathFindingClosedNode> PathFindFast(
            int threadId,
            ref CHexRegion fromRegion, ref CHexRegion toRegion)
        {
            //Создаём переменную для отслеживания наличия пути
            bool found = false;

            //Создаём счётчик шагов
            int stepsCount = 0;

            //Если фаза поиска больше 250
            if (RegionsData.openRegionValues[threadId] > 250)
            {
                //Обнуляем фазу
                RegionsData.openRegionValues[threadId] = 1;
                RegionsData.closeRegionValues[threadId] = 2;
            }
            //Иначе
            else
            {
                //Обновляем фазу
                RegionsData.openRegionValues[threadId] += 2;
                RegionsData.closeRegionValues[threadId] += 2;
            }
            //Очищаем очередь и путь
            RegionsData.pathFindingQueue[threadId].Clear();
            RegionsData.closedNodes[threadId].Clear();

            //Берём центр конечного региона
            Vector3 destinationCenter = toRegion.center;

            //Создаём переменную для следующего региона
            int nextRegionIndex;

            //Обнуляем данные стартового региона в массиве
            RegionsData.pathfindingArray[threadId][fromRegion.Index].distance = 0;
            RegionsData.pathfindingArray[threadId][fromRegion.Index].priority = 2;
            RegionsData.pathfindingArray[threadId][fromRegion.Index].prevIndex = fromRegion.Index;
            RegionsData.pathfindingArray[threadId][fromRegion.Index].status = RegionsData.openRegionValues[threadId];

            //Заносим стартовый регион в очередь
            RegionsData.pathFindingQueue[threadId].Push(fromRegion.Index);

            //Пока в очереди есть регионы
            while (RegionsData.pathFindingQueue[threadId].regionsCount > 0)
            {
                //Берём первый регион в очереди как текущий
                int currentRegionIndex = RegionsData.pathFindingQueue[threadId].Pop();

                //Если данный регион уже вышел за границу поиска, то переходим с следующему
                if (RegionsData.pathfindingArray[threadId][currentRegionIndex].status == RegionsData.closeRegionValues[threadId])
                {
                    continue;
                }

                //Если индекс региона равен индексу конечного региона
                if (currentRegionIndex == toRegion.Index)
                {
                    //Выводим регион за границу поиска
                    RegionsData.pathfindingArray[threadId][currentRegionIndex].status = RegionsData.closeRegionValues[threadId];

                    //Отмечаем, что путь найден, и выходим из цикла
                    found = true;
                    break;
                }

                //Если счётчик шагов больше предела
                if (stepsCount >= RegionsData.pathfindingSearchLimit)
                {
                    return null;
                }

                //Берём текущий регион
                RegionsData.regionPEs[currentRegionIndex].Unpack(world, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool[regionIndices[currentRegionEntity]];

                //Для каждого соседа текущего региона
                for (int a = 0; a < currentRegion.neighbourRegionPEs.Length; a++)
                {
                    //Берём соседа
                    currentRegion.neighbourRegionPEs[a].Unpack(world, out int neighbourRegionEntity);
                    ref CHexRegion neighbourRegion = ref regionPool[regionIndices[neighbourRegionEntity]];
                    nextRegionIndex = neighbourRegion.Index;

                    //Рассчитываем расстояние до соседа
                    float newDistance = RegionsData.pathfindingArray[threadId][currentRegionIndex].distance + neighbourRegion.crossCost;

                    //Если регион находится в границе поиска или уже выведен за границу
                    if (RegionsData.pathfindingArray[threadId][nextRegionIndex].status == RegionsData.openRegionValues[threadId]
                        || RegionsData.pathfindingArray[threadId][nextRegionIndex].status == RegionsData.closeRegionValues[threadId])
                    {
                        //Если расстояние до региона меньше или равно новому, то переходим к следующему соседу
                        if (RegionsData.pathfindingArray[threadId][nextRegionIndex].distance <= newDistance)
                        {
                            continue;
                        }
                    }
                    //Иначе обновляем расстояние

                    //Обновляем индекс предыдущего региона и расстояние
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].prevIndex = currentRegionIndex;
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].distance = newDistance;

                    //Рассчитываем приоритет поиска
                    //Рассчитываем угол, используемый в качестве эвристики
                    float angle = Vector3.Angle(destinationCenter, neighbourRegion.center);

                    //Обновляем приоритет региона и статус
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].priority = newDistance + 2f * angle;
                    RegionsData.pathfindingArray[threadId][nextRegionIndex].status = RegionsData.openRegionValues[threadId];

                    //Заносим регион в очередь
                    RegionsData.pathFindingQueue[threadId].Push(nextRegionIndex);
                }

                //Обновляем счётчик шагов
                stepsCount++;

                //Выводим текущий регион за границу поиска
                RegionsData.pathfindingArray[threadId][currentRegionIndex].status = RegionsData.closeRegionValues[threadId];
            }

            //Если путь найден
            if (found == true)
            {
                //Очищаем список пути
                RegionsData.closedNodes[threadId].Clear();

                //Берём конечный регион
                int pos = toRegion.Index;

                //Создаём временную структуру и заносим в неё данные конечного региона
                DPathFindingNodeFast tempRegion = RegionsData.pathfindingArray[threadId][toRegion.Index];
                DPathFindingClosedNode stepRegion;

                //Переносим данные из активных в итоговые
                stepRegion.priority = tempRegion.priority;
                stepRegion.distance = tempRegion.distance;
                stepRegion.prevIndex = tempRegion.prevIndex;
                stepRegion.index = toRegion.Index;

                //Пока индекс региона не равен индексу предыдущего,
                //то есть пока не достигнут стартовый регион
                while (stepRegion.index != stepRegion.prevIndex)
                {
                    //Заносим регион в список пути
                    RegionsData.closedNodes[threadId].Add(stepRegion);

                    //Берём активные данные предыдущего региона
                    pos = stepRegion.prevIndex;
                    tempRegion = RegionsData.pathfindingArray[threadId][pos];

                    //Переносим данные из активных в итоговые
                    stepRegion.priority = tempRegion.priority;
                    stepRegion.distance = tempRegion.distance;
                    stepRegion.prevIndex = tempRegion.prevIndex;
                    stepRegion.index = pos;
                }
                //Заносим последний регион в список пути
                RegionsData.closedNodes[threadId].Add(stepRegion);

                //Возвращаем список пути
                return RegionsData.closedNodes[threadId];
            }
            return null;
        }
    }
}