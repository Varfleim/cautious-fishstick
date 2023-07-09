
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;

namespace SandOcean
{
    public class SEventsControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������� �����
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecOLAEONewCreatedEventFilter = default;
        readonly EcsPoolInject<EEcORAEONewCreated> ecOLAEONewCreatedEventPool = default;

        //������
        EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ������ EcOLAEO
            foreach (int eventEntity in ecOLAEONewCreatedEventFilter.Value)
            {
                //���� ��������� �������
                ref EEcORAEONewCreated ecOLAEONewCreatedEvent = ref ecOLAEONewCreatedEventPool.Value.Get(eventEntity);

                //���� ������� ����� ����������
                if (inputData.Value.mapMode == MapMode.Distance)
                {
                    //���� �����������-�������� OLAEO - ����������� ������
                    if (ecOLAEONewCreatedEvent.organizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
                    {
                        //���� �������� ����� ������ ����� ���
                        if (changeMapModeRequestFilter.Value.GetEntitiesCount() == 0)
                        {
                            //�� ����������� ���������� ������ ����� ����������
                            MapChangeModeRequest(MapMode.Distance);
                        }
                    }
                }
            }
        }

        void MapChangeModeRequest(
            MapMode mapMode)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ������ �����
            int changeMapModeRequestEntity = world.Value.NewEntity();
            ref RChangeMapMode changeMapModeRequest = ref changeMapModeRequestPool.Value.Add(changeMapModeRequestEntity);

            //��������� ��������� ����� �����
            changeMapModeRequest.mapMode = mapMode;
        }
    }
}