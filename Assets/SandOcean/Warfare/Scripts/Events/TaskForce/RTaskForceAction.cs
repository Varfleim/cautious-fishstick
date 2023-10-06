
using Leopotam.EcsLite;

using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Warfare.Fleet
{
    public enum TaskForceActionType : byte
    {
        None,
        ChangeMissionSearch,
        ChangeMissionStrikeGroup,
        ChangeMissionReinforcement,
        ChangeMissionHold,
        ChangeTemplate
    }

    public readonly struct RTaskForceAction
    {
        public RTaskForceAction(
            EcsPackedEntity taskForcePE,
            TaskForceActionType actionType,
            DTFTemplate template)
        {
            this.taskForcePE = taskForcePE;

            this.actionType = actionType;

            this.template = template;
        }

        public readonly EcsPackedEntity taskForcePE;

        public readonly TaskForceActionType actionType;

        public readonly DTFTemplate template;
    }
}