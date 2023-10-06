using Leopotam.EcsLite;
using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.TaskForce.Template;
using SandOcean.Warfare.Ship;
using SandOcean.UI.Events;

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

        public static void RefreshTaskForceUISelfRequest(
            EcsWorld world,
            EcsPool<CTaskForceDisplayedSummaryPanel> taskForceDisplayedSummaryPanelPool, EcsPool<SRObjectRefreshUI> refreshUISelfRequestPool,
            ref CTaskForce taskForce,
            RefreshUIType requestType = RefreshUIType.Refresh)
        {
            //Берём сущность оперативной группы
            taskForce.selfPE.Unpack(world, out int taskForceEntity);

            //Если группа имеет отображаемую обзорную панель
            if (taskForceDisplayedSummaryPanelPool.Has(taskForceEntity) == true)
            {
                //Если группа не имеет самозапроса обновления интерфейса
                if (refreshUISelfRequestPool.Has(taskForceEntity) == false)
                {
                    //То назначаем ей самозапрос обновления интерфейса
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(taskForceEntity);

                    //Заполняем данные самозапроса
                    selfRequestComp = new(requestType);
                }
                //Иначе
                else
                {
                    //Берём самозапрос обновления интерфейса
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(taskForceEntity);

                    //Если запрашивалось не удаление
                    if (selfRequestComp.requestType != RefreshUIType.Delete)
                    {
                        //То обновляем самозапрос
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


        public static void RefreshTFTemplateUISelfRequest(
            EcsWorld world,
            EcsPool<CTFTemplateDisplayedSummaryPanel> tFTemplateDisplayedSummaryPanelPool, EcsPool<SRObjectRefreshUI> refreshUISelfRequestPool,
            ref CTFTemplateDisplayedSummaryPanel tfTemplateDisplayedSummaryPanel,
            RefreshUIType requestType = RefreshUIType.Refresh)
        {
            //Берём сущность шаблона оперативной группы
            tfTemplateDisplayedSummaryPanel.selfPE.Unpack(world, out int tFTemplateEntity);

            //Если шаблон имеет отображаемую обзорную панель
            //if (tFTemplateDisplayedSummaryPanelPool.Has(tFTemplateEntity) == true)
            //{
                //Если шаблон не имеет самозапроса обновления интерфейса
                if (refreshUISelfRequestPool.Has(tFTemplateEntity) == false)
                {
                    //То назначаем ему самозапрос обновления интерфейса
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(tFTemplateEntity);

                    //Заполняем данные самозапроса
                    selfRequestComp = new(requestType);
                }
                //Иначе
                else
                {
                    //Берём самозапрос обновления интерфейса
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(tFTemplateEntity);

                    //Если запрашивалось не удаление
                    if (selfRequestComp.requestType != RefreshUIType.Delete)
                    {
                        //То обновляем самозапрос
                        selfRequestComp.requestType = requestType;
                    }
                }
            //}
        }
    }
}