
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
            //��� ������� ���������� �������� ������ �������� � ������
            /*for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� �������� ������ ��������
                int sGMovingEntity = sGMovingEntities[a];
                ref CSGMoving sGMoving = ref sGMovingPool[sGMovingIndices[sGMovingEntity]];

                //���� ������ ����� ���� ������� ����� ����
                if (sGMoving.pathPoints.First.Value.movementType == MovementType.Pathfinding)
                {
                    //���� ������ ����� ����
                    LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                    //���� ��������� ������ ��������
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[sGMovingEntity]];

                    //���� ��� ���� - ������
                    if (firstPathPoint.Value.targetType == MovementTargetType.RAEO)
                    {
                        //���� ��������� �������
                        firstPathPoint.Value.targetPE.Unpack(world, out int targetIslandEntity);
                        ref CIsland targetIsland = ref islandPool[islandIndices[targetIslandEntity]];

                        //����������, ��������� �� ������ � ��� �� �������
                        DestinationPointRegion destinationPointRegion;

                        //���� ������ ��������� � ��� �� �������
                        if (targetIsland.parentRegionPE.EqualsTo(shipGroup.parentRegionPE) == true)
                        {
                            //���������, ��� ������ ��������� � ��� �� �������
                            destinationPointRegion = DestinationPointRegion.CurrentRegion;
                        }
                        //�����
                        else
                        {
                            //���������, ��� ������ ��������� � ������ �������
                            destinationPointRegion = DestinationPointRegion.OtherRegion;
                        }

                        //����� ������ ���� ���, ������� ����������, �� ��������� �� ������ �� �������� ���������, 
                        //� ���� ���, �� ����� ���������� ���� �� ���� �� ������� ����������

                        //

                        //��������� ����� ����� ���� ����� ������
                        sGMoving.pathPoints.AddAfter(firstPathPoint, new DShipGroupPathPoint(
                            targetIsland.center,
                            targetIsland.selfPE,
                            MovementTargetType.RAEO,
                            MovementType.Direct,
                            destinationPointRegion,
                            firstPathPoint.Value.destinationPointTask));
                    }

                    //������� ������ ����� ����
                    sGMoving.pathPoints.RemoveFirst();

                    //��������� ��������� �������� � ����� ��������
                    sGMoving.mode = ShipGroupMoving.Moving;
                }
            }*/
        }
    }
}