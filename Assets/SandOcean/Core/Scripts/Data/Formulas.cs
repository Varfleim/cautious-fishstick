
using System;
using System.Collections.Generic;

using UnityEngine;

namespace SandOcean
{
    public class Formulas
    {
        public static System.Random random;

        public static double RandomGaussian(double mean, double standardDeviation)
        {
            double x1 = 1 - random.NextDouble();
            double x2 = 1 - random.NextDouble();

            double y1 = Math.Sqrt(-2.0 * Math.Log(x1)) * Math.Cos(2.0 * Math.PI * x2);
            return y1 * standardDeviation + mean;
        }

        public static double RandomHalfGaussian(double mean, double standardDeviation)
        {
            return Math.Abs(RandomGaussian(mean, standardDeviation));
        }

        public static float EnginePower(
            float powerPerSize,
            float size,
            float boost)
        {
            float power
                = powerPerSize 
                * size
                * boost;

            return power;
        }

        public static float ReactorEnergy(
            float energyPerSize,
            float size,
            float boost)
        {
            float energy
                = energyPerSize
                * size
                * boost;

            return energy;
        }

        public static float FuelTankCapacity(
            float fuelCompression,
            float size)
        {
            float capacity
                = fuelCompression
                * size;

            return capacity;
        }

        public static float ExtractionEquipmentExtractionSpeed(
            float speedPerSize,
            float size)
        {
            float extractionSpeed
                = speedPerSize
                * size;

            return extractionSpeed;
        }

        public static float GunSizeCalculate(
            float gunCaliber,
            float gunBarrelLength)
        {
            float gunCaliberMeters
                = gunCaliber
                / 100;

            float gunBarrelLengthMeters
                = gunCaliberMeters
                * gunBarrelLength;

            float gunCaliberAreaMeters2
                = (float)(gunCaliberMeters
                * gunCaliberMeters
                * Math.PI
                * 1.1);

            float gunSize
                = gunCaliberAreaMeters2
                * gunBarrelLengthMeters;

            return gunSize;
        }

        public static float EnergyGunDamageCalculate(
            float gunCaliber,
            float gunBarrelLength)
        {
            float energyGunDamage
                = gunCaliber
                * gunBarrelLength;

            return energyGunDamage;
        }

        public static float ShipClassMaxSpeed(
            float totalEnginePower,
            float shipMass)
        {
            float maxSpeed;

            //Если масса корабля больше нуля
            if (shipMass
                > 0)
            {
                maxSpeed
                = totalEnginePower
                / shipMass
                * 1f;
            }
            //Иначе
            else
            {
                maxSpeed
                    = 0;
            }

            return maxSpeed;
        }
    }
}