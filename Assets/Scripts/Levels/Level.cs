using UnityEngine;
using UnityEngine.SceneManagement;

[CreateAssetMenu(fileName = "Level")]
public class Level : ScriptableObject
{
    public Scene mainScene;
    public Scene[] additiveScenes;

    void PreLoad()
    {
    }
}