
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;

namespace SandOcean.Ship.Moving
{
    public class SMTRegionShipGroupOwnershipChange : EcsThreadSystem<TRegionShipGroupOwnershipChange,
        CHexRegion,
        CShipGroup,
        CSGMoving>
    {
        readonly EcsWorldInject world = default;

        readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;

        protected override int GetChunkSize(IEcsSystems systems)
        {
            return 16;
        }

        protected override EcsWorld GetWorld(IEcsSystems systems)
        {
            return systems.GetWorld();
        }

        protected override EcsFilter GetFilter(EcsWorld world)
        {
            return world.Filter<CHexRegion>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TRegionShipGroupOwnershipChange thread)
        {
            thread.world = world.Value;

            thread.mapGenerationData = mapGenerationData.Value;
        }
    }
}