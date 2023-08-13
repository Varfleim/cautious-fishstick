
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Diplomacy;

namespace SandOcean.Player
{
    public class SPlayerControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Игроки
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //События игроков
        readonly EcsFilterInject<Inc<RPlayerCreating>> playerCreatingRequestFilter = default;
        readonly EcsPoolInject<RPlayerCreating> playerCreatingRequestPool;

        //События дипломатии
        readonly EcsPoolInject<ROrganizationCreating> organizationCreatingRequestPool = default;
        
        //Данные
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса создания игрока в новой игре
            foreach (int playerCreatingRequestEntity in playerCreatingRequestFilter.Value)
            {
                //Берём компонент запроса
                ref RPlayerCreating playerCreatingRequest = ref playerCreatingRequestPool.Value.Get(playerCreatingRequestEntity);

                //Создаём нового игрока
                PlayerCreating(ref playerCreatingRequest);

                world.Value.DelEntity(playerCreatingRequestEntity);
            }
        }

        void PlayerCreating(
            ref RPlayerCreating playerCreatingRequest)
        {
            //Создаём новую сущность и назначаем ей компонент игрока
            int playerEntity = world.Value.NewEntity();
            ref CPlayer player = ref playerPool.Value.Add(playerEntity);

            //Заполняем основные данные игрока
            player = new(world.Value.PackEntity(playerEntity), playerCreatingRequest.playerName);

            //Заносим PE игрока в данные ввода
            inputData.Value.playerPE = player.selfPE;

            //Запрашиваем создание организации игрока
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
            //Создаём новую сущность и назначаем ей компонент запроса создания организации
            int requestEntity = world.Value.NewEntity();
            ref ROrganizationCreating organizationCreatingRequest = ref organizationCreatingRequestPool.Value.Add(requestEntity);

            //Указываем название организации
            organizationCreatingRequest.organizationName = organizationName;

            //Если это организация, принадлежащая игроку
            if (isPlayerOrganization == true)
            {
                //Указываем это 
                organizationCreatingRequest.isPlayerOrganization = isPlayerOrganization;

                //Указываем PE игрока-владельца
                organizationCreatingRequest.ownerPlayerPE = ownerPlayerPE;
            }
        }
    }
}