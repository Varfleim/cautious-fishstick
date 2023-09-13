using Leopotam.EcsLite;

using SandOcean.UI;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce
{
    public static class TaskForceFunctions
    {
        public static void ReinforcementCheckSelfRequest(
            EcsWorld world,
            EcsPool<SRReserveFleetReinforcementCheck> reserveFleetReinforcementCheckSelfRequestPool,
            EcsPool<CFleet> fleetPool,
            EcsPool<SRTaskForceReinforcementCheck> taskForceReinforcementCheckSelfRequestPool,
            ref CTaskForce taskForce)
        {
            //Если шаблон группы не пуст
            if (taskForce.template != null)
            {
                //Берём родительский флот оперативной группы
                taskForce.parentFleetPE.Unpack(world, out int fleetEntity);
                ref CFleet fleet = ref fleetPool.Get(fleetEntity);

                //Берём сущность резервного флота
                fleet.reserveFleetPE.Unpack(world, out int reserveFleetEntity);

                //Если резервный флот не имеет самозапроса проверки пополнения
                if (reserveFleetReinforcementCheckSelfRequestPool.Has(reserveFleetEntity) == false)
                {
                    //Назначаем резервному флоту самозапрос проверки пополнения
                    ref SRReserveFleetReinforcementCheck selfRequestComp = ref reserveFleetReinforcementCheckSelfRequestPool.Add(reserveFleetEntity);
                }

                //Берём сущность группы и назначаем ей самозапрос проверки пополнения
                taskForce.selfPE.Unpack(world, out int taskForceEntity);

                //Если группа не имеет самозапроса проверки пополнения
                if (taskForceReinforcementCheckSelfRequestPool.Has(taskForceEntity) == false)
                {
                    //Назначаем группе самозапрос проверки пополнения
                    ref SRTaskForceReinforcementCheck selfRequestComp = ref taskForceReinforcementCheckSelfRequestPool.Add(taskForceEntity);
                }
            }
        }

        public static void RefreshUISelfRequest(
            EcsWorld world,
            EcsPool<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool, EcsPool<SRTaskForceRefreshUI> taskForceRefreshUISelfRequestPool,
            ref CTaskForce taskForce,
            RefresUIType requestType = RefresUIType.Refresh)
        {
            //Берём сущность оперативной группы
            taskForce.selfPE.Unpack(world, out int taskForceEntity);

            //Если группа имеет отображаемую обзорную панель
            if (taskForceDisplayedSummaryPanelPool.Has(taskForceEntity) == true)
            {
                //Если группа не имеет самозапроса обновления интерфейса
                if (taskForceRefreshUISelfRequestPool.Has(taskForceEntity) == false)
                {
                    //То назначаем ей самозапрос обновления интерфейса
                    ref SRTaskForceRefreshUI selfRequestComp = ref taskForceRefreshUISelfRequestPool.Add(taskForceEntity);

                    //Заполняем данные самозапроса
                    selfRequestComp = new(requestType);
                }
                //Иначе
                else
                {
                    //Берём самозапрос обновления интерфейса
                    ref SRTaskForceRefreshUI selfRequestComp = ref taskForceRefreshUISelfRequestPool.Add(taskForceEntity);

                    //Если запрашивалось не удаление
                    if (selfRequestComp.requestType != RefresUIType.Delete)
                    {
                        //То обновляем запрос
                        selfRequestComp.requestType = requestType;
                    }
                }
            }
        }

        public static void ShipTransfer(
            ref CTaskForce fromTaskForce, ref CTaskForce toTaskForce,
            DShip ship)
        {
            //Удаляем корабль из списка исходной оперативной группы
            fromTaskForce.ships.Remove(ship);

            //Заносим корабль в список целевой группы
            toTaskForce.ships.Add(ship);
        }
    }
}