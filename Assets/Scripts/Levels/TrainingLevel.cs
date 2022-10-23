using UnityEngine;
using GameFramework.Logging;
using GameFramework.LevelManagement;

[CreateAssetMenu(menuName = MENU_PATH + "TrainingLevel", fileName = "TrainingLevel")]
public class TrainingLevel : BasicLevelAsset
{
    public override LevelAsyncOperation PerformLoad()
    {
        LevelAsyncOperation operation = base.PerformLoad();

        IGameLogger logger = GameLog.CreateLogger("TrainingLevel");

        // var sceneObject = SceneManager.FindSceneObject(_scenes[0]);
        SceneObject sceneObject = null;
        var spawnPositions = sceneObject.playerSpawnPoints;

        Transform spawnPoint = null;
        foreach (var spawnPos in spawnPositions)
        {
            if (spawnPos is not null)
            {
                spawnPoint = spawnPos;
                break;
            }
        }

        if (spawnPoint is not null)
        {
            var localPlayer = PlayerManager.localPlayer;
            if (localPlayer is not null)
            {
                CharacterAsset charSource = CharacterRegistry.Instance.GetAsset("SwatGuy CharacterAsset");
                charSource.instanceHandler.Reserve(4, 5);

                Character character = charSource.instanceHandler.Create(
                    spawnPoint.position, spawnPoint.rotation);

                logger.Information("Created Character");

                // player possess character
                localPlayer.Possess(character);
                logger.Information("Player Possessed Character");
            }
        }

        return operation;
    }
}