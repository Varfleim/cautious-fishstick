
using System;

using SandOcean.Technology;
using SandOcean.Economy.Building;
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
            DBuildingType[] buildingTypes,
            DShipType[] shipTypes,
            DShipPart[] shipParts,
            DShipPartCoreTechnology[] shipPartCoreTechnologies,
            DShipPartDirectionOfImprovement[] shipPartDirectionsOfImprovement,
            DShipPartImprovement[] shipPartImprovements,
            DShipClass[] shipClasses,
            DEngine[] engines,
            DReactor[] reactors,
            DHoldFuelTank[] fuelTanks,
            DExtractionEquipment[] solidExtractionEquipments,
            DGunEnergy[] energyGuns)
        {
            this.contentSetName = contentSetName;


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

        public DContentSet(
            string contentSetName)
        {
            this.contentSetName = contentSetName;


            technologies = new DTechnology[0];

            buildingTypes = new DBuildingType[0];

            shipTypes = new DShipType[0];

            shipParts = new DShipPart[0];
            shipPartCoreTechnologies = new DShipPartCoreTechnology[0];
            shipPartDirectionsOfImprovement = new DShipPartDirectionOfImprovement[0];
            shipPartImprovements = new DShipPartImprovement[0];

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

        public DBuildingType[] buildingTypes;

        public DShipType[] shipTypes;

        public DShipPart[] shipParts;
        public DShipPartCoreTechnology[] shipPartCoreTechnologies;
        public DShipPartDirectionOfImprovement[] shipPartDirectionsOfImprovement;
        public DShipPartImprovement[] shipPartImprovements;

        public DShipClass[] shipClasses;

        public DEngine[] engines;
        public DReactor[] reactors;
        public DHoldFuelTank[] fuelTanks;
        public DExtractionEquipment[] solidExtractionEquipments;
        public DGunEnergy[] energyGuns;
    }
}