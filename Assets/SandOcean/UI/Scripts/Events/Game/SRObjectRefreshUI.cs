namespace SandOcean.UI.Events
{
    public enum RefreshUIType : byte
    {
        Refresh,
        Delete
    }

    public struct SRObjectRefreshUI
    {
        public SRObjectRefreshUI(
            RefreshUIType requestType)
        {
            this.requestType = requestType;
        }

        public RefreshUIType requestType;
    }
}