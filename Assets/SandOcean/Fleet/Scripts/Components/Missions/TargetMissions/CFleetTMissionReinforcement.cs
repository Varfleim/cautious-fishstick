
using System.Collections.Generic;

using Leopotam.EcsLite;

namespace SandOcean.Warfare.TaskForce.Missions
{
    public struct CFleetTMissionReinforcement
    {
        public CFleetTMissionReinforcement(int a)
        {
            activeTaskForcePEs = new();
        }

        public List<EcsPackedEntity> activeTaskForcePEs;
    }
}