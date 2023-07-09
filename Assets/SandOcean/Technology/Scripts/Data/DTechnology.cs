
using System;
using System.Collections.Generic;

using SandOcean.Designer.Game;

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

            this.engines = new List<DContentObjectRef>();
            this.reactors = new List<DContentObjectRef>();
            this.fuelTanks = new List<DContentObjectRef>();
            this.extractionEquipmentSolids = new List<DContentObjectRef>();
            this.energyGuns = new List<DContentObjectRef>();
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

        public List<DContentObjectRef> engines;
        public List<DContentObjectRef> reactors;
        public List<DContentObjectRef> fuelTanks;
        public List<DContentObjectRef> extractionEquipmentSolids;
        public List<DContentObjectRef> energyGuns;
    }
}