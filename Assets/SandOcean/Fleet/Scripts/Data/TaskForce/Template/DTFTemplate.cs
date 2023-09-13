
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public class DTFTemplate
    {
        public DTFTemplate(
            string selfName)
        {
            this.selfName = selfName;

            shipTypes = new DCountedShipType[0];

            taskForces = new();
        }

        public string selfName;

        public DCountedShipType[] shipTypes;

        public List<EcsPackedEntity> taskForces;
    }
}