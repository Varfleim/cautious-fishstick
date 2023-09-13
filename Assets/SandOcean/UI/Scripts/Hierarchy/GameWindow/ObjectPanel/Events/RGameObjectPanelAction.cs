
using Leopotam.EcsLite;

namespace SandOcean.UI.GameWindow.Object.Events
{
    public enum ObjectPanelActionRequestType : byte
    {
        FleetManager,
        FleetManagerFleets,
        FleetManagerTaskForceTemplates,
        FleetManagerTaskForceTemplatesList,
        FleetManagerTaskForceTemplatesDesigner,

        Organization,
        OrganizationOverview,

        Region,
        RegionOverview,
        RegionOrganizations,

        ORAEO,
        ORAEOOverview,

        CloseObjectPanel
    }

    public readonly struct RGameObjectPanelAction
    {
        public RGameObjectPanelAction(
            ObjectPanelActionRequestType requestType,
            EcsPackedEntity objectPE,
            bool isRefresh)
        {
            this.requestType = requestType;

            this.objectPE = objectPE;

            this.isRefresh = isRefresh;
        }

        public readonly ObjectPanelActionRequestType requestType;

        public readonly EcsPackedEntity objectPE;

        public readonly bool isRefresh;
    }
}