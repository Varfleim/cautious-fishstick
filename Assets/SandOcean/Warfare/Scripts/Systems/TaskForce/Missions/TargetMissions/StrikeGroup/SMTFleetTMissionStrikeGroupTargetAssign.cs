
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class SMTFleetTMissionStrikeGroupTargetAssign : EcsThreadSystem<TFleetTMissionStrikeGroupTargetAssign,
        CFleetTMissionStrikeGroup, CFleet,
        CTaskForce, CTaskForceTargetMission>
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
            return world.Filter<CFleetTMissionStrikeGroup>().End();
        }

        protected override void SetData(IEcsSystems systems, ref TFleetTMissionStrikeGroupTargetAssign thread)
        {
            thread.world = world.Value;
        }
    }
}