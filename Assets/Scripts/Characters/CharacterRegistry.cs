using System;
using UnityEngine;

public static class CharacterRegistry
{
    static CharacterRegistry()
    {
        _sources = new CharacterSource[0];
    }

    public static void Init()
    {
        LoadSources();
    }

    public static void Shutdown()
    {
        UnloadSources();
    }

    public static void LoadSources()
    {
        _sources = Resources.LoadAll<CharacterSource>("");
        foreach (var source in _sources)
        {
            source.OnLoad();
        }
    }

    public static void UnloadSources()
    {
        foreach (var source in _sources)
        {
            source.OnUnload();
            Resources.UnloadAsset(source);
        }
    }

    public static CharacterSource GetSource(string charName)
    {
        if (String.IsNullOrEmpty(charName))
        {
            return null;
        }

        foreach (var source in _sources)
        {
            if (source.characterName == charName)
            {
                return source;
            }
        }

        return null;
    }

    public static int Count => _sources.Length;

    private static CharacterSource[] _sources;
}