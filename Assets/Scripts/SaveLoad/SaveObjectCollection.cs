using System;
using System.Collections.Generic;
using DeepDreams.SaveLoad.Data;
using UnityEngine;

namespace DeepDreams.SaveLoad
{
    public class SaveObject
    {
        public readonly string id;
        public SaveData saveData;

        public SaveObject(string newId = null)
        {
            id = string.IsNullOrEmpty(newId) ? Guid.NewGuid().ToString() : newId;
        }
    }
    public class SaveObjectCollection
    {
        private readonly Dictionary<Type, SaveObject> _saveDatas;

        public SaveObjectCollection()
        {
            _saveDatas = new Dictionary<Type, SaveObject>();
        }

        public SaveObject TryGetData<T>() where T : SaveData, new()
        {
            if (_saveDatas.TryGetValue(typeof(T), out SaveObject saveObject))
            {
                // return (T)saveObject.saveData;
                return saveObject;
            }

            Debug.LogError(
                $"Save Data Collection Error: No save data object of type <color=red>{typeof(T).Name}</color> was found.");

            return null;
        }

        public bool TryAddData<T>(SaveObject saveObject) where T : SaveData
        {
            if (_saveDatas.TryAdd(typeof(T), saveObject))
            {
                return true;
            }

            Debug.LogError(
                $"Save Data Collection Error: Save data object of type <color=red>{typeof(T).Name}</color> could not be added. Maybe object already exists?");

            return false;
        }

        public bool TryAddData(Type type, SaveObject saveObject)
        {
            if (_saveDatas.TryAdd(type, saveObject))
            {
                return true;
            }

            Debug.LogError(
                $"Save Data Collection Error: Save data object of type <color=red>{type.Name}</color> could not be added. Maybe object already exists?");

            return false;
        }
    }
}