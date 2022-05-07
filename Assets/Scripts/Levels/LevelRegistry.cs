using UnityEngine;

[CreateAssetMenu(fileName = "LevelRegistry")]
public class LevelRegistry : ScriptableObject
{
    public Level BootstrapLevel;
    public Level TrainingLevel;
}