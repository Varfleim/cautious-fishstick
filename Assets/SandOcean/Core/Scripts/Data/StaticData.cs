
using UnityEngine;

using SandOcean.Ship;
using SandOcean.Map;

namespace SandOcean
{
    [CreateAssetMenu]
    public class StaticData : ScriptableObject
    {
        public GOChunk chunkPrefab;
        public GORegion regionPrefab;

        public GOShipGroup shipGroupPrefab;
    }
}