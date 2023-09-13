
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map;
using SandOcean.Organization;
using SandOcean.UI.GameWindow.Object.Region.Events;

namespace SandOcean.AEO.RAEO
{
    public class SRAEOChangeOwner : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Объекты карты
        //readonly EcsPoolInject<CMapObject> mapObjectPool = default;

        //Карта
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;

        readonly EcsPoolInject<CEconomicORAEO> economicORAEOPool = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<SRORAEOAction, CExplorationORAEO>> oRAEOActionSelfRequestFilter = default;
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;

        readonly EcsPoolInject<SRRefreshRAEOObjectPanel> refreshRAEOObjectPanelSRPool = default;

        //Общие события
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;


        //Данные
        //readonly EcsCustomInject<Ether.MapData> mapData = default;

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

            //Создаём событие, сообщающее о создании нового EcORAEO
            ObjectNewCreatedEvent(ecORAEO.selfPE, ObjectNewCreatedType.EcORAEO);
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