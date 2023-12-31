namespace SandOcean.UI.DesignerWindow.Events
{
    public enum DesignerActionType : byte
    {
        None,
        SaveContentObject,
        LoadContentSetObject,
        DeleteContentSetObject,
        DisplayContentSetPanelList,
        DisplayContentSetPanel,
        HideContentSetPanel,
        OpenWorkshop,
        OpenGame,
    }

    public struct RDesignerAction
    {
        public DesignerActionType actionType;

        public bool isCurrentContentSet;

        public int contentSetIndex;

        public int objectIndex;
    }
}