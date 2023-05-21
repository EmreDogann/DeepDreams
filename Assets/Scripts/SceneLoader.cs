using MyBox;
using UnityEngine;
#if !UNITY_EDITOR
using UnityEngine.SceneManagement;
#endif

namespace DeepDreams
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private bool _loadImmediately;
        [SerializeField] private SceneReference _sceneToLoad;

#if !UNITY_EDITOR
        private void Awake()
        {
            if (_loadImmediately)
            {
                SceneManager.LoadScene(_sceneToLoad.SceneName, LoadSceneMode.Additive);
            }
            else
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            SceneManager.LoadScene(_sceneToLoad.SceneName, LoadSceneMode.Additive);
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
#endif
    }
}