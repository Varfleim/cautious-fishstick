
using System;
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.UI;
using SandOcean.Player;
using SandOcean.Technology;
using SandOcean.Map.Events;
using SandOcean.AEO.RAEO;

namespace SandOcean.Diplomacy
{
    public class SOrganizationControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Игроки
        readonly EcsPoolInject<CPlayer> playerPool = default;

        //Дипломатия
        readonly EcsFilterInject<Inc<COrganization>> organizationFilter = default;
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsFilterInject<Inc<CRegionAEO>> regionAEOFilter = default;
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;


        //События карты
        readonly EcsPoolInject<RRegionInitializer> regionInitializerPool = default;

        //События дипломатии
        readonly EcsFilterInject<Inc<ROrganizationCreating>> organizationCreatingRequestFilter = default;
        readonly EcsPoolInject<ROrganizationCreating> organizationCreatingRequestPool = default;

        readonly EcsPoolInject<EOrganizationNewCreated> organizationNewCreatedEventPool = default;

        //События технологий
        readonly EcsPoolInject<RTechnologyCalculateModifiers> technologyCalculateModifiersRequestPool = default;

        //События административно-экономических объектов
        readonly EcsFilterInject<Inc<SRORAEOCreate, COrganization>> oRAEOCreateSRFilter = default;
        readonly EcsPoolInject<SRORAEOCreate> oRAEOCreateSRPool = default;


        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;
        readonly EcsCustomInject<InputData> inputData = default;

        readonly EcsCustomInject<RuntimeData> runtimeData = default;

        public void Run(IEcsSystems systems)
        {
            //Для каждого запроса создания организации
            foreach (int organizationCreatingRequestEntity in organizationCreatingRequestFilter.Value)
            {
                //Берём компонент запроса
                ref ROrganizationCreating organizationCreatingRequest = ref organizationCreatingRequestPool.Value.Get(organizationCreatingRequestEntity);

                //Создаём новую организацию
                OrganizationCreating(ref organizationCreatingRequest);

                world.Value.DelEntity(organizationCreatingRequestEntity);
            }

            //Если фильтр самозапросов создания ORAEO не пуст
            if (oRAEOCreateSRFilter.Value.GetEntitiesCount() > 0)
            {
                //Создаём ORAEO
                ORAEOCreating();
            }
        }

        void OrganizationCreating(
            ref ROrganizationCreating organizationCreatingRequest)
        {
            //Создаём новую сущность и назначаем ей компонент организации
            int organizationEntity = world.Value.NewEntity();
            ref COrganization organization = ref organizationPool.Value.Add(organizationEntity);

            //Заполняем основные данные организации
            organization = new(
                world.Value.PackEntity(organizationEntity), runtimeData.Value.organizationsCount, organizationCreatingRequest.organizationName);

            //Обновляем счётчик организаций
            runtimeData.Value.organizationsCount++;

            //Если организация принадлежит игроку
            if (organizationCreatingRequest.isPlayerOrganization == true)
            {
                //Берём компонент игрока
                organizationCreatingRequest.ownerPlayerPE.Unpack(world.Value, out int playerEntity);
                ref CPlayer player = ref playerPool.Value.Get(playerEntity);

                //Заносим PE принадлежащей организации
                player.ownedOrganizationPE = organization.selfPE;

                //Заносим PE игрока-владельца
                organization.ownerPlayerPE = player.selfPE;

                //Заносим PE организации игрока в данные ввода
                inputData.Value.playerOrganizationPE = organization.selfPE;
            }

            //Назначаем организации её технологии (базовые и прописанные в шаблоне)
            OrganizationAssignTechnologies(ref organization);

            //Пересчитываем модификаторы технологий организации
            TechnologyCalculateOrganizationModifiersRequest(ref organization);

            //Создаём набор контента для данных организации
            OrganizationContentSetCreating(ref organization);

            //Назначаем организации компонент самозапроса создания ORAEO
            ref SRORAEOCreate oRAEOCreateRequest = ref oRAEOCreateSRPool.Value.Add(organizationEntity);

            //Создаём новую сущность и назначаем ей компонент запроса инициализатора региона
            int regionInitializerRequestEntity = world.Value.NewEntity();
            ref RRegionInitializer regionInitializerRequest = ref regionInitializerPool.Value.Add(regionInitializerRequestEntity);

            //Заполняем основные данные запроса
            regionInitializerRequest = new(
                organization.selfPE, 
                new DContentObjectLink[1], 
                10);

            //Создаём событие, сообщающее о создании новой организации
            OrganizationNewCreatedEvent( ref organization);
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

        void TechnologyCalculateOrganizationModifiersRequest(
            ref COrganization organization)
        {
            //Создаём новую сущность и назначаем ей компонент запроса рассчёта модификаторов технологий
            int requestEntity = world.Value.NewEntity();
            ref RTechnologyCalculateModifiers technologyCalculateModifiersRequest
                = ref technologyCalculateModifiersRequestPool.Value.Add(requestEntity);

            //Указываем PE организации, для которой требуется пересчитать модификаторы
            technologyCalculateModifiersRequest.organizationPE
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

        void ORAEOCreating()
        {
            //Создаём временный список DORAEO
            List<DOrganizationRAEO> tempDORAEO = new();

            //Определяем количество организаций
            int organizationsCount = organizationFilter.Value.GetEntitiesCount();

            //Для каждого RAEO
            foreach (int rAEOEntity in regionAEOFilter.Value)
            {
                //Берём компонент RAEO
                ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(rAEOEntity);

                //Очищаем временный список
                tempDORAEO.Clear();

                //Для каждого самозапроса создания ORAEO
                foreach (int oRAEOCreateSelfRequestEntity in oRAEOCreateSRFilter.Value)
                {
                    //Берём компонент самозапроса и компонент организации
                    ref COrganization organization = ref organizationPool.Value.Get(oRAEOCreateSelfRequestEntity);

                    //Создаём новую сущность и назначаем ей компонент ExplorationORAEO
                    int oRAEOEntity = world.Value.NewEntity();
                    ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Add(oRAEOEntity);

                    //Заполняем основные данные ExORAEO
                    exORAEO = new(
                        world.Value.PackEntity(oRAEOEntity),
                        organization.selfPE,
                        rAEO.selfPE,
                        0);

                    //Создаём структуру для хранения данных организации непосредственно в RAEO
                    DOrganizationRAEO organizationRAEOData = new(
                        exORAEO.selfPE,
                        ORAEOType.Exploration);

                    //Заносим её во временный список
                    tempDORAEO.Add(organizationRAEOData);
                }

                //Сохраняем старый размер массива
                int oldArraySize = rAEO.organizationRAEOs.Length;

                //Расширяем массив DORAEO
                Array.Resize(
                    ref rAEO.organizationRAEOs,
                    organizationsCount);

                //Для каждого DORAEO во временном массиве
                for (int a = 0; a < tempDORAEO.Count; a++)
                {
                    //Вставляем DORAEO в массив по индексу
                    rAEO.organizationRAEOs[oldArraySize++] = tempDORAEO[a];
                }

                UnityEngine.Debug.LogWarning(rAEO.organizationRAEOs.Length);
            }

            //Для каждого самозапроса создания ORAEO
            foreach (int oRAEOCreateSelfRequestEntity in oRAEOCreateSRFilter.Value)
            {
                //Удаляем с сущности организации компонент самозапроса
                oRAEOCreateSRPool.Value.Del(oRAEOCreateSelfRequestEntity);
            }
        }

        void OrganizationNewCreatedEvent(
            ref COrganization organization)
        {
            //Создаём новую сущность и назначаем ей компонент события создания новой организации
            int eventEntity = world.Value.NewEntity();
            ref EOrganizationNewCreated eventComp = ref organizationNewCreatedEventPool.Value.Add(eventEntity);

            //Заполняем данные события
            eventComp = new(organization.selfPE);
        }
    }
}