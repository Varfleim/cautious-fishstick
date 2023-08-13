
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Diplomacy;

namespace SandOcean.Player
{
    public class SPlayerControl : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //������� �������
        readonly EcsFilterInject<Inc<RPlayerCreating>> playerCreatingRequestFilter = default;
        readonly EcsPoolInject<RPlayerCreating> playerCreatingRequestPool;

        //������� ����������
        readonly EcsPoolInject<ROrganizationCreating> organizationCreatingRequestPool = default;
        
        //������
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ������ � ����� ����
            foreach (int playerCreatingRequestEntity in playerCreatingRequestFilter.Value)
            {
                //���� ��������� �������
                ref RPlayerCreating playerCreatingRequest = ref playerCreatingRequestPool.Value.Get(playerCreatingRequestEntity);

                //������ ������ ������
                PlayerCreating(ref playerCreatingRequest);

                world.Value.DelEntity(playerCreatingRequestEntity);
            }
        }

        void PlayerCreating(
            ref RPlayerCreating playerCreatingRequest)
        {
            //������ ����� �������� � ��������� �� ��������� ������
            int playerEntity = world.Value.NewEntity();
            ref CPlayer player = ref playerPool.Value.Add(playerEntity);

            //��������� �������� ������ ������
            player = new(world.Value.PackEntity(playerEntity), playerCreatingRequest.playerName);

            //������� PE ������ � ������ �����
            inputData.Value.playerPE = player.selfPE;

            //����������� �������� ����������� ������
            OrganizationCreatingRequest(
                playerCreatingRequest.playerOrganizationName,
                true,
                player.selfPE);
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