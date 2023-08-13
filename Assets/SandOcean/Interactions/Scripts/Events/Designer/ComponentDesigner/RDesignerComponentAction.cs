
namespace SandOcean.UI.Events
{
    public enum DesignerComponentActionType : byte
    {
        None,
        ChangeCoreTechnology,
    }

    public struct RDesignerComponentAction
    {
        public DesignerComponentActionType actionType;

        public TechnologyComponentCoreModifierType componentCoreModifierType;

        public int technologyDropdownIndex;
    }
}