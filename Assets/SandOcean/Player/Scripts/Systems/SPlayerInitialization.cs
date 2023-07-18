
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Diplomacy;

namespace SandOcean.Player
{
    public class SPlayerInitialization : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Игроки
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //События игроков
        readonly EcsFilterInject<Inc<EStartNewGamePlayerCreating>> startNewGamePlayerCreatingEventFilter = default;
        readonly EcsPoolInject<EStartNewGamePlayerCreating> startNewGamePlayerCreatingEventPool = default;

        //События дипломатии
        readonly EcsPoolInject<EOrganizationCreating> organizationCreatingEventPool = default;
        
        //Данные
        readonly EcsCustomInject<InputData> inputData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события создания игрока в новой игре
            foreach (int newGamePlayerCreatingEventEntity
                in startNewGamePlayerCreatingEventFilter.Value)
            {
                //Берём компонент события создания игрока в новой игре
                ref EStartNewGamePlayerCreating playerCreatingEvent
                    = ref startNewGamePlayerCreatingEventPool.Value.Get(
                        newGamePlayerCreatingEventEntity);

                //Создаём нового игрока
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
            //Создаём новую сущность и назначаем ей компонент игрока
            int playerEntity 
                = world.Value.NewEntity();
            ref CPlayer player
                = ref playerPool.Value.Add(
                    playerEntity);

            //Сохраняем собственную PE
            player.selfPE
                = world.Value.PackEntity(
                    playerEntity);

            //Заносим PE игрока в данные ввода
            inputData.Value.playerPE
                = player.selfPE;

            //Указываем имя игрока
            player.selfName
                = playerName;

            //Создаём событие, запрашивающее создание организации для игрока
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