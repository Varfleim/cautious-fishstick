
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

namespace SandOcean.Ship.Moving
{
    public class SMTShipGroupMoving : EcsThreadSystem<TShipGroupMoving,
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

        protected override void SetData(IEcsSystems systems, ref TShipGroupMoving thread)
        {
            thread.world = world.Value;
        }
    }
}