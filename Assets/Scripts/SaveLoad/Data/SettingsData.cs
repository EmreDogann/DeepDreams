using System;

namespace DeepDreams.SaveLoad.Data
{
    [Serializable]
    public class SettingsData : SaveData
    {
        public float hFieldOfView;
        public float headBobbingAmount;
        public float masterVolume;
        public float sfxVolume;

        public SettingsData()
        {
            hFieldOfView = 90;
            headBobbingAmount = 1.0f;
            masterVolume = 0.75f;
            sfxVolume = 1.0f;
        }
    }
}