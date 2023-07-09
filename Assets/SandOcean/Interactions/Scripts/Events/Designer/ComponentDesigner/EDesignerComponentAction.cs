
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using SandOcean.Designer;
using SandOcean.Technology;

namespace SandOcean.UI.Events
{
    public enum DesignerComponentActionType : byte
    {
        None,
        ChangeCoreTechnology,
    }

    public struct EDesignerComponentAction
    {
        public DesignerComponentActionType actionType;

        public TechnologyComponentCoreModifierType componentCoreModifierType;

        public int technologyDropdownIndex;
    }
}