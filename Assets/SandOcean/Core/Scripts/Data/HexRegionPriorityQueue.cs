
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
            //ЅерЄм первую €чейку в списке по старому приоритету
            DHexRegionPriority currentCell = list[oldPriority];
            //ЅерЄм следующую €чейку в списке по старому приоритету
            DHexRegionPriority nextCell = currentCell.NextWithSamePriority;

            //≈сли PE первой €чейки соответствут искомому PE
            if (currentCell.cellPE.EqualsTo(cellPE) == true)
            {
                //“о перва€ €чейка - искома€

                //«амен€ем первую €чейку в списке на следующую, тем самым удал€€ запись
                list[oldPriority] = nextCell;
            }
            //»наче
            else
            {
                //ѕока PE следующей €чейки не соответствует искомому
                while (nextCell.cellPE.EqualsTo(cellPE) == false)
                {
                    //ƒелаем следующую €чейку в списке текущей
                    currentCell = nextCell;

                    //ЅерЄм следующую €чейку в списке
                    nextCell = currentCell.NextWithSamePriority;
                }

                //ѕосле этого получаетс€, что следующа€ €чейка в списке - это искома€

                //ƒелаем ѕќ—Ћ≈следующую €чейку в списке следующей, тем самым удал€€ запись о искомой
                currentCell.NextWithSamePriority = nextCell.NextWithSamePriority;
            }

            //«аносим искомую €чейку в список по новому приоритету
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