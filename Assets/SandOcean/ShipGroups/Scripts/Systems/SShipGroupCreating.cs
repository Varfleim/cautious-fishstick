
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map;

namespace SandOcean.Ship
{
    public class SShipGroupCreating : IEcsRunSystem
	{
		//Миры
		readonly EcsWorldInject world = default;

		//Объекты карты
		readonly EcsPoolInject<CMapObject> mapObjectPool = default;

		//Космос
		readonly EcsPoolInject<CHexRegion> regionPool = default;

		//Корабли
		readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

		//События кораблей
		readonly EcsFilterInject<Inc<EShipGroupCreating>> shipGroupCreatingSpaceEventFilter = default;
		readonly EcsPoolInject<EShipGroupCreating> shipGroupCreatingSpaceEventPool = default;

		//Данные
		readonly EcsCustomInject<StaticData> staticData = default;
		readonly EcsCustomInject<SceneData> sceneData = default;
		//readonly EcsCustomInject<ContentData> contentData = default;

		public void Run(IEcsSystems systems)
		{
			//Для каждого события создания группы кораблей
			foreach (int shipGroupCreatingEventEntity
                in shipGroupCreatingSpaceEventFilter.Value)
            {
				//Берём компонент события создания группы кораблей
				ref EShipGroupCreating shipGroupCreatingEvent
					= ref shipGroupCreatingSpaceEventPool.Value.Get(shipGroupCreatingEventEntity);

				//Берём компонент родительского региона
				shipGroupCreatingEvent.parentRegionPE.Unpack(world.Value, out int parentRegionEntity);
				ref CHexRegion parentRegion = ref regionPool.Value.Get(parentRegionEntity);

				//Создаём группу кораблей на карте
				ShipGroupCreateOnMap(
					ref parentRegion,
					ref shipGroupCreatingEvent);

				world.Value.DelEntity(shipGroupCreatingEventEntity);
            }
		}

		void ShipGroupCreateOnMap(
            ref CHexRegion parentRegion,
            ref EShipGroupCreating shipGroupCreatingEvent)
        {
			//Создаём новую сущность и назначаей ей компоненты группы кораблей и MO
			int shipGroupEntity = world.Value.NewEntity();
			ref CShipGroup shipGroup = ref shipGroupPool.Value.Add(shipGroupEntity);
			ref CMapObject shipGroupMO = ref mapObjectPool.Value.Add(shipGroupEntity);

			//Создаём объект группы кораблей
			GOShipGroup shipGroupGO = GameObject.Instantiate(staticData.Value.shipGroupPrefab);

			//Заполняем основные данные группы кораблей
			shipGroup = new(
				world.Value.PackEntity(shipGroupEntity),
				shipGroupCreatingEvent.ownerOrganizationPE,
				parentRegion.selfPE,
				parentRegion.Position);//shipGroupCreatingEvent.position);

			//Указываем, что группа кораблей находится в эфире
			shipGroup.dockingMode = ShipGroupDockingMode.Ether;

			//Заполняем основные данные MO группы кораблей
			shipGroupMO = new(
				shipGroup.selfPE,
				MapObjectType.ShipGroup,
				shipGroupGO.shipGroupTransform,
				shipGroupGO.shipGroupMeshRenderer);

			//Прикрепляем GO группы кораблей к центральному объекту
			shipGroupMO.Transform.SetParent(sceneData.Value.coreObject, false);
			
			//Перемещаем GO на соответствующую позицию
			shipGroupMO.LocalPosition = shipGroupCreatingEvent.position;

			//Назначаем GO его PE
			shipGroupGO.gOMapObject.mapObjectPE = shipGroup.selfPE;

			//Заносим группу кораблей в список групп кораблей региона
			parentRegion.shipGroups.AddLast(shipGroup.selfPE);
        }
	}
}