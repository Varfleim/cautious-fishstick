using System.Collections.Generic;

using SandOcean.Technology;
using SandOcean.Warfare.Ship;

namespace SandOcean.Designer.Game
{
    public struct TempLoadingData
    {
        public TempLoadingData(int a)
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

        public List<DTechnology> technologies;


        public List<DShipType> shipTypes;
        public List<DShipClass> shipClasses;

        public List<DEngine> engines;
        public List<DReactor> reactors;
        public List<DHoldFuelTank> fuelTanks;
        public List<DExtractionEquipment> solidExtractionEquipments;
        public List<DGunEnergy> energyGuns;
    }
}