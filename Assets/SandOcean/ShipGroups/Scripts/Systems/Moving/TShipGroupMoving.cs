
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
            //Для каждого компонента движения группы кораблей в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент движения группы кораблей
                int sGMovingEntity = sGMovingEntities[a];
                ref CSGMoving sGMoving = ref sGMovingPool[sGMovingIndices[sGMovingEntity]];

                //Если группа кораблей находится в движении
                if (sGMoving.mode == ShipGroupMoving.Moving)
                {
                    //Берём компонент группы кораблей
                    ref CShipGroup shipGroup = ref shipGroupPool[shipGroupIndices[sGMovingEntity]];

                    //Берём первую точку пути
                    LinkedListNode<DShipGroupPathPoint> firstPathPoint = sGMoving.pathPoints.First;

                    //Передвигаем группу кораблей в сторону точки назначения
                    shipGroup.position = MoveTowards(
                        shipGroup.position, firstPathPoint.Value.destinationPoint,
                        1,
                        out bool isDestinationReached);

                    //Если точка назначения достигнута
                    if (isDestinationReached == true)
                    {
                        //Если точка назначения была промежуточной точкой
                        if (firstPathPoint.Value.destinationPointTask == DestinationPointTask.Moving)
                        {
                            //Удаляем первую точку пути
                            sGMoving.pathPoints.RemoveFirst();
                        }
                        //Иначе, если точка назначения была целью для посадки
                        else if(firstPathPoint.Value.destinationPointTask == DestinationPointTask.Landing)
                        {
                            //Переводим компонент движения в режим посадки
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
            //Находим вектор между стартовой позицией и целевой
            Vector3 diff
                = b - a;

            //Находим его длину
            float magnitude
                = diff.magnitude;

            Debug.LogWarning(magnitude);

            //Если длина меньше шага перемещения
            if (magnitude <= step
                //ИЛИ длина равна нулю
                || magnitude <= double.Epsilon)
            {
                //Отмечаем, что цель достигнута
                isReached
                    = true;

                //Возвращаем целевую позицию
                return b;
            }
            //Иначе
            else
            {
                //Отмечаем, что цель не достигнута
                isReached
                    = false;

                //Возвращаем стартовую позицию, смещённую на шаг перемещения
                return
                    a + diff / magnitude * step;
            }
        }
    }
}