
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

namespace SandOcean.Ship.Moving
{
    public class SMTShipGroupPathfinding : EcsThreadSystem<TShipGroupPathfinding,
        CSGMoving,
        CShipGroup>
    {
        readonly EcsWorldInject world = default;

        protected override int GetChunkSize(IEcsSystems systems)
        {
            return 128;
        }

        protected override EcsWorld GetWorld(IEcsSystems systems)
        {
            return systems.GetWorld();
        }

        protected override EcsFilter GetFilter(EcsWorld world)
        {
            return world.Filter<CSGMoving>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TShipGroupPathfinding thread)
        {
            thread.world = world.Value;
        }
    }
}