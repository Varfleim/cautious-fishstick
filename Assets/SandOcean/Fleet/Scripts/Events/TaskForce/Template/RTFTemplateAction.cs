
using Leopotam.EcsLite;

namespace SandOcean.Warfare.TaskForce.Template
{
    public enum TFTemplateActionType : byte
    {
        Delete
    }

    public readonly struct RTFTemplateAction
    {
        public RTFTemplateAction(
            EcsPackedEntity organizationPE,
            DTFTemplate template,
            TFTemplateActionType requestType)
        {
            this.organizationPE = organizationPE;

            this.template = template;

            this.requestType = requestType;
        }

        public readonly EcsPackedEntity organizationPE;

        public readonly DTFTemplate template;

        public readonly TFTemplateActionType requestType;
    }
}