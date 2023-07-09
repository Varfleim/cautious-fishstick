
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

namespace SandOcean.Ship.Moving
{
    public struct TShipGroupMoving : IEcsThread<
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
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //���� ��������� �������� ������ ��������
                int sGMovingEntity = sGMovingEntities[a];
                ref CSGMoving sGMoving = ref sGMovingPool[sGMovingIndices[sGMovingEntity]];

                //���� ������ �������� ��������� � ��������
                if (sGMoving.mode == ShipGroupMoving.Moving)
                {
                    //���� ��������� ������ ��������
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[sGMovingEntity]];

                    //���� ������ ����� ����
                    LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                    //����������� ������ �������� � ������� ����� ����������
                    shipGroup.position = MoveTowards(
                        shipGroup.position, firstPathPoint.Value.destinationPoint,
                        1,
                        out bool isDestinationReached);

                    //���� ����� ���������� ����������
                    if (isDestinationReached == true)
                    {
                        //���� ����� ���������� ���� ������������� ������
                        if (firstPathPoint.Value.destinationPointTask == DestinationPointTask.Moving)
                        {
                            //������� ������ ����� ����
                            sGMoving.pathPoints.RemoveFirst();
                        }
                        //�����, ���� ����� ���������� ���� ����� ��� �������
                        else if(firstPathPoint.Value.destinationPointTask == DestinationPointTask.Landing)
                        {
                            //��������� ��������� �������� � ����� �������
                            sGMoving.mode = ShipGroupMoving.Landing;
                        }

                        Debug.LogError("Finish!");
                    }
                }

            }
        }

        Vector3 MoveTowards(
            Vector3 a, Vector3 b,
            float step,
            out bool isReached)
        {
            //������� ������ ����� ��������� �������� � �������
            Vector3 diff
                = b - a;

            //������� ��� �����
            float magnitude
                = diff.magnitude;

            Debug.LogWarning(magnitude);

            //���� ����� ������ ���� �����������
            if (magnitude <= step
                //��� ����� ����� ����
                || magnitude <= double.Epsilon)
            {
                //��������, ��� ���� ����������
                isReached
                    = true;

                //���������� ������� �������
                return b;
            }
            //�����
            else
            {
                //��������, ��� ���� �� ����������
                isReached
                    = false;

                //���������� ��������� �������, ��������� �� ��� �����������
                return
                    a + diff / magnitude * step;
            }
        }
    }
}