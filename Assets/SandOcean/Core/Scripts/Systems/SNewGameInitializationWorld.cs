
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Player;
using SandOcean.Diplomacy;
using SandOcean.Map;

namespace SandOcean
{
    public class SNewGameInitializationWorld : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������� �������
        readonly EcsPoolInject<EStartNewGamePlayerCreating> startNewGamePlayerCreatingEventPool = default;

        //������� �����
        readonly EcsPoolInject<EMapGeneration> mapGenerationEventPool = default;

        //������� ����������
        readonly EcsPoolInject<EOrganizationCreating> organizationCreatingEventPool = default;

        //����� �������
        readonly EcsFilterInject<Inc<EStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<EStartNewGame> startNewGameEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //���� ��������� ������� ������ ����� ����
                ref EStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //����������� �������� ������
                PlayerCreatingEvent(
                    "Player",
                    "PlayerOrganization");

                //����������� �������� ����������� ��
                OrganizationCreatingEvent(
                    "AIOrganization");

                int eventEntity = world.Value.NewEntity();
                ref EMapGeneration mapGenerationEvent
                    = ref mapGenerationEventPool.Value.Add(eventEntity);

                mapGenerationEvent.chunkCountX
                    = 5;
                mapGenerationEvent.chunkCountZ
                    = 5;

                mapGenerationEvent.islandCount = 100;
            }
        }

        void PlayerCreatingEvent(
            string playerName,
            string playerOrganizationName)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������
            int eventEntity = world.Value.NewEntity();
            ref EStartNewGamePlayerCreating playerCreatingEvent
                = ref startNewGamePlayerCreatingEventPool.Value.Add(eventEntity);

            //��������� ��� ������
            playerCreatingEvent.playerName
                = playerName;

            //��������� �������� ��� �����������
            playerCreatingEvent.playerOrganizationName
                = playerOrganizationName;
        }

        void OrganizationCreatingEvent(
            string organizationName,
            bool isPlayerOrganization = false,
            EcsPackedEntity ownerPlayerPE = new())
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� �����������
            int eventEntity = world.Value.NewEntity();
            ref EOrganizationCreating organizationCreatingEvent = ref organizationCreatingEventPool.Value.Add(eventEntity);

            //��������� �������� �����������
            organizationCreatingEvent.organizationName = organizationName;

            //���� ��� �����������, ������������� ������
            if (isPlayerOrganization == true)
            {
                //��������� ��� 
                organizationCreatingEvent.isPlayerOrganization = isPlayerOrganization;

                //��������� PE ������-���������
                organizationCreatingEvent.ownerPlayerPE = ownerPlayerPE;
            }
        }
    }
}