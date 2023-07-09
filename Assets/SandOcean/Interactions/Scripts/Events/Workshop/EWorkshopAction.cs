namespace SandOcean.UI.Events
{
    public enum WorkshopActionType : byte
    {
        None,
        OpenMainMenu,
        DisplayContentSet,
        OpenDesigner
    }

    public struct EWorkshopAction
    {
        public WorkshopActionType actionType;

        public int contentSetIndex;

        public Designer.DesignerType designerType;
    }
}