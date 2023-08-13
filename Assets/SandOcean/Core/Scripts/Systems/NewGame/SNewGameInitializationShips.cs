
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map;
using SandOcean.Ship;

namespace SandOcean
{
    public class SNewGameInitializationShips : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Карта
        //readonly EcsPoolInject<CHexChunk> chunkPool = default;

        //readonly EcsPoolInject<CHexRegion> regionPool = default;

        //События кораблей
        //readonly EcsPoolInject<EShipGroupCreating> shipGroupCreatingEventPool = default;

        //Общие события
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;

        //Данные
        //readonly EcsCustomInject<SpaceGenerationData> spaceGenerationData = default;
        //readonly EcsCustomInject<InputData> inputData = default;

        //readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события начала новой игры
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //Берём компонент события начала новой игры
                ref RStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //Берём компонент первого региона
                /*spaceGenerationData.Value.cellPEs[0].Unpack(world.Value, out int regionEntity);
                ref CHexCell region = ref regionPool.Value.Get(regionEntity);

                //Создаём новую сущность и назначаем ей компонент события создания группы кораблей в эфире
                int shipGroupCreatingEventEntity = world.Value.NewEntity();
                ref EShipGroupCreating shipGroupCreatingEvent = ref shipGroupCreatingEventPool.Value.Add(shipGroupCreatingEventEntity);

                //Указываем организацию игрока как организацию-владельца
                shipGroupCreatingEvent.ownerOrganizationPE = inputData.Value.playerOrganizationPE;

                //Указываем регион как родительский
                shipGroupCreatingEvent.parentRegionPE = region.selfPE;

                //Указываем позицию, где требуется создать группу кораблей
                shipGroupCreatingEvent.position = new(0, 0, 0);

                //Определяем длину массива запрошенных кораблей
                shipGroupCreatingEvent.orderedShips = new DOrderedShipSpace[(int)eUI.Value.newGameMenuWindow.startShipsNumberSlider.value];

                //Определяем дистанцию между кораблями
                double distance = 2;

                //Для каждого корабля в массиве
                for (int a = 0; a < shipGroupCreatingEvent.orderedShips.Length; a++)
                {
                    //Если это первый корабль в массиве
                    if (a == 0)
                    {
                        //Заносим данные нового запрошенного корабля в массив
                        shipGroupCreatingEvent.orderedShips[a] = new(
                            new DVector3(
                                distance, 0, 0),
                            0,
                            0);
                    }
                    //Иначе
                    else
                    {
                        //Заносим данные нового запрошенного корабля в массив
                        shipGroupCreatingEvent.orderedShips[a] = new(
                            new(
                                shipGroupCreatingEvent.orderedShips[a - 1].shipPosition.x + distance,
                                0,
                                0),
                            0,
                            0);
                    }
                }*/

                world.Value.DelEntity(startNewGameEventEntity);
            }
        }
    }
}