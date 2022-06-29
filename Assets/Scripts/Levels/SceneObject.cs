using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SceneObject : MonoBehaviour
{
    public const string globalTag = "SceneObject";

    public Transform[] playerSpawnPoints;
}