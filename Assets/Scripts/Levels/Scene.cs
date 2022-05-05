using UnityEngine;
using UnityEditor;

[CreateAssetMenu(fileName = "Scene")]
public class Scene : ScriptableObject
{
#if UNITY_EDITOR
    public SceneAsset sceneAsset;
#endif

    public int sceneIndex = -1;
}