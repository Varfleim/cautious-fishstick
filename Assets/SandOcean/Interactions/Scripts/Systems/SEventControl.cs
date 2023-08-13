
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;
using SandOcean.Diplomacy;

namespace SandOcean
{
    public class SEventControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //События игры
        readonly EcsPoolInject<RGameCreatePanel> gameCreatePanelRequestPool = default;

        //События карты
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //События дипломатии
        readonly EcsFilterInject<Inc<EOrganizationNewCreated>> organizationNewCreatedEventFilter = default;
        readonly EcsPoolInject<EOrganizationNewCreated> organizationNewCreatedEventPool = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecOLAEONewCreatedEventFilter = default;
        readonly EcsPoolInject<EEcORAEONewCreated> ecOLAEONewCreatedEventPool = default;

        //Общие события
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameRequestFilter = default;

        readonly EcsPoolInject<EcsGroupSystemState> ecsGroupSystemStatePool = default;


        //Данные
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса начала новой игры
            foreach (int eventEntity in startNewGameRequestFilter.Value)
            {
                //Запрашиваем выключение группы систем "NewGame"
                EcsGroupSystemStateEvent("NewGame", false);

                world.Value.DelEntity(eventEntity);
            }

            //Для каждого события создания новой организации
            foreach (int eventEntity in organizationNewCreatedEventFilter.Value)
            {
                //Берём событие
                ref EOrganizationNewCreated eventComp = ref organizationNewCreatedEventPool.Value.Get(eventEntity);

                //Запрашиваем создание обзорной панели ORAEO
                GameCreatePanelRequest(
                    CreatingPanelType.ORAEOBriefInfoPanel,
                    eventComp.organizationPE);
            }

            //Для каждого события создания нового EcOLAEO
            foreach (int eventEntity in ecOLAEONewCreatedEventFilter.Value)
            {
                //Берём событие
                ref EEcORAEONewCreated eventComp = ref ecOLAEONewCreatedEventPool.Value.Get(eventEntity);

                //Если активен режим расстояния
                if (inputData.Value.mapMode == MapMode.Distance)
                {
                    //Если организация-владелец OLAEO - организация игрока
                    if (eventComp.organizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                    {
                        //Если запросов смены режима карты нет
                        if (changeMapModeRequestFilter.Value.GetEntitiesCount() == 0)
                        {
                            //То запрашиваем обновление режима карты расстояния
                            MapChangeModeRequest(MapMode.Distance);
                        }
                    }
                }
            }
        }

        void GameCreatePanelRequest(
            CreatingPanelType creatingPanelType,
            EcsPackedEntity ownerOrganizationPE)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания панели в игре
            int requestEntity = world.Value.NewEntity();
            ref RGameCreatePanel requestComp = ref gameCreatePanelRequestPool.Value.Add(requestEntity);

            //Заполняем основные данные запроса
            requestComp = new(
                creatingPanelType,
                ownerOrganizationPE);
        }

        void MapChangeModeRequest(
            MapMode mapMode)
        {
            //Создаём новую сущность и назначаем ей компонент запроса смены режима карты
            int requestEntity = world.Value.NewEntity();
            ref RChangeMapMode requestComp = ref changeMapModeRequestPool.Value.Add(requestEntity);

            //Указываем требуемый режим карты
            requestComp.mapMode = mapMode;
        }

        void EcsGroupSystemStateEvent(
            string systemGroupName,
            bool systemGroupState)
        {
            //Создаём новую сущность и назначаем ей компонент события смены состояния группы систем
            int eventEntity = world.Value.NewEntity();
            ref EcsGroupSystemState eventComp = ref ecsGroupSystemStatePool.Value.Add(eventEntity);

            //Указываем название группы систем
            eventComp.Name = systemGroupName;
            //Указываем состояние группы систем
            eventComp.State = systemGroupState;
        }
    }
}