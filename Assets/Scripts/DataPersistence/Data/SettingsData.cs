using System;

namespace DeepDreams.DataPersistence.Data
{
    [Serializable]
    public class SettingsData : PersistentData
    {
        public float hFieldOfView;
        public bool headBobbing;
        public float masterVolume;
        public float sfxVolume;

        public SettingsData()
        {
            hFieldOfView = 90;
            headBobbing = true;
            masterVolume = 0.75f;
            sfxVolume = 1.0f;
        }
    }
}