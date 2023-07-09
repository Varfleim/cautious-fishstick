
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.AEO.RAEO;

namespace SandOcean.Ship.Moving
{
    public class SMTORAEOShipGroupLanding : EcsThreadSystem<TORAEOShipGroupLanding,
        CEconomicORAEO,
        CShipGroup>
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
            return world.Filter<CEconomicORAEO>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TORAEOShipGroupLanding thread)
        {
            thread.world = world.Value;
        }
    }
}