
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map;
using SandOcean.Diplomacy;

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

        readonly EcsPoolInject<EEcORAEONewCreated> ecORAEONewCreatedEventPool = default;

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

                    //Увеличиваем видимость региона
                    //region.IncreaseVisibility();
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

            //Для каждого события смены владельца
            /*foreach (int eventEntity in oRAEOActionSelfRequestFilter.Value)
            {
                //Берём компонент события смены владельца
                ref SRORAEOAction rAEOChangeOwnerEvent = ref oRAEOActionSelfRequestPool.Value.Get(eventEntity);

                //Берём компонент RAEO
                rAEOChangeOwnerEvent.rAEOPE.Unpack(world.Value, out int rAEOEntity);
                ref CLocarAEO rAEO = ref locarAEOPool.Value.Get(rAEOEntity);

                //Берём компонент организации
                rAEOChangeOwnerEvent.organizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                //Если событие запрашивает колонизацию RAEO
                if (rAEOChangeOwnerEvent.actionType == ORAEOActionType.Colonization)
                {
                    //Колонизируем RAEO
                    RAEOColonize(
                        ref rAEO,
                        ref organization);
                }

                //Если сущность RAEO не имеет компонента самозапроса обновления панели объекта
                if (refreshRAEOObjectPanelSRPool.Value.Has(rAEOEntity) == false)
                {
                    //Назначаем сущности компонент самозапроса обновления панели объекта
                    refreshRAEOObjectPanelSRPool.Value.Add(rAEOEntity);
                }

                world.Value.DelEntity(eventEntity);
            }*/
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
            EcORAEONewCreatedEvent(
                ref organization,
                ref ecORAEO);
        }

        void EcORAEONewCreatedEvent(
            ref COrganization organization,
            ref CEconomicORAEO ecORAEO)
        {
            //Создаём новую сущность и назначаем ей компонент события создания нового EcORAEO
            int eventEntity = world.Value.NewEntity();
            ref EEcORAEONewCreated ecORAEONewCreatedEvent = ref ecORAEONewCreatedEventPool.Value.Add(eventEntity);

            //Заполняем данные события
            ecORAEONewCreatedEvent = new(
                organization.selfPE,
                ecORAEO.selfPE);
        }
    }
}