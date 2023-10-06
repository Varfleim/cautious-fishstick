
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Map;
using SandOcean.Warfare.TaskForce;

namespace SandOcean.Warfare.Fleet.Moving
{
    public class SMTTaskForcePathfinding : EcsThreadSystem<TTaskForcePathfinding,
        SRTaskForceFindPath, 
        CTaskForce, CTaskForceMovement,
        CHexRegion>
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
            return world.Filter<SRTaskForceFindPath>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TTaskForcePathfinding thread)
        {
            thread.world = world.Value;
        }
    }
}