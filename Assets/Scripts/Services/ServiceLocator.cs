using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DeepDreams.Services
{
    public sealed class ServiceLocator : IServiceLocator
    {
        private readonly IDictionary<Type, IService> _services = new Dictionary<Type, IService>();

        public static IServiceLocator Instance { get; private set; } = new ServiceLocator();

        private ServiceLocator() {}

#if UNITY_EDITOR
        // Ensures static values are correctly reset when Unity Domain Reloading is disabled.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Init()
        {
            Instance = new ServiceLocator();
        }
#endif

        // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        // private static void InitializeManagerLevel()
        // {
        //     // Checks if persistent level is already loaded.
        //     for (int sceneIndex = 0; sceneIndex < SceneManager.sceneCount; ++sceneIndex)
        //     {
        //         if (SceneManager.GetSceneAt(sceneIndex).name == "ManagerLevel")
        //         {
        //             return;
        //         }
        //     }
        //
        //     SceneManager.LoadScene("ManagerLevel", LoadSceneMode.Additive);
        // }

        public T GetService<T>() where T : class, IService
        {
            Assert.IsNotNull(_services, "Someone has requested a service prior to the locator's initialization.");

            Assert.IsTrue(_services.ContainsKey(typeof(T)), "Could not find service: " + typeof(T));
            T service = (T)_services[typeof(T)];
            Assert.IsNotNull(service, typeof(T) + " could not be found.");
            return service;
        }

        /// <summary>
        ///     Registers the service with the current service locator.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="service">Service instance.</param>
        public void Register<T>(T service) where T : class, IService
        {
            Type key = typeof(T);

            if (_services.ContainsKey(key))
            {
                Debug.LogError(
                    $"Attempted to register service of type {key} which is already registered with the {GetType().Name}.");
                return;
            }

            _services.Add(key, service);
        }

        /// <summary>
        ///     Unregisters the service from the current service locator.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        public void Unregister<T>() where T : class, IService
        {
            Type key = typeof(T);

            if (!_services.ContainsKey(key))
            {
                Debug.LogError(
                    $"Attempted to unregister service of type {key} which is not registered with the {GetType().Name}.");
                return;
            }

            _services.Remove(key);
        }
    }
}