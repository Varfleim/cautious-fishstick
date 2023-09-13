
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public struct TReserveFleetReinforcementRequest : IEcsThread<
        SRReserveFleetReinforcementCheck, CFleet, CReserveFleet>
    {
        public EcsWorld world;

        int[] fleetEntities;

        SRReserveFleetReinforcementCheck[] reserveFleetReinforcementCheckSelfRequestPool;
        int[] reserveFleetReinforcementCheckSelfRequestIndices;

        CFleet[] fleetPool;
        int[] fleetIndices;

        CReserveFleet[] reserveFleetPool;
        int[] reserveFleetIndices;

        public void Init(
            int[] entities,
            SRReserveFleetReinforcementCheck[] pool1, int[] indices1,
            CFleet[] pool2, int[] indices2,
            CReserveFleet[] pool3, int[] indices3)
        {
            fleetEntities = entities;

            reserveFleetReinforcementCheckSelfRequestPool = pool1;
            reserveFleetReinforcementCheckSelfRequestIndices = indices1;

            fleetPool = pool2;
            fleetIndices = indices2;

            reserveFleetPool = pool3;
            reserveFleetIndices = indices3;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Для каждого резервного флота с самозапросом проверки пополнения
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём самозапрос, флот и его компонент резервного флота
                int reserveFleetEntity = fleetEntities[a];
                ref SRReserveFleetReinforcementCheck selfRequestComp = ref reserveFleetReinforcementCheckSelfRequestPool[reserveFleetReinforcementCheckSelfRequestIndices[reserveFleetEntity]];
                ref CFleet fleet = ref fleetPool[fleetIndices[reserveFleetEntity]];
                ref CReserveFleet reserveFleet = ref reserveFleetPool[reserveFleetIndices[reserveFleetEntity]];

                //Если у флота есть корабли в резерве
                if (true)
                {
                    //Для каждого дочернего флота
                    for (int b = 0; b < reserveFleet.ownedFleetPEs.Count; b++)
                    {
                        //Берём дочерний флот
                        reserveFleet.ownedFleetPEs[b].Unpack(world, out int ownedFleetEntity);
                        ref CFleet ownedFleet = ref fleetPool[fleetIndices[ownedFleetEntity]];

                        //Для каждого запроса пополнения
                        for (int c = 0; c < ownedFleet.tFReinforcementRequests.Count; c++)
                        {
                            //Если есть запросы кораблей
                            if (ownedFleet.tFReinforcementRequests[c].shipTypes.Count > 0)
                            {
                                //Берём запрос пополнения
                                DTFReinforcementRequest tFReinforcementRequest = ownedFleet.tFReinforcementRequests[c];

                                //Создаём запрос новой группы пополнения
                                DTaskForceReinforcementCreating taskForceReinforcementCreating = new(
                                    ownedFleet.ownedTaskForcePEs[c]);

                                //Для каждого запрошенного типа корабля
                                for (int d = 0; d < tFReinforcementRequest.shipTypes.Count; d++)
                                {
                                    //Если количество запрошенных кораблей больше количества отправленных
                                    if (tFReinforcementRequest.shipTypes[d].requestedShipCount
                                        > tFReinforcementRequest.shipTypes[d].sentedShipCount)
                                    {
                                        //Определяем, сколько кораблей нужно отправить
                                        int requiredShipCount = tFReinforcementRequest.shipTypes[d].requestedShipCount
                                            - tFReinforcementRequest.shipTypes[d].sentedShipCount;

                                        //ТЕСТ
                                        //Создаём структуру для запрошенного типа корабля
                                        DCountedShipType shipType = new(
                                            tFReinforcementRequest.shipTypes[d].shipType,
                                            requiredShipCount);

                                        //Заносим её в список зарошенных типов кораблей
                                        taskForceReinforcementCreating.shipTypes.Add(shipType);

                                        //Обновляем количество отправленных для пополнения кораблей
                                        tFReinforcementRequest.shipTypes[d].sentedShipCount += requiredShipCount;
                                        //ТЕСТ
                                    }
                                }

                                //Если запрос группы не пуст
                                if (taskForceReinforcementCreating.shipTypes.Count > 0)
                                {
                                    //Заносим запрос в список
                                    reserveFleet.requestedTaskForceReinforcements.Add(taskForceReinforcementCreating);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}