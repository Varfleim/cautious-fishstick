
using Leopotam.EcsLite;

namespace SandOcean.UI.GameWindow.Object
{
    public enum CreatingPanelType : byte
    {
        None,
        ORAEOBriefInfoPanel,
        FleetOverviewPanel,
        TaskForceOverviewPanel
    }

    public readonly struct RGameCreatePanel
    {
        public RGameCreatePanel(
            CreatingPanelType creatingPanelType,
            EcsPackedEntity objectPE)
        {
            this.creatingPanelType = creatingPanelType;

            this.objectPE = objectPE;
        }

        public readonly CreatingPanelType creatingPanelType;

        public readonly EcsPackedEntity objectPE;
    }
}