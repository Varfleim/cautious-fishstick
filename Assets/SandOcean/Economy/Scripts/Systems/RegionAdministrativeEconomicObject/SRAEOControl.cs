
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.Map;
using SandOcean.Organization;
using SandOcean.Economy.Building;
using SandOcean.UI.GameWindow.Object.Region.Events;

namespace SandOcean.AEO.RAEO
{
    public class SRAEOControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Карта
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Организации
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Экономика
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;
        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;
        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        //События экономики
        readonly EcsFilterInject<Inc<SRORAEOAction, CExplorationORAEO>> oRAEOActionSelfRequestFilter = default;
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        readonly EcsPoolInject<RBuildingCreating> buildingCreatingRequestPool = default;

        readonly EcsPoolInject<SRRefreshRAEOObjectPanel> refreshRAEOObjectPanelSRPool = default;

        //Общие события
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;


        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого ORAEO, для которого требуется совершить действие
            foreach (int oRAEOEntity in oRAEOActionSelfRequestFilter.Value)
            {
                //Берём компонент события и ExORAEO
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);
                ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Get(oRAEOEntity);

                //Берём компонент родительского RAEO
                exORAEO.parentRAEOPE.Unpack(world.Value, out int rAEOEntity);
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //Если требуется колонизировать RAEO
                if (oRAEOActionSR.actionType == ORAEOActionType.Colonization)
                {
                    //Берём компонент родительской организации
                    exORAEO.organizationPE.Unpack(world.Value, out int organizationEntity);
                    ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                    //Колонизируем RAEO
                    RAEOColonize(
                        ref organization,
                        ref rAEO,
                        ref exORAEO);

                    //Берём компонент региона
                    ref CHexRegion region = ref regionPool.Value.Get(rAEOEntity);
                }

                //Если сущность RAEO не имеет компонента самозапроса обновления панели объекта
                if (refreshRAEOObjectPanelSRPool.Value.Has(rAEOEntity) == false)
                {
                    //Назначаем сущности компонент самозапроса обновления панели объекта
                    refreshRAEOObjectPanelSRPool.Value.Add(rAEOEntity);
                }

                //Удаляем с сущности ORAEO компонент самозапроса 
                oRAEOActionSelfRequestPool.Value.Del(oRAEOEntity);
            }
        }

        void RAEOColonize(
            ref COrganization organization,
            ref CRegionAEO rAEO,
            ref CExplorationORAEO exORAEO)
        {
            //Указываем организацию-владельца RAEO
            rAEO.ownerOrganizationPE = organization.selfPE;
            rAEO.ownerOrganizationIndex = organization.selfIndex;

            //Указываем, что ORAEO имеет экономический компонент
            rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOType = ORAEOType.Economic;

            //Берём сущность ORAEO и назначаем ей компонент EcORAEO
            rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
            ref CEconomicORAEO ecORAEO = ref economicORAEOPool.Value.Add(oRAEOEntity);

            //Заполняем основные данные EcORAEO
            ecORAEO = new(world.Value.PackEntity(oRAEOEntity));

            //Заносим PE ORAEO в список организации
            organization.ownedORAEOPEs.Add(ecORAEO.selfPE);

            //ТЕСТ
            //Создаём запрос создания сооружения нулевого типа
            BuildingCreatingRequest(
                ref ecORAEO,
                contentData.Value.contentSets[0].buildingTypes[0]);

            BuildingCreatingRequest(
                ref ecORAEO,
                contentData.Value.contentSets[0].buildingTypes[1]);
            //ТЕСТ

            //Создаём событие, сообщающее о создании нового EcORAEO
            ObjectNewCreatedEvent(ecORAEO.selfPE, ObjectNewCreatedType.EcORAEO);
        }

        void BuildingCreatingRequest(
            ref CEconomicORAEO ecORAEO,
            DBuildingType buildingType)
        {
            //Создаём новую сущность и назначаем ей запрос создания сооружения
            int requestEntity = world.Value.NewEntity();
            ref RBuildingCreating requestComp = ref buildingCreatingRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                ecORAEO.selfPE,
                buildingType);
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