
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class STaskForcePathfindingRequestDelete : IEcsRunSystem
    {
        //—обыти€ флотов
        readonly EcsFilterInject<Inc<SRTaskForceFindPath>> taskForceFindPathSelfRequestFilter = default;
        readonly EcsPoolInject<SRTaskForceFindPath> taskForceFindPathSelfRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //ƒл€ каждого самозапроса поиска пути
            foreach (int selfRequestEntity in taskForceFindPathSelfRequestFilter.Value)
            {
                //”дал€ем самозапрос с сущности, оставл€€ только компоненты оперативной группы
                taskForceFindPathSelfRequestPool.Value.Del(selfRequestEntity);
            }
        }
    }
}