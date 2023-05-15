using DeepDreams.SaveLoad;
using DeepDreams.SaveLoad.Data;
using DeepDreams.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DeepDreams
{
    public class GameLoader : MonoBehaviour
    {
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

            ServiceLocator.Instance.GetService<ISaveManager>().Load<GameData>();
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }
    }
}