
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct CFleetTMissionStrikeGroup
    {
        public CFleetTMissionStrikeGroup(int a)
        {
            activeTaskForcePEs = new();
        }

        public List<EcsPackedEntity> activeTaskForcePEs;
    }
}