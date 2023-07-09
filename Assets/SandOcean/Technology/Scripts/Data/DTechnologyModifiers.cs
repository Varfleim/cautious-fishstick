using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean.Technology
{
    [Serializable]
    public struct DTechnologyModifiers
    {
        public DTechnologyModifiers(
            int a)
        {
            this.designerMinEngineSize = float.MaxValue;
            this.designerMaxEngineSize = float.MinValue;

            this.designerMinEngineBoost = float.MaxValue;
            this.designerMaxEngineBoost = float.MinValue;


            this.designerMinReactorSize = float.MaxValue;
            this.designerMaxReactorSize = float.MinValue;

            this.designerMinReactorBoost = float.MaxValue;
            this.designerMaxReactorBoost = float.MinValue;


            this.designerMinFuelTankSize = float.MaxValue;
            this.designerMaxFuelTankSize = float.MinValue;


            this.designerMinSolidExtractionEquipmentSize = float.MaxValue;
            this.designerMaxSolidExtractionEquipmentSize = float.MinValue;


            this.designerMinEnergyGunCaliber = float.MaxValue;
            this.designerMaxEnergyGunCaliber = float.MinValue;

            this.designerMinEnergyGunBarrelLength = float.MaxValue;
            this.designerMaxEnergyGunBarrelLength = float.MinValue;
        }

        public float designerMinEngineSize;
        public float designerMaxEngineSize;

        public float designerMinEngineBoost;
        public float designerMaxEngineBoost;


        public float designerMinReactorSize;
        public float designerMaxReactorSize;

        public float designerMinReactorBoost;
        public float designerMaxReactorBoost;


        public float designerMinFuelTankSize;
        public float designerMaxFuelTankSize;


        public float designerMinSolidExtractionEquipmentSize;
        public float designerMaxSolidExtractionEquipmentSize;


        public float designerMinEnergyGunCaliber;
        public float designerMaxEnergyGunCaliber;

        public float designerMinEnergyGunBarrelLength;
        public float designerMaxEnergyGunBarrelLength;
    }
}