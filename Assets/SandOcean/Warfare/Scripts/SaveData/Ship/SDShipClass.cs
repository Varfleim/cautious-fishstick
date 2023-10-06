
using System;

using SandOcean.Warfare.Ship;

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
            SDShipClassComponent[] energyGuns,
            SDShipClassPart[] shipParts)
        {
            this.className = className;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.extractionEquipmentSolids = extractionEquipmentSolids;
            this.energyGuns = energyGuns;

            this.shipParts = shipParts;
        }

        public string className;

        public SDShipClassComponent[] engines;
        public SDShipClassComponent[] reactors;
        public SDShipClassComponent[] fuelTanks;
        public SDShipClassComponent[] extractionEquipmentSolids;
        public SDShipClassComponent[] energyGuns;

        public SDShipClassPart[] shipParts;
    }
}