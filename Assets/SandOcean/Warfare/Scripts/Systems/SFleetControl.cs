
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.Organization;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.Ship;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.Warfare.Fleet
{
    public class SFleetControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Организации
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Военное дело
        readonly EcsPoolInject<CFleet> fleetPool = default;
        readonly EcsPoolInject<CReserveFleet> reserveFleetPool = default;
        readonly EcsPoolInject<CFleetPMissionSearch> fleetPatrolMissionSearchPool = default;
        readonly EcsPoolInject<CFleetTMissionStrikeGroup> fleetTargetMissionStrikeGroupPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        readonly EcsPoolInject<CTaskForcePMissionSearch> taskForcePatrolMissionSearchPool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;
        readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> taskForceTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;

        readonly EcsFilterInject<Inc<CTFTemplateDisplayedSummaryPanel>> tFTemplateDisplayedSummaryPanelFilter = default;
        readonly EcsPoolInject<CTFTemplateDisplayedSummaryPanel> tFTemplateDisplayedSummaryPanelPool = default;


        //События военного дела
        readonly EcsFilterInject<Inc<RFleetCreating>> fleetCreatingRequestFilter = default;
        readonly EcsPoolInject<RFleetCreating> fleetCreatingRequestPool = default;

        readonly EcsPoolInject<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<RTaskForceCreating>> taskForceCreatingRequestFilter = default;
        readonly EcsPoolInject<RTaskForceCreating> taskForceCreatingRequestPool = default;

        readonly EcsFilterInject<Inc<RTaskForceAction>> taskForceActionRequestFilter = default;
        readonly EcsPoolInject<RTaskForceAction> taskForceActionRequestPool = default;

        readonly EcsPoolInject<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool = default;

        //Общие события
        readonly EcsPoolInject<SRObjectRefreshUI> objectRefreshUISelfRequestPool = default;

        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса создания флота
            foreach (int requestEntity in fleetCreatingRequestFilter.Value)
            {
                //Берём запрос
                ref RFleetCreating requestComp = ref fleetCreatingRequestPool.Value.Get(requestEntity);

                //Создаём новый флот
                FleetCreate(ref requestComp);

                world.Value.DelEntity(requestEntity);
            }

            //Для каждого запроса создания оперативной группы
            foreach (int requestEntity in taskForceCreatingRequestFilter.Value)
            {
                //Берём запрос
                ref RTaskForceCreating requestComp = ref taskForceCreatingRequestPool.Value.Get(requestEntity);

                //Создаём новую группу
                TaskForceCreate(ref requestComp);

                world.Value.DelEntity(requestEntity);
            }

            //Для каждого запроса действия оперативной группы
            foreach (int requestEntity in taskForceActionRequestFilter.Value)
            {
                //Берём запрос
                ref RTaskForceAction requestComp = ref taskForceActionRequestPool.Value.Get(requestEntity);

                //Берём группу
                requestComp.taskForcePE.Unpack(world.Value, out int taskForceEntity);
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                //Если запрашивается изменение миссии группы
                if (requestComp.actionType >= TaskForceActionType.ChangeMissionSearch 
                    && requestComp.actionType <= TaskForceActionType.ChangeMissionHold)
                {
                    //Изменяем миссию группы
                    TaskForceChangeMission(
                        ref taskForce, 
                        requestComp.actionType);
                }
                //Если запрашивается изменение шаблона группы
                if(requestComp.actionType == TaskForceActionType.ChangeTemplate)
                {
                    //Изменяем шаблон группы
                    TaskForceChangeTemplate(
                        ref taskForce,
                        requestComp.template);
                }

                world.Value.DelEntity(requestEntity);
            }

            //Для каждого запроса создания шаблона оперативной группы
            foreach (int requestEntity in tFTemplateCreatingRequestFilter.Value)
            {
                //Берём запрос
                ref RTFTemplateCreating requestComp = ref tFTemplateCreatingRequestPool.Value.Get(requestEntity);

                //Если запрашивается обновление шаблона
                if (requestComp.isUpdate == true)
                {
                    //Обновляем шаблон группы
                    TFTemplateUpdate(ref requestComp);
                }
                //Иначе
                else
                {
                    //Создаём новый шаблон группы
                    TFTemplateCreate(ref requestComp);
                }

                world.Value.DelEntity(requestEntity);
            }

            //Для каждого запроса действия шаблона оперативной группы
            foreach (int requestEntity in tFTemplateActionRequestFilter.Value)
            {
                //Берём запрос
                ref RTFTemplateAction requestComp = ref tFTemplateActionRequestPool.Value.Get(requestEntity);

                //Если запрашивается удаление шаблона
                if (requestComp.requestType == TFTemplateActionType.Delete)
                {
                    //Удаляем шаблон группы
                    TFTemplateDelete(ref requestComp);
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        #region Fleet
        void FleetCreate(
            ref RFleetCreating requestComp)
        {
            //Берём родительскую организацию флота
            requestComp.parentOrganizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Создаём новую сущность и назначаем ей компонент флота
            int fleetEntity = world.Value.NewEntity();
            ref CFleet fleet = ref fleetPool.Value.Add(fleetEntity);

            //Заполняем основные данные флота
            fleet = new(
                world.Value.PackEntity(fleetEntity),
                organization.selfPE,
                organization.defaultReserveFleetPE);

            //Если запрашивается создание резервного флота
            if (requestComp.isReserve == true)
            {
                //Досоздаём флот как резервный
                FleetReserveCreate(
                    ref organization,
                    fleetEntity, ref fleet);
            }
            //Иначе
            else
            {
                //Заносим флот в список организации
                organization.ownedFleets.Add(fleet.selfPE);

                //Берём базовый резервный флот организации
                organization.defaultReserveFleetPE.Unpack(world.Value, out int reserveFleetEntity);
                ref CReserveFleet reserveFleet = ref reserveFleetPool.Value.Get(reserveFleetEntity);

                //Заносим флот в список резервного флота
                reserveFleet.ownedFleetPEs.Add(fleet.selfPE);
            }

            //Создаём событие, сообщающее о создании нового флота
            ObjectNewCreatedEvent(fleet.selfPE, ObjectNewCreatedType.Fleet);
        }

        void FleetReserveCreate(
            ref COrganization organization,
            int fleetEntity, ref CFleet fleet)
        {
            //Назначаем флоту компонент резервного флота
            ref CReserveFleet reserveFleet = ref reserveFleetPool.Value.Add(fleetEntity);

            //Заполняем основные данные флота
            reserveFleet = new(0);

            //Если у организации ещё нет базового резервного флота
            if (organization.defaultReserveFleetPE.Unpack(world.Value, out int reserveFleetEntity) == false)
            {
                //Указываем данный флот как базовый резервный
                organization.defaultReserveFleetPE = fleet.selfPE;
            }

            //Заносим флот в список резервных флотов организации
            organization.reserveFleetPEs.Add(fleet.selfPE);

            //Запрашиваем создание оперативной группы без шаблона, которая будет основной группой данного флота
            TaskForceCreatingRequest(ref fleet);
        }
        #endregion

        #region TaskForce
        void TaskForceCreate(
            ref RTaskForceCreating requestComp)
        {
            //Берём родительский флот оперативной группы
            requestComp.ownerFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //Создаём новую сущность и назначаем ей компонент оперативной группы
            int taskForceEntity = world.Value.NewEntity();
            ref CTaskForce taskForce = ref taskForcePool.Value.Add(taskForceEntity);

            //Заполняем основные данные группы
            taskForce = new(
                world.Value.PackEntity(taskForceEntity),
                fleet.selfPE);

            taskForce.rand = UnityEngine.Random.Range(0, 50);

            //Заносим группу в список флота
            fleet.ownedTaskForcePEs.Add(taskForce.selfPE);
            //Создаём новый запрос пополнения и заносим его в данные группы и в данные флота
            DTFReinforcementRequest reinforcementRequest = new();
            taskForce.reinforcementRequest = reinforcementRequest;
            fleet.tFReinforcementRequests.Add(reinforcementRequest);

            //Если группа имеет шаблон
            if (requestComp.template != null)
            {
                //Изменяем шаблон группы
                TaskForceChangeTemplate(
                    ref taskForce,
                    requestComp.template);
            }

            //Изменяем миссию группы
            TaskForceChangeMission(
                ref taskForce,
                TaskForceActionType.ChangeMissionHold);

            //Запрашиваем обновление интерфейса группы
            TaskForceFunctions.RefreshTaskForceUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                objectRefreshUISelfRequestPool.Value,
                ref taskForce);

            //Создаём событие, сообщающее о создании новой группы
            ObjectNewCreatedEvent(
                taskForce.selfPE, 
                ObjectNewCreatedType.TaskForce);

            for (int a = 0; a < taskForce.ships.Count; a++)
            {
                UnityEngine.Debug.LogWarning(taskForce.ships[a].shipType.ObjectName);
            }
        }

        #region Missions
        void TaskForceChangeMission(
            ref CTaskForce taskForce,
            TaskForceActionType actionType)
        {
            //Если запрашивается активация миссии поиска оперативной группы
            if (actionType == TaskForceActionType.ChangeMissionSearch)
            {
                //Меняем миссию группы на поиск
                TaskForceChangeMissionSearch(ref taskForce);

                taskForce.activeMissionType = TaskForceMissionType.PatrolSearchMission;
            }
            //Иначе, если запрашивается активация миссии ударной группы
            else if (actionType == TaskForceActionType.ChangeMissionStrikeGroup)
            {
                //Меняем миссию группы на ударную группу
                TaskForceChangeMissionStrikeGroup(ref taskForce);

                taskForce.activeMissionType = TaskForceMissionType.TargetMissionStrikeGroup;
            }
            //Иначе, если запрашивается активация миссии удержания группы
            else if (actionType == TaskForceActionType.ChangeMissionHold)
            {
                taskForce.activeMissionType = TaskForceMissionType.HoldMission;
            }

            UnityEngine.Debug.LogWarning(taskForce.rand + " ! " + taskForce.activeMissionType);
        }

        void TaskForceChangeMissionSearch(
            ref CTaskForce taskForce)
        {
            //Берём родительский флот оперативной группы
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //Если флот не имеет компонента поиска
            if (fleetPatrolMissionSearchPool.Value.Has(fleetEntity) == false)
            {
                //Назначаем флоту компонент миссии поиска
                ref CFleetPMissionSearch pMissionSearch = ref fleetPatrolMissionSearchPool.Value.Add(fleetEntity);

                //Заполняем основные данные компонента миссии
                pMissionSearch = new(0);
            }

            //Берём компонент миссии поиска
            ref CFleetPMissionSearch fleetPMissionSearch = ref fleetPatrolMissionSearchPool.Value.Get(fleetEntity);

            //Берём сущность группы и назначаем ей основную патрульную миссию и миссию поиска
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);
            ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool.Value.Add(taskForceEntity);
            ref CTaskForcePMissionSearch tFPMissionSearch = ref taskForcePatrolMissionSearchPool.Value.Add(taskForceEntity);

            //Заполняем основные данные компонента патрульной миссии
            tFPatrolMission = new(0);

            //Заполняем основные данные компонента миссии поиска
            tFPMissionSearch = new(0);

            //Заносим группу в список групп, занятых миссией поиска
            fleetPMissionSearch.activeTaskForcePEs.Add(taskForce.selfPE);

            if (fleet.fleetRegions.Count > 0)
            {
                taskForce.currentRegionPE = fleet.fleetRegions[UnityEngine.Random.Range(0, fleet.fleetRegions.Count)].regionPE;
            }
        }

        void TaskForceChangeMissionStrikeGroup(
            ref CTaskForce taskForce)
        {
            //Берём родительский флот оперативной группы
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //Если флот не имеет компонента миссии ударной группы
            if (fleetTargetMissionStrikeGroupPool.Value.Has(fleetEntity) == false)
            {
                //Назначаем флоту компонент миссии ударной группы
                ref CFleetTMissionStrikeGroup tMissionStrikeGroup = ref fleetTargetMissionStrikeGroupPool.Value.Add(fleetEntity);

                //Заполняем основные данные компонента миссии
                tMissionStrikeGroup = new(0);
            }

            //Берём компонент миссии ударной группы
            ref CFleetTMissionStrikeGroup fleetTMissionStrikeGroup = ref fleetTargetMissionStrikeGroupPool.Value.Get(fleetEntity);

            //Берём сущность группы и назначаем ей основную целевую миссию и миссию ударной группы
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);
            ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool.Value.Add(taskForceEntity);
            ref CTaskForceTMissionStrikeGroup tFTMissionStrikeGroup = ref taskForceTargetMissionStrikeGroupPool.Value.Add(taskForceEntity);

            //Заполняем основные данные компонента целевой миссии
            tFTargetMission = new(0);

            //Заполняем основные данные компонента миссии ударной группы
            tFTMissionStrikeGroup = new(0);

            //Заносим группу в список групп, занятых миссией ударной группы
            fleetTMissionStrikeGroup.activeTaskForcePEs.Add(taskForce.selfPE);

            if (fleet.fleetRegions.Count > 0)
            {
                taskForce.currentRegionPE = fleet.fleetRegions[UnityEngine.Random.Range(0, fleet.fleetRegions.Count)].regionPE;
            }
        }
        #endregion

        void TaskForceChangeTemplate(
            ref CTaskForce taskForce,
            DTFTemplate template)
        {
            //Если у оперативной группы был шаблон
            if (taskForce.template != null)
            {
                //Берём текущий шаблон группы 
                DTFTemplate currentTemplate = taskForce.template;

                //Удаляем оперативную группу из списка в шаблоне
                currentTemplate.taskForces.Remove(taskForce.selfPE);

                //Удаляем ссылку на шаблон из данных группы
                taskForce.template = null;
            }

            //Заносим группу в список в новом шаблоне
            template.taskForces.Add(taskForce.selfPE);

            //Даём группе ссылку на шаблон
            taskForce.template = template;

            //Запрашиваем проверку пополнения
            TaskForceFunctions.ReinforcementCheckSelfRequest(
                world.Value,
                reserveFleetReinforcementCheckSelfRequestPool.Value,
                fleetPool.Value,
                taskForceReinforcementCheckSelfRequestPool.Value, 
                ref taskForce);

            //Запрашиваем обновление интерфейса группы
            TaskForceFunctions.RefreshTaskForceUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                objectRefreshUISelfRequestPool.Value, 
                ref taskForce);
        }

        void TaskForceCreatingRequest(
            ref CFleet fleet,
            DTFTemplate template = null)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания оперативной группы
            int requestEntity = world.Value.NewEntity();
            ref RTaskForceCreating requestComp = ref taskForceCreatingRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            requestComp = new(
                fleet.selfPE,
                template);
        }
        #endregion

        #region TFTemplate
        readonly EcsFilterInject<Inc<RTFTemplateCreating>> tFTemplateCreatingRequestFilter = default;
        readonly EcsPoolInject<RTFTemplateCreating> tFTemplateCreatingRequestPool = default;
        void TFTemplateCreate(
            ref RTFTemplateCreating requestComp)
        {
            //Берём организацию
            requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Создаём новый шаблон оперативной группы и заполняем основные данные
            DTFTemplate template = new(
                requestComp.tFTemplateName);

            //Изменяем размер массива типов кораблей в шаблоне
            template.shipTypes = new DCountedShipType[requestComp.shipTypes.Count];

            //Для каждого типа корабля в списке в запросе
            for (int a = 0; a < requestComp.shipTypes.Count; a++)
            {
                //Заносим тип корабля в массив в шаблоне
                template.shipTypes[a] = new(
                    requestComp.shipTypes[a].shipType,
                    requestComp.shipTypes[a].shipCount);
            }

            //Заносим новый шаблон в список в данных организации
            OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Add(template);
        }

        void TFTemplateUpdate(
            ref RTFTemplateCreating requestComp)
        {
            //Берём организацию
            requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Для каждого шаблона оперативной группы
            for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
            {
                //Если шаблон совпадает с запрошенным для изменения
                if (OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a] == requestComp.updatingTemplate)
                {
                    //Берём шаблон
                    DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                    //Перезаписываем данные шаблона
                    template.selfName = requestComp.tFTemplateName;

                    //Изменяем размер массива типов кораблей в шаблоне
                    template.shipTypes = new DCountedShipType[requestComp.shipTypes.Count];

                    //Для каждого типа корабля в списке в запросе
                    for (int b = 0; b < requestComp.shipTypes.Count; b++)
                    {
                        //Заносим тип корабля в массив в шаблоне
                        template.shipTypes[b] = new(
                            requestComp.shipTypes[b].shipType,
                            requestComp.shipTypes[b].shipCount);
                    }

                    //Для каждой группы, использующей данный шаблон
                    for (int b = 0; b < template.taskForces.Count; b++)
                    {
                        //Берём группу
                        template.taskForces[b].Unpack(world.Value, out int taskForceEntity);
                        ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                        //Запрашиваем проверку пополнения
                        TaskForceFunctions.ReinforcementCheckSelfRequest(
                            world.Value,
                            reserveFleetReinforcementCheckSelfRequestPool.Value,
                            fleetPool.Value,
                            taskForceReinforcementCheckSelfRequestPool.Value, 
                            ref taskForce);
                    }

                    break;
                }
            }
        }

        readonly EcsFilterInject<Inc<RTFTemplateAction>> tFTemplateActionRequestFilter = default;
        readonly EcsPoolInject<RTFTemplateAction> tFTemplateActionRequestPool = default;
        void TFTemplateDelete(
            ref RTFTemplateAction requestComp)
        {
            //Берём организацию
            requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //Берём удаляемый шаблон
            DTFTemplate template = requestComp.template;

            //Для каждого шаблона оперативной группы, имеющего отображаемую обзорную панель
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //Берём компонент отображаемой обзорной панели
                ref CTFTemplateDisplayedSummaryPanel tFTemplateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //Если панель компонент отображает удаляемый шаблон
                if (tFTemplateDisplayedSummaryPanel.CheckTemplate(template))
                {
                    //Запрашиваем удаление интерфейса
                    TaskForceFunctions.RefreshTFTemplateUISelfRequest(
                        world.Value,
                        tFTemplateDisplayedSummaryPanelPool.Value,
                        objectRefreshUISelfRequestPool.Value,
                        ref tFTemplateDisplayedSummaryPanel,
                        RefreshUIType.Delete);

                    //Выходим из цикла, поскольку нужная панель найдена
                    break;
                }
            }

            //Удаляем шаблон из данных организации
            OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Remove(template);
        }
        #endregion

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