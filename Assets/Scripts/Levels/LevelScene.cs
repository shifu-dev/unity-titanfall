using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "LevelScene")]
public class LevelScene : ScriptableObject
{
#if UNITY_EDITOR

    [SerializeField] private SceneAsset m_sceneAsset;

    public void UpdateSceneData()
    {
    }

#endif

    public int sceneIndex;
    public SceneObject sceneObject { get; protected set; }
    public Scene scene { get; protected set; }

    public void LoadScene(LoadSceneMode loadSceneMode)
    {
        scene = SceneManager.GetSceneAt(sceneIndex);
        SceneManager.LoadScene(sceneIndex, loadSceneMode);
    }

    public AsyncOperation LoadSceneAsync(LoadSceneMode loadSceneMode)
    {
        scene = SceneManager.GetSceneByBuildIndex(sceneIndex);
        LevelManager.logger.Info("LEVELSCENE: Loading... | Index: " + sceneIndex + " | Scene: " + (scene == null ? false : true));
        return SceneManager.LoadSceneAsync(sceneIndex, loadSceneMode);
    }

    public SceneObject FindSceneObject(bool force = false)
    {
        if (force || sceneObject == null)
        {
            LevelManager.logger.Info("LEVELSCENE: FindingSceneObject | force: " + force + " | SceneObject: " + sceneObject);

            GameObject[] sceneGameObjects = GameObject.FindGameObjectsWithTag(SceneObject.GlobalTag);
            LevelManager.logger.Info("LEVELSCENE: Found " + sceneGameObjects.Length + " SceneObjects");

            foreach (var go in sceneGameObjects)
            {
                if (go.scene.buildIndex == sceneIndex)
                {
                    SceneObject so = go.GetComponent<SceneObject>();
                    if (so)
                    {
                        sceneObject = so;
                        break;
                    }
                }
            }
        }

        if (sceneObject)
        {
            LevelManager.logger.Info("LEVELSCENE: Found SceneObject");
            return sceneObject;
        }

        LevelManager.logger.Info("LEVELSCENE: Could Not Found SceneObject");
        return sceneObject;
    }
}