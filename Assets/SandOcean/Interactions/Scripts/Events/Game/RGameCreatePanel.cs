
using Leopotam.EcsLite;

namespace SandOcean.UI.Events
{
    public enum CreatingPanelType : byte
    {
        None,
        ORAEOBriefInfoPanel
    }

    public readonly struct RGameCreatePanel
    {
        public RGameCreatePanel(
            CreatingPanelType creatingPanelType, 
            EcsPackedEntity ownerOrganizationPE)
        {
            this.creatingPanelType = creatingPanelType;
            
            this.ownerOrganizationPE = ownerOrganizationPE;
        }

        public readonly CreatingPanelType creatingPanelType;

        public readonly EcsPackedEntity ownerOrganizationPE;
    }
}