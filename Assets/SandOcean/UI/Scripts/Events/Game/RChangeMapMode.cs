
using Leopotam.EcsLite;

namespace SandOcean.Map.Events
{
    public enum ChangeMapModeRequestType : byte
    {
        None,
        Galaxy,
        Sector,
        PlanetSystem,
        Distance
    }

    public struct RChangeMapMode
    {
        public ChangeMapModeRequestType requestType;

        public UI.MapMode mapMode;

        public EcsPackedEntity activeObject;
    }
}