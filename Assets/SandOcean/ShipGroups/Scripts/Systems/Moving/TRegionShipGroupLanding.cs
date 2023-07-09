
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.AEO.RAEO;

namespace SandOcean.Ship.Moving
{
    public struct TRegionShipGroupLanding : IEcsThread<
        CHexRegion,
        CShipGroup,
        CSGMoving,
        CRegionAEO,
        CEconomicORAEO>
    {
        public EcsWorld world;

        int[] regionEntities;

        CHexRegion[] regionPool;
        int[] regionIndices;

        CShipGroup[] shipGroupPool;
        int[] shipGroupIndices;

        CSGMoving[] sGMovingPool;
        int[] sGMovingIndices;

        CRegionAEO[] regionAEOPool;
        int[] regionAEOIndices;

        CEconomicORAEO[] economicORAEOPool;
        int[] economicORAEOIndices;

        public void Init(
            int[] entities,
            CHexRegion[] pool1, int[] indices1,
            CShipGroup[] pool2, int[] indices2,
            CSGMoving[] pool3, int[] indices3,
            CRegionAEO[] pool4, int[] indices4,
            CEconomicORAEO[] pool5, int[] indices5)
        {
            regionEntities = entities;

            regionPool = pool1;
            regionIndices = indices1;

            shipGroupPool = pool2;
            shipGroupIndices = indices2;

            sGMovingPool = pool3;
            sGMovingIndices = indices3;

            regionAEOPool = pool4;
            regionAEOIndices = indices4;

            economicORAEOPool = pool5;
            economicORAEOIndices = indices5;
        }

        public void Execute(int fromIndex, int beforeIndex)
        {
            //Для каждого региона в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент региона
                int regionEntity = regionEntities[a];
                ref CHexRegion region = ref regionPool[regionIndices[regionEntity]];

                //Берём первую группу кораблей в списке
                LinkedListNode<EcsPackedEntity> currentShipGroupNode = region.shipGroups.First;
                //Для каждой группы кораблей в регионе
                while (currentShipGroupNode != null)
                {
                    //Берём компонент группы кораблей
                    currentShipGroupNode.Value.Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //Если группа кораблей находится в режиме движения
                    if (shipGroup.movingMode == Ship.ShipGroupMovingMode.Moving)
                    {
                        //Берём компонент движения 
                        ref CSGMoving sGMoving = ref sGMovingPool[sGMovingIndices[shipGroupEntity]];

                        //Если компонент движения находится в режиме посадки
                        if (sGMoving.mode == ShipGroupMoving.Landing)
                        {
                            //Берём первую точку пути
                            LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                            //Если тип цели - RAEO
                            if (firstPathPoint.Value.targetType == MovementTargetType.RAEO)
                            {
                                //Берём компонент RAEO 
                                firstPathPoint.Value.targetPE.Unpack(world, out int rAEOEntity);
                                ref CRegionAEO rAEO = ref regionAEOPool[regionAEOIndices[rAEOEntity]];

                                //Заносим группу кораблей в список групп, садящихся на RAEO
                                rAEO.landingShipGroups.Add(shipGroup.selfPE);
                            }
                            //Иначе, если тип цели - (собственный) EconomicORAEO
                            else if (firstPathPoint.Value.targetType == MovementTargetType.EconomicORAEO)
                            {
                                //Берём компонент EcORAEO
                                firstPathPoint.Value.targetPE.Unpack(world, out int oRAEOEntity);
                                ref CEconomicORAEO ecORAEO = ref economicORAEOPool[economicORAEOIndices[oRAEOEntity]];

                                //Заносим группу кораблей в список групп, садящихся на EcORAEO
                                ecORAEO.landingShipGroups.Add(shipGroup.selfPE);
                            }

                            //Удаляем первую точку пути
                            sGMoving.pathPoints.RemoveFirst();

                            //Переводим компонент движения в режим ожидания
                            sGMoving.mode = ShipGroupMoving.Waiting;
                        }
                    }

                    //Берём следующую группу кораблей в качестве текущей
                    currentShipGroupNode = currentShipGroupNode.Next;
                }
            }
        }
    }
}