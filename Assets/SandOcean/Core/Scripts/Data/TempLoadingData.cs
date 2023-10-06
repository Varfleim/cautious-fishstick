using System.Collections.Generic;

using SandOcean.Technology;
using SandOcean.Economy.Building;
using SandOcean.Warfare.Ship;

namespace SandOcean.Designer.Game
{
    public struct TempLoadingData
    {
        public TempLoadingData(int a)
        {
            technologies = new();

            shipTypes = new();
            shipParts = new();
            shipPartCoreTechnologies = new();
            shipPartDirectionsOfImprovement = new();
            shipPartImprovements = new();
            shipClasses = new();
            buildingTypes = new();
            engines = new();
            reactors = new();
            fuelTanks = new();
            solidExtractionEquipments = new();
            energyGuns = new();
        }

        public List<DTechnology> technologies;

        public List<DBuildingType> buildingTypes;

        public List<DShipType> shipTypes;

        public List<DShipPart> shipParts;
        public List<DShipPartCoreTechnology> shipPartCoreTechnologies;
        public List<DShipPartDirectionOfImprovement> shipPartDirectionsOfImprovement;
        public List<DShipPartImprovement> shipPartImprovements;

        public List<DShipClass> shipClasses;

        public List<DEngine> engines;
        public List<DReactor> reactors;
        public List<DHoldFuelTank> fuelTanks;
        public List<DExtractionEquipment> solidExtractionEquipments;
        public List<DGunEnergy> energyGuns;
    }
}