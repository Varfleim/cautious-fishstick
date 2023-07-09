
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

using Leopotam.EcsLite;

namespace SandOcean
{
    public struct EStartNewGame
    {
        public EcsPackedEntity playerPE;

        public EcsPackedEntity galaxyPE;

        public bool isTechnologiesCalculated;
    }
}