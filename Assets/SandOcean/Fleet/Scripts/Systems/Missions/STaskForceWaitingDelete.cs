
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean.Warfare.Fleet
{
    public class STaskForceWaitingDelete : IEcsRunSystem
    {
        //Флоты
        readonly EcsFilterInject<Inc<CTaskForcePatrolMission, CTaskForceWaiting>> taskForcePatrolMissionFilter = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        readonly EcsPoolInject<CTaskForceWaiting> taskForceWaitingPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы с патрульной миссией и компонентом ожидания
            foreach (int taskForceEntity in taskForcePatrolMissionFilter.Value)
            {
                //Берём патрульную миссию
                ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool.Value.Get(taskForceEntity);

                //Если группа не находится в режиме ожидания
                if (tFPatrolMission.missionStatus != TaskForceMissionStatus.Waiting)
                {
                    //Удаляем компонент ожидания
                    taskForceWaitingPool.Value.Del(taskForceEntity);
                }
            }
        }
    }
}