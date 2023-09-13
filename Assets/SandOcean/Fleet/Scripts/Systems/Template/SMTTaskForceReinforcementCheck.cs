
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public class SMTTaskForceReinforcementCheck : EcsThreadSystem<TTaskForceReinforcementCheck,
        SRTaskForceReinforcementCheck, CTaskForce>
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
            return world.Filter<SRTaskForceReinforcementCheck>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TTaskForceReinforcementCheck thread)
        {
            thread.world = world.Value;
        }
    }
}