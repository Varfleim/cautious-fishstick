
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class SMTFleetPMissionSearchRegionUpdate : EcsThreadSystem<TFleetPMissionSearchRegionUpdate,
        CFleetPMissionSearch, CFleet,
        CTaskForce, CTaskForcePatrolMission, CTaskForceWaiting>
    {
        readonly EcsWorldInject world = default;

        protected override int GetChunkSize(IEcsSystems systems)
        {
            return 32;
        }

        protected override EcsWorld GetWorld(IEcsSystems systems)
        {
            return systems.GetWorld();
        }

        protected override EcsFilter GetFilter(EcsWorld world)
        {
            return world.Filter<CFleetPMissionSearch>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TFleetPMissionSearchRegionUpdate thread)
        {
            thread.world = world.Value;
        }
    }
}