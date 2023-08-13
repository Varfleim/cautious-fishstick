
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Player;
using SandOcean.Diplomacy;
using SandOcean.Map.Events;

namespace SandOcean
{
    public class SNewGameInitializationMain : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //События игроков
        readonly EcsPoolInject<RPlayerCreating> playerCreatingRequestPool = default;

        //События карты
        readonly EcsPoolInject<RMapGeneration> mapGenerationRequestPool = default;

        //События дипломатии
        readonly EcsPoolInject<ROrganizationCreating> organizationCreatingRequestPool = default;

        //Общие события
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameRequestFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameRequestPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса начала новой игры
            foreach (int startNewGameRequestEntity in startNewGameRequestFilter.Value)
            {
                //Берём компонент запроса
                ref RStartNewGame startNewGameRequest = ref startNewGameRequestPool.Value.Get(startNewGameRequestEntity);

                //Запрашиваем создание игрока
                PlayerCreatingRequest(
                    "Player",
                    "PlayerOrganization");

                //Запрашиваем генерацию карты
                MapGenerationRequest(7, 4, 50);

                //Запрашиваем создание организации ИИ
                OrganizationCreatingRequest("AIOrganization");
            }
        }

        void PlayerCreatingRequest(
            string playerName,
            string playerOrganizationName)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания игрока
            int requestEntity = world.Value.NewEntity();
            ref RPlayerCreating playerCreatingRequest = ref playerCreatingRequestPool.Value.Add(requestEntity);

            //Указываем имя игрока
            playerCreatingRequest.playerName = playerName;

            //Указываем название его организации
            playerCreatingRequest.playerOrganizationName = playerOrganizationName;
        }

        void MapGenerationRequest(
            int chunkCountX, int chunkCountZ,
            int subdivisions)
        {
            //Создаём новую сущность и назначаем ей компонент запроса создания карты
            int requestEntity = world.Value.NewEntity();
            ref RMapGeneration mapGenerationRequest = ref mapGenerationRequestPool.Value.Add(requestEntity);

            //Заполняем данные запроса
            mapGenerationRequest = new(chunkCountX, chunkCountZ, subdivisions);
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