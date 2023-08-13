
using Leopotam.EcsLite;

namespace SandOcean.UI.Events
{
    public enum DisplayObjectPanelRequestType : byte
    {
        Organization,
        OrganizationOverview,

        Region,
        RegionOverview,
        RegionOrganizations,

        ORAEO,
        ORAEOOverview
    }

    public readonly struct RGameDisplayObjectPanel
    {
        public RGameDisplayObjectPanel(
            DisplayObjectPanelRequestType requestType, 
            EcsPackedEntity objectPE,
            bool isRefresh)
        {
            this.requestType = requestType;

            this.objectPE = objectPE;

            this.isRefresh = isRefresh;
        }

        public readonly DisplayObjectPanelRequestType requestType;

        public readonly EcsPackedEntity objectPE;

        public readonly bool isRefresh;
    }
}