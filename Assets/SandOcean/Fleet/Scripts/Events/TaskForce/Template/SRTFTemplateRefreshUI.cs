using SandOcean.UI;

namespace SandOcean.Warfare.TaskForce.Template
{
    public readonly struct SRTFTemplateRefreshUI
    {
        public SRTFTemplateRefreshUI(
            RefresUIType requestType)
        {
            this.requestType = requestType;
        }

        public readonly RefresUIType requestType;
    }
}