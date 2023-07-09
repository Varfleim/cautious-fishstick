using System;
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Player;
using SandOcean.Technology;
using SandOcean.AEO.RAEO;

namespace SandOcean.Diplomacy
{
    public class SOrganizationInitialization : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;

        //Игроки
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //События дипломатии
        readonly EcsFilterInject<Inc<EOrganizationCreating>> organizationCreatingEventFilter = default;
        readonly EcsPoolInject<EOrganizationCreating> organizationCreatingEventPool = default;

        //События технологий
        readonly EcsPoolInject<ETechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;

        //События административно-экономических объектов
        readonly EcsPoolInject<SRORAEOCreate> oRAEOCreateSRPool = default;

        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<RuntimeData> runtimeData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого события создания организации
            foreach (int organizationCreatingEventEntity in organizationCreatingEventFilter.Value)
            {
                //Берём компонент события создания организации
                ref EOrganizationCreating organizationCreatingEvent = ref organizationCreatingEventPool.Value.Get(organizationCreatingEventEntity);

                //Создаём новую организацию
                OrganizationCreating(ref organizationCreatingEvent);

                world.Value.DelEntity(organizationCreatingEventEntity);
            }
        }

        void OrganizationCreating(
            ref EOrganizationCreating organizationCreatingEvent)
        {
            //Создаём новую сущность и назначаем ей компонент организации
            int organizationEntity = world.Value.NewEntity();
            ref COrganization organization = ref organizationPool.Value.Add(organizationEntity);

            //Заполняем основные данные организации
            organization = new(
                world.Value.PackEntity(organizationEntity),
                runtimeData.Value.organizationsCount,
                organizationCreatingEvent.organizationName);

            //Обновляем счётчик организаций
            runtimeData.Value.organizationsCount++;

            //Если организация принадлежит игроку
            if (organizationCreatingEvent.isPlayerOrganization == true)
            {
                //Берём компонент игрока
                organizationCreatingEvent.ownerPlayerPE.Unpack(world.Value, out int playerEntity);
                ref CPlayer player = ref playerPool.Value.Get(playerEntity);

                //Заносим PE принадлежащей организации
                player.ownedOrganizationPE = organization.selfPE;

                //Заносим PE игрока-владельца
                organization.ownerPlayerPE = player.selfPE;

                //Заносим PE принадлежащей организации в данные ввода
                inputData.Value.playerOrganizationPE = organization.selfPE;
            }

            //Назначаем организации её технологии (базовые и прописанные в шаблоне)
            OrganizationAssignTechnologies(ref organization);

            //Пересчитываем модификаторы технологий организации
            TechnologyCalculateOrganizationModifiersEvent(ref organization);

            //Создаём набор контента для данных организации
            OrganizationContentSetCreating(ref organization);

            //Назначаем организации компонент самозапроса создания ORAEO
            ref SRORAEOCreate oRAEOCreateEvent = ref oRAEOCreateSRPool.Value.Add(organizationEntity);
        }

        void OrganizationAssignTechnologies(
            ref COrganization organization)
        {
            //Берём число описаний наборов контента
            int contentSetDescriptionsNumber
                = contentData.Value.contentSetDescriptions.Length;

            //Создаём в данных организации массив словарей для технологий
            organization.technologies
                = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

            //Для каждого набора контента меньше данного числа
            for (int a = 0; a < contentSetDescriptionsNumber; a++)
            {
                //Создаём словарь
                organization.technologies[a]
                    = new Dictionary<int, DOrganizationTechnology>();

                //Для каждой технологии в наборе контента
                for (int b = 0; b < contentData.Value.contentSets[a].technologies.Length; b++)
                {
                    //Если технология является базовой
                    if (contentData.Value.contentSets[a].technologies[b].IsBaseTechnology
                        == true)
                    {
                        //Заносим её в список открытых технологий
                        organization.technologies[a].Add(
                            b,
                            new DOrganizationTechnology(
                                true));
                    }
                }
            }
        }

        void TechnologyCalculateOrganizationModifiersEvent(
            ref COrganization organization)
        {
            //Создаём новую сущность и назначаем ей компонент события рассчёта модификаторов технологий
            int eventEntity = world.Value.NewEntity();
            ref ETechnologyCalculateModifiers technologyCalculateModifiersEvent
                = ref technologyCalculateModifiersEventPool.Value.Add(eventEntity);

            //Указываем PE организации, для которой требуется пересчитать модификаторы
            technologyCalculateModifiersEvent.organizationPE
                = organization.selfPE;
        }

        void OrganizationContentSetCreating(
            ref COrganization organization)
        {
            //Сохраняем длину массива наборов контента
            int contentSetsArrayLength
                = contentData.Value.contentSets.Length;

            //Сохраняем в данных организации индекс её набора контента
            organization.contentSetIndex
                = contentData.Value.contentSets.Length;

            UnityEngine.Debug.LogWarning(organization.contentSetIndex);

            //Создаём новый набор контента для данных организации
            DContentSet organizationContentSet 
                = new(
                    organization.selfName);

            //Изменяем размер массива наборов контента
            Array.Resize(
                ref contentData.Value.contentSets,
                contentData.Value.contentSets.Length
                + 1);

            //Заносим новый набор контента в массив
            contentData.Value.contentSets[contentData.Value.contentSets.Length - 1]
                = organizationContentSet;

            //Изменяем размер массива названий наборов контента
            Array.Resize(
                ref contentData.Value.contentSetNames,
                contentData.Value.contentSetNames.Length
                + 1);

            //Заносим новое название в массив
            contentData.Value.contentSetNames[contentData.Value.contentSetNames.Length - 1]
                = organization.selfName;
        }
    }
}