using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using GameFramework.Logging;
using GameFramework.LevelManagement;
using GameFramework;

[CreateAssetMenu(menuName = MENU_PATH + "TrainingLevel", fileName = "TrainingLevel")]
public class TrainingLevel : BasicLevelAsset
{
    public override LevelAsyncOperation PerformLoad()
    {
        LevelAsyncOperation levelOp = base.PerformLoad();
        var op = new AsyncOperationSource();
        levelOp.AddOperation(op);

        _PerformLoad(op);
        return levelOp;
    }

    private async void _PerformLoad(AsyncOperationSource op)
    {
        IGameLogger logger = GameLog.System.CreateLogger("TrainingLevel");

        var sceneObject = SceneManager.System.FindSceneObjectFor(_scenes[0]) as SceneObject;
        Transform spawnPoint = sceneObject.playerSpawnPoints.
            FirstOrDefault(trans => trans is not null);

        if (spawnPoint is not null)
        {
            var localPlayer = PlayerManager.CreateLocalPlayer();
            if (localPlayer is not null)
            {
                string charName = "SwatGuy";
                CharacterAsset charAsset = await Addressables.LoadAssetAsync<CharacterAsset>(
                    $"Characters/{charName}").Task;

                if (charAsset is null)
                {
                    Debug.LogError($"Could not load character asset, {charName}", this);
                    throw new UnityException();
                }

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

        op.SetCompleted();
    }
}