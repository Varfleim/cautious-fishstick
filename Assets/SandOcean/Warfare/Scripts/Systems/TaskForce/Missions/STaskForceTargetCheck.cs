
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class STaskForceTargetCheck : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Карта
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Флоты
        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> tFPatrolMissionPool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> tFTargetMissionPool = default;
        readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> tFTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CTaskForceTMissionReinforcement> tFTargetMissionReinforcementPool = default;

        readonly EcsPoolInject<CTaskForceWaiting> taskForceWaitingPool = default;

        //События флотов
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForcePatrolMission, SRTaskForceTargetCheck>> tFPatrolMissionFilter = default;
        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceTargetMission, SRTaskForceTargetCheck>> tFTargetMissionFilter = default;
        readonly EcsPoolInject<SRTaskForceTargetCheck> tFTargetCheckSelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceReinforcement> tFReinforcementSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы, выполняющей патрульную миссию и проверяющей цель
            foreach (int taskForceEntity in tFPatrolMissionFilter.Value)
            {
                //Берём группу и патрульную миссию
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForcePatrolMission tFPMission = ref tFPatrolMissionPool.Value.Get(taskForceEntity);

                //Если группа находится в режиме движения, то она проверяет регион согласно миссии
                if (tFPMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //Назначаем группе компонент ожидания 
                    TaskForceAssignWaiting(taskForceEntity, ref tFPMission);
                }
                //Иначе, если группа находится в режиме ремонта
                else
                {

                }

                tFTargetCheckSelfRequestPool.Value.Del(taskForceEntity);
            }

            //Для каждой оперативной группы, выполняющей целевую миссию и проверяющей цель
            foreach (int taskForceEntity in tFTargetMissionFilter.Value)
            {
                //Берём группу и целевую миссию
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref CTaskForceTargetMission tFTargetMission = ref tFTargetMissionPool.Value.Get(taskForceEntity);

                //Если группа находится в режиме движения
                if (tFTargetMission.missionStatus == TaskForceMissionStatus.Movement)
                {
                    //Если цель группы - оперативная группа
                    if (taskForce.movementTargetType == TaskForceMovementTargetType.TaskForce)
                    {
                        //Берём целевую группу
                        taskForce.movementTargetPE.Unpack(world.Value, out int targetTaskForceEntity);
                        ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

                        //Если целевая группа находится в текущем регионе группы, то встреча происходит нормально
                        if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.currentRegionPE) == true)
                        {
                            //Проверяем, к чему приводит встреча, соответственно миссии группы
                            TaskForceTargetTaskForceCheck(
                                taskForceEntity,
                                ref taskForce, ref tFTargetMission,
                                ref targetTaskForce);

                            UnityEngine.Debug.LogWarning("! " + "Полноценный перехват" + " ! " + targetTaskForce.rand);
                        }
                        //Иначе, если целевая группа находится в предыдущем регионе текущей группы,
                        //а текущая группа находится в предыдущем регионе целевой группы
                        else if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.previousRegionPE) == true
                            && targetTaskForce.previousRegionPE.EqualsTo(taskForce.currentRegionPE) == true)
                        {
                            //Если у целевой группы нет компонента миссии ударной группы
                            if (tFTargetMissionStrikeGroupPool.Value.Has(targetTaskForceEntity) == false
                                //ИЛИ целью целевой группы не является текущая группа
                                || targetTaskForce.movementTargetPE.EqualsTo(taskForce.selfPE) == false)
                            {
                                //То текущая группа поджидает целевую в своём предыдущем регионе
                                //- таким образом отменяется её передвижение в текущий

                                //Берём текущий регион текущей группы
                                taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                                ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                                //Удаляем группу из списка групп в регионе
                                currentRegion.taskForcePEs.Remove(taskForce.selfPE);

                                //Берём предыдущий регион текущей группы
                                taskForce.previousRegionPE.Unpack(world.Value, out int previousRegionEntity);
                                ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                                //Заносим группу в список групп в регионе
                                previousRegion.taskForcePEs.Add(taskForce.selfPE);

                                //Меняем текущий регион текущей группы
                                taskForce.currentRegionPE = previousRegion.selfPE;

                                //Удаляем предыдущий регион текущей группы
                                taskForce.previousRegionPE = new();


                                //Проверяем, к чему приводит встреча, соответственно миссии группы
                                TaskForceTargetTaskForceCheck(
                                    taskForceEntity,
                                    ref taskForce, ref tFTargetMission,
                                    ref targetTaskForce);


                                UnityEngine.Debug.LogWarning("! " + "Поджидает в своём регионе целевую группу, идущую не за ней" + " ! " + targetTaskForce.rand);
                            }
                            //Иначе группы охотятся друг на друга, и встреча происходит в предыдущем регионе той группы, которая двигается медленнее
                            else
                            {
                                //ТЕСТ
                                if (0.75f >= 0.5f) //UnityEngine.Random.value
                                {
                                    //Возвращаем текущую в предыдущий

                                    //Берём текущий регион текущей группы
                                    taskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                                    ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                                    //Удаляем группу из списка групп в регионе
                                    currentRegion.taskForcePEs.Remove(taskForce.selfPE);

                                    //Берём предыдущий регион текущей группы
                                    taskForce.previousRegionPE.Unpack(world.Value, out int previousRegionEntity);
                                    ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                                    //Заносим группу в список групп в регионе
                                    previousRegion.taskForcePEs.Add(taskForce.selfPE);

                                    //Меняем текущий регион текущей группы
                                    taskForce.currentRegionPE = previousRegion.selfPE;

                                    //Удаляем предыдущий регион текущей группы
                                    taskForce.previousRegionPE = new();

                                    UnityEngine.Debug.LogWarning("! " + "Ударная группа противника приходит первой" + " ! " + targetTaskForce.rand);
                                }
                                else
                                {
                                    //Возвращаем целевую в предыдущий

                                    //Берём текущий регион целевой группы
                                    targetTaskForce.currentRegionPE.Unpack(world.Value, out int currentRegionEntity);
                                    ref CHexRegion currentRegion = ref regionPool.Value.Get(currentRegionEntity);

                                    //Удаляем группа из списка групп в регионе
                                    currentRegion.taskForcePEs.Remove(targetTaskForce.selfPE);

                                    //Берём предыдущий регион целевой группы
                                    targetTaskForce.previousRegionPE.Unpack(world.Value, out int previousRegionEntity);
                                    ref CHexRegion previousRegion = ref regionPool.Value.Get(previousRegionEntity);

                                    //Заносим группу в список групп в регионе
                                    previousRegion.taskForcePEs.Add(targetTaskForce.selfPE);

                                    //Меняем текущий регион целевой группы
                                    targetTaskForce.currentRegionPE = previousRegion.selfPE;

                                    //Удаляем предыдущий регион целевой группы
                                    targetTaskForce.previousRegionPE = new();

                                    UnityEngine.Debug.LogWarning("! " + "Своя ударная группа приходит первой" + " ! " + targetTaskForce.rand);
                                }
                                //ТЕСТ

                                //Проверяем, к чему приводит встреча, соответственно миссии группы
                                TaskForceTargetTaskForceCheck(
                                    taskForceEntity,
                                    ref taskForce, ref tFTargetMission,
                                    ref targetTaskForce);
                            }
                        }
                        //Иначе группы разминулись, встреча не произошла, требуется дальнейшее движение
                        else
                        {
                            //Переводим группу в режим движения
                            tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;


                            UnityEngine.Debug.LogWarning("! " + "Разминулись" + " ! " + targetTaskForce.rand);
                        }
                    }

                    tFTargetCheckSelfRequestPool.Value.Del(taskForceEntity);
                }
            }
        }

        void TaskForceTargetTaskForceCheck(
            int taskForceEntity,
            ref CTaskForce taskForce, ref CTaskForceTargetMission tFTargetMission,
            ref CTaskForce targetTaskForce)
        {
            //Если группа имеет компонент миссии ударной группы
            if (tFTargetMissionStrikeGroupPool.Value.Has(taskForceEntity) == true)
            {
                //Переводим группу в режим боя

                //Запрашиваем начало боя

                //ТЕСТ
                //Переводим группу в пустой режим, тем самым освобождая её
                tFTargetMission.missionStatus = TaskForceMissionStatus.None;
                //ТЕСТ
            }
            //Иначе, если группа имеет компонент миссии пополнения
            else if (tFTargetMissionReinforcementPool.Value.Has(taskForceEntity) == true)
            {
                //Запрашиваем пополнение текущей группой целевой
                TaskForceAssignReinforcementSelfRequest(
                    taskForceEntity,
                    ref targetTaskForce);
            }
        }

        void TaskForceAssignWaiting(
            int taskForceEntity, ref CTaskForcePatrolMission tFPMission)
        {
            //Назначаем оперативной группе компонент ожидания
            ref CTaskForceWaiting tFWaiting = ref taskForceWaitingPool.Value.Add(taskForceEntity);

            //Заполняем основные данные компонента
            tFWaiting = new(0);

            //Указываем, что группа находится в режиме ожидания
            tFPMission.missionStatus = TaskForceMissionStatus.Waiting;
        }

        void TaskForceAssignBattle()
        {

        }

        void TaskForceAssignReinforcementSelfRequest(
            int taskForceEntity,
            ref CTaskForce targetTaskForce)
        {
            //Назначаем оперативной группе компонент пополнения
            ref SRTaskForceReinforcement requestComp = ref tFReinforcementSelfRequestPool.Value.Add(taskForceEntity);

            //Заполняем данные самозапроса
            requestComp = new(targetTaskForce.selfPE);
        }
    }
}