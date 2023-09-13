
using System.Collections.Generic;
using System.Linq;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Organization;
using SandOcean.Designer.Workshop;

namespace SandOcean.Technology
{
    public class STechnologyWorkshopCalculation : IEcsInitSystem
    {
        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;


        public void Init(IEcsSystems systems)
        {
            //Производим рассчёт технологий в данных мастерской
            TechnologyWorkshopCalculation();
        }

        void TechnologyWorkshopCalculation()
        {
            //Очищаем структуру для хранения модификаторов
            contentData.Value.globalTechnologyModifiers = new DTechnologyModifiers(0);

            //Берём ссылку на общие модификаторы технологий
            ref DTechnologyModifiers totalTechnologyModifiers = ref contentData.Value.globalTechnologyModifiers;

            //Создаём структуру для временных данных
            TempTechnologyData tempTechnologyData = new(0);

            //Берём число наборов контента с описаниями - это те наборы контента, что могут иметь технологии
            int contentSetDescriptionsNumber = contentData.Value.wDContentSetDescriptions.Length;

            //Очищаем массив словарей глобальных "исследованных" технологий
            contentData.Value.globalTechnologies = new Dictionary<int, DOrganizationTechnology>[contentSetDescriptionsNumber];

            //Для каждого набора контента меньше данного числа
            for (int a = 0; a < contentSetDescriptionsNumber; a++)
            {
                //Создаём новый словарь
                contentData.Value.globalTechnologies[a] = new Dictionary<int, DOrganizationTechnology>();

                //Для каждой технологии
                for (int b = 0; b < contentData.Value.wDContentSets[a].technologies.Length; b++)
                {
                    //Берём ссылку на данные технологии
                    ref readonly WDTechnology technology = ref contentData.Value.wDContentSets[a].technologies[b];

                    //Для каждого основного модификатора компонента
                    for (int c = 0; c < technology.technologyComponentCoreModifiers.Length; c++)
                    {
                        //Определяем тип модификатора и соответствующие действия
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