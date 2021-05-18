using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Grenade : ItemThrow
{
    [Header("Grenade")]
    [SerializeField] private RendererVisibleHandler _rendererVisibleHandler;
    [SerializeField] private float _damage = 15f;
    [SerializeField] private float _power = 700f;
    [SerializeField] private float _delayExplode = 0.5f;
    [SerializeField] private LayerMask _layerDamagable;
    [SerializeField] private float _radiusOffset = 0.15f;
    [SerializeField] private GameObject _trailGO;


    [Header("Visual Before Explosion")]
    [SerializeField] private Transform _outerCircle;
    [SerializeField] private SpriteRenderer _innerCircle;
    [SerializeField] private Color _targetColor;
    [SerializeField] private float _shakePower = 0.2f;
    [SerializeField] private float _shakeDuration = 0.2f;


    private Color _startColor;
    private float _timer = 0;


    private bool _isThrew = false;
    private bool _isGoingToExplode = false;
    private PlayerManager _playerThrow; // the player who threw this
    private bool _isVisible = true;


    private float _radiusThrowing;

    protected override void Update()
    {
        if (!_isThrew)
        {
            if (isClient)
                base.Update();
        }
        else
        {
            if (isServer && !_isGoingToExplode && _rigidBody.velocity.sqrMagnitude <= .5f)
                StartGoingToExplode();

            if (isClient && _isGoingToExplode)
            {
                CirclePredictionVisualHandler();
            }
        }

    }


    #region Server

    [Server]
    public override void ServerRemove(bool isThrow = false)
    {
        _playerThrow = this.connectionToClient.identity.GetComponent<GamePlayer>().playerManager;

        _radiusThrowing = _aimingProperties.radiusThrowing;

        base.ServerRemove(isThrow);
    }
    [Server]
    private void StartGoingToExplode()
    {
        _rigidBody.velocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
        //Start Expolsion
        _isGoingToExplode = true;
        if (isServerOnly)
            Invoke(nameof(Explosion), _delayExplode);


        RpcStartGoingToExplode();
    }
    private void Explosion()
    {
        if (isServer)
            TakeDamage();
        if (isClient)
        {
            ClientVFXExplosion();
        }

        Destroy(this.gameObject);
    }
    [Server]
    private void TakeDamage()
    {
        _radiusThrowing *= (1 - _radiusOffset);
        Vector3 explosionPose = transform.position;

        Collider[] colliders = Physics.OverlapSphere(explosionPose, _radiusThrowing, _layerDamagable);
        foreach (Collider coll in colliders)
        {
            //check if this is teamate 
            PlayerManager collidePlayer = coll.GetComponent<PlayerManager>();
            if (_playerThrow != collidePlayer && _playerThrow.IsTeammate(collidePlayer.playerLinks.gamePlayer)) { return; }

            Rigidbody rb = coll.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(_power, explosionPose, _radiusThrowing, 0.01f, ForceMode.Force);

                //Damage the player
                float distancePlayer = (rb.position - explosionPose).magnitude;
                float ratio = distancePlayer / _radiusThrowing;
                int damageToPlayer = Mathf.RoundToInt(_damage * (1 - ratio));
                damageToPlayer = Mathf.Max(0, damageToPlayer);

                Health damagedPlayerHealth = rb.GetComponent<Health>();
                damagedPlayerHealth.TakeDamage(damageToPlayer);
                if (damagedPlayerHealth.GetCurrentHealth() <= 0)
                {
                    _playerThrow.playerLinks.gamePlayer.stats.AddKill();
                }
            }

        }
    }
    [Server]
    protected override void ThrowPointAnimation(Vector3 launchDirection, float launchForce)
    {
        _rigidBody.freezeRotation = false;
        base.ThrowPointAnimation(launchDirection, launchForce);
        _isEquiped = true;
        _isThrew = true;
        RpcGrenadeThrow();
    }


    #endregion


    #region Client

    public override void OnStartClient()
    {
        base.OnStartClient();
        _rendererVisibleHandler.OnVisibleChanged += (bool isVisible) => _isVisible = isVisible;
    }
    public override void OnStopClient()
    {
        base.OnStopClient();
        _rendererVisibleHandler.OnVisibleChanged -= (bool isVisible) => _isVisible = isVisible;
    }

    [Client]
    public override void Use(bool isUsing, bool isOnceUsing)
    {
        base.Use(isUsing, isOnceUsing);

        if (!isOnceUsing)
        {
            bool inputAim = true;
            bool inputThrow = false;

            _playerHolding.playerAiming.HandleInputThrow(inputAim, inputThrow, false); //aim
        }

    }
    [Client]
    public override void StopUse()
    {
        base.StopUse();
        
        if(!_isThrowing)
        {
            bool inputAim = false;
            bool inputThrow = true;

            _playerHolding.playerAiming.HandleInputThrow(inputAim, inputThrow, false); //throw
        }      
    }
    [Client]
    private void CirclePredictionVisualHandler()
    {
        _innerCircle.transform.localScale += Vector3.one * ((1 - _radiusOffset) / (_delayExplode)) * Time.deltaTime;
        _timer += (1 / _delayExplode) * Time.deltaTime;
        _innerCircle.color = Color.Lerp(_startColor, _targetColor, _timer);
    }

    [Client]
    private void ClientVFXExplosion()
    {
        VFX vfx = PoolManager.Instance.RequestVFXExplosion(transform.position, Quaternion.identity);
        vfx.transform.localScale = Vector3.one * _radiusThrowing * 2;
        if (_isVisible)
            CameraShake.Instance.Shake(_shakeDuration, _shakePower);
    }

    [ClientRpc]
    private void RpcGrenadeThrow()
    {
        _isThrew = true;
        _trailGO.SetActive(true);

        _radiusThrowing = _aimingProperties.radiusThrowing;
        _outerCircle.localScale = Vector3.one * _radiusThrowing * 2f;
        _innerCircle.transform.localScale = Vector3.zero;
        _startColor = _innerCircle.color;
    }

    [ClientRpc]
    private void RpcStartGoingToExplode()
    {
        _smoothSyncMirror.enabled = false;

        _rigidBody.velocity = Vector3.zero;
        _rigidBody.angularVelocity = Vector3.zero;
        //Start Expolsion
        _isGoingToExplode = true;
        _delayExplode -= (float)NetworkTime.rtt / 2f;
        Invoke(nameof(Explosion), _delayExplode);

        //Visual PreExplosion
        Vector3 outerCirclePos = transform.position;
        outerCirclePos.y -= .16f;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, 5f, _whatIsGround))
            outerCirclePos.y = hitInfo.point.y + 0.025f;
        _outerCircle.position = outerCirclePos;
        _outerCircle.rotation = Quaternion.Euler(90, 0, 0);
        _outerCircle.gameObject.SetActive(true);

    }



    #endregion
}
