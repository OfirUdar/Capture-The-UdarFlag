using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Weapon : Item
{
    [Header("Weapon")]
    [SerializeField] private ProjectileType _projectileType;
    [SerializeField] private Transform _shotPoint;
    [SerializeField] private GameObject _vfxShoot;
    [Space]
    [SerializeField] private float _fireRate = 0.1f;
    [SerializeField] private float _distance = 100f;
    [SerializeField] private int _damage = 10;
    [SerializeField] private int _bulletAmountMagazine = 30;
    [SerializeField] private int _bulletsPerShot = 1;
    [SerializeField] private float _timeBtwBulletsPerShot = 0.05f;
    [SerializeField] private float _spreadAngle;
    [SerializeField] private float _shakePower = 0.05f;
    [SerializeField] private float _shakeDuration = 0.1f;
    [SerializeField] private bool _spreadRandom = true;
    [SerializeField] private bool _allowHoldButton = true;

    [Header("Audio")]
    [SerializeField] private Audio _shootAudio;
    [SerializeField] private Audio _reloadAudio;


    private float _timer = 0;
    [SyncVar(hook = nameof(ClientHandleCurrentBulletAmountChanged))]
    private int _currentBulletAmount;
    private int _currentBulletsAmountPerShot;
    private bool _canShoot = false;
    [SyncVar]
    private bool _isReloading = false;


    public static event Action<int, int> AuthorityOnAmountBulletChanged;




    public int GetCurrentBulletAmount()
    {
        return _currentBulletAmount;
    }
    public int GetBulletAmountMagazine()
    {
        return _bulletAmountMagazine;
    }

    public Vector3 GetShotPoint()
    {
        return _shotPoint.position;
    }

    private void OnEnable()
    {
        if (isServer && _currentBulletAmount <= 0)
            Invoke(nameof(StartReload), 0.1f);
    }

    private void OnDisable()
    {
        if (isClient)
        {
            if (_isReloading)
                ClientReloadStopped();
        }
        if (isServer)
        {
            //Stop Reloading
            StopReload();
        }
    }

    protected override void Update()
    {
        if (isClient)
        {
            base.Update();
        }
        if (!_canShoot)
        {
            if (_timer >= _fireRate)
                _canShoot = true;
            else
                _timer += Time.deltaTime;
        }

    }

    #region Server

    public override void OnStartServer()
    {
        _currentBulletAmount = _bulletAmountMagazine;
    }

    public override void ServerRemove(bool isThrow = false)
    {
        StopReload();
        base.ServerRemove();
    }

    [Server]
    private void ShootBulletAnimation()
    {
        _playerHolding.animManager.TriggerShoot(ShootBullet);
    }
    [Server]
    private void ShootBullet()
    {
        //Spread
        float spreadInstance = 0f;
        if (_spreadAngle != 0)
        {
            if (_spreadRandom)
                spreadInstance = UnityEngine.Random.Range(-_spreadAngle, _spreadAngle);
            else
                spreadInstance = -_spreadAngle + (_spreadAngle * 2) / (_bulletsPerShot - 1) * (_bulletsPerShot - _currentBulletsAmountPerShot);

        }

        //Because of TOPDOWN Game - eular.x=0

        Quaternion projectileRotation = Quaternion.LookRotation(_shotPoint.forward, Vector3.up);
        projectileRotation = Quaternion.Euler(0, projectileRotation.eulerAngles.y + spreadInstance, projectileRotation.eulerAngles.z);

        Projectile projectile = PoolManager.Instance.ServerRequestProjectile(_shotPoint.position, projectileRotation, _projectileType, connectionToClient);
        projectile.Setup(_distance, _damage);

        _currentBulletAmount = Mathf.Max(0, _currentBulletAmount - 1);
        _currentBulletsAmountPerShot--;


        if (_currentBulletAmount > 0 && _currentBulletsAmountPerShot > 0)
            Invoke(nameof(ShootBullet), _timeBtwBulletsPerShot);
        else
        {
            if (_currentBulletAmount <= 0)
            {
                StartReload();
            }

            RpcBulletShot();// rpc on bullet shoot
        }

    }


    [Server]
    private void ServerHandleReloadComplete()
    {
        _currentBulletAmount = _bulletAmountMagazine;
        _isReloading = false;
    }
    [Server]
    private void StartReload()
    {
        if (_isReloading || _currentBulletAmount == _bulletAmountMagazine) { return; }

        _isReloading = true;
        RpcStartReload();
        _playerHolding.animManager.StartReload(ServerHandleReloadComplete);
    }
    [Server]
    private void StopReload()
    {
        if (!_isReloading) { return; }

        _isReloading = false;
        _playerHolding.animManager.StopReload();
    }

    [Command]
    private void CmdShootBullet()
    {
        if (!_canShoot || _isReloading) { return; }


        _currentBulletsAmountPerShot = _bulletsPerShot;

        ShootBulletAnimation();
        _timer = 0;
        _canShoot = false;
    }
    [Command]
    public void CmdReload()
    {
        StartReload();
    }

    #endregion

    #region Client


    [Client]
    private void ShootingHandler()
    {
        if (_canShoot && _currentBulletAmount > 0)
        {
            CmdShootBullet();
            _timer = 0;
            _canShoot = false;
        }

    }

    [ClientRpc]
    private void RpcBulletShot()
    {
        _vfxShoot.SetActive(true);
        _playerHolding.activeItem.itemAudio.PlayOneShot(_shootAudio);
        if (hasAuthority)
            CameraShake.Instance.Shake(_shakeDuration, _shakePower);
    }

    [ClientRpc]
    private void RpcStartReload()
    {
        _playerHolding.activeItem.itemAudio.Play(_reloadAudio);
    }
    [Client]
    private void ClientReloadStopped()
    {
        _playerHolding.activeItem.itemAudio.Stop();
    }

    [Client]
    private void ClientHandleCurrentBulletAmountChanged(int oldBulletAmount, int newBulletAmount)
    {
        if (hasAuthority)
            AuthorityOnAmountBulletChanged?.Invoke(newBulletAmount, _bulletAmountMagazine);
    }

    [Client]
    public override void Use(bool isUsing, bool isOnceUsing)
    {
        base.Use(isUsing, isOnceUsing);
        bool isShooting = _allowHoldButton ? isUsing : isOnceUsing;
        if (isShooting)
            ShootingHandler();
    }


    #endregion

}



