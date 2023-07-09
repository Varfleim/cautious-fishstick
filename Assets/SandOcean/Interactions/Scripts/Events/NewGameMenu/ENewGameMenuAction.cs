namespace SandOcean.UI.Events
{
    public enum NewGameMenuActionType : byte
    {
        None,
        OpenMainMenu,
        StartNewGame
    }

    public struct ENewGameMenuAction
    {
        public NewGameMenuActionType actionType;
    }
}