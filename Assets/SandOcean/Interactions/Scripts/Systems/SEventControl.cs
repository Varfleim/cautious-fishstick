
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;
using Leopotam.EcsLite.ExtendedSystems;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;
using SandOcean.Diplomacy;

namespace SandOcean
{
    public class SEventControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������� ����
        readonly EcsPoolInject<RGameCreatePanel> gameCreatePanelRequestPool = default;

        //������� �����
        readonly EcsFilterInject<Inc<RChangeMapMode>> changeMapModeRequestFilter = default;
        readonly EcsPoolInject<RChangeMapMode> changeMapModeRequestPool = default;

        //������� ����������
        readonly EcsFilterInject<Inc<EOrganizationNewCreated>> organizationNewCreatedEventFilter = default;
        readonly EcsPoolInject<EOrganizationNewCreated> organizationNewCreatedEventPool = default;

        //������� ���������������-������������� ��������
        readonly EcsFilterInject<Inc<EEcORAEONewCreated>> ecOLAEONewCreatedEventFilter = default;
        readonly EcsPoolInject<EEcORAEONewCreated> ecOLAEONewCreatedEventPool = default;

        //����� �������
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameRequestFilter = default;

        readonly EcsPoolInject<EcsGroupSystemState> ecsGroupSystemStatePool = default;


        //������
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int eventEntity in startNewGameRequestFilter.Value)
            {
                //����������� ���������� ������ ������ "NewGame"
                EcsGroupSystemStateEvent("NewGame", false);

                world.Value.DelEntity(eventEntity);
            }

            //��� ������� ������� �������� ����� �����������
            foreach (int eventEntity in organizationNewCreatedEventFilter.Value)
            {
                //���� �������
                ref EOrganizationNewCreated eventComp = ref organizationNewCreatedEventPool.Value.Get(eventEntity);

                //����������� �������� �������� ������ ORAEO
                GameCreatePanelRequest(
                    CreatingPanelType.ORAEOBriefInfoPanel,
                    eventComp.organizationPE);
            }

            //��� ������� ������� �������� ������ EcOLAEO
            foreach (int eventEntity in ecOLAEONewCreatedEventFilter.Value)
            {
                //���� �������
                ref EEcORAEONewCreated eventComp = ref ecOLAEONewCreatedEventPool.Value.Get(eventEntity);

                //���� ������� ����� ����������
                if (inputData.Value.mapMode == MapMode.Distance)
                {
                    //���� �����������-�������� OLAEO - ����������� ������
                    if (eventComp.organizationPE.EqualsTo(inputData.Value.playerOrganizationPE) == true)
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

        void GameCreatePanelRequest(
            CreatingPanelType creatingPanelType,
            EcsPackedEntity ownerOrganizationPE)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������ � ����
            int requestEntity = world.Value.NewEntity();
            ref RGameCreatePanel requestComp = ref gameCreatePanelRequestPool.Value.Add(requestEntity);

            //��������� �������� ������ �������
            requestComp = new(
                creatingPanelType,
                ownerOrganizationPE);
        }

        void MapChangeModeRequest(
            MapMode mapMode)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ������ �����
            int requestEntity = world.Value.NewEntity();
            ref RChangeMapMode requestComp = ref changeMapModeRequestPool.Value.Add(requestEntity);

            //��������� ��������� ����� �����
            requestComp.mapMode = mapMode;
        }

        void EcsGroupSystemStateEvent(
            string systemGroupName,
            bool systemGroupState)
        {
            //������ ����� �������� � ��������� �� ��������� ������� ����� ��������� ������ ������
            int eventEntity = world.Value.NewEntity();
            ref EcsGroupSystemState eventComp = ref ecsGroupSystemStatePool.Value.Add(eventEntity);

            //��������� �������� ������ ������
            eventComp.Name = systemGroupName;
            //��������� ��������� ������ ������
            eventComp.State = systemGroupState;
        }
    }
}