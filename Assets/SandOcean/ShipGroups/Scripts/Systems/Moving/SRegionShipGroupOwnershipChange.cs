
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map;

namespace SandOcean.Ship.Moving
{
    public class SRegionShipGroupOwnershipChange : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Объекты карты
        readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //Карта
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //readonly EcsFilterInject<Inc<CRegion>> regionFilter = default;
        //readonly EcsPoolInject<CRegion> regionPool = default;

        //Группы кораблей
        readonly EcsPoolInject<CShipGroup> shipGroupPool = default;


        public void Run(IEcsSystems systems)
        {
            //Для каждого региона
            foreach (int regionEntity
                in regionFilter.Value)
            {
                //Берём компонент региона
                ref CHexRegion region
                    = ref regionPool.Value.Get(regionEntity);

                //Для каждой группы кораблей в списке меняющих регион
                for (int a = 0; a < region.ownershipChangeShipGroups.Count; a++)
                {
                    //Берём компонент группы кораблей
                    region.ownershipChangeShipGroups[a].Unpack(world.Value, out int shipGroupEntity);
                    ref CShipGroup shipGroup = ref shipGroupPool.Value.Get(shipGroupEntity);
                    ref CMapObject shipGroupMO = ref mapObjectPool.Value.Get(shipGroupEntity);

                    //Берём компонент региона, в который переходит группа кораблей
                    shipGroup.parentRegionPE.Unpack(world.Value, out int newRegionEntity);
                    ref CHexRegion newRegion = ref regionPool.Value.Get(newRegionEntity);

                    //Заносим группу кораблей в список групп в регионе
                    newRegion.shipGroups.AddLast(shipGroup.selfPE);

                    //Прикрепляем GO группы кораблей к GO региона
                    //shipGroupEO.Transform.SetParent(newRegion.transform, true);
                }
                //Очищаем список 
                region.ownershipChangeShipGroups.Clear();
            }
        }
    }
}