
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.Fleet
{
    public struct CFleet
    {
        public CFleet(
            EcsPackedEntity selfPE,
            EcsPackedEntity parentOrganizationPE,
            EcsPackedEntity reserveFleetPE)
        {
            this.selfPE = selfPE;

            this.parentOrganizationPE = parentOrganizationPE;
            this.reserveFleetPE = reserveFleetPE;

            fleetRegions = new(10);

            ownedTaskForcePEs = new(10);
            tFReinforcementRequests = new(10);
        }

        public readonly EcsPackedEntity selfPE;

        public readonly EcsPackedEntity parentOrganizationPE;
        public EcsPackedEntity reserveFleetPE;

        public List<DFleetRegion> fleetRegions;

        public List<EcsPackedEntity> ownedTaskForcePEs;
        public List<DTFReinforcementRequest> tFReinforcementRequests;
    }
}