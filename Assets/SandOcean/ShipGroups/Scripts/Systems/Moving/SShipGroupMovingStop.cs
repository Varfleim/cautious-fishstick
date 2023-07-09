
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.Ship.Moving
{
    public class SShipGroupMovingStop : IEcsRunSystem
    {
        //Группы кораблей
        readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

        readonly EcsFilterInject<Inc<CSGMoving>> sGMovingFilter = default;
        readonly EcsPoolInject<CSGMoving> sGMovingPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого компонента движения группы кораблей
            foreach (int sGMovingEntity in sGMovingFilter.Value)
            {
                //Берём компонент движения 
                ref CSGMoving sGMoving = ref sGMovingPool.Value.Get(sGMovingEntity);

                //Если список точек пути пуст
                if (sGMoving.pathPoints.Count == 0)
                {
                    //Берём компонент группы кораблей
                    ref CShipGroup shipGroup = ref shipGroupPool.Value.Get(sGMovingEntity);

                    //Переводим группу кораблей в режим ожидания
                    shipGroup.movingMode = Ship.ShipGroupMovingMode.Idle;

                    //Удаляем компонент движения 
                    sGMovingPool.Value.Del(sGMovingEntity);
                }
            }
        }
    }
}