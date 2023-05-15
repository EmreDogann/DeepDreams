using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using UnityEngine;

namespace DeepDreams
{
    public class SaveableObjectTest : MonoBehaviour, ISaveable<GameData>, ISaveable<SettingsData>
    {
        private void Start() {}

        public void SaveData(GameData saveData) {}

        public void LoadData(GameData saveData) {}

        public void SaveData(SettingsData saveData) {}

        public void LoadData(SettingsData saveData) {}
    }
}