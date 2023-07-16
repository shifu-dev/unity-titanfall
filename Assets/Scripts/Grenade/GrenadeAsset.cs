using UnityEngine;

[CreateAssetMenu(menuName = "Grenades/Grenade Asset")]
public class GrenadeAsset : ScriptableObject
{
    [SerializeField] protected string _GrenadeName;
    public string GrenadeName => _GrenadeName;

    [Header("GRENADE"), Space]
    [SerializeField, Min(0)]
    protected float _triggerTime;

    [SerializeField]
    protected bool _canStopTrigger;
}