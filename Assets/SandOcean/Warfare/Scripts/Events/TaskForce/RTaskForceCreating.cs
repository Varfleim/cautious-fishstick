
using Leopotam.EcsLite;

using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.Fleet
{
    public readonly struct RTaskForceCreating
    {
        public RTaskForceCreating(
            EcsPackedEntity ownerFleetPE,
            DTFTemplate template)
        {
            this.ownerFleetPE = ownerFleetPE;

            this.template = template;
        }

        public readonly EcsPackedEntity ownerFleetPE;

        public readonly DTFTemplate template;
    }
}