
using Leopotam.EcsLite;
using Leopotam.EcsLite.Threads;

using SandOcean.Warfare.Fleet;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct TFleetMissionControl : IEcsThread<
        CFleet,
        CTaskForce>
    {
        public EcsWorld world;

        int[] fleetEntities;

        CFleet[] fleetPool;
        int[] fleetIndices;

        CTaskForce[] taskForcePool;
        int[] taskForceIndices;

        public void Init(
            int[] entities,
            CFleet[] pool1, int[] indices1,
            CTaskForce[] pool2, int[] indices2)
        {
            fleetEntities = entities;

            fleetPool = pool1;
            fleetIndices = indices1;

            taskForcePool = pool2;
            taskForceIndices = indices2;
        }

        public void Execute(int threadId, int fromIndex, int beforeIndex)
        {
            //Для каждого флота в потоке
            for (int a = fromIndex; a < beforeIndex; a++)
            {
                //Берём флот
                int fleetEntity = fleetEntities[a];
                ref CFleet fleet = ref fleetPool[fleetIndices[fleetEntity]];

                //Для каждого региона флота
                for (int b = 0; b < fleet.fleetRegions.Count; b++)
                {

                }
            }
        }
    }
}