
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Warfare.TaskForce;
using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class STaskForcePathfindingRequestAssign : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Флоты
        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForcePatrolMission>, Exc<CTaskForceMovement>> taskForcePatrolMissionFilter = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceTargetMission>, Exc<CTaskForceMovement>> taskForceTargetMissionFilter = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;

        readonly EcsPoolInject<CTaskForceMovement> taskForceMovementPool = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceMovementToMoving>> taskForceMovementToMovingFilter = default;
        readonly EcsPoolInject<CTaskForceMovementToMoving> taskForceMovementToMovingPool = default;

        //События флотов
        readonly EcsPoolInject<SRTaskForceFindPath> taskForceFindPathSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы, имеюшей патрульную миссию, но не имеющей компонента движения
            foreach (int taskForceEntity in taskForcePatrolMissionFilter.Value)
            {
                //Берём патрульную миссию группы
                ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool.Value.Get(taskForceEntity);

                //Если группа находится в режиме движения
                if (tFPatrolMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //Назначаем компоненты движения группе
                    TaskForceAssignMovement(taskForceEntity);
                }
            }

            //Для каждой оперативной группы, имеющей целевую миссию, но не имеющей компонента движения
            foreach (int taskForceEntity in taskForceTargetMissionFilter.Value)
            {
                //Берём целевую миссию группы
                ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool.Value.Get(taskForceEntity);

                //Если группа находится в режиме движения
                if (tFTargetMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //Назначаем компоненты движения группе
                    TaskForceAssignMovement(taskForceEntity);
                }
            }

            //Для каждой оперативной группы, движущейся к движущейся цели
            foreach (int taskForceEntity in taskForceMovementToMovingFilter.Value)
            {
                //Берём группу и компонент движения к движущейся цели
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForceMovementToMoving tFMovementToMoving = ref taskForceMovementToMovingPool.Value.Get(taskForceEntity);

                //Если цель движения - оперативная группа
                if (taskForce.movementTargetType == TaskForceMovementTargetType.TaskForce)
                {
                    //Берём целевую группу
                    taskForce.movementTargetPE.Unpack(world.Value, out int targetTaskForceEntity);
                    ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

                    //Если она находится не в том регионе, что при предыдущей проверке
                    if (targetTaskForce.currentRegionPE.EqualsTo(tFMovementToMoving.movingTargetLastRegionPE) == false)
                    {
                        //Запрашиваем поиск пути до текущего региона целевой группы
                        TaskForceFindPathSelfRequest(
                            taskForceEntity,
                            targetTaskForce.currentRegionPE);

                        //Обновляем последний регион цели движения
                        tFMovementToMoving.movingTargetLastRegionPE = targetTaskForce.currentRegionPE;
                    }
                }
            }
        }

        void TaskForceAssignMovement(
            int taskForceEntity)
        {
            //Берём группу
            ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

            //Если группа имеет цель движения
            if (taskForce.movementTargetPE.Unpack(world.Value, out int movingTargetEntity))
            {
                //Назначаем группе компонент движения
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool.Value.Add(taskForceEntity);

                //Заполняем данные компонента
                tFMovement = new(0);

                //Если цель движения - это регион
                if (taskForce.movementTargetType == TaskForceMovementTargetType.Region)
                {
                    //То можно сразу запрашивать поиск пути до целевого региона
                    TaskForceFindPathSelfRequest(
                        taskForceEntity,
                        taskForce.movementTargetPE);
                }
                //Иначе, если цель движения - оперативная группа
                else if (taskForce.movementTargetType == TaskForceMovementTargetType.TaskForce)
                {
                    //Берём целевую группу
                    ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(movingTargetEntity);

                    //Запрашиваем поиск пути до текущего региона целевой группы
                    TaskForceFindPathSelfRequest(
                        taskForceEntity,
                        targetTaskForce.currentRegionPE);

                    //Назначаем изначальной группе компонент движения к движущейся цели
                    ref CTaskForceMovementToMoving tFMovementToMoving = ref taskForceMovementToMovingPool.Value.Add(taskForceEntity);

                    //Заполняем данные компонента
                    tFMovementToMoving = new(targetTaskForce.currentRegionPE);
                }
                //Иначе, если цель движения - 

            }
        }

        void TaskForceFindPathSelfRequest(
            int taskForceEntity,
            EcsPackedEntity targetRegionPE)
        {
            //Назначаем сущности оперативной группы самозапрос поиска пути 
            ref SRTaskForceFindPath selfRequestComp = ref taskForceFindPathSelfRequestPool.Value.Add(taskForceEntity);

            //Заполняем данные самозапроса
            selfRequestComp = new(targetRegionPE);
        }
    }
}