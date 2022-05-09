using UnityEngine;

[CreateAssetMenu(menuName = "Grenades/Grenade Asset")]
public class GrenadeAsset : ScriptableObject
{
    [SerializeField] protected string m_GrenadeName;
    public string GrenadeName => m_GrenadeName;
}