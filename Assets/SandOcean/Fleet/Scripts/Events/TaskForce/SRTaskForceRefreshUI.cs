using SandOcean.UI;

namespace SandOcean.Warfare.TaskForce
{
    public struct SRTaskForceRefreshUI
    {
        public SRTaskForceRefreshUI(
            RefresUIType requestType)
        {
            this.requestType = requestType;
        }

        public RefresUIType requestType;
    }
}