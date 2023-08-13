
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Player;
using SandOcean.Diplomacy;
using SandOcean.Map.Events;

namespace SandOcean
{
    public class SNewGameInitializationMain : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;


        //������� �������
        readonly EcsPoolInject<RPlayerCreating> playerCreatingRequestPool = default;

        //������� �����
        readonly EcsPoolInject<RMapGeneration> mapGenerationRequestPool = default;

        //������� ����������
        readonly EcsPoolInject<ROrganizationCreating> organizationCreatingRequestPool = default;

        //����� �������
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameRequestFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� ������ ����� ����
            foreach (int startNewGameRequestEntity in startNewGameRequestFilter.Value)
            {
                //���� ��������� �������
                ref RStartNewGame startNewGameRequest = ref startNewGameRequestPool.Value.Get(startNewGameRequestEntity);

                //����������� �������� ������
                PlayerCreatingRequest(
                    "Player",
                    "PlayerOrganization");

                //����������� ��������� �����
                MapGenerationRequest(7, 4, 50);

                //����������� �������� ����������� ��
                OrganizationCreatingRequest("AIOrganization");
            }
        }

        void PlayerCreatingRequest(
            string playerName,
            string playerOrganizationName)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� ������
            int requestEntity = world.Value.NewEntity();
            ref RPlayerCreating playerCreatingRequest = ref playerCreatingRequestPool.Value.Add(requestEntity);

            //��������� ��� ������
            playerCreatingRequest.playerName = playerName;

            //��������� �������� ��� �����������
            playerCreatingRequest.playerOrganizationName = playerOrganizationName;
        }

        void MapGenerationRequest(
            int chunkCountX, int chunkCountZ,
            int subdivisions)
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� �����
            int requestEntity = world.Value.NewEntity();
            ref RMapGeneration mapGenerationRequest = ref mapGenerationRequestPool.Value.Add(requestEntity);

            //��������� ������ �������
            mapGenerationRequest = new(chunkCountX, chunkCountZ, subdivisions);
        }

        void OrganizationCreatingRequest(
            string organizationName,
            bool isPlayerOrganization = false,
            EcsPackedEntity ownerPlayerPE = new())
        {
            //������ ����� �������� � ��������� �� ��������� ������� �������� �����������
            int requestEntity = world.Value.NewEntity();
            ref ROrganizationCreating organizationCreatingRequest = ref organizationCreatingRequestPool.Value.Add(requestEntity);

            //��������� �������� �����������
            organizationCreatingRequest.organizationName = organizationName;

            //���� ��� �����������, ������������� ������
            if (isPlayerOrganization == true)
            {
                //��������� ��� 
                organizationCreatingRequest.isPlayerOrganization = isPlayerOrganization;

                //��������� PE ������-���������
                organizationCreatingRequest.ownerPlayerPE = ownerPlayerPE;
            }
        }
    }
}