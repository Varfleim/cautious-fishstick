using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Leopotam.EcsLite;
using Leopotam.EcsLite.Di;

namespace SandOcean.UI.Camera
{
    public class SUICameraInitialization : IEcsInitSystem
    {
        public void Init(IEcsSystems systems)
        {
            //Инициализируем камеру
            InitializeCamera();
        }

        void InitializeCamera()
        {

        }
    }
}