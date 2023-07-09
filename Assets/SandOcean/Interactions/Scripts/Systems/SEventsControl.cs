
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;

namespace SandOcean
{
    public class SEventsControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //События карты
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecOLAEONewCreatedEventFilter = default;
        readonly EcsPoolInject<EEcORAEONewCreated> ecOLAEONewCreatedEventPool = default;

        //Данные
        EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события создания нового EcOLAEO
            foreach (int eventEntity in ecOLAEONewCreatedEventFilter.Value)
            {
                //Берём компонент события
                ref EEcORAEONewCreated ecOLAEONewCreatedEvent = ref ecOLAEONewCreatedEventPool.Value.Get(eventEntity);

                //Если активен режим расстояния
                if (inputData.Value.mapMode == MapMode.Distance)
                {
                    //Если организация-владелец OLAEO - организация игрока
                    if (ecOLAEONewCreatedEvent.organizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
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

        void MapChangeModeRequest(
            MapMode mapMode)
        {
            //Создаём новую сущность и назначаем ей компонент запроса смены режима карты
            int changeMapModeRequestEntity = world.Value.NewEntity();
            ref RChangeMapMode changeMapModeRequest = ref changeMapModeRequestPool.Value.Add(changeMapModeRequestEntity);

            //Указываем требуемый режим карты
            changeMapModeRequest.mapMode = mapMode;
        }
    }
}