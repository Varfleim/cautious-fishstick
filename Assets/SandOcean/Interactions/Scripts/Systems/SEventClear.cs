
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.AEO.RAEO;
using SandOcean.Diplomacy;

namespace SandOcean.UI
{
    public class SEventClear : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //События дипломатии
        readonly EcsFilterInject<Inc<EOrganizationNewCreated>> organizationNewCreatedEventFilter = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecORAEONewCreatedEventFilter = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события создания новой организации
            foreach (int eventEntity in organizationNewCreatedEventFilter.Value)
            {
                //Удаляем сущность события
                world.Value.DelEntity(eventEntity);
            }

            //Для каждого события создания нового EcORAEO
            foreach (int eventEntity in ecORAEONewCreatedEventFilter.Value)
            {
                //Удаляем сущность события
                world.Value.DelEntity(eventEntity);
            }
        }
    }
}