
using System;

using SandOcean.Warfare.Ship;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDContentSet
    {
        public SDContentSet(
            SDTechnology[] technologies,
            SDBuildingType[] buildingTypes,
            SDShipType[] shipTypes,
            SDShipPart[] shipParts, 
            SDShipPartCoreTechnology[] shipPartCoreTechnologies, 
            SDShipPartTypeDirectionOfImprovement[] shipPartDirectionsOfImprovement,
            SDShipPartImprovement[] shipPartImprovements,
            SDShipClass[] shipClasses,
            SDEngine[] engines,
            SDReactor[] reactors,
            SDHoldFuelTank[] fuelTanks,
            SDExtractionEquipment[] extractionEquipmentSolids,
            SDGunEnergy[] energyGuns)
        {
            this.technologies = technologies;

            this.buildingTypes = buildingTypes;

            this.shipTypes = shipTypes;

            this.shipParts = shipParts;
            this.shipPartCoreTechnologies = shipPartCoreTechnologies;
            this.shipPartDirectionsOfImprovement = shipPartDirectionsOfImprovement;
            this.shipPartImprovements = shipPartImprovements;

            this.shipClasses = shipClasses;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.extractionEquipmentSolids = extractionEquipmentSolids;
            this.energyGuns = energyGuns;
        }

        public SDTechnology[] technologies;

        public SDBuildingType[] buildingTypes;

        public SDShipType[] shipTypes;

        public SDShipPart[] shipParts;
        public SDShipPartCoreTechnology[] shipPartCoreTechnologies;
        public SDShipPartTypeDirectionOfImprovement[] shipPartDirectionsOfImprovement;
        public SDShipPartImprovement[] shipPartImprovements;

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