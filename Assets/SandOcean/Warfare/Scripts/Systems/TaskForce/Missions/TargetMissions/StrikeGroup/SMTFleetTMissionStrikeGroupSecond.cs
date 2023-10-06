
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;
using SandOcean.Warfare.Fleet.Moving;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public class SMTFleetTMissionStrikeGroupSecond : EcsThreadSystem<TFleetTMissionStrikeGroupSecond,
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

        protected override void SetData(IEcsSystems systems, ref TFleetTMissionStrikeGroupSecond thread)
        {
            thread.world = world.Value;
        }
    }
}