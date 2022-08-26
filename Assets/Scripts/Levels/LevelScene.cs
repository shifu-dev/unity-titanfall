using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LevelScene")]
public class LevelScene : ScriptableObject
{
#if UNITY_EDITOR

    [SerializeField]
    private SceneAsset _sceneAsset;

    public void UpdateSceneData()
    {
    }

#endif

    public void LoadScene(LoadSceneMode loadSceneMode)
    {
        _scene = SceneManager.GetSceneAt(_sceneIndex);
        SceneManager.LoadScene(_sceneIndex, loadSceneMode);
    }

    public AsyncOperation LoadSceneAsync(LoadSceneMode loadSceneMode)
    {
        _scene = SceneManager.GetSceneByBuildIndex(_sceneIndex);
        return SceneManager.LoadSceneAsync(_sceneIndex, loadSceneMode);
    }

    public SceneObject FindSceneObject(bool force = false)
    {
        if (force || _sceneObject == null)
        {
            GameObject[] sceneGameObjects = GameObject.FindGameObjectsWithTag(SceneObject.globalTag);

            foreach (var go in sceneGameObjects)
            {
                if (go.scene.buildIndex == _sceneIndex)
                {
                    SceneObject so = go.GetComponent<SceneObject>();
                    if (so)
                    {
                        _sceneObject = so;
                        break;
                    }
                }
            }
        }

        if (_sceneObject)
        {
            return _sceneObject;
        }

        return _sceneObject;
    }

    protected int _sceneIndex;

    protected SceneObject _sceneObject;
    public SceneObject sceneObject
    {
        get => _sceneObject;
    }

    protected Scene _scene;
    public Scene scene
    {
        get => _scene;
    }
}