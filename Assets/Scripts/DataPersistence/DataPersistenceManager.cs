using System;
using System.Collections.Generic;
using System.Linq;
using DeepDreams.DataPersistence.Data;
using DeepDreams.Utils;
using TypeReferences;
using UnityEngine;

namespace DeepDreams.DataPersistence
{
    public class PersistentDataObject
    {
        public readonly string id;
        public PersistentData persistentData;

        public PersistentDataObject(string newId = null)
        {
            id = string.IsNullOrEmpty(newId) ? UtilHelpers.GenerateGuid().ToString() : newId;
        }
    }
    public class DataPersistenceManager : MonoBehaviour
    {
        [Serializable]
        private class PersistentDataDescriptor
        {
            public string fileName;
            [Inherits(typeof(PersistentData), ShowNoneElement = false, Grouping = Grouping.None, ShortName = true)]
            public TypeReference dataType;
            public bool useEncryption;
        }

        [SerializeField] private List<PersistentDataDescriptor> persistentDataObjectDescriptors;

        public static DataPersistenceManager instance { get; private set; }
        private FileDataHandler _fileDataHandler;

        private List<PersistentDataObject> _dataObjects;
        private List<IDataPersistence<PersistentData>> _dataPersistenceCallers;
        private Dictionary<string, FieldAccessor> _fieldAccessors;

        private void Awake()
        {
            if (instance != null) Debug.LogError("Found more than one Data Persistence Manager in the scene.");

            instance = this;

            _dataObjects = new List<PersistentDataObject>();
            _fileDataHandler = new FileDataHandler();

            persistentDataObjectDescriptors = persistentDataObjectDescriptors
                .GroupBy(x => x.dataType)
                .Select(y => y.First())
                .ToList();

            foreach (PersistentDataDescriptor descriptor in persistentDataObjectDescriptors)
            {
                PersistentDataObject dataObject = new PersistentDataObject();
                dataObject.persistentData = (PersistentData)Activator.CreateInstance(descriptor.dataType);

                _dataObjects.Add(dataObject);
                _fileDataHandler.AddFile(Application.persistentDataPath, descriptor.fileName, descriptor.useEncryption, dataObject.id);
            }

            _fieldAccessors = new Dictionary<string, FieldAccessor>();
        }

        private void Start()
        {
            _dataPersistenceCallers = FindAllDataPersistenceCallers();
            InitializeAllDataPersistenceCallers();

            LoadData<SettingsData>();
        }

        private void InitializeAllDataPersistenceCallers()
        {
            for (int i = 0; i < _dataPersistenceCallers.Count; i++)
            {
                _dataPersistenceCallers[i].InitDataPersistence();
            }
        }

        public FieldAccessor GetFieldAccessor<T>(string key)
        {
            if (!_fieldAccessors.ContainsKey(key)) _fieldAccessors.Add(key, new FieldAccessor(typeof(T), key));

            return _fieldAccessors[key];
        }

        public void SaveData<T>() where T : PersistentData, new()
        {
            PersistentDataObject dataObject = _dataObjects.FirstOrDefault(p => p.persistentData.GetType() == typeof(T));

            if (dataObject.persistentData == null)
                Debug.LogError($"Data Persistence Saving Error: No data object of type <color=red>{typeof(T).Name}</color> was found.");

            foreach (var persistenceObject in _dataPersistenceCallers.OfType<IDataPersistence<T>>())
                persistenceObject.SaveData(dataObject.persistentData);

            _fileDataHandler.Save(dataObject);
        }

        public void LoadData<T>() where T : PersistentData, new()
        {
            int dataObjectIndex = _dataObjects.FindIndex(data => data.persistentData.GetType() == typeof(T));

            if (dataObjectIndex == -1)
            {
                Debug.LogError($"Data Persistence Loading Error: No data object of type <color=red>{typeof(T).Name}</color> was found.");
                return;
            }

            T dataObject = _fileDataHandler.Load<T>(_dataObjects[dataObjectIndex].id);

            PersistentDataObject persistentDataObject = _dataObjects[dataObjectIndex];
            if (dataObject == null)
                Debug.LogWarning($"No file representing type <color=red>{typeof(T).Name}</color> was found. Using defaults.");
            else persistentDataObject.persistentData = dataObject;

            T dataObjectToSend = (T)persistentDataObject.persistentData;

            foreach (var persistenceObject in _dataPersistenceCallers.OfType<IDataPersistence<T>>())
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