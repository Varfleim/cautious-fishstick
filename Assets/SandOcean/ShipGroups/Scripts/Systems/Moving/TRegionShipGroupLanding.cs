
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
            //��� ������� ������� � ������
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� �������
                int regionEntity = regionEntities[a];
                ref CHexRegion region = ref regionPool[regionIndices[regionEntity]];

                //���� ������ ������ �������� � ������
                LinkedListNode<EcsPackedEntity> currentShipGroupNode = region.shipGroups.First;
                //��� ������ ������ �������� � �������
                while (currentShipGroupNode != null)
                {
                    //���� ��������� ������ ��������
                    currentShipGroupNode.Value.Unpack(world, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[shipGroupEntity]];

                    //���� ������ �������� ��������� � ������ ��������
                    if (shipGroup.movingMode == Ship.ShipGroupMovingMode.Moving)
                    {
                        //���� ��������� �������� 
                        ref CSGMoving sGMoving = ref sGMovingPool[sGMovingIndices[shipGroupEntity]];

                        //���� ��������� �������� ��������� � ������ �������
                        if (sGMoving.mode == ShipGroupMoving.Landing)
                        {
                            //���� ������ ����� ����
                            LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                            //���� ��� ���� - RAEO
                            if (firstPathPoint.Value.targetType == MovementTargetType.RAEO)
                            {
                                //���� ��������� RAEO 
                                firstPathPoint.Value.targetPE.Unpack(world, out int rAEOEntity);
                                ref CRegionAEO rAEO = ref regionAEOPool[regionAEOIndices[rAEOEntity]];

                                //������� ������ �������� � ������ �����, ��������� �� RAEO
                                rAEO.landingShipGroups.Add(shipGroup.selfPE);
                            }
                            //�����, ���� ��� ���� - (�����������) EconomicORAEO
                            else if (firstPathPoint.Value.targetType == MovementTargetType.EconomicORAEO)
                            {
                                //���� ��������� EcORAEO
                                firstPathPoint.Value.targetPE.Unpack(world, out int oRAEOEntity);
                                ref CEconomicORAEO ecORAEO = ref economicORAEOPool[economicORAEOIndices[oRAEOEntity]];

                                //������� ������ �������� � ������ �����, ��������� �� EcORAEO
                                ecORAEO.landingShipGroups.Add(shipGroup.selfPE);
                            }

                            //������� ������ ����� ����
                            sGMoving.pathPoints.RemoveFirst();

                            //��������� ��������� �������� � ����� ��������
                            sGMoving.mode = ShipGroupMoving.Waiting;
                        }
                    }

                    //���� ��������� ������ �������� � �������� �������
                    currentShipGroupNode = currentShipGroupNode.Next;
                }
            }
        }
    }
}