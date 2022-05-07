using UnityEngine;

[CreateAssetMenu(fileName = "TrainingLevel")]
public class TrainingLevel : Level
{
    protected override void AfterLoadLevel()
    {
        var sceneObject = mainScene.sceneObject;
        var startPositions = sceneObject.PlayerStartPositions;

        if (startPositions.Length > 0)
        {
            var startPoint = startPositions[0];
            if (startPoint != null)
            {
                var localPlayer = PlayerManager.localPlayer;
                if (localPlayer != null)
                {
                    // Create Character
                    GameObject CharacterPrefab = Resources.Load<GameObject>("Manny");
                    GameObject CharacterGameObject = GameObject.Instantiate(CharacterPrefab,
                        startPoint.position, startPoint.rotation);

                    CharacterGameObject.name = "Manny";
                    Character character = CharacterGameObject.GetComponent<Character>();
                    Debug.Log("TrainingLevel: Created Character");

                    // Player Possess Character
                    localPlayer.Possess(character);
                    Debug.Log("TrainingLevel: Player Possessed Character");
                }
            }
        }
    }
}