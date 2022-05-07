using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class SceneObject : MonoBehaviour
{
    public const string GlobalTag = "SceneObject";

    public Transform[] PlayerStartPositions;
}