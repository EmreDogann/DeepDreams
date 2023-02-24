using UnityEngine;

namespace DeepDreams
{
    public abstract class Singleton<T> : MonoBehaviour
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                if (Equals(_instance, null) || _instance == null || _instance.Equals(null))
                {
                    var instanceGO = FindObjectOfType<Singleton<T>>();
                    _instance = instanceGO.GetComponent<T>();
                    return _instance;
                }

                return _instance;
            }
            set => _instance = value;
        }

        // The child must call SingletonBuilder() with a reference to itself.
        protected void SingletonBuilder(T newInstance)
        {
            // If another already exists, forget this one
            var instanceGO = FindObjectsOfType<Singleton<T>>();

            if (instanceGO.Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            if (_instance == null)
            {
                _instance = newInstance;
            }
            else if (_instance.Equals(newInstance))
            {
                Debug.LogWarning("Found two singletons of type " + this);
                Destroy(gameObject);
            }
        }
    }
}