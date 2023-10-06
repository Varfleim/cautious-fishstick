
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.AEO.RAEO;

namespace SandOcean.Economy.Building
{
    public class SBuildingControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Экономика
        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        readonly EcsPoolInject<CBuilding> buildingPool = default;
        readonly EcsPoolInject<CBuildingDisplayedSummaryPanel> buildingDisplayedSummaryPanelPool = default;

        //События экономики
        readonly EcsFilterInject<Inc<RBuildingCreating>> buildingCreatingRequestFilter = default;
        readonly EcsPoolInject<RBuildingCreating> buildingCreatingRequestPool = default;

        //Общие события
        readonly EcsPoolInject<SRObjectRefreshUI> objectRefreshUISelfRequestPool = default;

        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса создания сооружения
            foreach (int requestEntity in buildingCreatingRequestFilter.Value)
            {
                //Берём запрос
                ref RBuildingCreating requestComp = ref buildingCreatingRequestPool.Value.Get(requestEntity);

                //Создаём новое сооружение
                BuildingCreate(ref requestComp);

                world.Value.DelEntity(requestEntity);
            }
        }

        void BuildingCreate(
            ref RBuildingCreating requestComp)
        {
            //Берём родительский EcORAEO сооружения
            requestComp.parentORAEOPE.Unpack(world.Value, out int oRAEOEntity);
            ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Get(oRAEOEntity);

            //Создаём новую сущность и назначаем ей компонент сооружения
            int buildingEntity = world.Value.NewEntity();
            ref CBuilding building = ref buildingPool.Value.Add(buildingEntity);

            //Заполняем основные данные сооружения
            building = new(
                world.Value.PackEntity(buildingEntity), requestComp.buildingType,
                ecORAEO.selfPE);

            //Заносим сооружение в список EcORAEO
            ecORAEO.ownedBuildings.Add(new(building.selfPE));

            //Запрашиваем обновление интерфейса сооружения
            BuildingFunctions.RefreshBuildingUISelfRequest(
                world.Value,
                buildingDisplayedSummaryPanelPool.Value,
                objectRefreshUISelfRequestPool.Value,
                ref building);

            //Создаём событие, сообщающее о создании нового сооружения
            ObjectNewCreatedEvent(building.selfPE, ObjectNewCreatedType.Building);
        }

        void ObjectNewCreatedEvent(
            EcsPackedEntity objectPE, ObjectNewCreatedType objectNewCreatedType)
        {
            //Создаём новую сущность и назначаем ей компонент события создания нового объекта
            int eventEntity = world.Value.NewEntity();
            ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Add(eventEntity);

            //Заполняем данные события
            eventComp = new(objectPE, objectNewCreatedType);
        }
    }
}