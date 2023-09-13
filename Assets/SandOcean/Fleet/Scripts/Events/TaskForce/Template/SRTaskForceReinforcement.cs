
using Leopotam.EcsLite;

namespace SandOcean.Warfare.TaskForce.Template
{
    public readonly struct SRTaskForceReinforcement
    {
        public SRTaskForceReinforcement(
            EcsPackedEntity targetTaskForcePE)
        {
            this.targetTaskForcePE = targetTaskForcePE;
        }

        public readonly EcsPackedEntity targetTaskForcePE;
    }
}