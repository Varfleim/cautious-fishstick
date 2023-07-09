
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.AEO.RAEO;

namespace SandOcean.UI
{
    public class SEventsClear : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecORAEONewCreatedEventFilter = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ������ EcORAEO
            foreach (int eventEntity in ecORAEONewCreatedEventFilter.Value)
            {
                //������� �������� �������
                world.Value.DelEntity(eventEntity);
            }
        }
    }
}