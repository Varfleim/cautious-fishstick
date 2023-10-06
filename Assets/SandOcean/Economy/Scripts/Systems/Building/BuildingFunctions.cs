using Leopotam.EcsLite;

using SandOcean.UI.Events;

namespace SandOcean.Economy.Building
{
    public static class BuildingFunctions
    {
        public static void RefreshBuildingUISelfRequest(
            EcsWorld world,
            EcsPool<CBuildingDisplayedSummaryPanel> buildingDisplayedSummaryPanelPool, EcsPool<SRObjectRefreshUI> refreshUISelfRequestPool,
            ref CBuilding building,
            RefreshUIType requestType = RefreshUIType.Refresh)
        {
            //Берём сущность сооружения
            building.selfPE.Unpack(world, out int buildingEntity);

            //Если сооружение имеет отображаемую обзорную панель
            if (buildingDisplayedSummaryPanelPool.Has(buildingEntity) == true)
            {
                //Если сооружение не имеет самозапроса обновления интерфейса
                if (refreshUISelfRequestPool.Has(buildingEntity))
                {
                    //То назначаем ей самозапрос обновления интерфейса
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(buildingEntity);

                    //Заполняем данные самозапроса
                    selfRequestComp = new(requestType);
                }
                //Иначе
                else
                {
                    //Берём самозапрос обновления интерфейса
                    ref SRObjectRefreshUI selfRequestComp = ref refreshUISelfRequestPool.Add(buildingEntity);

                    //Если запрашивалось не удаление
                    if (selfRequestComp.requestType != RefreshUIType.Delete)
                    {
                        //То обновляем самозапрос
                        selfRequestComp.requestType = requestType;
                    }
                }
            }
        }
    }
}