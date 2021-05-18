using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Bullet : Projectile
{
    public override void OnStopClient()
    {
        OnStop();
    }

    [ClientCallback]
    protected override void StartEffect()
    {
        PoolManager.Instance.RequestVFXBulletCollison(transform.position, Quaternion.identity);
        if (_isTakenDamage)
            PoolManager.Instance.RequestVFXTakeDamage(transform.position, Quaternion.identity);
    }
}
