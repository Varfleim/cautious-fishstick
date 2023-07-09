
using UnityEngine;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map;

namespace SandOcean.Ship
{
    public class SShipGroupCreating : IEcsRunSystem
	{
		//����
		readonly EcsWorldInject world = default;

		//������� �����
		readonly EcsPoolInject<CMapObject> mapObjectPool = default;

		//������
		readonly EcsPoolInject<CHexRegion> regionPool = default;

		//�������
		readonly EcsPoolInject<CShipGroup> shipGroupPool = default;

		//������� ��������
		readonly EcsFilterInject<Inc<EShipGroupCreating>> shipGroupCreatingSpaceEventFilter = default;
		readonly EcsPoolInject<EShipGroupCreating> shipGroupCreatingSpaceEventPool = default;

		//������
		readonly EcsCustomInject<StaticData> staticData = default;
		readonly EcsCustomInject<SceneData> sceneData = default;
		//readonly EcsCustomInject<ContentData> contentData = default;

		public void Run(IEcsSystems systems)
		{
			//��� ������� ������� �������� ������ ��������
			foreach (int shipGroupCreatingEventEntity
                in shipGroupCreatingSpaceEventFilter.Value)
            {
				//���� ��������� ������� �������� ������ ��������
				ref EShipGroupCreating shipGroupCreatingEvent
					= ref shipGroupCreatingSpaceEventPool.Value.Get(shipGroupCreatingEventEntity);

				//���� ��������� ������������� �������
				shipGroupCreatingEvent.parentRegionPE.Unpack(world.Value, out int parentRegionEntity);
				ref CHexRegion parentRegion = ref regionPool.Value.Get(parentRegionEntity);

				//������ ������ �������� �� �����
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
			//������ ����� �������� � ��������� �� ���������� ������ �������� � MO
			int shipGroupEntity = world.Value.NewEntity();
			ref CShipGroup shipGroup = ref shipGroupPool.Value.Add(shipGroupEntity);
			ref CMapObject shipGroupMO = ref mapObjectPool.Value.Add(shipGroupEntity);

			//������ ������ ������ ��������
			GOShipGroup shipGroupGO = GameObject.Instantiate(staticData.Value.shipGroupPrefab);

			//��������� �������� ������ ������ ��������
			shipGroup = new(
				world.Value.PackEntity(shipGroupEntity),
				shipGroupCreatingEvent.ownerOrganizationPE,
				parentRegion.selfPE,
				parentRegion.Position);//shipGroupCreatingEvent.position);

			//���������, ��� ������ �������� ��������� � �����
			shipGroup.dockingMode = ShipGroupDockingMode.Ether;

			//��������� �������� ������ MO ������ ��������
			shipGroupMO = new(
				shipGroup.selfPE,
				MapObjectType.ShipGroup,
				shipGroupGO.shipGroupTransform,
				shipGroupGO.shipGroupMeshRenderer);

			//����������� GO ������ �������� � ������������ �������
			shipGroupMO.Transform.SetParent(sceneData.Value.coreObject, false);
			
			//���������� GO �� ��������������� �������
			shipGroupMO.LocalPosition = shipGroupCreatingEvent.position;

			//��������� GO ��� PE
			shipGroupGO.gOMapObject.mapObjectPE = shipGroup.selfPE;

			//������� ������ �������� � ������ ����� �������� �������
			parentRegion.shipGroups.AddLast(shipGroup.selfPE);
        }
	}
}