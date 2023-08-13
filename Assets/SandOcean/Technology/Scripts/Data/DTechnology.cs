
using System;
using System.Collections.Generic;

namespace SandOcean.Technology
{
    [Serializable]
    public struct DTechnology : IContentObject, ITechnology
    {
        public DTechnology(
            string technologyName, 
            bool isBaseTechnology,
            DTechnologyModifier[] technologyModifiers,
            DTechnologyComponentCoreModifier[] technologyComponentCoreModifiers)
        {
            this.technologyName = technologyName;

            this.isBaseTechnology = isBaseTechnology;

            this.technologyModifiers = technologyModifiers;
            this.technologyComponentCoreModifiers = technologyComponentCoreModifiers;

            this.engines = new List<DContentObjectLink>();
            this.reactors = new List<DContentObjectLink>();
            this.fuelTanks = new List<DContentObjectLink>();
            this.extractionEquipmentSolids = new List<DContentObjectLink>();
            this.energyGuns = new List<DContentObjectLink>();
        }

        public string ObjectName
        {
            get
            {
                return technologyName;
            }
            set
            {
                technologyName
                    = value;
            }
        }
        string technologyName;

        public bool IsBaseTechnology
        {
            get
            {
                return isBaseTechnology;
            }
            set
            {
                isBaseTechnology
                    = value;
            }
        }
        bool isBaseTechnology;

        public DTechnologyModifier[] technologyModifiers;
        public DTechnologyComponentCoreModifier[] technologyComponentCoreModifiers;

        public List<DContentObjectLink> engines;
        public List<DContentObjectLink> reactors;
        public List<DContentObjectLink> fuelTanks;
        public List<DContentObjectLink> extractionEquipmentSolids;
        public List<DContentObjectLink> energyGuns;
    }
}