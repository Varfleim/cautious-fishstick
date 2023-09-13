
using Leopotam.EcsLite;

namespace SandOcean.UI.GameWindow.Object.FleetManager.Events
{
    public enum FleetManagerSubpanelActionRequestType : byte
    {
        FleetsTabFillTemplatesNewList,
        FleetsTabFillTemplatesChangeList
    }

    public readonly struct RGameFleetManagerSubpanelAction
    {
        public RGameFleetManagerSubpanelAction(
            FleetManagerSubpanelActionRequestType requestType,
            EcsPackedEntity organizationPE,
            EcsPackedEntity taskForcePE)
        {
            this.requestType = requestType;

            this.organizationPE = organizationPE;

            this.taskForcePE = taskForcePE;
        }

        public readonly FleetManagerSubpanelActionRequestType requestType;

        public readonly EcsPackedEntity organizationPE;

        public readonly EcsPackedEntity taskForcePE;
    }
}