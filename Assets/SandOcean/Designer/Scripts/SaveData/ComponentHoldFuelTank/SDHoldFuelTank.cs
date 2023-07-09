using System;

namespace SandOcean.Designer.Save
{
    [Serializable]
    public struct SDHoldFuelTank
    {
        public SDHoldFuelTank(
            string modelName,
            SDComponentCoreTechnology[] coreTechnologies,
            float fuelTankSize)
        {
            this.modelName = modelName;

            this.coreTechnologies = coreTechnologies;

            this.fuelTankSize = fuelTankSize;
        }

        public string modelName;

        public SDComponentCoreTechnology[] coreTechnologies;

        public float fuelTankSize;
    }

    [Serializable]
    public class SDFuelTankModelsClass
    {
        public SDHoldFuelTank[] engineModels;
    }
}