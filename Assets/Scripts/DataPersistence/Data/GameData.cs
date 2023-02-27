using UnityEngine;

namespace DeepDreams.DataPersistence.Data
{
    public class GameData : PersistentData
    {
        public Vector3 playerPosition;

        public GameData()
        {
            playerPosition = Vector3.zero;
        }
    }
}