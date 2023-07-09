
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    public struct TempLoadingWorkshopData
    {
        public TempLoadingWorkshopData(
            int a)
        {
            this.technologies = new List<WDTechnology>();

            this.shipClasses = new List<WDShipClass>();

            this.engines = new List<WDEngine>();
            this.reactors = new List<WDReactor>();
            this.fuelTanks = new List<WDHoldFuelTank>();
            this.solidExtractionEquipments = new List<WDExtractionEquipment>();
            this.energyGuns = new List<WDGunEnergy>();
        }

        public List<WDTechnology> technologies;

        public List<WDShipClass> shipClasses;

        public List<WDEngine> engines;
        public List<WDReactor> reactors;
        public List<WDHoldFuelTank> fuelTanks;
        public List<WDExtractionEquipment> solidExtractionEquipments;
        public List<WDGunEnergy> energyGuns;
    }
}