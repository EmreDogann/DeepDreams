using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace DeepDreams
{
    public class ServiceLocator : Singleton<ServiceLocator>
    {
        private IDictionary<Type, MonoBehaviour> serviceReferences;

        protected void Awake()
        {
            SingletonBuilder(this);
            serviceReferences = new Dictionary<Type, MonoBehaviour>();
        }

        public T GetService<T>() where T : MonoBehaviour, new()
        {
            Assert.IsNotNull(serviceReferences, "Someone has requested a service prior to the locator's intialization.");

            bool serviceLocated = serviceReferences.ContainsKey(typeof(T));

            if (!serviceLocated)
            {
                serviceReferences.Add(typeof(T), FindObjectOfType<T>());
            }

            Assert.IsTrue(serviceReferences.ContainsKey(typeof(T)), "Could not find service: " + typeof(T));
            T service = (T)serviceReferences[typeof(T)];
            Assert.IsNotNull(service, typeof(T) + " could not be found.");
            return service;
        }
    }
}