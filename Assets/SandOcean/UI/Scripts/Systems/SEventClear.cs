
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.UI.Events
{
    public class SEventClear : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //����� �������
        readonly EcsFilterInject<Inc<EObjectNewCreated>> objectNewCreatedEventFilter = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ������ �������
            foreach (int eventEntity in objectNewCreatedEventFilter.Value)
            {
                //������� �������� �������
                world.Value.DelEntity(eventEntity);
            }
        }
    }
}