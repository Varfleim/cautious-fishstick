
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
            //Создаём список для отслеживания приоритета целей
            List<DFleetTargetPriority> fleetTargetPriorities = new();

            //Для каждого флота с миссией ударной группы в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём флот
                int fleetEntity = fleetEntities[a];
                ref CFleetTMissionStrikeGroup fleetTMissionStrikeGroup = ref fleetTargetMissionStrikeGroupPool[fleetTargetMissionStrikeGroupIndices[fleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[fleetEntity]];

                //Если у флота есть регионы
                if (fleet.fleetRegions.Count > 0)
                {
                    //Проверяем, есть ли у флота свободная группа
                    bool hasFreeForce = false;

                    //Для каждой активной группы
                    for (int b = 0; b < fleetTMissionStrikeGroup.activeTaskForcePEs.Count; b++)
                    {
                        //Берём целевую миссию группы
                        fleetTMissionStrikeGroup.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                        ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool[taskForceTargetMissionIndices[taskForceEntity]];

                        //Если группа не занята
                        if (tFTargetMission.missionStatus == TaskForceMissionStatus.None)
                        {
                            //Отмечаем, что свободная группа есть, и выходим из цикла
                            hasFreeForce = true;

                            break;
                        }
                    }

                    //Если у флота есть свободная группа
                    if (hasFreeForce == true)
                    {
                        //Очищаем список приоритетов
                        fleetTargetPriorities.Clear();

                        //Перебираем обнаруженные группы противника, выбираем подходящую и отправляем группу на перехват

                        //ТЕСТ
                        //Для каждой группы данного флота
                        for (int b = 0; b < fleet.ownedTaskForcePEs.Count; b++)
                        {
                            //Берём сущность группы
                            fleet.ownedTaskForcePEs[b].Unpack(world, out int tFEntity);

                            //Заносим группу во временный список
                            fleetTargetPriorities.Add(new(tFEntity, 0));
                        }

                        //Сортируем список
                        fleetTargetPriorities.Sort();

                        //Для каждой активной группы
                        for (int b = 0; b < fleetTMissionStrikeGroup.activeTaskForcePEs.Count; b++)
                        {
                            //Берём целевую миссию группы
                            fleetTMissionStrikeGroup.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                            ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool[taskForceTargetMissionIndices[taskForceEntity]];

                            //Если группа не занята
                            if (tFTargetMission.missionStatus == TaskForceMissionStatus.None)
                            {
                                //Берём группу
                                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                                //Для каждой предположительной цели
                                for (int c = 0; c < fleetTargetPriorities.Count; c++)
                                {
                                    if (b + c > 2)
                                    {

                                        //Если это не текущая группа
                                        if (fleetTargetPriorities[c].targetEntity != taskForceEntity)
                                        {
                                            //Берём целевую группу
                                            ref CTaskForce targetTaskForce = ref taskForcePool[taskForceIndices[fleetTargetPriorities[c].targetEntity]];

                                            //Назначаем текущей группе целевую группу
                                            taskForce.movementTargetPE = targetTaskForce.selfPE;
                                            taskForce.movementTargetType = TaskForceMovementTargetType.TaskForce;

                                            //Если целевая группа находится не в том же регионе, что текущая
                                            if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.currentRegionPE) == false)
                                            {
                                                //Переводим группу в режим движения
                                                tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;
                                            }
                                            //Иначе
                                            else
                                            {
                                                //Переводим группу в режим боя


                                                //ТЕСТ
                                                //Переводим группу в пустой режим, тем самым освобождая её
                                                tFTargetMission.missionStatus = TaskForceMissionStatus.None;
                                                //ТЕСТ
                                            }

                                            //Выходим из цикла
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                        //ТЕСТ
                    }
                }
            }
        }
    }
}