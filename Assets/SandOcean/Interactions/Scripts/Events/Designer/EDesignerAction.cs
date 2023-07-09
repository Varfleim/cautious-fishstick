namespace SandOcean.UI.Events
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

    public struct EDesignerAction
    {
        public DesignerActionType actionType;

        public bool isCurrentContentSet;

        public int contentSetIndex;

        public int objectIndex;
    }
}