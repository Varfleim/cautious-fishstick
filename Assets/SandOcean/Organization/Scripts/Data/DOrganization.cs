
using System.Collections.Generic;

using SandOcean.Warfare.TaskForce.Template;

namespace SandOcean.Organization
{
    public struct DOrganization
    {
        public DOrganization(int a)
        {
            tFTemplates = new(5);
        }

        public List<DTFTemplate> tFTemplates;
    }
}