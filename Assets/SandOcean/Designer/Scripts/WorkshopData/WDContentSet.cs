
using System;
using System.Collections.Generic;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDContentSet : IContentSet
    {
        public WDContentSet(
            string contentSetName,
            WDTechnology[] technologies,
            WDShipClass[] shipClasses,
            WDEngine[] engines,
            WDReactor[] reactors,
            WDHoldFuelTank[] fuelTanks,
            WDExtractionEquipment[] solidExtractionEquipments,
            WDGunEnergy[] energyGuns)
        {
            this.contentSetName = contentSetName;

            this.isActive = true;

            this.gameContentSetIndex = -1;

            this.technologies = technologies;


            this.shipClasses = shipClasses;

            this.engines = engines;
            this.reactors = reactors;
            this.fuelTanks = fuelTanks;
            this.solidExtractionEquipments = solidExtractionEquipments;
            this.energyGuns = energyGuns;
        }

        public WDContentSet(string contentSetName)
        {
            this.contentSetName = contentSetName;

            this.isActive = true;

            this.gameContentSetIndex = -1;

            this.technologies = new WDTechnology[0];

            this.shipClasses = new WDShipClass[0];

            this.engines = new WDEngine[0];
            this.reactors = new WDReactor[0];
            this.fuelTanks = new WDHoldFuelTank[0];
            this.solidExtractionEquipments = new WDExtractionEquipment[0];
            this.energyGuns = new WDGunEnergy[0];
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


        public bool isActive;

        public int gameContentSetIndex;


        public WDTechnology[] technologies;

        public WDShipClass[] shipClasses;

        public WDEngine[] engines;
        public WDReactor[] reactors;
        public WDHoldFuelTank[] fuelTanks;
        public WDExtractionEquipment[] solidExtractionEquipments;
        public WDGunEnergy[] energyGuns;
    }
}