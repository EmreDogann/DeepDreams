using System.Collections.Generic;
using System.Linq;
using DeepDreams.DataPersistence.Data;
using DeepDreams.Utils;
using UnityEngine;

namespace DeepDreams.DataPersistence
{
    public class DataPersistenceManager : MonoBehaviour
    {
        [Header("File Storage Config")]
        [SerializeField] private string fileName;
        [SerializeField] private bool useEncryption;

        public static DataPersistenceManager instance { get; private set; }
        private FileDataHandler _fileDataHandler;

        private List<PersistentData> _persistentDataObjects;
        private List<IDataPersistence<PersistentData>> _dataPersistenceCallers;
        // private Dictionary<PersistentData, List<IDataPersistence<PersistentData>>>
        private Dictionary<string, FieldAccessor> _fieldAccessors;

        private void Awake()
        {
            if (instance != null) Debug.LogError("Found more than one Data Persistence Manager in the scene.");

            instance = this;

            _fieldAccessors = new Dictionary<string, FieldAccessor>();

            _persistentDataObjects = new List<PersistentData>();
            _persistentDataObjects.Add(new GameData());
            _persistentDataObjects.Add(new SettingsData());
        }

        private void Start()
        {
            _fileDataHandler = new FileDataHandler(Application.persistentDataPath, fileName, useEncryption);

            _dataPersistenceCallers = FindAllDataPersistenceCallers();
            InitializeAllDataPersistenceCallers();

            LoadData<SettingsData>();
        }

        private void InitializeAllDataPersistenceCallers()
        {
            // float startTime = Time.realtimeSinceStartup;

            for (int i = 0; i < _dataPersistenceCallers.Count; i++)
            {
                _dataPersistenceCallers[i].InitDataPersistence();
            }

            // Debug.Log($"Time elapsed: {Time.realtimeSinceStartup - startTime}");
        }

        public FieldAccessor GetFieldAccessor<T>(string key)
        {
            if (!_fieldAccessors.ContainsKey(key)) _fieldAccessors.Add(key, new FieldAccessor(typeof(T), key));

            return _fieldAccessors[key];
        }

        public void SaveData<T>() where T : PersistentData, new()
        {
            T dataObject = _persistentDataObjects.OfType<T>().FirstOrDefault();

            if (dataObject == null)
                Debug.LogError($"Data Persistence Saving Error: No data object of type {typeof(T).Name} was found. Using defaults.");

            foreach (var persistenceObject in _dataPersistenceCallers.OfType<IDataPersistence<T>>())
                // Debug.Log("Saving: " + persistenceObject.GetType().FullName);
                persistenceObject.SaveData(dataObject);

            _fileDataHandler.Save(dataObject);
        }

        public void LoadData<T>() where T : PersistentData, new()
        {
            int dataObjectIndex = _persistentDataObjects.FindIndex(data => data.GetType() == typeof(T));

            if (dataObjectIndex == -1)
            {
                Debug.LogError($"Data Persistence Loading Error: No data object of type {typeof(T).Name} was found. Creating default one.");
                _persistentDataObjects.Add(new T());
                dataObjectIndex = _persistentDataObjects.Count() - 1;
            }

            T dataObject = _fileDataHandler.Load<T>();

            if (dataObject == null) Debug.LogWarning($"No file representing type {typeof(T).Name} was found. Using defaults.");
            else _persistentDataObjects[dataObjectIndex] = dataObject;

            T dataObjectToSend = (T)_persistentDataObjects[dataObjectIndex];

            foreach (var persistenceObject in _dataPersistenceCallers.OfType<IDataPersistence<T>>())
                // Debug.Log("Loading: " + persistenceObject.GetType().FullName);
                persistenceObject.LoadData(dataObjectToSend);
        }

        private void OnApplicationQuit()
        {
            SaveData<SettingsData>();
        }

        private List<IDataPersistence<PersistentData>> FindAllDataPersistenceCallers()
        {
            // Note: Scripts implementing the interface must inherit from MonoBehaviour in order to be found.
            var dataPersistenceObjects = FindObjectsOfType<MonoBehaviour>(true).OfType<IDataPersistence<PersistentData>>();
            var dataPersistenceBases = dataPersistenceObjects.ToList();
            return new List<IDataPersistence<PersistentData>>(dataPersistenceBases);
        }
    }
}