
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;
using SandOcean.Organization;
using SandOcean.Warfare.Fleet;
using SandOcean.UI.GameWindow.Object;
using SandOcean.Warfare.TaskForce;

namespace SandOcean
{
    public class SEventControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Флоты
        readonly EcsPoolInject<CFleet> fleetPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;

        //События игры
        readonly EcsPoolInject<RGameCreatePanel> gameCreatePanelRequestPool = default;

        //События карты
        //readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //События дипломатии

        //События административно-экономических объектов

        //События флотов

        //Общие события
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameRequestFilter = default;

        readonly EcsFilterInject<Inc<EObjectNewCreated>> objectNewCreatedEventFilter = default;
        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        readonly EcsPoolInject<EcsGroupSystemState> ecsGroupSystemStatePool = default;


        //Данные
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса начала новой игры
            foreach (int eventEntity in startNewGameRequestFilter.Value)
            {
                //Запрашиваем выключение группы систем "NewGame"
                EcsGroupSystemStateEvent("NewGame", false);

                world.Value.DelEntity(eventEntity);
            }

            //Для каждого события создания нового объекта
            foreach (int eventEntity in objectNewCreatedEventFilter.Value)
            {
                //Берём событие
                ref EObjectNewCreated eventComp = ref objectNewCreatedEventPool.Value.Get(eventEntity);

                //Если событие сообщает о создании новой организации
                if (eventComp.objectNewCreatedType == ObjectNewCreatedType.Organization)
                {
                    //Запрашиваем создание обзорной панели ORAEO
                    GameCreatePanelRequest(
                        CreatingPanelType.ORAEOBriefInfoPanel,
                        eventComp.objectPE);
                }
                //Иначе, если событие сообщает о создании нового EcORAEO
                else if (eventComp.objectNewCreatedType == ObjectNewCreatedType.EcORAEO)
                {
                    //Если активен режим расстояния
                    if (inputData.Value.mapMode == MapMode.Distance)
                    {
                        //Если организация-владелец ORAEO - организация игрока
                        /*if (eventComp.organizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                        {
                            //Если запросов смены режима карты нет
                            if (changeMapModeRequestFilter.Value.GetEntitiesCount() == 0)
                            {
                                //То запрашиваем обновление режима карты расстояния
                                MapChangeModeRequest(MapMode.Distance);
                            }
                        }*/
                    }
                }
                //Иначе, если событие сообщает о создании нового флота
                else if (eventComp.objectNewCreatedType == ObjectNewCreatedType.Fleet)
                {
                    //Берём флот
                    eventComp.objectPE.Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //Если активен менеджер флотов
                    if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                    {
                        //Если организация-владелец флота - организация игрока
                        if (fleet.parentOrganizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                        {
                            //Запрашиваем создание обзорной панели флота
                            GameCreatePanelRequest(
                                CreatingPanelType.FleetOverviewPanel,
                                fleet.selfPE);
                        }
                    }
                }
                //Иначе, если событие сообщает о создании новой оперативной группы
                else if (eventComp.objectNewCreatedType == ObjectNewCreatedType.TaskForce)
                {
                    //Берём оперативную группу
                    eventComp.objectPE.Unpack(world.Value, out int taskForceEntity);
                    ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                    //Берём флот-владелец
                    taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
                    ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

                    //Если активен менеджер флотов
                    if (eUI.Value.gameWindow.objectPanel.activeObjectSubpanelType == ObjectSubpanelType.FleetManager)
                    {
                        //Если организация-владелец флота - организация игрока
                        if (fleet.parentOrganizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                        {
                            //Запрашиваем создание обзорной панели оперативной группы
                            GameCreatePanelRequest(
                                CreatingPanelType.TaskForceOverviewPanel,
                                taskForce.selfPE);
                        }
                    }
                }
            }
        }

        void GameCreatePanelRequest(
            CreatingPanelType creatingPanelType,
            EcsPackedEntity objectPE)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания панели в игре
            int requestEntity = world.Value.NewEntity();
            ref RGameCreatePanel requestComp = ref gameCreatePanelRequestPool.Value.Add(requestEntity);

            //Заполняем основные данные запроса
            requestComp = new(
                creatingPanelType,
                objectPE);
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