using System;
using UnityEngine;

public class CharacterRegistry : AssetRegistry<CharacterAsset>
{
    public static readonly CharacterRegistry Instance = new CharacterRegistry();

    public const string LOG_CATEGORY = "CHARACTER ASSET REGISTRY";

    public CharacterRegistry() : base(LOG_CATEGORY) { }
}