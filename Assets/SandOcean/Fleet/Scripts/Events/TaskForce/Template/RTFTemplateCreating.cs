
using System.Collections.Generic;

using Leopotam.EcsLite;

using SandOcean.Warfare.Ship;

namespace SandOcean.Warfare.TaskForce.Template
{
    public readonly struct RTFTemplateCreating
    {
        public RTFTemplateCreating(
            EcsPackedEntity organizationPE,
            string tFTemplateName,
            List<DCountedShipType> shipTypes,
            bool isUpdate,
            DTFTemplate updatingTemplate)
        {
            this.organizationPE = organizationPE;

            this.tFTemplateName = tFTemplateName;

            this.shipTypes = shipTypes;

            this.isUpdate = isUpdate;

            this.updatingTemplate = updatingTemplate;
        }

        public readonly EcsPackedEntity organizationPE;

        public readonly string tFTemplateName;

        public readonly List<DCountedShipType> shipTypes;

        public readonly bool isUpdate;

        public readonly DTFTemplate updatingTemplate;
    }
}