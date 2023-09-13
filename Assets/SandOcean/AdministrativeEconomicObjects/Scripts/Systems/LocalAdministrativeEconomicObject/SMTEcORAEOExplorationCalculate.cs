
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

namespace SandOcean.AEO.RAEO
{
    public class SMTEcORAEOExplorationCalculate : EcsThreadSystem<TEcORAEOExplorationCalculate,
        CEconomicORAEO,
        CExplorationORAEO>
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

        protected override void SetData(IEcsSystems systems, ref TEcORAEOExplorationCalculate thread)
        {
            thread.world = world.Value;
        }
    }
}