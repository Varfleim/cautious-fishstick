
using System;
using System.Collections.Generic;

using SandOcean.Warfare.Ship;

namespace SandOcean.Designer.Workshop
{
    [Serializable]
    public struct WDContentSet : IContentSet
    {
        public WDContentSet(
            string contentSetName,
            WDTechnology[] technologies,
            WDBuildingType[] buildingTypes,
            WDShipType[] shipTypes,
            WDShipPart[] shipParts,
            WDShipPartCoreTechnology[] shipPartCoreTechnologies,
            WDShipPartDirectionOfImprovement[] shipPartDirectionsOfImprovement,
            WDShipPartImprovement[] shipPartImprovements,
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

            this.buildingTypes = buildingTypes;

            this.shipTypes = shipTypes;

            this.shipParts = shipParts;
            this.shipPartCoreTechnologies = shipPartCoreTechnologies;
            this.shipPartDirectionsOfImprovement = shipPartDirectionsOfImprovement;
            this.shipPartImprovements = shipPartImprovements;

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

            buildingTypes = new WDBuildingType[0];

            shipTypes = new WDShipType[0];

            shipParts = new WDShipPart[0];
            shipPartCoreTechnologies = new WDShipPartCoreTechnology[0];
            shipPartDirectionsOfImprovement = new WDShipPartDirectionOfImprovement[0];
            shipPartImprovements = new WDShipPartImprovement[0];

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

        public WDBuildingType[] buildingTypes;

        public WDShipType[] shipTypes;

        public WDShipPart[] shipParts;
        public WDShipPartCoreTechnology[] shipPartCoreTechnologies;
        public WDShipPartDirectionOfImprovement[] shipPartDirectionsOfImprovement;
        public WDShipPartImprovement[] shipPartImprovements;

        public WDShipClass[] shipClasses;

        public WDEngine[] engines;
        public WDReactor[] reactors;
        public WDHoldFuelTank[] fuelTanks;
        public WDExtractionEquipment[] solidExtractionEquipments;
        public WDGunEnergy[] energyGuns;
    }
}