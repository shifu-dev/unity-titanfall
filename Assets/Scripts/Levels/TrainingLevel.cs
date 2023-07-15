using UnityEngine;
using GameFramework.Logging;
using GameFramework.LevelManagement;

[CreateAssetMenu(menuName = MENU_PATH + "TrainingLevel", fileName = "TrainingLevel")]
public class TrainingLevel : BasicLevelAsset
{
    public override LevelAsyncOperation PerformLoad()
    {
        LevelAsyncOperation operation = base.PerformLoad();

        IGameLogger logger = GameLog.System.CreateLogger("TrainingLevel");

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
            var localPlayer = PlayerManager.CreateLocalPlayer();
            if (localPlayer is not null)
            {
                CharacterAsset charSource = CharacterAssetRegistry.Instance.GetAsset(
                    "SwatGuy CharacterAsset");
                var charInstanceHandler = charSource.instanceHandler;
                charInstanceHandler.SetMaxReserve(5);
                charInstanceHandler.Reserve(4);

                Character character = charInstanceHandler.Create(
                    spawnPoint.position, spawnPoint.rotation);

                logger.Information("Created Character");
                localPlayer.Possess(character);
            }
            else
            {
                logger.Error("Could not create Local Player");
            }
        }

        return operation;
    }
}