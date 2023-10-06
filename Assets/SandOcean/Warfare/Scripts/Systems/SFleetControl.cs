
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
        //����
        readonly EcsWorldInject world = default;


        //�����������
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //������� ����
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


        //������� �������� ����
        readonly EcsFilterInject<Inc<RFleetCreating>> fleetCreatingRequestFilter = default;
        readonly EcsPoolInject<RFleetCreating> fleetCreatingRequestPool = default;

        readonly EcsPoolInject<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<RTaskForceCreating>> taskForceCreatingRequestFilter = default;
        readonly EcsPoolInject<RTaskForceCreating> taskForceCreatingRequestPool = default;

        readonly EcsFilterInject<Inc<RTaskForceAction>> taskForceActionRequestFilter = default;
        readonly EcsPoolInject<RTaskForceAction> taskForceActionRequestPool = default;

        readonly EcsPoolInject<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool = default;

        //����� �������
        readonly EcsPoolInject<SRObjectRefreshUI> objectRefreshUISelfRequestPool = default;

        readonly EcsPoolInject<EObjectNewCreated> objectNewCreatedEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� �����
            foreach (int requestEntity in fleetCreatingRequestFilter.Value)
            {
                //���� ������
                ref RFleetCreating requestComp = ref fleetCreatingRequestPool.Value.Get(requestEntity);

                //������ ����� ����
                FleetCreate(ref requestComp);

                world.Value.DelEntity(requestEntity);
            }

            //��� ������� ������� �������� ����������� ������
            foreach (int requestEntity in taskForceCreatingRequestFilter.Value)
            {
                //���� ������
                ref RTaskForceCreating requestComp = ref taskForceCreatingRequestPool.Value.Get(requestEntity);

                //������ ����� ������
                TaskForceCreate(ref requestComp);

                world.Value.DelEntity(requestEntity);
            }

            //��� ������� ������� �������� ����������� ������
            foreach (int requestEntity in taskForceActionRequestFilter.Value)
            {
                //���� ������
                ref RTaskForceAction requestComp = ref taskForceActionRequestPool.Value.Get(requestEntity);

                //���� ������
                requestComp.taskForcePE.Unpack(world.Value, out int taskForceEntity);
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                //���� ������������� ��������� ������ ������
                if (requestComp.actionType >= TaskForceActionType.ChangeMissionSearch 
                    && requestComp.actionType <= TaskForceActionType.ChangeMissionHold)
                {
                    //�������� ������ ������
                    TaskForceChangeMission(
                        ref taskForce, 
                        requestComp.actionType);
                }
                //���� ������������� ��������� ������� ������
                if(requestComp.actionType == TaskForceActionType.ChangeTemplate)
                {
                    //�������� ������ ������
                    TaskForceChangeTemplate(
                        ref taskForce,
                        requestComp.template);
                }

                world.Value.DelEntity(requestEntity);
            }

            //��� ������� ������� �������� ������� ����������� ������
            foreach (int requestEntity in tFTemplateCreatingRequestFilter.Value)
            {
                //���� ������
                ref RTFTemplateCreating requestComp = ref tFTemplateCreatingRequestPool.Value.Get(requestEntity);

                //���� ������������� ���������� �������
                if (requestComp.isUpdate == true)
                {
                    //��������� ������ ������
                    TFTemplateUpdate(ref requestComp);
                }
                //�����
                else
                {
                    //������ ����� ������ ������
                    TFTemplateCreate(ref requestComp);
                }

                world.Value.DelEntity(requestEntity);
            }

            //��� ������� ������� �������� ������� ����������� ������
            foreach (int requestEntity in tFTemplateActionRequestFilter.Value)
            {
                //���� ������
                ref RTFTemplateAction requestComp = ref tFTemplateActionRequestPool.Value.Get(requestEntity);

                //���� ������������� �������� �������
                if (requestComp.requestType == TFTemplateActionType.Delete)
                {
                    //������� ������ ������
                    TFTemplateDelete(ref requestComp);
                }

                world.Value.DelEntity(requestEntity);
            }
        }

        #region Fleet
        void FleetCreate(
            ref RFleetCreating requestComp)
        {
            //���� ������������ ����������� �����
            requestComp.parentOrganizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //������ ����� �������� � ��������� �� ��������� �����
            int fleetEntity = world.Value.NewEntity();
            ref CFleet fleet = ref fleetPool.Value.Add(fleetEntity);

            //��������� �������� ������ �����
            fleet = new(
                world.Value.PackEntity(fleetEntity),
                organization.selfPE,
                organization.defaultReserveFleetPE);

            //���� ������������� �������� ���������� �����
            if (requestComp.isReserve == true)
            {
                //�������� ���� ��� ���������
                FleetReserveCreate(
                    ref organization,
                    fleetEntity, ref fleet);
            }
            //�����
            else
            {
                //������� ���� � ������ �����������
                organization.ownedFleets.Add(fleet.selfPE);

                //���� ������� ��������� ���� �����������
                organization.defaultReserveFleetPE.Unpack(world.Value, out int reserveFleetEntity);
                ref CReserveFleet reserveFleet = ref reserveFleetPool.Value.Get(reserveFleetEntity);

                //������� ���� � ������ ���������� �����
                reserveFleet.ownedFleetPEs.Add(fleet.selfPE);
            }

            //������ �������, ���������� � �������� ������ �����
            ObjectNewCreatedEvent(fleet.selfPE, ObjectNewCreatedType.Fleet);
        }

        void FleetReserveCreate(
            ref COrganization organization,
            int fleetEntity, ref CFleet fleet)
        {
            //��������� ����� ��������� ���������� �����
            ref CReserveFleet reserveFleet = ref reserveFleetPool.Value.Add(fleetEntity);

            //��������� �������� ������ �����
            reserveFleet = new(0);

            //���� � ����������� ��� ��� �������� ���������� �����
            if (organization.defaultReserveFleetPE.Unpack(world.Value, out int reserveFleetEntity) == false)
            {
                //��������� ������ ���� ��� ������� ���������
                organization.defaultReserveFleetPE = fleet.selfPE;
            }

            //������� ���� � ������ ��������� ������ �����������
            organization.reserveFleetPEs.Add(fleet.selfPE);

            //����������� �������� ����������� ������ ��� �������, ������� ����� �������� ������� ������� �����
            TaskForceCreatingRequest(ref fleet);
        }
        #endregion

        #region TaskForce
        void TaskForceCreate(
            ref RTaskForceCreating requestComp)
        {
            //���� ������������ ���� ����������� ������
            requestComp.ownerFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //������ ����� �������� � ��������� �� ��������� ����������� ������
            int taskForceEntity = world.Value.NewEntity();
            ref CTaskForce taskForce = ref taskForcePool.Value.Add(taskForceEntity);

            //��������� �������� ������ ������
            taskForce = new(
                world.Value.PackEntity(taskForceEntity),
                fleet.selfPE);

            taskForce.rand = UnityEngine.Random.Range(0, 50);

            //������� ������ � ������ �����
            fleet.ownedTaskForcePEs.Add(taskForce.selfPE);
            //������ ����� ������ ���������� � ������� ��� � ������ ������ � � ������ �����
            DTFReinforcementRequest reinforcementRequest = new();
            taskForce.reinforcementRequest = reinforcementRequest;
            fleet.tFReinforcementRequests.Add(reinforcementRequest);

            //���� ������ ����� ������
            if (requestComp.template != null)
            {
                //�������� ������ ������
                TaskForceChangeTemplate(
                    ref taskForce,
                    requestComp.template);
            }

            //�������� ������ ������
            TaskForceChangeMission(
                ref taskForce,
                TaskForceActionType.ChangeMissionHold);

            //����������� ���������� ���������� ������
            TaskForceFunctions.RefreshTaskForceUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                objectRefreshUISelfRequestPool.Value,
                ref taskForce);

            //������ �������, ���������� � �������� ����� ������
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
            //���� ������������� ��������� ������ ������ ����������� ������
            if (actionType == TaskForceActionType.ChangeMissionSearch)
            {
                //������ ������ ������ �� �����
                TaskForceChangeMissionSearch(ref taskForce);

                taskForce.activeMissionType = TaskForceMissionType.PatrolSearchMission;
            }
            //�����, ���� ������������� ��������� ������ ������� ������
            else if (actionType == TaskForceActionType.ChangeMissionStrikeGroup)
            {
                //������ ������ ������ �� ������� ������
                TaskForceChangeMissionStrikeGroup(ref taskForce);

                taskForce.activeMissionType = TaskForceMissionType.TargetMissionStrikeGroup;
            }
            //�����, ���� ������������� ��������� ������ ��������� ������
            else if (actionType == TaskForceActionType.ChangeMissionHold)
            {
                taskForce.activeMissionType = TaskForceMissionType.HoldMission;
            }

            UnityEngine.Debug.LogWarning(taskForce.rand + " ! " + taskForce.activeMissionType);
        }

        void TaskForceChangeMissionSearch(
            ref CTaskForce taskForce)
        {
            //���� ������������ ���� ����������� ������
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //���� ���� �� ����� ���������� ������
            if (fleetPatrolMissionSearchPool.Value.Has(fleetEntity) == false)
            {
                //��������� ����� ��������� ������ ������
                ref CFleetPMissionSearch pMissionSearch = ref fleetPatrolMissionSearchPool.Value.Add(fleetEntity);

                //��������� �������� ������ ���������� ������
                pMissionSearch = new(0);
            }

            //���� ��������� ������ ������
            ref CFleetPMissionSearch fleetPMissionSearch = ref fleetPatrolMissionSearchPool.Value.Get(fleetEntity);

            //���� �������� ������ � ��������� �� �������� ���������� ������ � ������ ������
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);
            ref CTaskForcePatrolMission tFPatrolMission = ref taskForcePatrolMissionPool.Value.Add(taskForceEntity);
            ref CTaskForcePMissionSearch tFPMissionSearch = ref taskForcePatrolMissionSearchPool.Value.Add(taskForceEntity);

            //��������� �������� ������ ���������� ���������� ������
            tFPatrolMission = new(0);

            //��������� �������� ������ ���������� ������ ������
            tFPMissionSearch = new(0);

            //������� ������ � ������ �����, ������� ������� ������
            fleetPMissionSearch.activeTaskForcePEs.Add(taskForce.selfPE);

            if (fleet.fleetRegions.Count > 0)
            {
                taskForce.currentRegionPE = fleet.fleetRegions[UnityEngine.Random.Range(0, fleet.fleetRegions.Count)].regionPE;
            }
        }

        void TaskForceChangeMissionStrikeGroup(
            ref CTaskForce taskForce)
        {
            //���� ������������ ���� ����������� ������
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //���� ���� �� ����� ���������� ������ ������� ������
            if (fleetTargetMissionStrikeGroupPool.Value.Has(fleetEntity) == false)
            {
                //��������� ����� ��������� ������ ������� ������
                ref CFleetTMissionStrikeGroup tMissionStrikeGroup = ref fleetTargetMissionStrikeGroupPool.Value.Add(fleetEntity);

                //��������� �������� ������ ���������� ������
                tMissionStrikeGroup = new(0);
            }

            //���� ��������� ������ ������� ������
            ref CFleetTMissionStrikeGroup fleetTMissionStrikeGroup = ref fleetTargetMissionStrikeGroupPool.Value.Get(fleetEntity);

            //���� �������� ������ � ��������� �� �������� ������� ������ � ������ ������� ������
            taskForce.selfPE.Unpack(world.Value, out int taskForceEntity);
            ref CTaskForceTargetMission tFTargetMission = ref taskForceTargetMissionPool.Value.Add(taskForceEntity);
            ref CTaskForceTMissionStrikeGroup tFTMissionStrikeGroup = ref taskForceTargetMissionStrikeGroupPool.Value.Add(taskForceEntity);

            //��������� �������� ������ ���������� ������� ������
            tFTargetMission = new(0);

            //��������� �������� ������ ���������� ������ ������� ������
            tFTMissionStrikeGroup = new(0);

            //������� ������ � ������ �����, ������� ������� ������� ������
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
            //���� � ����������� ������ ��� ������
            if (taskForce.template != null)
            {
                //���� ������� ������ ������ 
                DTFTemplate currentTemplate = taskForce.template;

                //������� ����������� ������ �� ������ � �������
                currentTemplate.taskForces.Remove(taskForce.selfPE);

                //������� ������ �� ������ �� ������ ������
                taskForce.template = null;
            }

            //������� ������ � ������ � ����� �������
            template.taskForces.Add(taskForce.selfPE);

            //��� ������ ������ �� ������
            taskForce.template = template;

            //����������� �������� ����������
            TaskForceFunctions.ReinforcementCheckSelfRequest(
                world.Value,
                reserveFleetReinforcementCheckSelfRequestPool.Value,
                fleetPool.Value,
                taskForceReinforcementCheckSelfRequestPool.Value, 
                ref taskForce);

            //����������� ���������� ���������� ������
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
            //������ ����� �������� � ��������� �� ��������� ������� �������� ����������� ������
            int requestEntity = world.Value.NewEntity();
            ref RTaskForceCreating requestComp = ref taskForceCreatingRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
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
            //���� �����������
            requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //������ ����� ������ ����������� ������ � ��������� �������� ������
            DTFTemplate template = new(
                requestComp.tFTemplateName);

            //�������� ������ ������� ����� �������� � �������
            template.shipTypes = new DCountedShipType[requestComp.shipTypes.Count];

            //��� ������� ���� ������� � ������ � �������
            for (int a = 0; a < requestComp.shipTypes.Count; a++)
            {
                //������� ��� ������� � ������ � �������
                template.shipTypes[a] = new(
                    requestComp.shipTypes[a].shipType,
                    requestComp.shipTypes[a].shipCount);
            }

            //������� ����� ������ � ������ � ������ �����������
            OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Add(template);
        }

        void TFTemplateUpdate(
            ref RTFTemplateCreating requestComp)
        {
            //���� �����������
            requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //��� ������� ������� ����������� ������
            for (int a = 0; a < OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Count; a++)
            {
                //���� ������ ��������� � ����������� ��� ���������
                if (OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a] == requestComp.updatingTemplate)
                {
                    //���� ������
                    DTFTemplate template = OrganizationsData.organizationData[organization.selfIndex].tFTemplates[a];

                    //�������������� ������ �������
                    template.selfName = requestComp.tFTemplateName;

                    //�������� ������ ������� ����� �������� � �������
                    template.shipTypes = new DCountedShipType[requestComp.shipTypes.Count];

                    //��� ������� ���� ������� � ������ � �������
                    for (int b = 0; b < requestComp.shipTypes.Count; b++)
                    {
                        //������� ��� ������� � ������ � �������
                        template.shipTypes[b] = new(
                            requestComp.shipTypes[b].shipType,
                            requestComp.shipTypes[b].shipCount);
                    }

                    //��� ������ ������, ������������ ������ ������
                    for (int b = 0; b < template.taskForces.Count; b++)
                    {
                        //���� ������
                        template.taskForces[b].Unpack(world.Value, out int taskForceEntity);
                        ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);

                        //����������� �������� ����������
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
            //���� �����������
            requestComp.organizationPE.Unpack(world.Value, out int organizationEntity);
            ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

            //���� ��������� ������
            DTFTemplate template = requestComp.template;

            //��� ������� ������� ����������� ������, �������� ������������ �������� ������
            foreach (int templateEntity in tFTemplateDisplayedSummaryPanelFilter.Value)
            {
                //���� ��������� ������������ �������� ������
                ref CTFTemplateDisplayedSummaryPanel tFTemplateDisplayedSummaryPanel = ref tFTemplateDisplayedSummaryPanelPool.Value.Get(templateEntity);

                //���� ������ ��������� ���������� ��������� ������
                if (tFTemplateDisplayedSummaryPanel.CheckTemplate(template))
                {
                    //����������� �������� ����������
                    TaskForceFunctions.RefreshTFTemplateUISelfRequest(
                        world.Value,
                        tFTemplateDisplayedSummaryPanelPool.Value,
                        objectRefreshUISelfRequestPool.Value,
                        ref tFTemplateDisplayedSummaryPanel,
                        RefreshUIType.Delete);

                    //������� �� �����, ��������� ������ ������ �������
                    break;
                }
            }

            //������� ������ �� ������ �����������
            OrganizationsData.organizationData[organization.selfIndex].tFTemplates.Remove(template);
        }
        #endregion

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