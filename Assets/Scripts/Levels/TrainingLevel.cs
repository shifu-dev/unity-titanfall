using UnityEngine;
using UnityEngine.AddressableAssets;
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
                string charName = "Swat Guy";
                CharacterAsset charAsset = Addressables.LoadAssetAsync<CharacterAsset>(
                    $"Characters/{charName}").Result;

                if (charAsset is null)
                {
                    Debug.LogError($"Could not load character asset[{charName}]", this);
                    throw new UnityException();
                }

                // CharacterAsset charAsset = CharacterAssetRegistry.Instance.GetAsset(
                //     "SwatGuy CharacterAsset");
                var charInstanceHandler = charAsset.instanceHandler;
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