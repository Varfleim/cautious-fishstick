
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

namespace SandOcean.Ship.Moving
{
    public struct TShipGroupPathfinding : IEcsThread<
        CSGMoving,
        CShipGroup>
    {
        public EcsWorld world;

        int[] sGMovingEntities;

        CSGMoving[] sGMovingPool;
        int[] sGMovingIndices;

        CShipGroup[] shipGroupPool;
        int[] shipGroupIndices;

        public void Init(
            int[] entities,
            CSGMoving[] pool1, int[] indices1,
            CShipGroup[] pool2, int[] indices2)
        {
            sGMovingEntities = entities;

            sGMovingPool = pool1;
            sGMovingIndices = indices1;

            shipGroupPool = pool2;
            shipGroupIndices = indices2;
        }

        public void Execute(int fromIndex, int beforeIndex)
        {
            //Для каждого компонента движения группы кораблей в потоке
            /*for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент движения группы кораблей
                int sGMovingEntity = sGMovingEntities[a];
                ref CSGMoving sGMoving = ref sGMovingPool[sGMovingIndices[sGMovingEntity]];

                //Если первая точка пути требует найти путь
                if (sGMoving.pathPoints.First.Value.movementType == MovementType.Pathfinding)
                {
                    //Берём первую точку пути
                    LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                    //Берём компонент группы кораблей
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[sGMovingEntity]];

                    //Если тип цели - остров
                    if (firstPathPoint.Value.targetType == MovementTargetType.RAEO)
                    {
                        //Берём компонент острова
                        firstPathPoint.Value.targetPE.Unpack(world, out int targetIslandEntity);
                        ref CIsland targetIsland = ref islandPool[islandIndices[targetIslandEntity]];

                        //Определяем, находится ли остров в том же регионе
                        DestinationPointRegion destinationPointRegion;

                        //Если остров находится в том же регионе
                        if (targetIsland.parentRegionPE.EqualsTo(shipGroup.parentRegionPE) == true)
                        {
                            //Указываем, что остров находится в том же регионе
                            destinationPointRegion = DestinationPointRegion.CurrentRegion;
                        }
                        //Иначе
                        else
                        {
                            //Указываем, что остров находится в другом регионе
                            destinationPointRegion = DestinationPointRegion.OtherRegion;
                        }

                        //Здесь должен быть код, который определяет, не находится ли остров за пределом дальности, 
                        //и если так, то нужно рассчитать путь до него по союзным аэродромам

                        //

                        //Добавляем новую точку пути после первой
                        sGMoving.pathPoints.AddAfter(firstPathPoint, new DShipGroupPathPoint(
                            targetIsland.center,
                            targetIsland.selfPE,
                            MovementTargetType.RAEO,
                            MovementType.Direct,
                            destinationPointRegion,
                            firstPathPoint.Value.destinationPointTask));
                    }

                    //Удаляем первую точку пути
                    sGMoving.pathPoints.RemoveFirst();

                    //Переводим компонент движения в режим движения
                    sGMoving.mode = ShipGroupMoving.Moving;
                }
            }*/
        }
    }
}