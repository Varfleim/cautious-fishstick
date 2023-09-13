
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
            //Для каждого флота с миссией поиска в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём флот
                int fleetEntity = fleetEntities[a];
                ref CFleetPMissionSearch fleetPMissionSearch = ref fleetPatrolMissionSearchPool[fleetPatrolMissionSearchIndices[fleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[fleetEntity]];

                //Если у флота есть регионы
                if (fleet.fleetRegions.Count > 0)
                {
                    //Проверяем, есть ли у флота свободная группа
                    bool hasFreeForce = false;

                    //Для каждой активной группы
                    for (int b = 0; b < fleetPMissionSearch.activeTaskForcePEs.Count; b++)
                    {
                        //Берём патрульную миссию группы
                        fleetPMissionSearch.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                        ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool[taskForcePatrolMissionIndices[taskForceEntity]];

                        //Если группа не находится не занята
                        if (tFPatrolMission.missionStatus == TaskForceMissionStatus.None)
                        {
                            //Отмечаем, что свободная группа есть, и выходим из цикла
                            hasFreeForce = true;

                            break;
                        }
                    }

                    //Если у флота есть свободная группа
                    if (hasFreeForce == true)
                    {
                        //Создаём массив для отслеживания приоритета поиска регионов
                        DFleetRegionPriority[] regionSearchPriorities = new DFleetRegionPriority[fleet.fleetRegions.Count];

                        //Для каждого региона флота
                        for (int b = 0; b < fleet.fleetRegions.Count; b++)
                        {
                            //Рассчитываем сумму тиков с последнего поиска соседей
                            float neighbourAverageLastTime = 0;

                            //Для каждого соседа
                            for (int c = 0; c < fleet.fleetRegions[b].neighbourRegions.Count; c++)
                            {
                                //Прибавляем время с последнего поиска
                                neighbourAverageLastTime += fleet.fleetRegions[b].neighbourRegions[c].searchMissionLastTime;
                            }

                            //Делим полученное число на количество соседей
                            neighbourAverageLastTime /= fleet.fleetRegions[b].neighbourRegions.Count;

                            //Заносим полученное число и время с последнего поиска самого региона в массив
                            regionSearchPriorities[b] = new(b, neighbourAverageLastTime + fleet.fleetRegions[b].searchMissionLastTime);
                        }

                        //Сортируем массив приоритетов
                        Array.Sort(regionSearchPriorities);

                        //Создаём переменную, определяющую, в какой регион будет отправлена группа
                        int forceRegionIndex = regionSearchPriorities.Length - 1;

                        //Для каждой ищущей группы
                        for (int b = 0; b < fleetPMissionSearch.activeTaskForcePEs.Count; b++)
                        {
                            //Если индекс региона меньше нуля
                            if (forceRegionIndex < 0)
                            {
                                //Возвращаем его на максимальный
                                forceRegionIndex = regionSearchPriorities.Length - 1;
                            }

                            //Берём патрульную миссию группы
                            fleetPMissionSearch.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                            ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool[taskForcePatrolMissionIndices[taskForceEntity]];

                            //Если группа не находится ни в режиме движения, ни ожидания
                            if (tFPatrolMission.missionStatus == TaskForceMissionStatus.None)
                            {
                                //Берём группу
                                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                                //Берём регион с наибольшим приоритетом - последний минус количество отправленных групп
                                DFleetRegion fleetRegion = fleet.fleetRegions[regionSearchPriorities[forceRegionIndex].regionIndex];

                                //Назначаем группе целевой регион
                                taskForce.movementTargetPE = fleetRegion.regionPE;
                                taskForce.movementTargetType = TaskForceMovementTargetType.Region;

                                //Переводим группу в режим движения
                                tFPatrolMission.missionStatus = TaskForceMissionStatus.Movement;

                                //Уменьшаем индекс региона, следующим беря регион с меньшим индексом
                                forceRegionIndex--;
                            }
                        }
                    }
                }
            }
        }
    }
}