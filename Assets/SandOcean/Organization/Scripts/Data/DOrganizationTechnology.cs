
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Organization
{
    public struct DOrganizationTechnology
    {
        public DOrganizationTechnology(
            bool isResearched)
        {
            this.isResearched = isResearched;
        }

        public bool isResearched;
    }
}