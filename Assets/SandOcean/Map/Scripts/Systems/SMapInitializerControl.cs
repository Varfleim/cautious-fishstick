
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Map.Events;
using SandOcean.Organization;
using SandOcean.AEO.RAEO;

namespace SandOcean.Map
{
    public class SMapInitializerControl : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Карта
        readonly EcsFilterInject<Inc<CHexRegion>> regionFilter = default;
        readonly EcsPoolInject<CHexRegion> regionPool = default;

        //readonly EcsPoolInject<CHexRegionGenerationData> regionGenerationDataPool = default;

        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;

        //Административно-экономические объекты
        readonly EcsPoolInject<CRegionAEO> regionAEOPool = default;

        readonly EcsPoolInject<CExplorationORAEO> explorationORAEOPool = default;


        //События карты
        readonly EcsFilterInject<Inc<RRegionInitializer>> regionInitializerFilter = default;
        readonly EcsPoolInject<RRegionInitializer> regionInitializerPool = default;

        //События административно-экономических объектов
        readonly EcsPoolInject<SRORAEOAction> oRAEOActionSelfRequestPool = default;


        //Данные
        //readonly EcsCustomInject<MapGenerationData> mapGenerationData = default;

        public void Run(IEcsSystems systems)
        {
            //Применяем инициализаторы организаций
            MapRegionOrganizationInitializers();

            //Генерируем прочие инициализаторы


            //Применяем прочие инициализаторы
            MapRegionMiscellaneousInitializers();
        }

        void MapRegionOrganizationInitializers()
        {
            //Для каждого инициализатора региона
            /*foreach (int regionInitializerEntity in regionInitializerFilter.Value)
            {
                //Берём инициализатор
                ref RRegionInitializer regionInitializer = ref regionInitializerPool.Value.Get(regionInitializerEntity);

                //Проходим по дочерним инициализаторам, создаём запросы и заносим их в список дочерних внутри этого запроса,
                //а также указывая в дочерних PE родительского.
                //Стоит вынести это в отдельную функцию

                //Затем проходим по дочерним и повторяем
            }*/

            //Создаём временный словарь для регионов
            //int1 - сущность региона, int2 - сущность инициализатора
            Dictionary<int, int> initializedRegions = new();

            //Создаём временный список для регионов инициализаторов, порождённых текущим первичным
            List<int> currentRegions = new();

            //Очищаем словарь регионов
            initializedRegions.Clear();

            //Для каждого инициализатора региона
            foreach (int regionInitializerEntity in regionInitializerFilter.Value)
            {
                //Берём инициализатор
                ref RRegionInitializer newRegionInitializer = ref regionInitializerPool.Value.Get(regionInitializerEntity);

                //Если сущность родительского региона - -1, то это первичный инициализатор
                if (newRegionInitializer.parentInitializerEntity == -1)
                {
                    //Очищаем список текущих регионов
                    currentRegions.Clear();

                    //Иерархически генерируем позиции для инициализатора и его дочерних инициализаторов
                    InitializerPositionGeneration(
                        initializedRegions,
                        currentRegions,
                        regionInitializerEntity);
                }
            }

            //Для каждого инициализатора региона в словаре
            foreach (KeyValuePair<int, int> kVPRegion in initializedRegions)
            {
                //Берём регион и инициализатор
                ref CHexRegion region = ref regionPool.Value.Get(kVPRegion.Key);
                ref RRegionInitializer regionInitializer = ref regionInitializerPool.Value.Get(kVPRegion.Value);

                //Применяем инициализатор к региону
                RegionInitialization(ref region, ref regionInitializer);
            }

            //Для каждого инициализатора региона
            foreach (int regionInitializerEntity in regionInitializerFilter.Value)
            {
                world.Value.DelEntity(regionInitializerEntity);
            }
        }

        void MapRegionMiscellaneousInitializers()
        {

        }

        bool InitializerPositionGeneration(
            Dictionary<int, int> initializedRegions,
            List<int> siblingRegions,
            int regionInitializerEntity)
        {
            //Берём инициализатор
            ref RRegionInitializer regionInitializer = ref regionInitializerPool.Value.Get(regionInitializerEntity);

            bool isValidRegion = false;

            //Пока не найден новый регион
            while (isValidRegion == false)
            {
                //Берём сущность случайного региона
                int regionEntity;

                //Тут должно быть условие, которое определяет, какая случайная функция выбирает регион:
                //1. Для первичных это просто случайная функция;
                //2. Для вторичных это поиск региона в радиусе от родительского
                if (true)
                {
                    //Берём сущность случайного региона
                    RegionsData.GetRegionRandom().Unpack(world.Value, out regionEntity);
                }

                //Если сущность региона отсутствует в словаре
                if (initializedRegions.ContainsKey(regionEntity) == false)
                {
                    //Берём данный региона
                    ref CHexRegion region = ref regionPool.Value.Get(regionEntity);

                    bool isValidConditions = true;

                    //Для каждого условия инициализатора
                    //for ()
                    //{
                    //Для каждого региона, уже присутствующего в словаре
                    foreach (KeyValuePair<int, int> kVPRegion in initializedRegions)
                    {
                        //Берём существующий регион и его инициализатор
                        ref CHexRegion oldRegion = ref regionPool.Value.Get(kVPRegion.Key);
                        ref RRegionInitializer oldRegionInitializer = ref regionInitializerPool.Value.Get(kVPRegion.Value);

                        //Находим путь между регионами
                        List<int> path = RegionsData.FindPath(
                            world.Value,
                            regionFilter.Value, regionPool.Value,
                            ref region, ref oldRegion,
                            regionInitializer.minDistanceBetweenInitializers);

                        //Если путь существует, то расстояние до региона меньше допустимого
                        if (path != null)
                        {
                            UnityEngine.Debug.LogWarning(path.Count);

                            //Указываем, что расстояние недействительно
                            isValidConditions = false;

                            break;
                        }
                    }
                    //}

                    //Если условия действительны
                    if (isValidConditions == true)
                    {
                        //Ввести проверку того, есть ли дочерние инициализаторы

                        //Заносим сущности региона и инициализатора в словарь
                        initializedRegions.Add(regionEntity, regionInitializerEntity);

                        //Создаём временный список для регионов дочерних инициализаторов
                        List<int> childrenInitializerRegions = ListPool<int>.Get();

                        //Для каждого дочернего инициализатора
                        /*for ()
                        {
                            //Берём сущность инициализатора
                            int childerInitializerEntity = -1;

                            //Рекурсивно запускаем эту же функцию
                            isValidConditions = InitializerPositionGeneration(
                                initializedRegions,
                                childrenInitializerRegions,
                                childerInitializerEntity);

                            //Если условия ложны
                            if (isValidConditions == false)
                            {
                                break;
                            }
                        }*/

                        //Если условия действительны
                        if (isValidConditions == true)
                        {
                            //Заносим сущность региона в список родственных регионов
                            siblingRegions.Add(regionEntity);

                            //Прибавляем к списку родственных регионов список дочерних
                            siblingRegions.AddRange(childrenInitializerRegions);

                            //Отмечаем, что новый регион был найден
                            isValidRegion = true;
                        }
                        //Иначе
                        else
                        {
                            //Удаляем запись о текущем регионе из словаря
                            initializedRegions.Remove(regionEntity);

                            //Удаляем из словаря регионы дочерних инициализаторов
                            for (int a = 0; a < childrenInitializerRegions.Count; a++)
                            {
                                initializedRegions.Remove(childrenInitializerRegions[a]);
                            }
                        }

                        //Возвращаем список в пул
                        ListPool<int>.Add(childrenInitializerRegions);
                    }
                }
            }

            return isValidRegion;
        }

        void RegionInitialization(
            ref CHexRegion region,
            ref RRegionInitializer regionInitializer)
        {
            //Берём RAEO
            region.selfPE.Unpack(world.Value, out int regionEntity);
            ref CRegionAEO rAEO = ref regionAEOPool.Value.Get(regionEntity);

            //ТЕСТ
            //Для каждого эффекта
            for (int a = 0; a < regionInitializer.effectLinks.Length; a++)
            {
                //Берём эффект
                //ТЕСТ - ссылку на эффект
                ref readonly DContentObjectLink effect = ref regionInitializer.effectLinks[a];

                //Берём инициализируемую организацию
                regionInitializer.initializedOrganizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization = ref organizationPool.Value.Get(organizationEntity);

                //Берём ExORAEO данной организации
                rAEO.organizationRAEOs[organization.selfIndex].organizationRAEOPE.Unpack(world.Value, out int oRAEOEntity);
                ref CExplorationORAEO exORAEO = ref explorationORAEOPool.Value.Get(oRAEOEntity);

                //Назначаем сущности ORAEO самозапрос действия
                ref SRORAEOAction oRAEOActionSR = ref oRAEOActionSelfRequestPool.Value.Add(oRAEOEntity);

                //Заполняем данные самозапроса
                oRAEOActionSR = new SRORAEOAction(ORAEOActionType.Colonization);
            }
            //ТЕСТ
        }
    }
}