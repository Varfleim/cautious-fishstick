
using System.Collections.Generic;

using SandOcean.Warfare.Ship;

namespace SandOcean.Designer.Workshop
{
    public struct TempLoadingWorkshopData
    {
        public TempLoadingWorkshopData(
            int a)
        {
            technologies = new();

            buildingTypes = new();

            shipTypes = new();

            shipParts = new();
            shipPartCoreTechnologies = new();
            shipPartDirectionsOfImprovement = new();
            shipPartImprovements = new();

            shipClasses = new();

            engines = new();
            reactors = new();
            fuelTanks = new();
            solidExtractionEquipments = new();
            energyGuns = new();
        }

        public List<WDTechnology> technologies;

        public List<WDBuildingType> buildingTypes;

        public List<WDShipType> shipTypes;

        public List<WDShipPart> shipParts;
        public List<WDShipPartCoreTechnology> shipPartCoreTechnologies;
        public List<WDShipPartDirectionOfImprovement> shipPartDirectionsOfImprovement;
        public List<WDShipPartImprovement> shipPartImprovements;

        public List<WDShipClass> shipClasses;

        public List<WDEngine> engines;
        public List<WDReactor> reactors;
        public List<WDHoldFuelTank> fuelTanks;
        public List<WDExtractionEquipment> solidExtractionEquipments;
        public List<WDGunEnergy> energyGuns;
    }
}