using DeepDreams.SaveLoad.Data;
using DeepDreams.Services;
using DeepDreams.Utils;

namespace DeepDreams.SaveLoad
{
    public interface ISaveManager : IService
    {
        public FieldAccessor GetFieldAccessor<T>(string key);
        public void Save<T>() where T : SaveData, new();

        public void Load<T>() where T : SaveData, new();

        public void RegisterListener<T>(ISaveable<T> listener) where T : SaveData, new();
        public void UnregisterListener<T>(ISaveable<T> listener) where T : SaveData, new();
    }
}