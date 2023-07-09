
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Player;
using SandOcean.Diplomacy;
using SandOcean.Map;

namespace SandOcean
{
    public class SNewGameInitializationWorld : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //События игроков
        readonly EcsPoolInject<EStartNewGamePlayerCreating> startNewGamePlayerCreatingEventPool = default;

        //События карты
        readonly EcsPoolInject<EMapGeneration> mapGenerationEventPool = default;

        //События дипломатии
        readonly EcsPoolInject<EOrganizationCreating> organizationCreatingEventPool = default;

        //Общие события
        readonly EcsFilterInject<Inc<EStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<EStartNewGame> startNewGameEventPool = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события начала новой игры
            foreach (int startNewGameEventEntity
                in startNewGameEventFilter.Value)
            {
                //Берём компонент события начала новой игры
                ref EStartNewGame startNewGameEvent
                    = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //Запрашиваем создание игрока
                PlayerCreatingEvent(
                    "Player",
                    "PlayerOrganization");

                //Запрашиваем создание организации ИИ
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
            //Создаём новую сущность и назначаем ей компонент события создания игрока
            int eventEntity = world.Value.NewEntity();
            ref EStartNewGamePlayerCreating playerCreatingEvent
                = ref startNewGamePlayerCreatingEventPool.Value.Add(eventEntity);

            //Указываем имя игрока
            playerCreatingEvent.playerName
                = playerName;

            //Указываем название его организации
            playerCreatingEvent.playerOrganizationName
                = playerOrganizationName;
        }

        void OrganizationCreatingEvent(
            string organizationName,
            bool isPlayerOrganization = false,
            EcsPackedEntity ownerPlayerPE = new())
        {
            //Создаём новую сущность и назначаем ей компонент события создания организации
            int eventEntity = world.Value.NewEntity();
            ref EOrganizationCreating organizationCreatingEvent = ref organizationCreatingEventPool.Value.Add(eventEntity);

            //Указываем название организации
            organizationCreatingEvent.organizationName = organizationName;

            //Если это организация, принадлежащая игроку
            if (isPlayerOrganization == true)
            {
                //Указываем это 
                organizationCreatingEvent.isPlayerOrganization = isPlayerOrganization;

                //Указываем PE игрока-владельца
                organizationCreatingEvent.ownerPlayerPE = ownerPlayerPE;
            }
        }
    }
}