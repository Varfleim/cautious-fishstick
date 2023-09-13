namespace SandOcean.UI.DesignerWindow.Events
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