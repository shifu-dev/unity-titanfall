using System;
using UnityEngine;

public class GameInstance : IGameInstanceChannelPipeline
{
    #region Singeltion Interface

    protected static GameInstance Instance = new GameInstance();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void StaticSubsystemRegistrationRuntimeMethod()
    {
        Instance.SubsystemRegistrationRuntimeMethod();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    static void StaticAfterAssembliesLoadedRuntimeMethod()
    {
        Instance.AfterAssembliesLoadedRuntimeMethod();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    static void StaticBeforeSplashScreenRuntimeMethod()
    {
        Instance.BeforeSplashScreenRuntimeMethod();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void StaticBeforeSceneLoadRuntimeMethod()
    {
        Instance.BeforeSceneLoadRuntimeMethod();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void StaticAfterSceneLoadRuntimeMethod()
    {
        Instance.AfterSceneLoadRuntimeMethod();
    }

    #endregion

    protected GameInstanceChannel UpdateChannel;

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    protected virtual void SubsystemRegistrationRuntimeMethod()
    {
        Debug.Log("GAMEINSTANCE: SubsystemRegistration");
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    protected virtual void AfterAssembliesLoadedRuntimeMethod()
    {
        Debug.Log("GAMEINSTANCE: AfterAssembliesLoaded");
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
    protected virtual void BeforeSplashScreenRuntimeMethod()
    {
        Debug.Log("GAMEINSTANCE: BeforeSplashScreen");
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    protected virtual void BeforeSceneLoadRuntimeMethod()
    {
        Debug.Log("GAMEINSTANCE: BeforeSceneLoad");
    }

    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    protected virtual void AfterSceneLoadRuntimeMethod()
    {
        Debug.Log("GAMEINSTANCE: AfterSceneLoad");

        // Setup GameInstanceUpdateChannel
        GameObject UpdateChannelPrefab = Resources.Load<GameObject>("GameInstanceUpdateChannelPrefab");
        GameObject UpdateChannelGameObject = GameObject.Instantiate(UpdateChannelPrefab);
        UpdateChannelGameObject.name = "GameInstanceUpdateChannel";
        UpdateChannel = UpdateChannelGameObject.GetComponent<GameInstanceChannel>();
        UpdateChannel.Pipeline = this;
        GameObject.DontDestroyOnLoad(UpdateChannel);
        Debug.Log("GAMEINSTANCE: Created GameInstanceUpdateChannel");

        // Create Player
        GameObject PlayerPrefab = Resources.Load<GameObject>("PlayerPrefab");
        GameObject PlayerGameObject = GameObject.Instantiate(PlayerPrefab);
        PlayerGameObject.name = "LocalPlayer";
        Player player = PlayerGameObject.GetComponent<Player>();
        GameObject.DontDestroyOnLoad(PlayerGameObject);
        Debug.Log("GAMEINSTANCE: Created LocalPlayer");

        // Create Character
        GameObject CharacterPrefab = Resources.Load<GameObject>("Manny");
        GameObject CharacterGameObject = GameObject.Instantiate(CharacterPrefab, new Vector3(0, 1, 9), Quaternion.identity);
        CharacterGameObject.name = "Manny";
        Character character = CharacterGameObject.GetComponent<Character>();
        Debug.Log("GAMEINSTANCE: Created Character");

        // Player Possess Character
        player.Possess(character);
        Debug.Log("GAMEINSTANCE: Player Possessed Character");
    }

    public void AwakeByChannel(GameInstanceChannel Channel)
    {
        Debug.Log("GAMEINSTANCE: AwakeByChannel");
    }

    public void StartByChannel(GameInstanceChannel Channel)
    {
        Debug.Log("GAMEINSTANCE: StartByChannel");
    }

    public void UpdateByChannel(GameInstanceChannel Channel)
    {
        // Debug.Log("GAMEINSTANCE: UpdateByChannel");
    }
}