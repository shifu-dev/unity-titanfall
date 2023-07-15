using UnityEngine;

[CreateAssetMenu(menuName = "Grenades/Grenade Asset")]
public class GrenadeAsset : ScriptableObject
{
    [SerializeField] protected string _GrenadeName;
    public string GrenadeName => _GrenadeName;
}