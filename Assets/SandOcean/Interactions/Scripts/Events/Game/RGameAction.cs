namespace SandOcean.UI.Events
{
    public enum GameActionType : byte
    {
        None,
        PauseOn,
        PauseOff
    }

    public struct RGameAction
    {
        public GameActionType actionType;

        public int contentSetIndex;
    }
}