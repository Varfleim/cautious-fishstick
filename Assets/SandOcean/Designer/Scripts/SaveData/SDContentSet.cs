
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDContentSet
    {
        public SDContentSet(
            SDTechnology[] technologies,
            SDShipType[] shipTypes,
            SDShipClass[] shipClasses,
            SDEngine[] engines,
            SDReactor[] reactors,
            SDHoldFuelTank[] fuelTanks,
            SDExtractionEquipment[] extractionEquipmentSolids,
            SDGunEnergy[] energyGuns)
        {
            this.technologies = technologies;


            this.shipTypes = shipTypes;
            this.shipClasses = shipClasses;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.extractionEquipmentSolids = extractionEquipmentSolids;
            this.energyGuns = energyGuns;
        }

        public SDTechnology[] technologies;


        public SDShipType[] shipTypes;
        public SDShipClass[] shipClasses;

        public SDEngine[] engines;
        public SDReactor[] reactors;
        public SDHoldFuelTank[] fuelTanks;
        public SDExtractionEquipment[] extractionEquipmentSolids;
        public SDGunEnergy[] energyGuns;
    }

    [Serializable]
    public class SDContentSetClass
    {
        public SDContentSet contentSet;
    }
}