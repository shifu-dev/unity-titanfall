using System;
using UnityEngine;

public class CharacterAssetRegistry : AssetRegistry<CharacterAsset>
{
    public static readonly CharacterAssetRegistry Instance = new CharacterAssetRegistry();
}