
using Leopotam.EcsLite;

namespace SandOcean.UI.Events
{
    public enum DisplayObjectPanelEventType : byte
    {
        Organization,
        OrganizationOverview,

        Region,
        RegionOverview,
        RegionOrganizations,

        ORAEO,
        ORAEOOverview
    }

    public readonly struct EGameDisplayObjectPanel
    {
        public EGameDisplayObjectPanel(
            DisplayObjectPanelEventType eventType, 
            EcsPackedEntity objectPE,
            bool isRefresh)
        {
            this.eventType = eventType;

            this.objectPE = objectPE;

            this.isRefresh = isRefresh;
        }

        public readonly DisplayObjectPanelEventType eventType;

        public readonly EcsPackedEntity objectPE;

        public readonly bool isRefresh;
    }
}