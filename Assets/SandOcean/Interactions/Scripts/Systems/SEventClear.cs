
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.AEO.RAEO;
using SandOcean.Diplomacy;

namespace SandOcean.UI
{
    public class SEventClear : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������� ����������
        readonly EcsFilterInject<Inc<EOrganizationNewCreated>> organizationNewCreatedEventFilter = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecORAEONewCreatedEventFilter = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ����� �����������
            foreach (int eventEntity in organizationNewCreatedEventFilter.Value)
            {
                //������� �������� �������
                world.Value.DelEntity(eventEntity);
            }

            //��� ������� ������� �������� ������ EcORAEO
            foreach (int eventEntity in ecORAEONewCreatedEventFilter.Value)
            {
                //������� �������� �������
                world.Value.DelEntity(eventEntity);
            }
        }
    }
}