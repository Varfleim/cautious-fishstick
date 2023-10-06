
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce;
using SandOcean.Warfare.TaskForce.Missions;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class STaskForceMovementStop : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Карта
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Флоты
        readonly EcsPoolInject<CTaskForce> taskForcePool = default;

        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceMovement>> taskForceMovingFilter = default;
        readonly EcsPoolInject<CTaskForceMovement> taskForceMovementPool = default;
        readonly EcsPoolInject<CTaskForceMovementToMoving> taskForceMovementToMovingPool = default;


        //События флотов
        readonly EcsPoolInject<SRTaskForceTargetCheck> taskForceTargetCheckSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы с компонентом движения
            foreach (int taskForceEntity in taskForceMovingFilter.Value)
            {
                //Берём компонент движения
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool.Value.Get(taskForceEntity);

                //Если группа завершила движение
                if (tFMovement.isTraveled == true)
                {
                    //Отмечаем, что группа может продолжить движение
                    tFMovement.isTraveled = false;

                    //Берём группу
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Если группа имеет следующий регион в маршруте
                    if (tFMovement.pathRegionPEs.Count > 0)
                    {
                        //Берём текущий регион группы
                        taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                        ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                        //Удаляем группу из списка групп в регионе
                        currentRegion.taskForcePEs.Remove(taskForce.selfPE);

                        //Берём следующий регион в маршруте группы
                        tFMovement.pathRegionPEs[tFMovement.pathRegionPEs.Count - 1].Unpack(world.Value, out int nextRegionEntity);
                        ref CHexRegion nextRegion = ref regionPool.Value.Get(nextRegionEntity);

                        //Заносим группу в список групп в регионе
                        nextRegion.taskForcePEs.Add(taskForce.selfPE);

                        //Меняем текущий регион группы
                        taskForce.currentRegionPE = nextRegion.selfPE;

                        //Удаляем следующий (уже текущий) регион из маршрута
                        tFMovement.pathRegionPEs.RemoveAt(tFMovement.pathRegionPEs.Count - 1);

                        //Сохраняем предыдущий регион группы
                        taskForce.previousRegionPE = currentRegion.selfPE;
                    }

                    //Если в маршруте группы больше не осталось регионов
                    if (tFMovement.pathRegionPEs.Count == 0)
                    {
                        //Запрашиваем проверку цели
                        TaskForceAssignTargetCheck(taskForceEntity);

                        //Удаляем компонент движения
                        taskForceMovementPool.Value.Del(taskForceEntity);

                        //Если группа имеет компонент движения к движущейся цели, удаляем его
                        if (taskForceMovementToMovingPool.Value.Has(taskForceEntity) == true)
                        {
                            taskForceMovementToMovingPool.Value.Del(taskForceEntity);
                        }
                    }
                }
            }
        }

        void TaskForceAssignTargetCheck(
            int taskForceEntity)
        {
            //Назначаем оперативной группе самозапрос проверки цели
            ref SRTaskForceTargetCheck tfTargetCheck = ref taskForceTargetCheckSelfRequestPool.Value.Add(taskForceEntity);

            //Заполняем основные данные самозапроса
            tfTargetCheck = new(0);
        }
    }
}