
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.Warfare.Fleet.Moving
{
    public struct TTaskForceMovement : IEcsThread<
        CTaskForceMovement, CTaskForce,
        CHexRegion>
    {
        public EcsWorld world;

        int[] taskForceEntities;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        CTaskForceMovement[] taskForceMovementPool;
        int[] taskForceMovementIndices;

        CHexRegion[] regionPool;
        int[] regionIndices;

        public void Init(
            int[] entities,
            CTaskForceMovement[] pool1, int[] indices1,
            CTaskForce[] pool2, int[] indices2,
            CHexRegion[] pool3, int[] indices3)
        {
            taskForceEntities = entities;

            taskForceMovementPool = pool1;
            taskForceMovementIndices = indices1;

            taskForcePool = pool2;
            taskForceIndices = indices2;

            regionPool = pool3;
            regionIndices = indices3;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Для кажлой оперативной группы с компонентом движения
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём компонент движения и группу
                int taskForceEntity = taskForceEntities[a];
                ref CTaskForceMovement tFMovement = ref taskForceMovementPool[taskForceMovementIndices[taskForceEntity]];
                ref CTaskForce taskForce = ref taskForcePool[taskForceIndices[taskForceEntity]];

                //Если предыдущий регион не пуст
                if (taskForce.previousRegionPE.Unpack(world, out int previousRegionEntity))
                {
                    //Обнуляем предыдущий регион
                    taskForce.previousRegionPE = new();
                }

                //Если маршрут группы не пуст
                if (tFMovement.pathRegionPEs.Count > 0)
                {
                    //Берём последний регион в маршруте, то есть следующий регион пути
                    tFMovement.pathRegionPEs[tFMovement.pathRegionPEs.Count - 1].Unpack(world, out int nextRegionEntity);
                    ref CHexRegion nextRegion = ref regionPool[regionIndices[nextRegionEntity]];

                    //Рассчитываем скорость с учётом состава оперативной группы и особенностей следующего региона
                    float movementSpeed = 50;

                    //Прибавляем скорость к пройденному расстоянию
                    tFMovement.traveledDistance += movementSpeed;

                    //Если пройденное расстояние больше или равно расстояния между регионами
                    if (tFMovement.traveledDistance >= RegionsData.regionDistance)
                    {
                        //То группа переходит в следующий регион

                        //Отмечаем, что группа завершила перемещение
                        tFMovement.isTraveled = true;

                        //Обнуляем пройденное расстояние
                        tFMovement.traveledDistance = 0;

                        //UnityEngine.Debug.LogWarning("Finish 1! " + taskForce.rand);
                    }
                }
                //Иначе
                else
                {
                    //Группа уже находится в целевом регионе (что возможно только при изначально нулевом пути)

                    //Отмечаем, что группа завершила перемещение
                    tFMovement.isTraveled = true;

                    //Обнуляем пройденное расстояние
                    tFMovement.traveledDistance = 0;

                    //UnityEngine.Debug.LogWarning("Finish 2! " + taskForce.rand);
                }
            }
        }
    }
}