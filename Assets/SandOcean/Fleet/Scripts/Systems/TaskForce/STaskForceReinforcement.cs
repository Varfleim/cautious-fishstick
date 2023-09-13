
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce
{
    public class STaskForceReinforcement : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Флоты
        readonly EcsPoolInject<CFleet> fleetPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;


        //События флотов
        readonly EcsPoolInject<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, SRTaskForceReinforcement>> tFReinforcementSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceReinforcement> tFReinforcementSelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceRefreshUI> taskForceRefreshUISelfRequestPool = default;

        readonly EcsPoolInject<SRTaskForceEmptyCheck> taskForceEmptyCheckSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы, имеющей самозапрос пополнения
            foreach (int taskForceEntity in tFReinforcementSelfRequestFilter.Value)
            {
                //Берём группу и самозапрос
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceReinforcement selfRequestComp = ref tFReinforcementSelfRequestPool.Value.Get(taskForceEntity);

                //Переносим корабли
                TaskForceReinforcement(ref taskForce, ref selfRequestComp);

                //Удаляем самозапрос с сущности группы
                tFReinforcementSelfRequestPool.Value.Del(taskForceEntity);
            }
        }

        void TaskForceReinforcement(
            ref CTaskForce fromTaskForce, ref SRTaskForceReinforcement selfRequestComp)
        {
            //Берём целевую группу
            selfRequestComp.targetTaskForcePE.Unpack(world.Value, out int toTaskForceEntity);
            ref CTaskForce toTaskForce = ref taskForcePool.Value.Get(toTaskForceEntity);

            //Для каждого корабля в исходной группе в обратном порядке
            for (int a = fromTaskForce.ships.Count - 1; a >= 0; a--)
            {
                //Берём корабль
                DShip ship = fromTaskForce.ships[a];

                //Переносим корабль
                TaskForceFunctions.ShipTransfer(
                    ref fromTaskForce, ref toTaskForce,
                    ship);

                //Для каждого типа корабля в запросе целевой группы
                for (int b = 0; b < toTaskForce.reinforcementRequest.shipTypes.Count; b++)
                {
                    //Если это тип корабля
                    if (toTaskForce.reinforcementRequest.shipTypes[b].shipType == ship.shipType)
                    {
                        //Сокращаем количество запрошенных и отправленных кораблей
                        toTaskForce.reinforcementRequest.shipTypes[b].requestedShipCount--;
                        toTaskForce.reinforcementRequest.shipTypes[b].sentedShipCount--;

                        //Выходим из цикла, поскольку тип найден
                        break;
                    }
                }
            }
            //Очищаем список кораблей
            fromTaskForce.ships.Clear();

            //Запрашиваем проверку пополнения для целевой группы
            TaskForceFunctions.ReinforcementCheckSelfRequest(
                world.Value,
                reserveFleetReinforcementCheckSelfRequestPool.Value,
                fleetPool.Value,
                taskForceReinforcementCheckSelfRequestPool.Value,
                ref toTaskForce);

            //Запрашиваем обновление интерфейса целевой группы
            TaskForceFunctions.RefreshUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                taskForceRefreshUISelfRequestPool.Value,
                ref toTaskForce);


            //Берём сущность исходной группы
            fromTaskForce.selfPE.Unpack(world.Value, out int taskForceEntity);

            //Если исхожная группа пуста
            if (fromTaskForce.ships.Count == 0)
            {
                //Запрашиваем проверку пустой группы
                TaskForceDeleteSelfRequest(taskForceEntity);
            }
        }

        void TaskForceDeleteSelfRequest(
            int taskForceEntity)
        {
            //Назначаем сущности оперативной группы самозапрос удаления
            ref SRTaskForceEmptyCheck requestComp = ref taskForceEmptyCheckSelfRequestPool.Value.Add(taskForceEntity);
        }
    }
}