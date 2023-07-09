
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.UI.Events;
using SandOcean.Map;
using SandOcean.Ship;

namespace SandOcean
{
    public class SNewGameInitializationShips : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //�����
        //readonly EcsPoolInject<CHexChunk> chunkPool = default;

        //readonly EcsPoolInject<CHexRegion> regionPool = default;

        //������� ��������
        //readonly EcsPoolInject<EShipGroupCreating> shipGroupCreatingEventPool = default;

        //����� �������
        readonly EcsFilterInject<Inc<EStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<EStartNewGame> startNewGameEventPool = default;

        //������
        //readonly EcsCustomInject<SpaceGenerationData> spaceGenerationData = default;
        //readonly EcsCustomInject<InputData> inputData = default;

        //readonly EcsCustomInject<EUI> eUI = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //���� ��������� ������� ������ ����� ����
                ref EStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //���� ��������� ������� �������
                /*spaceGenerationData.Value.cellPEs[0].Unpack(world.Value, out int regionEntity);
                ref CHexCell region = ref regionPool.Value.Get(regionEntity);

                //������ ����� �������� � ��������� �� ��������� ������� �������� ������ �������� � �����
                int shipGroupCreatingEventEntity = world.Value.NewEntity();
                ref EShipGroupCreating shipGroupCreatingEvent = ref shipGroupCreatingEventPool.Value.Add(shipGroupCreatingEventEntity);

                //��������� ����������� ������ ��� �����������-���������
                shipGroupCreatingEvent.ownerOrganizationPE = inputData.Value.playerOrganizationPE;

                //��������� ������ ��� ������������
                shipGroupCreatingEvent.parentRegionPE = region.selfPE;

                //��������� �������, ��� ��������� ������� ������ ��������
                shipGroupCreatingEvent.position = new(0, 0, 0);

                //���������� ����� ������� ����������� ��������
                shipGroupCreatingEvent.orderedShips = new DOrderedShipSpace[(int)eUI.Value.newGameMenuWindow.startShipsNumberSlider.value];

                //���������� ��������� ����� ���������
                double distance = 2;

                //��� ������� ������� � �������
                for (int a = 0; a < shipGroupCreatingEvent.orderedShips.Length; a++)
                {
                    //���� ��� ������ ������� � �������
                    if (a == 0)
                    {
                        //������� ������ ������ ������������ ������� � ������
                        shipGroupCreatingEvent.orderedShips[a] = new(
                            new DVector3(
                                distance, 0, 0),
                            0,
                            0);
                    }
                    //�����
                    else
                    {
                        //������� ������ ������ ������������ ������� � ������
                        shipGroupCreatingEvent.orderedShips[a] = new(
                            new(
                                shipGroupCreatingEvent.orderedShips[a - 1].shipPosition.x + distance,
                                0,
                                0),
                            0,
                            0);
                    }
                }*/

                world.Value.DelEntity(startNewGameEventEntity);
            }
        }
    }
}