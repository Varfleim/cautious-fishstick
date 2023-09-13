
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

using SandOcean.Warfare.Fleet.Moving;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct TFleetTMissionStrikeGroupSecond : IEcsThread<
        CFleetTMissionStrikeGroup, CFleet,
        CTaskForce, CTaskForceTargetMission>
    {
        public EcsWorld world;

        int[] fleetEntities;

        CFleetTMissionStrikeGroup[] fleetTargetMissionStrikeGroupPool;
        int[] fleetTargetMissionStrikeGroupIndices;

        CFleet[] fleetPool;
        int[] fleetIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        CTaskForceTargetMission[] taskForceTargetMissionPool;
        int[] taskForceTargetMissionIndices;

        public void Init(
            int[] entities,
            CFleetTMissionStrikeGroup[] pool1, int[] indices1,
            CFleet[] pool2, int[] indices2,
            CTaskForce[] pool3, int[] indices3,
            CTaskForceTargetMission[] pool4, int[] indices4)
        {
            fleetEntities = entities;

            fleetTargetMissionStrikeGroupPool = pool1;
            fleetTargetMissionStrikeGroupIndices = indices1;

            fleetPool = pool2;
            fleetIndices = indices2;

            taskForcePool = pool3;
            taskForceIndices = indices3;

            taskForceTargetMissionPool = pool4;
            taskForceTargetMissionIndices = indices4;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Для каждого флота с миссией ударной группы в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {

            }
        }
    }
}