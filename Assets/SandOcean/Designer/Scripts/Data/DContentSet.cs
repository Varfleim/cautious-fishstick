
using System;

using SandOcean.Technology;
using SandOcean.Designer.Game;
using SandOcean.Warfare.Ship;

namespace SandOcean
{
    [Serializable]
    public struct DContentSet : IContentSet
    {
        public DContentSet(
            string contentSetName,
            DTechnology[] technologies,
            DShipType[] shipTypes,
            DShipClass[] shipClasses,
            DEngine[] engines,
            DReactor[] reactors,
            DHoldFuelTank[] fuelTanks,
            DExtractionEquipment[] solidExtractionEquipments,
            DGunEnergy[] energyGuns)
        {
            this.contentSetName = contentSetName;


            this.technologies = technologies;


            this.shipTypes = shipTypes;
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


            technologies = new DTechnology[0];


            shipTypes = new DShipType[0];
            shipClasses = new DShipClass[0];

            engines = new DEngine[0];
            reactors = new DReactor[0];
            fuelTanks = new DHoldFuelTank[0];
            solidExtractionEquipments = new DExtractionEquipment[0];
            energyGuns = new DGunEnergy[0];
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

        public DShipType[] shipTypes;
        public DShipClass[] shipClasses;

        public DEngine[] engines;
        public DReactor[] reactors;
        public DHoldFuelTank[] fuelTanks;
        public DExtractionEquipment[] solidExtractionEquipments;
        public DGunEnergy[] energyGuns;
    }
}