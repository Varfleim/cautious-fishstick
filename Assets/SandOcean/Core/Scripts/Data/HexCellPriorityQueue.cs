
using System.Collections;
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean
{
    public class HexCellPriorityQueue
    {
        List<DHexCellPriority> list = new();

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

            DHexCellPriority newCellPriority = new(
                cellPE,
                priority);

            newCellPriority.NextWithSamePriority
                = list[priority];

            list[priority]
                = newCellPriority;
        }

        public DHexCellPriority Dequeue()
        {
            count -= 1;

            for (; minimum < list.Count; minimum++)
            {
                DHexCellPriority cellPriority = list[minimum];

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
            DHexCellPriority currentCell = list[oldPriority];
            //���� ��������� ������ � ������ �� ������� ����������
            DHexCellPriority nextCell = currentCell.NextWithSamePriority;

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