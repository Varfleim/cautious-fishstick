
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct CFleetPMissionSearch
    {
        public CFleetPMissionSearch(int a)
        {
            activeTaskForcePEs = new();
        }

        public List<EcsPackedEntity> activeTaskForcePEs;
    }
}