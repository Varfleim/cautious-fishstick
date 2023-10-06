
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.AEO.RAEO;

namespace SandOcean.Economy.Building
{
    public class SBuildingControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //���������
        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        readonly EcsPoolInject<CBuilding> buildingPool = default;
        readonly EcsPoolInject<CBuildingDisplayedSummaryPanel> buildingDisplayedSummaryPanelPool = default;

        //������� ���������
        readonly EcsFilterInject<Inc<RBuildingCreating>> buildingCreatingRequestFilter = default;
        readonly EcsPoolInject<RBuildingCreating> buildingCreatingRequestPool = default;

        //����� �������
        readonly EcsPoolInject<SRObjectRefreshUI> objectRefreshUISelfRequestPool = default;

        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ����������
            foreach (int requestEntity in buildingCreatingRequestFilter.Value)
            {
                //���� ������
                ref RBuildingCreating requestComp = ref buildingCreatingRequestPool.Value.Get(requestEntity);

                //������ ����� ����������
                BuildingCreate(ref requestComp);

                world.Value.DelEntity(requestEntity);
            }
        }

        void BuildingCreate(
            ref RBuildingCreating requestComp)
        {
            //���� ������������ EcORAEO ����������
            requestComp.parentORAEOPE.Unpack(world.Value, out int oRAEOEntity);
            ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

            //������ ����� �������� � ��������� �� ��������� ����������
            int buildingEntity = world.Value.NewEntity();
            ref CBuilding building = ref buildingPool.Value.Add(buildingEntity);

            //��������� �������� ������ ����������
            building = new(
                world.Value.PackEntity(buildingEntity), requestComp.buildingType,
                ecORAEO.selfPE);

            //������� ���������� � ������ EcORAEO
            ecORAEO.ownedBuildings.Add(new(building.selfPE));

            //����������� ���������� ���������� ����������
            BuildingFunctions.RefreshBuildingUISelfRequest(
                world.Value,
                buildingDisplayedSummaryPanelPool.Value,
                objectRefreshUISelfRequestPool.Value,
                ref building);

            //������ �������, ���������� � �������� ������ ����������
            ObjectNewCreatedEvent(building.selfPE, ObjectNewCreatedType.Building);
        }

        void ObjectNewCreatedEvent(
            EcsPackedEntity objectPE, ObjectNewCreatedType objectNewCreatedType)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ �������
            int eventEntity = world.Value.NewEntity();
            ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Add(eventEntity);

            //��������� ������ �������
            eventComp = new(objectPE, objectNewCreatedType);
        }
    }
}