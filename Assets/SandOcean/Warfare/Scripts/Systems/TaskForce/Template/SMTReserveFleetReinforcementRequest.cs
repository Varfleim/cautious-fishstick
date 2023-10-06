
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Template
{
    public class SMTReserveFleetReinforcementRequest : EcsThreadSystem<TReserveFleetReinforcementRequest,
        SRReserveFleetReinforcementCheck, CFleet, CReserveFleet>
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
            return world.Filter<SRReserveFleetReinforcementCheck>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TReserveFleetReinforcementRequest thread)
        {
            thread.world = world.Value;
        }
    }
}