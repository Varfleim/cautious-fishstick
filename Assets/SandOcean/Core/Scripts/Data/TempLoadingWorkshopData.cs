
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    public struct TempLoadingWorkshopData
    {
        public TempLoadingWorkshopData(
            int a)
        {
            technologies = new();


            shipTypes = new();
            shipClasses = new();

            engines = new();
            reactors = new();
            fuelTanks = new();
            solidExtractionEquipments = new();
            energyGuns = new();
        }

        public List<WDTechnology> technologies;


        public List<WDShipType> shipTypes;
        public List<WDShipClass> shipClasses;

        public List<WDEngine> engines;
        public List<WDReactor> reactors;
        public List<WDHoldFuelTank> fuelTanks;
        public List<WDExtractionEquipment> solidExtractionEquipments;
        public List<WDGunEnergy> energyGuns;
    }
}