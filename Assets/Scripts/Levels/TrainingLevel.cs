using UnityEngine;
using GameLog;

using ILogger = GameLog.ILogger;

[CreateAssetMenu(fileName = "TrainingLevel")]
public class TrainingLevel : Level
{
    protected override void AfterLoadLevel()
    {
        ILogger logger = LevelManager.logger.GetOrCreateSubLogger("TrainingLevel");

        var sceneObject = mainScene.sceneObject;
        var spawnPositions = sceneObject.playerSpawnPoints;

        Transform spawnPoint = null;
        foreach (var spawnPos in spawnPositions)
        {
            if (spawnPos != null)
            {
                spawnPoint = spawnPos;
                break;
            }
        }

        if (spawnPoint != null)
        {
            var localPlayer = PlayerManager.localPlayer;
            if (localPlayer != null)
            {
                CharacterAsset charSource = CharacterRegistry.Instance.GetAsset("SwatGuy CharacterAsset");
                charSource.instanceHandler.Reserve(4, 5);

                Character character = charSource.instanceHandler.Create(
                    spawnPoint.position, spawnPoint.rotation);

                logger.Info("Created Character");

                // player possess character
                localPlayer.Possess(character);
                logger.Info("Player Possessed Character");
            }
        }
    }
}