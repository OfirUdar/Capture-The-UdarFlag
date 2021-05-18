using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


public class Projectile : NetworkBehaviour
{
    [SerializeField] private Rigidbody _rigidBody;
    [SerializeField] private float _speed = 35f;
    [SerializeField] private bool _isSplashDamage = false;
    [SerializeField] protected float _radiusSplashDamage = 2f;
    [SerializeField] private LayerMask _layerDamagable;

    private Vector3 _startPosition;
    private float _maxDistance = float.MaxValue;
    private int _damage;
    protected bool _isCollided;
    protected bool _isTakenDamage;
    private bool _isStopped;




    private void Update()
    {
        if (!_isCollided)
            transform.position += (transform.forward * _speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        CancelInvoke(nameof(UnSpawn));

        if (_isSplashDamage || other.gameObject.layer == Mathf.Log(_layerDamagable.value, 2))
        {
            if (isServer)
                TakeDamage(other);
            if (isClient)
                _isTakenDamage = true;
        }


        if (isServer)
            UnSpawn();
        if (isClientOnly)
            OnStop();
    }


    #region Server

    [Server]
    public void Setup(float distance, int damage)
    {
        _maxDistance = distance;
        _damage = damage;
        _startPosition = transform.position;

        float timeToUnSpawn = (_maxDistance / _speed);
        Invoke(nameof(UnSpawn), timeToUnSpawn);
    }

    [Server]
    private void UnSpawn()
    {
        NetworkServer.UnSpawn(this.gameObject);
        this.gameObject.SetActive(false); 
    }


    [Server]
    private void TakeDamage(Collider other)
    {
        if (_isSplashDamage)
        {
            TakeSplashDamage();
            return;
        }

        //Calculate the damage by the distance far
        float distance = (transform.position - _startPosition).magnitude;
        distance = Mathf.Min(_maxDistance, distance);
        int damage = (int)((1 - (distance / _maxDistance)) * _damage);


        // check if there is authority or teamate- if not damage this
        PlayerManager projectilePlayer = connectionToClient.identity.GetComponent<GamePlayer>().playerManager;
        PlayerManager damagedPlayer = other.GetComponent<PlayerManager>();

        if (!projectilePlayer.IsTeammate(damagedPlayer.playerLinks.gamePlayer)) // if is not teamate - damage to player!
        {
            Health damagedPlayerHealth = other.GetComponent<Health>();
            damagedPlayerHealth.TakeDamage(damage);
            if(damagedPlayerHealth.GetCurrentHealth()<=0)
            {
                projectilePlayer.playerLinks.gamePlayer.stats.AddKill();
            }
        }
    }
    [Server]
    private void TakeSplashDamage()
    {
        Vector3 explosionPose = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPose, _radiusSplashDamage, _layerDamagable);
        foreach (Collider coll in colliders)
        {
            //check if this is teamate 
            PlayerManager projectilePlayer = connectionToClient.identity.GetComponent<GamePlayer>().playerManager;
            PlayerManager collidePlayer = coll.GetComponent<PlayerManager>();
            if (projectilePlayer != collidePlayer && projectilePlayer.IsTeammate(collidePlayer.playerLinks.gamePlayer)) { return; }


            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(1250f * _radiusSplashDamage, explosionPose, _radiusSplashDamage, 0.01f, ForceMode.Force);

                //Damage the player
                float distancePlayer = (rb.position - explosionPose).magnitude;
                float ratio = distancePlayer / _radiusSplashDamage;
                int damageToPlayer = Mathf.RoundToInt(_damage * (1 - ratio));
                damageToPlayer = Mathf.Max(0, damageToPlayer);

                Health damagedPlayerHealth = rb.GetComponent<Health>();
                damagedPlayerHealth.TakeDamage(damageToPlayer);
                if (damagedPlayerHealth.GetCurrentHealth() <= 0)
                {
                    projectilePlayer.playerLinks.gamePlayer.stats.AddKill();
                }
            }
        }
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        _isTakenDamage = false;
        _isCollided = false;
        _isStopped = false;
    }


    [ClientCallback]
    protected void OnStop()
    {
        if(_isStopped) { return; }
        StartEffect();
        _isStopped = true;
    }

    [ClientCallback]
    protected virtual void StartEffect()
    {

    }

    #endregion


}

public enum ProjectileType
{
    Bullet,
    Rocket
}
