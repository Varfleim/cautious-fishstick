
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.Fleet
{
    public struct CReserveFleet
    {
        public CReserveFleet(int a)
        {
            ownedFleetPEs = new();

            requestedTaskForceReinforcements = new();
        }

        public List<EcsPackedEntity> ownedFleetPEs;

        public List<DTaskForceReinforcementCreating> requestedTaskForceReinforcements;
    }
}