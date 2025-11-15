using UnityEngine;
using Zenject;

namespace Installers
{
    public class CarInstaller : MonoInstaller
    {
        [SerializeField] private CarConfig config;
        [SerializeField] private Wheel[] wheels;
        public override void InstallBindings()
        {
            AddConfig();
            AddCarComponents();
            AddControllers();
            AddWheels();

            Container.Bind<InputManager>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        private void AddControllers()
        {
            Container.Bind<CarController>()
                .FromComponentInHierarchy()
                .AsSingle();
        }

        private void AddConfig()
        {
            Container.BindInstance(config)
                .AsSingle()
                .NonLazy();
        }

        private void AddCarComponents()
        {
            Container.Bind<BrakeSystem>()
                .AsSingle();

            Container.Bind<SteeringSystem>()
                .AsSingle();

            Container.Bind<EngineSystem>()
                .AsSingle();
        }

        private void AddWheels()
        {
            foreach (var wheel in wheels)
            {
                Container.Bind<IWheel>() 
                    .FromInstance(wheel)
                    .AsCached();

                Container.Bind<Wheel>() 
                    .FromInstance(wheel)
                    .AsCached();
            }
        }
    }
}