
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI.Events;
using SandOcean.Organization;
using SandOcean.Map;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.TaskForce.Missions;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce
{
    public class STaskForceEmptyCheck : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Карта
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //Флоты
        readonly EcsPoolInject<CFleet> fleetPool = default;
        //readonly EcsPoolInject<CFleetPMissionSearch> fleetPatrolMissionSearchPool = default;
        //readonly EcsPoolInject<CFleetTMissionStrikeGroup> fleetTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CFleetTMissionReinforcement> fleetTMissionReinforcementPool = default;

        readonly EcsPoolInject<CTaskForce> taskForcePool = default;
        //readonly EcsPoolInject<CTaskForcePatrolMission> taskForcePatrolMissionPool = default;
        //readonly EcsPoolInject<CTaskForcePMissionSearch> taskForcePatrolMissionSearchPool = default;
        readonly EcsPoolInject<CTaskForceTargetMission> taskForceTargetMissionPool = default;
        //readonly EcsPoolInject<CTaskForceTMissionStrikeGroup> taskForceTargetMissionStrikeGroupPool = default;
        readonly EcsPoolInject<CTaskForceTMissionReinforcement> taskForceTargetMissionReinforcementPool = default;
        readonly EcsPoolInject<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool = default;


        //События флотов
        readonly EcsPoolInject<SRTaskForceRefreshUI> tFRefreshUISelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, SRTaskForceEmptyCheck>> tFEmptyCheckSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceEmptyCheck> tFEmptyCheckSelfRequestPool = default;

        readonly EcsFilterInject<Inc<CTaskForce, CTaskForceTMissionReinforcement, SRTaskForceDelete>> tFTMissionReinforcementDeleteSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceDelete> tFDeleteSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждой оперативной группы с самозапросом проверки пустоты
            foreach (int taskForceEntity in tFEmptyCheckSelfRequestFilter.Value)
            {
                //Берём группу и самозапрос
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceEmptyCheck requestComp = ref tFEmptyCheckSelfRequestPool.Value.Get(taskForceEntity);

                //Если группа пуста
                if (taskForce.ships.Count == 0)
                {
                    //Если группа имеет миссию пополнения
                    if (taskForceTargetMissionReinforcementPool.Value.Has(taskForceEntity) == true)
                    {
                        //То группу следует удалить, поскольку такие группы не могут существовать пустыми

                        //Запрашиваем удаление группы
                        TaskForceDeleteSelfRequest(taskForceEntity);
                    }
                }

                tFEmptyCheckSelfRequestPool.Value.Del(taskForceEntity);
            }

            //Для каждой оперативной группы с миссией поиска и самозапросом удаления
            /*foreach (int taskForceEntity in tFPatrolMissionSearchDeleteSelfRequestFilter.Value)
            {
                //Берём группу и самозапрос
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceDelete requestComp = ref taskForceDeleteSelfRequestPool.Value.Get(taskForceEntity);
            }

            //Для каждой оперативной группы с миссией ударной группы и самозапросом удаления
            foreach (int taskForceEntity in tFPatrolMissionSearchDeleteSelfRequestFilter.Value)
            {
                //Берём группу и самозапрос
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceDelete requestComp = ref taskForceDeleteSelfRequestPool.Value.Get(taskForceEntity);
            }*/

            //Удаляем группы с миссией пополнения
            TaskForceTMissionReinforcementDelete();

            //Для каждой оперативной группы с самозапросом удаления
            /*foreach (int taskForceEntity in taskForceDeleteSelfRequestFilter.Value)
            {
                //Берём группу и самозапрос
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
            }*/
        }

        void TaskForceTMissionReinforcementDelete()
        {
            //Для каждой оперативной группы с миссией пополнения и самозапросом удаления
            foreach (int taskForceEntity in tFTMissionReinforcementDeleteSelfRequestFilter.Value)
            {
                //Берём группу и самозапрос
                ref CTaskForce taskForce = ref taskForcePool.Value.Get(taskForceEntity);
                ref SRTaskForceDelete requestComp = ref tFDeleteSelfRequestPool.Value.Get(taskForceEntity);

                //Берём родительский флот и его миссию пополнения
                taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
                ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);
                ref CFleetTMissionReinforcement fleetTMissionReinforcement = ref fleetTMissionReinforcementPool.Value.Get(fleetEntity);

                //Удаляем группу из списка активных групп
                fleetTMissionReinforcement.activeTaskForcePEs.Remove(taskForce.selfPE);

                //Если список активных групп пуст, то удаляем миссию пополнения флота
                if (fleetTMissionReinforcement.activeTaskForcePEs.Count == 0)
                {
                    fleetTMissionReinforcementPool.Value.Del(fleetEntity);
                }

                //Удаляем компонент целевой миссии и миссии пополнения
                taskForceTargetMissionPool.Value.Del(taskForceEntity);
                taskForceTargetMissionReinforcementPool.Value.Del(taskForceEntity);

                //Удаляем группу
                TaskForceDelete(taskForceEntity, ref taskForce);

                tFDeleteSelfRequestPool.Value.Del(taskForceEntity);
            }
        }

        void TaskForceDelete(
            int taskForceEntity, ref CTaskForce taskForce)
        {
            //Берём текущий регион группы
            taskForce.currentRegionPE.Unpack(world.Value, out int regionEntity);
            ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

            //Удаляем группу из списка групп в регионе
            region.taskForcePEs.Remove(taskForce.selfPE);

            //Берём родительский флот
            taskForce.parentFleetPE.Unpack(world.Value, out int fleetEntity);
            ref CFleet fleet = ref fleetPool.Value.Get(fleetEntity);

            //Удаляем группу из списка групп флота
            fleet.ownedTaskForcePEs.Remove(taskForce.selfPE);
            //Если у группы есть запрос пополнения
            if (taskForce.reinforcementRequest != null)
            {
                //Удаляем его из данных флота
                fleet.tFReinforcementRequests.Remove(taskForce.reinforcementRequest);
            }

            //Удаляем корабли группы
            TaskForceShipDelete(ref taskForce);

            //Если у группы есть шаблон
            if (taskForce.template != null)
            {
                //Удаляем группу из списка групп шаблона
                taskForce.template.taskForces.Remove(taskForce.selfPE);
            }

            UnityEngine.Debug.LogWarning("Task Force Deleted! " + taskForce.rand);

            //Запрашиваем удаление интерфейса группы
            TaskForceFunctions.RefreshUISelfRequest(
                world.Value,
                taskForceDisplayedSummaryPanelPool.Value,
                tFRefreshUISelfRequestPool.Value,
                ref taskForce,
                UI.RefresUIType.Delete);

            //Удаляем компонент группы
            taskForcePool.Value.Del(taskForceEntity);
        }

        void TaskForceShipDelete(
            ref CTaskForce taskForce)
        {
            //Для каждого корабля в оперативной группе
            for (int a = 0; a < taskForce.ships.Count; a++)
            {
                //Берём корабль
                DShip ship = taskForce.ships[a];

            }

            //Очищаем список кораблей
            taskForce.ships.Clear();
        }

        void TaskForceDeleteSelfRequest(
            int taskForceEntity)
        {
            //Назначаем сущности оперативной группы самозапрос удаления
            ref SRTaskForceDelete selfRequestComp = ref tFDeleteSelfRequestPool.Value.Add(taskForceEntity);
        }
    }
}