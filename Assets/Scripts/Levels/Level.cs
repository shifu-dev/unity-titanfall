using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum LevelStatus
{
    Loading,
    SubSceneLoading,
    Loaded,
    Unloading,
    Unloaded
}

[CreateAssetMenu(fileName = "Level")]
public class Level : ScriptableObject
{
    public string levelName;
    public LevelScene mainScene;
    public LevelScene[] additiveScenes;

    [Space]
    [SerializeField, ReadOnly]
    protected LevelStatus m_levelStatus = LevelStatus.Unloaded;
    public LevelStatus levelStatus => m_levelStatus;

    [SerializeField, ReadOnly]
    protected float m_progress = 0;
    public float progress => m_progress;

    public event Action<LevelStatus> onStatusUpdated;

    public virtual void LoadLevel()
    {
        BeforeLoadLevel();
        SetStatus(LevelStatus.Loading);

        mainScene.LoadScene(LoadSceneMode.Single);

        SetStatus(LevelStatus.SubSceneLoading);
        for (int i = 0; i < additiveScenes.Length; i++)
        {
            additiveScenes[i].LoadScene(LoadSceneMode.Additive);
        }

        SetStatus(LevelStatus.Loaded);
        AfterLoadLevel();
    }

    public virtual async Task LoadLevelAsync()
    {
        BeforeLoadLevel();
        SetStatus(LevelStatus.Loading);

        await mainScene.LoadSceneAsync(LoadSceneMode.Single);
        mainScene.FindSceneObject();

        if (additiveScenes.Length > 0)
        {
            SetStatus(LevelStatus.SubSceneLoading);
            AsyncOperation[] additiveOps = new AsyncOperation[additiveScenes.Length];
            for (int i = 0; i < additiveScenes.Length; i++)
            {
                additiveOps[i] = additiveScenes[i].LoadSceneAsync(LoadSceneMode.Additive);
            }

            await additiveOps;

            foreach (var scene in additiveScenes)
            {
                scene.FindSceneObject();
            }
        }

        SetStatus(LevelStatus.Loaded);
        AfterLoadLevel();
    }

    protected virtual void BeforeLoadLevel()
    {
    }

    protected virtual void AfterLoadLevel()
    {
    }

    public virtual void UnloadLevel()
    {
    }

    protected virtual void BeforeUnloadLevel()
    {
    }

    protected virtual void AfterUnloadLevel()
    {
    }

    protected void SetStatus(LevelStatus status)
    {
        if (m_levelStatus == status)
            return;

        m_levelStatus = status;

        OnStatusUpdated(m_levelStatus);
        if (onStatusUpdated != null)
        {
            onStatusUpdated(m_levelStatus);
        }
    }

    protected virtual void OnStatusUpdated(LevelStatus status)
    {
    }
}