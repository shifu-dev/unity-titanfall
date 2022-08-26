using System;
using UnityEngine;

public class CharacterRegistry : AssetRegistry<CharacterAsset>
{
    public static readonly CharacterRegistry Instance = new CharacterRegistry();
}