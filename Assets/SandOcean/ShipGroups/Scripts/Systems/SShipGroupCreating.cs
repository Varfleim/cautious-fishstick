
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
		readonly EcsFilterInject<Inc<RShipGroupCreating>> shipGroupCreatingRequestFilter = default;
		readonly EcsPoolInject<RShipGroupCreating> shipGroupCreatingRequestPool = default;

		//������
		readonly EcsCustomInject<StaticData> staticData = default;
		readonly EcsCustomInject<SceneData> sceneData = default;
		//readonly EcsCustomInject<ContentData> contentData = default;

		public void Run(IEcsSystems systems)
		{
			//��� ������� ������� �������� ������ ��������
			foreach (int shipGroupCreatingRequestEntity in shipGroupCreatingRequestFilter.Value)
            {
				//���� ��������� ������� �������� ������ ��������
				ref RShipGroupCreating shipGroupCreatingRequest = ref shipGroupCreatingRequestPool.Value.Get(shipGroupCreatingRequestEntity);

				//���� ��������� ������������� �������
				shipGroupCreatingRequest.parentRegionPE.Unpack(world.Value, out int parentRegionEntity);
				ref CHexRegion parentRegion = ref regionPool.Value.Get(parentRegionEntity);

				//������ ������ �������� �� �����
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
			//������ ����� �������� � ��������� �� ���������� ������ �������� � MO
			int shipGroupEntity = world.Value.NewEntity();
			ref CShipGroup shipGroup = ref shipGroupPool.Value.Add(shipGroupEntity);
			ref CMapObject shipGroupMO = ref mapObjectPool.Value.Add(shipGroupEntity);

			//������ ������ ������ ��������
			GOShipGroup shipGroupGO = GameObject.Instantiate(staticData.Value.shipGroupPrefab);

			//��������� �������� ������ ������ ��������
			shipGroup = new(
				world.Value.PackEntity(shipGroupEntity),
				shipGroupCreatingRequest.ownerOrganizationPE,
				parentRegion.selfPE,
				parentRegion.centerPoint.ProjectedVector3);//shipGroupCreatingEvent.position);

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
			shipGroupMO.LocalPosition = shipGroupCreatingRequest.position;

			//��������� GO ��� PE
			shipGroupGO.gOMapObject.mapObjectPE = shipGroup.selfPE;

			//������� ������ �������� � ������ ����� �������� �������
			parentRegion.shipGroups.AddLast(shipGroup.selfPE);
        }
	}
}