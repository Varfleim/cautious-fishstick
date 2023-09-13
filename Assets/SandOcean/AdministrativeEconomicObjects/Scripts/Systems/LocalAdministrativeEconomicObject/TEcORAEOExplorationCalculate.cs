
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

namespace SandOcean.AEO.RAEO
{
    public struct TEcORAEOExplorationCalculate : IEcsThread<
        CEconomicORAEO,
        CExplorationORAEO>
    {
        public EcsWorld world;

        int[] oRAEOEntities;

        CEconomicORAEO[] economicORAEOPool;
        int[] economicORAEOIndices;

        CExplorationORAEO[] explorationORAEOPool;
        int[] explorationORAEOIndices;

        public void Init(
            int[] entities,
            CEconomicORAEO[] pool1, int[] indices1,
            CExplorationORAEO[] pool2, int[] indices2)
        {
            oRAEOEntities = entities;

            economicORAEOPool = pool1;
            economicORAEOIndices = indices1;

            explorationORAEOPool = pool2;
            explorationORAEOIndices = indices2;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //��� ������� ORAEO � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� EcORAEO � ExORAEO
                int oRAEOEntity = oRAEOEntities[a];
                ref CEconomicORAEO ecORAEO = ref economicORAEOPool[economicORAEOIndices[oRAEOEntity]];
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool[explorationORAEOIndices[oRAEOEntity]];

                //������� 

                //���� ������ ������ �������� � ������
                /*LinkedListNode<EcsPackedEntity> currentShipGroupNode = ecORAEO.landedShipGroups.First;
                //��� ������ ������ �������� � ORAEO
                while (currentShipGroupNode != null)
                {
                    //���� ��������� ������ ��������
                    currentShipGroupNode.Value.Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //����
                    //���� ������ �������� ��������� � ������ �����������
                    if (shipGroup.movingMode == ShipGroupMovingMode.Idle)
                    {
                        //���� ������� ������������ ������ ������
                        if (exORAEO.explorationLevel < 10)
                        {
                            //����������� ������� ������������
                            exORAEO.explorationLevel++;
                        }
                    }
                    //����

                    //���� ��������� ������ �������� � �������� �������
                    currentShipGroupNode = currentShipGroupNode.Next;
                }*/
            }
        }
    }
}