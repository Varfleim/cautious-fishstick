
using Leopotam.EcsLite;

namespace SandOcean.AEO.RAEO
{
    public enum ORAEOActionType : byte
    {
        None,
        Colonization
    }

    public readonly struct SRORAEOAction
    {
        public SRORAEOAction( 
            ORAEOActionType actionType)
        {
            this.actionType = actionType;
        }

        public readonly ORAEOActionType actionType;
    }
}