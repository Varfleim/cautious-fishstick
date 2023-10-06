
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.Organization;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public class STaskForceReinforcementCreating : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Организации
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Флоты
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsPoolInject<CReserveFleet> reserveFleetPool = default;
        readonly EcsPoolInject<CFleetTMissionReinforcement> fleetTargetMissionReinforcementPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;
        readonly EcsPoolInject<CTaskForceTMissionReinforcement> taskForceTargetMissionReinforcementPool = default;

        //События флотов
        readonly EcsFilterInject<Inc<CFleet, CReserveFleet, SRReserveFleetReinforcementCheck>> reserveFleetReinforcementCheckSelfRequestFilter = default;
        readonly EcsPoolInject<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, SRTaskForceReinforcementCheck>> taskForceReinforcementCheckSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool = default;

        //Общие события
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы с самозапросом проверки пополнения
            foreach (int taskForceEntity in taskForceReinforcementCheckSelfRequestFilter.Value)
            {
                //Удаляем самозапрос
                taskForceReinforcementCheckSelfRequestPool.Value.Del(taskForceEntity);
            }

            //Для каждого резервного флота с самозапросом проверки пополнения
            foreach (int fleetEntity in reserveFleetReinforcementCheckSelfRequestFilter.Value)
            {
                //Берём резервный флот и самозапрос проверки пополнения
                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);
                ref CReserveFleet reserveFleet = ref reserveFleetPool.Value.Get(fleetEntity);
                ref SRReserveFleetReinforcementCheck selfRequestComp = ref reserveFleetReinforcementCheckSelfRequestPool.Value.Get(fleetEntity);

                //Для каждой запрошенной группы пополнения во флоте в обратном порядке
                for (int a = reserveFleet.requestedTaskForceReinforcements.Count - 1; a >= 0; a--)
                {
                    UnityEngine.Debug.LogWarning("!");

                    //Создаём группу пополнения
                    TaskForceReinforcementCreating(
                        reserveFleet.requestedTaskForceReinforcements[a]);

                    //Удаляем запрос группы из списка
                    reserveFleet.requestedTaskForceReinforcements.RemoveAt(a);
                }

                //Проверяем, завершено ли пополнение
                bool isReinforcementComplete = true;

                //Для каждого дочернего флота
                for (int a = 0; a < reserveFleet.ownedFleetPEs.Count; a++)
                {
                    //Берём дочерний флот
                    reserveFleet.ownedFleetPEs[a].Unpack(world.Value, out int ownedFleetEntity);
                    ref CFleet ownedFleet = ref fleetPool.Value.Get(ownedFleetEntity);

                    //Для каждого запроса пополнения
                    for (int b = 0; b < ownedFleet.tFReinforcementRequests.Count; b++)
                    {
                        //Если количество запрошенных типов кораблей больше нуля
                        if (fleet.tFReinforcementRequests[b].shipTypes.Count > 0)
                        {
                            //Отмечаем, что пополнение не завершено
                            isReinforcementComplete = false;

                            break;
                        }
                    }
                }

                //Если пополнение завершено
                if (isReinforcementComplete == true)
                {
                    //Удаляем самозапрос проверки пополнения с сущности флота
                    reserveFleetReinforcementCheckSelfRequestPool.Value.Del(fleetEntity);
                }
            }
        }

        void TaskForceReinforcementCreating(
            DTaskForceReinforcementCreating taskForceReinforcementCreating)
        {
            //Берём целевую оперативную группу
            taskForceReinforcementCreating.targetTaskForcePE.Unpack(world.Value, out int targetTaskForceEntity);
            ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

            //Берём родителький флот целевой группы
            targetTaskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //Берём родительскую организацию данного флота
            fleet.parentOrganizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);


            //Создаём новую сущность и назначаем ей компонент оперативной группы
            int taskForceEntity = world.Value.NewEntity();
            ref CTaskForce taskForce = ref taskForcePool.Value.Add(taskForceEntity);

            //Заполняем основные данные группы пополнения
            taskForce = new(
                world.Value.PackEntity(taskForceEntity),
                fleet.selfPE);

            taskForce.rand = UnityEngine.Random.Range(0, 50);

            //Заносим группу в список флота
            fleet.ownedTaskForcePEs.Add(taskForce.selfPE);

            //Изменяем миссию группы на пополнение
            TaskForceChangeMission(
                ref taskForce,
                TaskForceActionType.ChangeMissionReinforcement,
                taskForceReinforcementCreating);

            //Создаём корабли группы
            TaskForceCreateShips(
                ref taskForce,
                ref taskForceReinforcementCreating);

            //Создаём событие, сообщающее о создании новой группы
            ObjectNewCreatedEvent(taskForce.selfPE, ObjectNewCreatedType.TaskForce);
        }

        void TaskForceChangeMission(
            ref CTaskForce taskForce,
            TaskForceActionType actionType,
            DTaskForceReinforcementCreating taskForceReinforcementCreating = new())
        {
            //Если запрашивается активация миссии пополнения
            if (actionType == TaskForceActionType.ChangeMissionReinforcement)
            {
                //Меняем миссию группы на пополнение
                TaskForceChangeMissionReinforcement(
                    ref taskForce,
                    ref taskForceReinforcementCreating);

                taskForce.activeMissionType = TaskForceMissionType.TargetMissionReinforcement;
            }

            UnityEngine.Debug.LogWarning(taskForce.rand + " ! " + taskForce.activeMissionType);
        }

        void TaskForceChangeMissionReinforcement(
            ref CTaskForce taskForce,
            ref DTaskForceReinforcementCreating taskForceReinforcementCreating)
        {
            //Берём родительский флот оперативной группы
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //Если флот не имеет компонента пополнения
            if (fleetTargetMissionReinforcementPool.Value.Has(fleetEntity) == false)
            {
                //Назначаем флоту компонент миссии пополнения
                ref CFleetTMissionReinforcement tMissionReinforcement = ref fleetTargetMissionReinforcementPool.Value.Add(fleetEntity);

                //Заполняем основные данные компонента миссии
                tMissionReinforcement = new(0);
            }

            //Берём компонент миссии пополнения
            ref CFleetTMissionReinforcement fleetTMissionReinforcement = ref fleetTargetMissionReinforcementPool.Value.Get(fleetEntity);

            //Берём сущность группы и назначаем ей основную целевую миссию и миссию пополнения
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);
            ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool.Value.Add(taskForceEntity);
            ref CTaskForceTMissionReinforcement tFMissionReinforcement = ref taskForceTargetMissionReinforcementPool.Value.Add(taskForceEntity);

            //Заполняем основные данные компонента целевой миссии
            tFTargetMission = new(0);

            //Заполняем основные данные компонента миссии пополнения
            tFMissionReinforcement = new(0);

            //Заносим группу в список групп, выполняющих миссию пополнения
            fleetTMissionReinforcement.activeTaskForcePEs.Add(taskForce.selfPE);

            //Берём целевую группу
            taskForceReinforcementCreating.targetTaskForcePE.Unpack(world.Value, out int targetTaskForceEntity);
            ref CTaskForce targetTaskForce = ref taskForcePool.Value.Get(targetTaskForceEntity);

            //Назначаем группе целевую группу
            taskForce.movementTargetPE = targetTaskForce.selfPE;
            taskForce.movementTargetType = TaskForceMovementTargetType.TaskForce;

            if (fleet.fleetRegions.Count > 0)
            {
                taskForce.currentRegionPE = fleet.fleetRegions[UnityEngine.Random.Range(0, fleet.fleetRegions.Count)].regionPE;
            }


            UnityEngine.Debug.LogWarning("!1!");

            //Переводим группу в режим движения
            tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;

            //Если целевая группа находится не в том же регионе, что текущая
            /*if (targetTaskForce.currentRegionPE.EqualsTo(taskForce.currentRegionPE) == false)
            {
                UnityEngine.Debug.LogWarning("!1!");

                //Переводим группу в режим движения
                tFTargetMission.missionStatus = TaskForceMissionStatus.Movement;
            }
            //Иначе
            else
            {
                UnityEngine.Debug.LogWarning("!2!");

                //ТЕСТ
                //Переводим группу в пустой режим, тем самым освобождая её
                tFTargetMission.missionStatus = TaskForceMissionStatus.None;
                //ТЕСТ
            }*/
        }

        void TaskForceCreateShips(
            ref CTaskForce taskForce,
            ref DTaskForceReinforcementCreating taskForceReinforcementCreating)
        {
            //ТЕСТ
            //Для каждого типа корабля в запросе
            for (int a = 0; a < taskForceReinforcementCreating.shipTypes.Count; a++)
            {
                //Для каждого запрошенного корабля
                for (int b = 0; b < taskForceReinforcementCreating.shipTypes[a].shipCount; b++)
                {
                    //Создаём корабль
                    ShipCreate(
                        ref taskForce,
                        taskForceReinforcementCreating.shipTypes[a].shipType);
                }
            }
            //ТЕСТ
        }

        void ShipCreate(
            ref CTaskForce taskForce,
            DShipType shipType)
        {
            //Создаём корабль и заполняем его данные
            DShip ship = new(
                RuntimeData.shipCount, shipType);

            //Обновляем счётчик кораблей
            RuntimeData.shipCount++;

            //Заносим корабль в список кораблей группы
            taskForce.ships.Add(ship);
        }

        void ObjectNewCreatedEvent(
            EcsPackedEntity objectPE, ObjectNewCreatedType objectNewCreatedType)
        {
            //Создаём новую сущность и назначаем ей компонент события создания нового объекта
            int eventEntity = world.Value.NewEntity();
            ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Add(eventEntity);

            //Заполняем данные события
            eventComp = new(objectPE, objectNewCreatedType);
        }
    }
}