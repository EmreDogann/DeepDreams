using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using DeepDreams.Services;
using MyBox;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeepDreams
{
    public class GameLoader : MonoBehaviour
    {
        [OverrideLabel("Load Save?")]
        public bool loadSave = true;

        private void Start()
        {
            ServiceLocator.Instance.GetService<ISaveManager>().Load<SettingsData>();
        }

        private void OnEnable()
        {
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnSceneChanged(Scene scene, Scene scene1)
        {
            // Debug.Log($"Scene changed from: {scene.name} to {scene1.name}");

            if (loadSave)
            {
                ServiceLocator.Instance.GetService<ISaveManager>().Load<GameData>();
            }

            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }
}