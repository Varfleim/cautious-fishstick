
using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDGunEnergy
    {
        public SDGunEnergy(
            string modelName,
            SDComponentCoreTechnology[] coreTechnologies, 
            float energyGunCaliber,
            float energyGunBarrelLength)
        {
            this.modelName = modelName;

            this.coreTechnologies = coreTechnologies;

            this.gunCaliber = energyGunCaliber;
            this.gunBarrelLength = energyGunBarrelLength;
        }

        public string modelName;

        public SDComponentCoreTechnology[] coreTechnologies;

        public float gunCaliber;
        public float gunBarrelLength;
    }
}