using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DeepDreams.SaveLoad.Data;
using DeepDreams.Services;
using DeepDreams.Utils;
using MyBox;
using TypeReferences;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using Application = UnityEngine.Device.Application;

namespace DeepDreams.SaveLoad
{
    [Serializable]
    public class SaveFileDescriptor
    {
        public string fileName = "newfile.ini";
        [Inherits(typeof(SaveData), ShowNoneElement = false, Grouping = Grouping.None, ShortName = true)]
        public TypeReference dataType = typeof(GameData);
        public bool useEncryption;
#if UNITY_EDITOR
        public void ClearFileData()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);

            string fullFilePath = Path.Combine(Application.persistentDataPath, fileName);

            try
            {
                using (FileStream fs = new FileStream(fullFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
                {
                    lock (fs)
                    {
                        fs.SetLength(0);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Error occured when trying to clear file: {fullFilePath}\n{e}");
            }

            Debug.Log($"<color=green>{fullFilePath}</color> data cleared!");
        }
#endif
    }

    public class SaveManager : MonoBehaviour, ISaveManager
    {
        [Tooltip("Configuration options for save files.")]
        [SerializeField] private List<SaveFileDescriptor> saveFileDescriptors;
        [Tooltip("The scenes to exclude from saving and loading actions.")]
        [SerializeField] private List<SceneReference> sceneBlacklist;

        private FileDataHandler _fileDataHandler;
        private SaveObjectCollection _saveDataCollection;

        // Dictionary<Scene Handle (int), <Save Listener Type, Reference to listeners>>
        private Dictionary<int, Dictionary<Type, List<ISaveable>>> _listeners;
        private Dictionary<string, FieldAccessor> _fieldAccessors;

        // <ISaveable Runtime Type>
        private HashSet<Type> _iSaveableRuntimeTypes;
        // <ISaveable Runtime Type, List of types of classes that implements ISavable Runtime Type>
        private Dictionary<Type, List<Type>> _iSaveableImplementerTypes;

        // For debugging purposes.
        private Dictionary<int, Scene> _sceneHandles;

#if UNITY_EDITOR
        [ButtonMethod]
        private void OpenSaveDataLocation()
        {
            EditorUtility.OpenWithDefaultApp(Application.persistentDataPath);
        }
#endif

        private void Awake()
        {
            _iSaveableRuntimeTypes = new HashSet<Type>();
            _iSaveableImplementerTypes = new Dictionary<Type, List<Type>>();

            // Find all classes that implement the generic interface ISaveable<>
            var classes = UserDefinedAssemblies.GetUserCreatedAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => p.GetInterfaces()
                                .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ISaveable<>)) &&
                            p.IsClass);

            // Find the runtime types of the generic ISaveable<> interface that was implemented.
            foreach (Type classEntry in classes)
            {
                var implementedInterfaces = classEntry.GetInterfaces();

                foreach (Type interfaceEntry in implementedInterfaces)
                {
                    if (!interfaceEntry.IsConstructedGenericType)
                    {
                        continue;
                    }

                    _iSaveableRuntimeTypes.Add(interfaceEntry);

                    if (!_iSaveableImplementerTypes.ContainsKey(interfaceEntry))
                    {
                        _iSaveableImplementerTypes.Add(interfaceEntry, new List<Type>());
                    }

                    _iSaveableImplementerTypes[interfaceEntry].Add(classEntry);
                }
            }

            ServiceLocator.Instance.Register<ISaveManager>(this);

            _listeners = new Dictionary<int, Dictionary<Type, List<ISaveable>>>();
            _saveDataCollection = new SaveObjectCollection();
            _fileDataHandler = new FileDataHandler();

            saveFileDescriptors =
                saveFileDescriptors.GroupBy(x => x.dataType).Select(y => y.First()).ToList();

            foreach (SaveFileDescriptor descriptor in saveFileDescriptors)
            {
                SaveObject dataObject = new SaveObject();
                dataObject.saveData = (SaveData)Activator.CreateInstance(descriptor.dataType);

                _saveDataCollection.TryAddData(descriptor.dataType, dataObject);
                _fileDataHandler.AddFile(Application.persistentDataPath, descriptor.fileName,
                    descriptor.useEncryption,
                    dataObject.id);
            }

            _fieldAccessors = new Dictionary<string, FieldAccessor>();

            _sceneHandles = new Dictionary<int, Scene>();

            SceneManager.sceneLoaded += OnSceneLoad;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoad;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        private void OnSceneLoad(Scene scene, LoadSceneMode loadSceneMode)
        {
            // We don't want to search through scenes that don't need saving/loading.
            foreach (SceneReference blScene in sceneBlacklist)
            {
                if (scene.name == blScene.SceneName)
                {
                    _sceneHandles[scene.handle] = scene;
                    return;
                }
            }

            if (!_listeners.ContainsKey(scene.handle))
            {
                _sceneHandles[scene.handle] = scene;
                _listeners[scene.handle] = new Dictionary<Type, List<ISaveable>>();
            }

            _listeners[scene.handle] = FindAllInterfaceListeners(scene, _iSaveableRuntimeTypes);

            // Debug.Log($"{scene.name} scene's ISaveable listeners gathered.");

            SceneManager.SetActiveScene(scene);
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (_listeners.ContainsKey(scene.handle))
            {
                _listeners.Remove(scene.handle);
            }
        }

        public FieldAccessor GetFieldAccessor<T>(string key)
        {
            if (!_fieldAccessors.ContainsKey(key))
            {
                _fieldAccessors.Add(key, new FieldAccessor(typeof(T), key));
            }

            return _fieldAccessors[key];
        }

        public void Save<T>() where T : SaveData, new()
        {
            // SaveObject dataObject = _saveDataObjects.FirstOrDefault(p => p.saveData.GetType() == typeof(T));
            SaveObject dataObject = _saveDataCollection.TryGetData<T>();

            if (dataObject.saveData == null)
            {
                Debug.LogError(
                    $"Save Manager Saving: No data object of type <color=red>{typeof(T).Name}</color> was found.");
            }

            foreach (var entry in _listeners)
            {
                if (!entry.Value.TryGetValue(typeof(ISaveable<T>), out var listenersObject))
                {
                    Debug.LogWarning(
                        $"Save Manager Saving: No listener of type <color=red>{typeof(T).Name}</color> from scene {_sceneHandles[entry.Key].name} was found.");
                    continue;
                }

                var listeners = listenersObject.OfType<ISaveable<T>>();

                foreach (var saveObject in listeners)
                {
                    saveObject.SaveData((T)dataObject.saveData);
                }
            }

            _fileDataHandler.Save(dataObject);
        }

        public void Load<T>() where T : SaveData, new()
        {
            T loadedDataObject = _fileDataHandler.Load<T>(_saveDataCollection.TryGetData<T>().id);

            SaveObject saveDataObject = _saveDataCollection.TryGetData<T>();

            if (loadedDataObject == null)
            {
                Debug.LogWarning(
                    $"Save Manager Loading: No file representing type <color=red>{typeof(T).Name}</color> was found. Using defaults.");
            }
            else
            {
                saveDataObject.saveData = loadedDataObject;
            }

            foreach (var entry in _listeners)
            {
                if (!entry.Value.TryGetValue(typeof(ISaveable<T>), out var listenersObject))
                {
                    Debug.LogWarning(
                        $"Save Manager Loading: No listener of type <color=red>{typeof(T).Name}</color> from scene {_sceneHandles[entry.Key].name} was found.");
                    continue;
                }

                var listeners = listenersObject.OfType<ISaveable<T>>();

                foreach (var saveObject in listeners)
                {
                    saveObject.LoadData((T)saveDataObject.saveData);
                }
            }
        }

        public void RegisterListener<T>(ISaveable<T> listener) where T : SaveData, new()
        {
            int currentSceneHandle = SceneManager.GetActiveScene().handle;

            _listeners.TryAdd(currentSceneHandle, new Dictionary<Type, List<ISaveable>>());
            _listeners[currentSceneHandle].TryAdd(typeof(ISaveable<T>), new List<ISaveable>());

            if (_listeners[currentSceneHandle][typeof(ISaveable<T>)].Contains(listener))
            {
                Debug.LogWarning(
                    $"Save Manager Register: Listener already registered in scene <color=red>{SceneManager.GetActiveScene().name}</color>.");
                return;
            }

            _listeners[currentSceneHandle][typeof(ISaveable<T>)].Add(listener);
        }

        public void UnregisterListener<T>(ISaveable<T> listener) where T : SaveData, new()
        {
            int currentSceneHandle = SceneManager.GetActiveScene().handle;

            if (!_listeners.ContainsKey(currentSceneHandle))
            {
                Debug.LogWarning(
                    $"Save Manager Unregister: No scene handle of scene <color=red>{SceneManager.GetActiveScene().name}</color> was found in listener dictionary.");
                return;
            }

            if (!_listeners[currentSceneHandle].ContainsKey(typeof(ISaveable<T>)))
            {
                Debug.LogWarning(
                    $"Save Manager Unregister: No type <color=red>{typeof(T).Name}</color> was found in listener dictionary.");
                return;
            }

            _listeners[currentSceneHandle][typeof(ISaveable<T>)].Remove(listener);
        }

        private void OnApplicationQuit()
        {
            Save<SettingsData>();
            Save<GameData>();
        }

        // Note: Scripts implementing the interface must inherit from MonoBehaviour in order to be found.
        private Dictionary<Type, List<ISaveable>> FindAllInterfaceListeners(Scene currentScene,
            HashSet<Type> interfaceTypes)
        {
            var interfaces = new Dictionary<Type, List<ISaveable>>();
            var rootGameObjects = currentScene.GetRootGameObjects();

            foreach (GameObject rootGameObject in rootGameObjects)
            {
                var childrenInterfaces = rootGameObject.GetComponentsInChildren<ISaveable>(true);

                foreach (ISaveable childInterface in childrenInterfaces)
                {
                    foreach (Type type in interfaceTypes)
                    {
                        if (!_iSaveableImplementerTypes[type].Contains(childInterface.GetType()))
                        {
                            continue;
                        }

                        if (!interfaces.ContainsKey(type))
                        {
                            interfaces.Add(type, new List<ISaveable>());
                        }

                        interfaces[type].Add(childInterface);
                    }
                }
            }

            return interfaces;
        }
    }
}