
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
            //Для каждого флота с миссией поиска в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём флот
                int fleetEntity = fleetEntities[a];
                ref CFleetPMissionSearch fleetPMissionSearch = ref fleetPatrolMissionSearchPool[fleetPatrolMissionSearchIndices[fleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[fleetEntity]];

                //Для каждой ищущей оперативной группы
                for (int b = 0; b < fleetPMissionSearch.activeTaskForcePEs.Count; b++)
                {
                    //Берём её патрульную миссию
                    fleetPMissionSearch.activeTaskForcePEs[b].Unpack(world, out int taskForceEntity);
                    ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool[taskForcePatrolMissionIndices[taskForceEntity]];

                    //Если группа находится в режиме ожидания
                    if (tFPatrolMission.missionStatus == TaskForceMissionStatus.Waiting)
                    {
                        //Берём компонент ожидания
                        ref CTaskForceWaiting tFWaiting = ref taskForceWaitingPool[taskForceWaitingIndices[taskForceEntity]];

                        //Прибавляем 1 к тикам, проведённым в ожидании
                        tFWaiting.waitingTime++;

                        //Если время ожидания больше или равно максимальному необходимому
                        if (tFWaiting.waitingTime >= 3)
                        {
                            //Переводим группу в пустой режим, тем самым освобождая её и запрашивая удаление компонента ожидания
                            tFPatrolMission.missionStatus = TaskForceMissionStatus.None;
                        }
                    }
                }

                //Для каждого региона флота
                for (int b = 0; b < fleet.fleetRegions.Count; b++)
                {
                    //Прибавляем 1 к тикам с последнего поиска
                    fleet.fleetRegions[b].searchMissionLastTime++;

                    //Для каждой ищущей группы
                    for (int c = 0; c < fleetPMissionSearch.activeTaskForcePEs.Count; c++)
                    {
                        //Берём группу и её миссию
                        fleetPMissionSearch.activeTaskForcePEs[c].Unpack(world, out int taskForceEntity);
                        ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                        //Если группа находится в данном регионе
                        if (taskForce.currentRegionPE.EqualsTo(fleet.fleetRegions[b].regionPE) == true)
                        {
                            //Обнуляем тики с последнего поиска
                            fleet.fleetRegions[b].searchMissionLastTime = 0;
                        }
                    }
                }
            }
        }
    }
}