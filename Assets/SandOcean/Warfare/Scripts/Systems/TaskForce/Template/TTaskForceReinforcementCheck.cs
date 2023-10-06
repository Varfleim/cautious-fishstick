
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public struct TTaskForceReinforcementCheck : IEcsThread<
        SRTaskForceReinforcementCheck, CTaskForce>
    {
        public EcsWorld world;

        int[] taskForceEntities;

        SRTaskForceReinforcementCheck[] taskForceReinforcementCheckSelfRequestPool;
        int[] taskForceReinforcementCheckSelfRequestIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        public void Init(
            int[] entities,
            SRTaskForceReinforcementCheck[] pool1, int[] indices1,
            CTaskForce[] pool2, int[] indices2)
        {
            taskForceEntities = entities;

            taskForceReinforcementCheckSelfRequestPool = pool1;
            taskForceReinforcementCheckSelfRequestIndices = indices1;

            taskForcePool = pool2;
            taskForceIndices = indices2;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Для каждой оперативной группы с самозапросом проверки пополнения
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём самозапрос и группу
                int taskForceEntity = taskForceEntities[a];
                ref SRTaskForceReinforcementCheck selfRequestComp = ref taskForceReinforcementCheckSelfRequestPool[taskForceReinforcementCheckSelfRequestIndices[taskForceEntity]];
                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                //Берём шаблон группы
                DTFTemplate template = taskForce.template;

                //Для каждого типа корабля в шаблоне
                for (int b = 0; b < template.shipTypes.Length; b++)
                {
                    //Берём тип корабля
                    DShipType shipType = template.shipTypes[b].shipType;

                    //Подсчитываем, сколько кораблей данного типа есть в группе
                    int shipCount = 0;

                    //Для каждого корабля в группе
                    for (int c = 0; c < taskForce.ships.Count; c++)
                    {
                        //Берём корабль
                        DShip ship = taskForce.ships[c];

                        //Если корабль относится к данному типу
                        if (ship.shipType == shipType)
                        {
                            //Увеличиваем количество кораблей
                            shipCount += 1;
                        }
                    }

                    //Если количество кораблей меньше необходимого
                    if (shipCount < template.shipTypes[b].shipCount)
                    {
                        //Рассчитываем запрашиваемое количество кораблей
                        int requestedShipCount = template.shipTypes[b].shipCount - shipCount;

                        //Проверяем, есть ли уже данный тип корабля в запросе
                        bool isTypeRequested = false;

                        //Для каждого запрошенного типа корабля
                        for (int c = 0; c < taskForce.reinforcementRequest.shipTypes.Count; c++)
                        {
                            //Если тип корабля - искомый
                            if (taskForce.reinforcementRequest.shipTypes[c].shipType == shipType)
                            {
                                //Отмечаем, что данный тип есть в запросе
                                isTypeRequested = true;

                                //Изменяем количество запрошенных кораблей
                                taskForce.reinforcementRequest.shipTypes[c].requestedShipCount = requestedShipCount;

                                //Выходим из цикла, поскольку тип найден
                                break;
                            }
                        }

                        //Если типа нет в запросе
                        if (isTypeRequested == false)
                        {
                            //Запрашиваем нужное количество кораблей
                            taskForce.reinforcementRequest.shipTypes.Add(new(
                                shipType,
                                requestedShipCount));
                        }
                    }
                    //Иначе запрос удовлетворён
                    else
                    {
                        //Для каждого запрошенного типа корабля
                        for (int c = 0; c < taskForce.reinforcementRequest.shipTypes.Count; c++)
                        {
                            //Если тип корабля - искомый
                            if (taskForce.reinforcementRequest.shipTypes[c].shipType == shipType)
                            {
                                //Если количество отправленных кораблей равно нулю
                                if (taskForce.reinforcementRequest.shipTypes[c].sentedShipCount == 0)
                                {
                                    //Удаляем тип корабля из запроса
                                    taskForce.reinforcementRequest.shipTypes.RemoveAt(c);

                                    //Выходим из цикла, поскольку тип найден
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}