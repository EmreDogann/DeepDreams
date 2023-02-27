using DeepDreams.DataPersistence.Data;

namespace DeepDreams.DataPersistence
{
    public interface IDataPersistence<out T> where T : PersistentData
    {
        void InitDataPersistence();
        void LoadData(PersistentData persistentData);
        void SaveData(PersistentData persistentData);
    }
}