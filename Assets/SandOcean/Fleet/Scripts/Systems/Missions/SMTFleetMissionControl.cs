
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class SMTFleetMissionControl : EcsThreadSystem<TFleetMissionControl,
        CFleet,
        CTaskForce>
    {
        readonly EcsWorldInject world = default;

        protected override int GetChunkSize(IEcsSystems systems)
        {
            return 64;
        }

        protected override EcsWorld GetWorld(IEcsSystems systems)
        {
            return systems.GetWorld();
        }

        protected override EcsFilter GetFilter(EcsWorld world)
        {
            return world.Filter<CFleet>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TFleetMissionControl thread)
        {
            thread.world = world.Value;
        }
    }
}