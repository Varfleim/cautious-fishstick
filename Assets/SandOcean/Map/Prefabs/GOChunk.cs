
using UnityEngine;

namespace SandOcean.Map
{
    public class GOChunk : MonoBehaviour
    {
        public Transform chunkTransform;

        public Canvas chunkCanvas;
        public GOMapMesh terrain;
        public GOMapMesh rivers;
        public GOMapMesh roads;
        public GOMapMesh water;
        public GOMapMesh waterShore;
        public GOMapMesh estuaries;

        public GOMapFeatureManager features;
    }
}