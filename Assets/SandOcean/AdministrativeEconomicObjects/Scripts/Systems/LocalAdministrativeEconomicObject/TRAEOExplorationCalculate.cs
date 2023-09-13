
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Organization;

namespace SandOcean.AEO.RAEO
{
    public struct TRAEOExplorationCalculate : IEcsThread<
        CRegionAEO,
        CExplorationORAEO,
        COrganization>
    {
        public EcsWorld world;

        int[] regionAEOEntities;

        CRegionAEO[] regionAEOPool;
        int[] regionAEOIndices;

        CExplorationORAEO[] explorationORAEOPool;
        int[] explorationORAEOIndices;

        COrganization[] organizationPool;
        int[] organizationIndices;

        public void Init(
            int[] entities,
            CRegionAEO[] pool1, int[] indices1,
            CExplorationORAEO[] pool2, int[] indices2,
            COrganization[] pool3, int[] indices3)
        {
            regionAEOEntities = entities;

            regionAEOPool = pool1;
            regionAEOIndices = indices1;

            explorationORAEOPool = pool2;
            explorationORAEOIndices = indices2;

            organizationPool = pool3;
            organizationIndices = indices3;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Для каждого RAEO в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём RAEO
                int rAEOEntity = regionAEOEntities[a];
                ref CRegionAEO rAEO = ref regionAEOPool[regionAEOIndices[rAEOEntity]];

                //Берём первую группу кораблей в списке
                /*LinkedListNode<EcsPackedEntity> currentShipGroupNode = rAEO.landedShipGroups.First;
                //Для каждой группы кораблей в ORAEO
                while (currentShipGroupNode != null)
                {
                    //Берём группу кораблей
                    currentShipGroupNode.Value.Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //ТЕСТ
                    //Если группа кораблей находится в режиме бездействия
                    if (shipGroup.movingMode == ShipGroupMovingMode.Idle)
                    {
                        //Берём организацию-владельца группы кораблей
                        shipGroup.ownerOrganizationPE.Unpack(world, out int ownerOrganizationEntity);
                        ref COrganization ownerOrganization = ref organizationPool[organizationIndices[ownerOrganizationEntity]];

                        //Берём ExORAEO организации
                        rAEO.organizationRAEOs[ownerOrganization.selfIndex].organizationRAEOPE.Unpack(world, out int exORAEOEntity);
                        ref CExplorationORAEO exORAEO = ref explorationORAEOPool[explorationORAEOIndices[exORAEOEntity]];

                        //Если уровень исследования меньше десяти
                        if (exORAEO.explorationLevel < 10)
                        {
                            //Увеличиваем уровень исследования
                            exORAEO.explorationLevel++;
                        }
                    }
                    //ТЕСТ

                    //Берём следующую группу кораблей в качестве текущей
                    currentShipGroupNode = currentShipGroupNode.Next;
                }*/
            }
        }
    }
}