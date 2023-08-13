
using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Diplomacy;

namespace SandOcean.Technology
{
    public class STechnologyNewGameCalculation : IEcsRunSystem
    {
        //Общие события
        readonly EcsFilterInject<Inc<RStartNewGame>> startNewGameEventFilter = default;
        readonly EcsPoolInject<RStartNewGame> startNewGameEventPool = default;


        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;


        public void Run(IEcsSystems systems)
        {
            //Для каждого события начала новой игры
            foreach (int startNewGameEventEntity in startNewGameEventFilter.Value)
            {
                //Берём компонент события начала новой игры
                ref RStartNewGame startNewGameEvent = ref startNewGameEventPool.Value.Get(startNewGameEventEntity);

                //Производим глобальный рассчёт технологий в начале игры
                TechnologyNewGameCalculation();
            }
        }

        void TechnologyNewGameCalculation()
        {
            //Очищаем структуру для хранения модификаторов
            contentData.Value.globalTechnologyModifiers = new DTechnologyModifiers(0);

            //Берём ссылку на общие модификаторы технологий
            ref DTechnologyModifiers totalTechnologyModifiers = ref contentData.Value.globalTechnologyModifiers;

            //Создаём структуру для временных данных
            TempTechnologyData tempTechnologyData = new(0);

            //Берём число наборов контента с описаниями - это те наборы контента, что могут иметь технологии
            int contentSetDescriptionsNumber = contentData.Value.contentSetDescriptions.Length;

            //Очищаем массив словарей глобальных "исследованных" технологий
            contentData.Value.globalTechnologies = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

            //Для каждого набора контента меньше данного числа
            for (int a = 0; a < contentSetDescriptionsNumber; a++)
            {
                //Создаём новый словарь
                contentData.Value.globalTechnologies[a] = new Dictionary<int, DOrganizationTechnology>();

                //Для каждой технологии
                for (int b = 0; b < contentData.Value.contentSets[a].technologies.Length; b++)
                {
                    //Берём ссылку на данные технологии
                    ref readonly DTechnology technology = ref contentData.Value.contentSets[a].technologies[b];

                    //Для каждого основного модификатора компонента
                    for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                    {
                        //Определяем тип модификатора и соответствующее действие
                        TechnologyFunctions.TechnologyGlobalComponentCoreModifiersTypeCalculation(
                            technology.technologyComponentCoreModifiers[c],
                            a,
                            b,
                            ref tempTechnologyData);
                    }

                    //Для каждого модификатора
                    for (int c = 0; c < technology.technologyModifiers.Length; c++)
                    {
                        //Определяем тип модификатора и соответствующие действия
                        TechnologyFunctions.TechnologyModifierTypeCalculation(
                            technology.technologyModifiers[c],
                            ref totalTechnologyModifiers);
                    }

                    //Заносим технологию в словарь глобальных технологий как исследованную
                    contentData.Value.globalTechnologies[a].Add(b, new DOrganizationTechnology(true));
                }
            }

            //Сортируем список технологий, определяющих мощность двигателя на единицу размера,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesEnginePowerPerSize = tempTechnologyData.technologiesEnginePowerPerSize.OrderBy(
                x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих энергию реактора на единицу размера,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesReactorEnergyPerSize = tempTechnologyData.technologiesReactorEnergyPerSize.OrderBy(
                x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих сжатие топливного бака,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesFuelTankCompression = tempTechnologyData.technologiesFuelTankCompression.OrderBy(
                x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих скорость добычи оборудования для твёрдой добычи на единицу размера,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesSolidExtractionEquipmentSpeedPerSize = tempTechnologyData.technologiesExtractionEquipmentSolidSpeedPerSize.OrderBy(
                x => x.modifierValue).ToArray();

            //Сортируем список технологий, определяющих перезарядку энергетических орудий,
            //и заносим его в соответствующий массив
            contentData.Value.technologiesEnergyGunRecharge = tempTechnologyData.technologiesEnergyGunRecharge.OrderBy(
                x => x.modifierValue).ToArray();
        }
    }
}