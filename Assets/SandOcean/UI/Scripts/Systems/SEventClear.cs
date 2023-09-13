
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.UI.Events
{
    public class SEventClear : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Общие события
        readonly EcsFilterInject<Inc<EObjectNewCreated>> objectNewCreatedEventFilter = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события создания нового объекта
            foreach (int eventEntity in objectNewCreatedEventFilter.Value)
            {
                //Удаляем сущность события
                world.Value.DelEntity(eventEntity);
            }
        }
    }
}