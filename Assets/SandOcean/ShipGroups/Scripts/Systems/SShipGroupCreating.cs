
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
		readonly EcsFilterInject<Inc<RShipGroupCreating>> shipGroupCreatingRequestFilter = default;
		readonly EcsPoolInject<RShipGroupCreating> shipGroupCreatingRequestPool = default;

		//Данные
		readonly EcsCustomInject<StaticData> staticData = default;
		readonly EcsCustomInject<SceneData> sceneData = default;
		//readonly EcsCustomInject<ContentData> contentData = default;

		public void Run(IEcsSystems systems)
		{
			//Для каждого запроса создания группы кораблей
			foreach (int shipGroupCreatingRequestEntity in shipGroupCreatingRequestFilter.Value)
            {
				//Берём компонент запроса создания группы кораблей
				ref RShipGroupCreating shipGroupCreatingRequest = ref shipGroupCreatingRequestPool.Value.Get(shipGroupCreatingRequestEntity);

				//Берём компонент родительского региона
				shipGroupCreatingRequest.parentRegionPE.Unpack(world.Value, out int parentRegionEntity);
				ref CHexRegion parentRegion = ref regionPool.Value.Get(parentRegionEntity);

				//Создаём группу кораблей на карте
				ShipGroupCreateOnMap(
					ref parentRegion,
					ref shipGroupCreatingRequest);

				world.Value.DelEntity(shipGroupCreatingRequestEntity);
            }
		}

		void ShipGroupCreateOnMap(
            ref CHexRegion parentRegion,
            ref RShipGroupCreating shipGroupCreatingRequest)
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
				shipGroupCreatingRequest.ownerOrganizationPE,
				parentRegion.selfPE,
				parentRegion.centerPoint.ProjectedVector3);//shipGroupCreatingEvent.position);

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
			shipGroupMO.LocalPosition = shipGroupCreatingRequest.position;

			//Назначаем GO его PE
			shipGroupGO.gOMapObject.mapObjectPE = shipGroup.selfPE;

			//Заносим группу кораблей в список групп кораблей региона
			parentRegion.shipGroups.AddLast(shipGroup.selfPE);
        }
	}
}