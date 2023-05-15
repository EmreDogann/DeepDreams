using UnityEngine;

namespace DeepDreams.SaveLoad.Data
{
    public class GameData : SaveData
    {
        public Vector3 playerPosition;
        public Quaternion playerOrientation;

        public GameData()
        {
            playerPosition = Vector3.zero;
            playerOrientation = Quaternion.identity;
        }
    }
}