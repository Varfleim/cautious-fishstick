namespace SandOcean.UI.Events
{
    public enum MainMenuActionType : byte
    {
        None,
        OpenNewGameMenu,
        OpenLoadGameMenu,
        OpenWorkshop,
        OpenMainSettings
    }

    public struct EMainMenuAction
    {
        public MainMenuActionType actionType;
    }
}