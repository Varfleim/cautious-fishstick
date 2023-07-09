
using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Technology
{
    [Serializable]
    public readonly struct DTechnologyModifierGlobalSort
    {
        public DTechnologyModifierGlobalSort(
            int contentSetIndex, 
            int technologyIndex, 
            float modifierValue)
        {
            this.contentSetIndex = contentSetIndex;
            this.technologyIndex = technologyIndex;
            this.modifierValue = modifierValue;
        }

        public readonly int contentSetIndex;

        public readonly int technologyIndex;

        public readonly float modifierValue;
    }
}