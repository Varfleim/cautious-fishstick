namespace SandOcean.UI.Events
{
    public enum NewGameMenuActionType : byte
    {
        None,
        OpenMainMenu,
        StartNewGame
    }

    public struct RNewGameMenuAction
    {
        public NewGameMenuActionType actionType;
    }
}