
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.AEO.RAEO;

namespace SandOcean.UI
{
    public class SEventsClear : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecORAEONewCreatedEventFilter = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события создания нового EcORAEO
            foreach (int eventEntity in ecORAEONewCreatedEventFilter.Value)
            {
                //Удаляем сущность события
                world.Value.DelEntity(eventEntity);
            }
        }
    }
}