
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Organization;

namespace SandOcean.AEO.RAEO
{
    public class SMTRAEOExplorationCalculate : EcsThreadSystem<TRAEOExplorationCalculate,
        CRegionAEO,
        CExplorationORAEO,
        COrganization>
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
            return world.Filter<CRegionAEO>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TRAEOExplorationCalculate thread)
        {
            thread.world = world.Value;
        }
    }
}