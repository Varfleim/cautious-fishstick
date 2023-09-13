namespace SandOcean.UI.WorkshopWindow.Events
{
    public enum WorkshopActionType : byte
    {
        None,
        OpenMainMenu,
        DisplayContentSet,
        OpenDesigner
    }

    public struct RWorkshopAction
    {
        public WorkshopActionType actionType;

        public int contentSetIndex;

        public Designer.DesignerType designerType;
    }
}