
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;

namespace SandOcean.Ship.Moving
{
    public struct TRegionShipGroupOwnershipChange : IEcsThread<
        CHexRegion,
        CShipGroup,
        CSGMoving>
    {
        public EcsWorld world;

        public MapGenerationData mapGenerationData;

        int[] regionEntities;

        CHexRegion[] regionPool;
        int[] regionIndices;

        CShipGroup[] shipGroupPool;
        int[] shipGroupIndices;

        CSGMoving[] sGMovingPool;
        int[] sGMovingIndices;

        public void Init(
            int[] entities,
            CHexRegion[] pool1, int[] indices1,
            CShipGroup[] pool2, int[] indices2,
            CSGMoving[] pool3, int[] indices3)
        {
            regionEntities = entities;

            regionPool = pool1;
            regionIndices = indices1;

            shipGroupPool = pool2;
            shipGroupIndices = indices2;

            sGMovingPool = pool3;
            sGMovingIndices = indices3;
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

                        //Определяем, меняет ли группа кораблей регион
                        bool isRegionChanged = false;

                        //Если список точек назначения не пуст
                        if (sGMoving.pathPoints.Count > 0)
                        {
                            //Берём первую точку назначения
                            LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                            //Если точка назначения находится в другом регионе
                            if (firstPathPoint.Value.destinationPointRegion == DestinationPointRegion.OtherRegion)
                            {
                                //Рассчитываем расстояние до центра региона
                                RegionDistanceCalculation(
                                    ref region,
                                    ref shipGroup,
                                    out isRegionChanged);
                            }
                        }
                        //Иначе
                        else
                        {
                            //Рассчитываем расстояние до центра региона
                            RegionDistanceCalculation(
                                ref region,
                                ref shipGroup,
                                out isRegionChanged);
                        }

                        //Если группа кораблей меняет регион
                        if (isRegionChanged == true)
                        {
                            //Если группа кораблей не последняя в списке
                            if (currentShipGroupNode.Next != null)
                            {
                                //Берём следующую группу в качестве текущей
                                currentShipGroupNode = currentShipGroupNode.Next;

                                //Удаляем текущую группу из списка
                                region.shipGroups.Remove(currentShipGroupNode.Previous);

                                //Переходим к следующей группе кораблей
                                continue;
                            }
                            //Иначе
                            else
                            {
                                //Удаляем последнюю группу из списка
                                region.shipGroups.RemoveLast();

                                //Выходим из цикла
                                break;
                            }
                        }
                    }

                    //Берём следующую группу кораблей в качестве текущей
                    currentShipGroupNode = currentShipGroupNode.Next;
                }
            }
        }

        void RegionDistanceCalculation(
            ref CHexRegion parentRegion,
            ref CShipGroup shipGroup,
            out bool isRegionChanged)
        {
            //Вычисляем расстояние от текущего положения группы кораблей до центра региона
            double distance = Vector3.Distance(shipGroup.position, parentRegion.Position);

            //Если расстояние больше внешнего радиуса региона
            if (distance > MapGenerationData.outerRadius)
            {
                //Определяем, в каком регионе находится группа кораблей
                mapGenerationData.GetRegionPEFromPosition(shipGroup.position).Unpack(world, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool[regionIndices[currentRegionEntity]];

                //Меняем регион, в котором находится группа кораблей
                shipGroup.parentRegionPE = currentRegion.selfPE;

                //Заносим группу кораблей в список групп, меняющих регион
                parentRegion.ownershipChangeShipGroups.Add(shipGroup.selfPE);

                //Регион меняется
                isRegionChanged = true;
            }
            //Иначе
            else
            {
                //Регион не меняется
                isRegionChanged = false;
            }
        }
    }
}