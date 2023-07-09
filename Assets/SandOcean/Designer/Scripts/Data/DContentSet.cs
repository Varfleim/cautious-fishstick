
using System;

using SandOcean.Technology;
using SandOcean.Designer.Game;

namespace SandOcean
{
    [Serializable]
    public struct DContentSet : IContentSet
    {
        public DContentSet(
            string contentSetName,
            DTechnology[] technologies,
            DShipClass[] shipClasses,
            DEngine[] engines,
            DReactor[] reactors,
            DHoldFuelTank[] fuelTanks,
            DExtractionEquipment[] solidExtractionEquipments,
            DGunEnergy[] energyGuns)
        {
            this.contentSetName = contentSetName;


            this.technologies = technologies;
            

            this.shipClasses = shipClasses;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.solidExtractionEquipments = solidExtractionEquipments;
            this.energyGuns = energyGuns;
        }

        public DContentSet(
            string contentSetName)
        {
            this.contentSetName = contentSetName;


            this.technologies = new DTechnology[0];


            this.shipClasses = new DShipClass[0];

            this.engines = new DEngine[0];
            this.reactors = new DReactor[0];
            this.fuelTanks = new DHoldFuelTank[0];
            this.solidExtractionEquipments = new DExtractionEquipment[0];
            this.energyGuns = new DGunEnergy[0];
        }

        public string ContentSetName
        {
            get
            {
                return contentSetName;
            }
            set
            {
                contentSetName
                    = value;
            }
        }
        string contentSetName;


        public DTechnology[] technologies;


        public DShipClass[] shipClasses;

        public DEngine[] engines;
        public DReactor[] reactors;
        public DHoldFuelTank[] fuelTanks;
        public DExtractionEquipment[] solidExtractionEquipments;
        public DGunEnergy[] energyGuns;
    }
}