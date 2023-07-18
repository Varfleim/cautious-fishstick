
using System.Collections;
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean
{
    public class HexRegionPriorityQueue
    {
        List<DHexRegionPriority> list = new();

        public int Count
        {
            get
            {
                return count;
            }
        }
        int count;

        int minimum = int.MaxValue;

        public void Enqueue(
            EcsPackedEntity cellPE,
            int priority)
        {
            count += 1;

            if (priority < minimum)
            {
                minimum = priority;
            }

            while (priority >= list.Count)
            {
                list.Add(null);
            }

            DHexRegionPriority newCellPriority = new(
                cellPE,
                priority);

            newCellPriority.NextWithSamePriority
                = list[priority];

            list[priority]
                = newCellPriority;
        }

        public DHexRegionPriority Dequeue()
        {
            count -= 1;

            for (; minimum < list.Count; minimum++)
            {
                DHexRegionPriority cellPriority = list[minimum];

                if (cellPriority != null)
                {
                    list[minimum] = cellPriority.NextWithSamePriority;

                    return cellPriority;
                }
            }

            return null;
        }

        public void Change(
            EcsPackedEntity cellPE,
            int oldPriority,
            int newPriority)
        {
            //���� ������ ������ � ������ �� ������� ����������
            DHexRegionPriority currentCell = list[oldPriority];
            //���� ��������� ������ � ������ �� ������� ����������
            DHexRegionPriority nextCell = currentCell.NextWithSamePriority;

            //���� PE ������ ������ ������������ �������� PE
            if (currentCell.cellPE.EqualsTo(cellPE) == true)
            {
                //�� ������ ������ - �������

                //�������� ������ ������ � ������ �� ���������, ��� ����� ������ ������
                list[oldPriority] = nextCell;
            }
            //�����
            else
            {
                //���� PE ��������� ������ �� ������������� ��������
                while (nextCell.cellPE.EqualsTo(cellPE) == false)
                {
                    //������ ��������� ������ � ������ �������
                    currentCell = nextCell;

                    //���� ��������� ������ � ������
                    nextCell = currentCell.NextWithSamePriority;
                }

                //����� ����� ����������, ��� ��������� ������ � ������ - ��� �������

                //������ �������������� ������ � ������ ���������, ��� ����� ������ ������ � �������
                currentCell.NextWithSamePriority = nextCell.NextWithSamePriority;
            }

            //������� ������� ������ � ������ �� ������ ����������
            Enqueue(
                cellPE,
                newPriority);
            count -= 1;
        }

        public void Clear()
        {
            list.Clear();
            count = 0;

            minimum = int.MaxValue;
        }
    }
}