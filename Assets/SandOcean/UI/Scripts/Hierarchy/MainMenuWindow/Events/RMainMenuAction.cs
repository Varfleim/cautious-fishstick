namespace SandOcean.UI.MainMenu.Events
{
    public enum MainMenuActionType : byte
    {
        None,
        OpenNewGameMenu,
        OpenLoadGameMenu,
        OpenWorkshop,
        OpenMainSettings
    }

    public struct RMainMenuAction
    {
        public MainMenuActionType actionType;
    }
}