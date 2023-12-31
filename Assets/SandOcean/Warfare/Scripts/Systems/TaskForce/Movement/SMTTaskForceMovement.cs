
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class SMTTaskForceMovement : EcsThreadSystem<TTaskForceMovement,
        CTaskForceMovement, CTaskForce,
        CHexRegion>
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
            return world.Filter<CTaskForceMovement>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TTaskForceMovement thread)
        {
            thread.world = world.Value;
        }
    }
}