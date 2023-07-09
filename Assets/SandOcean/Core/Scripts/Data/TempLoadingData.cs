using System.Collections.Generic;

using SandOcean.Technology;

namespace SandOcean.Designer.Game
{
    public struct TempLoadingData
    {
        public TempLoadingData(int a)
        {
            technologies = new List<DTechnology>();

            shipClasses = new List<DShipClass>();

            engines = new List<DEngine>();
            reactors = new List<DReactor>();
            fuelTanks = new List<DHoldFuelTank>();
            solidExtractionEquipments = new List<DExtractionEquipment>();
            energyGuns = new List<DGunEnergy>();
        }

        public List<DTechnology> technologies;

        public List<DShipClass> shipClasses;

        public List<DEngine> engines;
        public List<DReactor> reactors;
        public List<DHoldFuelTank> fuelTanks;
        public List<DExtractionEquipment> solidExtractionEquipments;
        public List<DGunEnergy> energyGuns;
    }
}