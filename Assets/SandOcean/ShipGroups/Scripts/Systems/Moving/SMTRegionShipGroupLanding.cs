
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.AEO.RAEO;

namespace SandOcean.Ship.Moving
{
    public class SMTRegionShipGroupLanding : EcsThreadSystem<TRegionShipGroupLanding,
        CHexRegion,
        CShipGroup,
        CSGMoving,
        CRegionAEO,
        CEconomicORAEO>
    {
        readonly EcsWorldInject world = default;

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

        protected override void SetData(IEcsSystems systems, ref TRegionShipGroupLanding thread)
        {
            thread.world = world.Value;
        }
    }
}