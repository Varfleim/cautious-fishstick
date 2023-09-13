
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public struct DTaskForceReinforcementCreating
    {
        public DTaskForceReinforcementCreating(
            EcsPackedEntity targetTaskForcePE)
        {
            this.targetTaskForcePE = targetTaskForcePE;
            
            shipTypes = new();
        }

        public readonly EcsPackedEntity targetTaskForcePE;

        public List<DCountedShipType> shipTypes;
    }
}