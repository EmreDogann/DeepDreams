using DeepDreams.SaveLoad.Data;

namespace DeepDreams.SaveLoad
{
    public interface ISaveable {}

    public interface ISaveable<in T> : ISaveable where T : SaveData, new()
    {
        void SaveData(T saveData);
        void LoadData(T saveData);
    }
}