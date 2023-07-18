
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

                        //����������, ������ �� ������ �������� ������
                        bool isRegionChanged = false;

                        //���� ������ ����� ���������� �� ����
                        if (sGMoving.pathPoints.Count > 0)
                        {
                            //���� ������ ����� ����������
                            LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                            //���� ����� ���������� ��������� � ������ �������
                            if (firstPathPoint.Value.destinationPointRegion == DestinationPointRegion.OtherRegion)
                            {
                                //������������ ���������� �� ������ �������
                                RegionDistanceCalculation(
                                    ref region,
                                    ref shipGroup,
                                    out isRegionChanged);
                            }
                        }
                        //�����
                        else
                        {
                            //������������ ���������� �� ������ �������
                            RegionDistanceCalculation(
                                ref region,
                                ref shipGroup,
                                out isRegionChanged);
                        }

                        //���� ������ �������� ������ ������
                        if (isRegionChanged == true)
                        {
                            //���� ������ �������� �� ��������� � ������
                            if (currentShipGroupNode.Next != null)
                            {
                                //���� ��������� ������ � �������� �������
                                currentShipGroupNode = currentShipGroupNode.Next;

                                //������� ������� ������ �� ������
                                region.shipGroups.Remove(currentShipGroupNode.Previous);

                                //��������� � ��������� ������ ��������
                                continue;
                            }
                            //�����
                            else
                            {
                                //������� ��������� ������ �� ������
                                region.shipGroups.RemoveLast();

                                //������� �� �����
                                break;
                            }
                        }
                    }

                    //���� ��������� ������ �������� � �������� �������
                    currentShipGroupNode = currentShipGroupNode.Next;
                }
            }
        }

        void RegionDistanceCalculation(
            ref CHexRegion parentRegion,
            ref CShipGroup shipGroup,
            out bool isRegionChanged)
        {
            //��������� ���������� �� �������� ��������� ������ �������� �� ������ �������
            double distance = Vector3.Distance(shipGroup.position, parentRegion.Position);

            //���� ���������� ������ �������� ������� �������
            if (distance > MapGenerationData.outerRadius)
            {
                //����������, � ����� ������� ��������� ������ ��������
                mapGenerationData.GetRegionPEFromPosition(shipGroup.position).Unpack(world, out int currentRegionEntity);
                ref CHexRegion currentRegion = ref regionPool[regionIndices[currentRegionEntity]];

                //������ ������, � ������� ��������� ������ ��������
                shipGroup.parentRegionPE = currentRegion.selfPE;

                //������� ������ �������� � ������ �����, �������� ������
                parentRegion.ownershipChangeShipGroups.Add(shipGroup.selfPE);

                //������ ��������
                isRegionChanged = true;
            }
            //�����
            else
            {
                //������ �� ��������
                isRegionChanged = false;
            }
        }
    }
}