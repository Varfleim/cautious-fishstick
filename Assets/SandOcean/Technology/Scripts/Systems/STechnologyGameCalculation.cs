
using System.Collections.Generic;

using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

using SandOcean.Organization;

namespace SandOcean.Technology
{
    public class STechnologyGameCalculation : IEcsRunSystem
    {
        //Миры
        readonly EcsWorldInject world = default;


        //Дипломатия
        readonly EcsPoolInject<COrganization> organizationPool = default;


        //События технологий
        readonly EcsFilterInject<Inc<RTechnologyCalculateModifiers>> technologyCalculateModifiersEventFilter = default;
        readonly EcsPoolInject<RTechnologyCalculateModifiers> technologyCalculateModifiersEventPool = default;


        //Данные
        readonly EcsCustomInject<ContentData> contentData = default;


        public void Run(IEcsSystems systems)
        {
            //Для каждого события пересчёта модификаторов технологий организации
            foreach (int technologyCalculateModifiersEventEntity in technologyCalculateModifiersEventFilter.Value)
            {
                //Берём компонент события
                ref RTechnologyCalculateModifiers technologyCalculateModifiersEvent = ref technologyCalculateModifiersEventPool.Value.Get(technologyCalculateModifiersEventEntity);

                //Берём компонент организации
                technologyCalculateModifiersEvent.organizationPE.Unpack(world.Value, out int organizationEntity);
                ref COrganization organization= ref organizationPool.Value.Get(organizationEntity);

                //Запрашиваем пересчёт модификаторов технологий для указанной организации
                TechnologyOrganizationCalculation(ref organization);

                world.Value.DelEntity(technologyCalculateModifiersEventEntity);
            }
        }

        void TechnologyOrganizationCalculation(
            ref COrganization organization)
        {
            //Для каждого набора контента в массиве технологий организации
            for (int a = 0; a < organization.technologies.Length; a++)
            {
                //Для каждой технологии организации
                foreach (KeyValuePair<int, DOrganizationTechnology> kVPTechnology in organization.technologies[a])
                {
                    //Если технология полностью исследована
                    if (kVPTechnology.Value.isResearched == true)
                    {
                        //Берём ссылку на данные технологии
                        ref readonly DTechnology technology= ref contentData.Value.contentSets[a].technologies[kVPTechnology.Key];

                        //Для каждого модификатора технологии
                        for (int b = 0; b < technology.technologyModifiers.Length; b++)
                        {
                            //Определяем тип модификатора и соответствующие действия
                            TechnologyFunctions.TechnologyModifierTypeCalculation(
                                technology.technologyModifiers[b],
                                ref organization.technologyModifiers);
                        }
                    }
                }
            }
        }
    }
}