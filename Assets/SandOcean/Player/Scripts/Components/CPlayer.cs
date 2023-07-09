
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean.Player
{
    public struct CPlayer
    {
        public EcsPackedEntity selfPE;

        public string selfName;

        public EcsPackedEntity ownedOrganizationPE;
    }
}