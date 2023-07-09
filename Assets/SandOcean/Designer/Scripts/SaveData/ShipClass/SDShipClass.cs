
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDShipClass
    {
        public SDShipClass(
            string className,
            SDShipClassComponent[] engines,
            SDShipClassComponent[] reactors,
            SDShipClassComponent[] fuelTanks,
            SDShipClassComponent[] extractionEquipmentSolids,
            SDShipClassComponent[] energyGuns)
        {
            this.className = className;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.extractionEquipmentSolids = extractionEquipmentSolids;
            this.energyGuns = energyGuns;
        }

        public string className;

        public SDShipClassComponent[] engines;
        public SDShipClassComponent[] reactors;
        public SDShipClassComponent[] fuelTanks;
        public SDShipClassComponent[] extractionEquipmentSolids;
        public SDShipClassComponent[] energyGuns;
    }

    [Serializable]
    public class SDShipClassesClass
    {
        public SDShipClass[] shipClasses;
    }
}