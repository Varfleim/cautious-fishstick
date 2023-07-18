
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Diplomacy;

namespace SandOcean.Player
{
    public class SPlayerInitialization : IEcsRunSystem
    {
        //����
        readonly EcsWorldInject world = default;

        //������
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //������� �������
        readonly EcsFilterInject<Inc<EStartNewGamePlayerCreating>> startNewGamePlayerCreatingEventFilter = default;
        readonly EcsPoolInject<EStartNewGamePlayerCreating> startNewGamePlayerCreatingEventPool = default;

        //������� ����������
        readonly EcsPoolInject<EOrganizationCreating> organizationCreatingEventPool = default;
        
        //������
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //��� ������� ������� �������� ������ � ����� ����
            foreach (int newGamePlayerCreatingEventEntity
                in startNewGamePlayerCreatingEventFilter.Value)
            {
                //���� ��������� ������� �������� ������ � ����� ����
                ref EStartNewGamePlayerCreating playerCreatingEvent
                    = ref startNewGamePlayerCreatingEventPool.Value.Get(
                        newGamePlayerCreatingEventEntity);

                //������ ������ ������
                PlayerCreating(
                    playerCreatingEvent.playerName,
                    playerCreatingEvent.playerOrganizationName);

                world.Value.DelEntity(
                    newGamePlayerCreatingEventEntity);
            }
        }

        void PlayerCreating(
            string playerName,
            string playerOrganizationName)
        {
            //������ ����� �������� � ��������� �� ��������� ������
            int playerEntity 
                = world.Value.NewEntity();
            ref CPlayer player
                = ref playerPool.Value.Add(
                    playerEntity);

            //��������� ����������� PE
            player.selfPE
                = world.Value.PackEntity(
                    playerEntity);

            //������� PE ������ � ������ �����
            inputData.Value.playerPE
                = player.selfPE;

            //��������� ��� ������
            player.selfName
                = playerName;

            //������ �������, ������������� �������� ����������� ��� ������
            OrganizationCreatingEvent(
                playerOrganizationName,
                true,
                player.selfPE);
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