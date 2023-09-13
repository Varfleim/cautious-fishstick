
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
            WDShipType[] shipTypes,
            WDShipClass[] shipClasses,
            WDEngine[] engines,
            WDReactor[] reactors,
            WDHoldFuelTank[] fuelTanks,
            WDExtractionEquipment[] solidExtractionEquipments,
            WDGunEnergy[] energyGuns)
        {
            this.contentSetName = contentSetName;

            isActive = true;

            gameContentSetIndex = -1;

            this.technologies = technologies;


            this.shipTypes = shipTypes;
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

            isActive = true;

            gameContentSetIndex = -1;

            technologies = new WDTechnology[0];


            shipTypes = new WDShipType[0];
            shipClasses = new WDShipClass[0];

            engines = new WDEngine[0];
            reactors = new WDReactor[0];
            fuelTanks = new WDHoldFuelTank[0];
            solidExtractionEquipments = new WDExtractionEquipment[0];
            energyGuns = new WDGunEnergy[0];
        }

        public string ContentSetName
        {
            get
            {
                return contentSetName;
            }
            set
            {
                contentSetName = value;
            }
        }
        string contentSetName;


        public bool isActive;

        public int gameContentSetIndex;


        public WDTechnology[] technologies;


        public WDShipType[] shipTypes;
        public WDShipClass[] shipClasses;

        public WDEngine[] engines;
        public WDReactor[] reactors;
        public WDHoldFuelTank[] fuelTanks;
        public WDExtractionEquipment[] solidExtractionEquipments;
        public WDGunEnergy[] energyGuns;
    }
}