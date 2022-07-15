using UnityEngine;

public class BulletProjectile : Projectile
{
    [Label("Max Speed"), SerializeField]
    protected float _maxSpeed = 0f;
    public float maxSpeed => _maxSpeed;

    public override void Launch(Vector3 launchVector)
    {
    }

    public override void Stop()
    {
    }
}