using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : NetworkBehaviour
{
    public static PoolManager Instance { get; private set; }



    private void Awake()
    {
        Instance = this;
    }

    #region Server

    public override void OnStartServer()
    {
        ServerGenerateBulletList(15);
        ServerGenerateRocktList(10);
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        GenerateVFXListsHandler();
    }

    #endregion


    //vfx shows on the Client side
    #region VFX Pool Handler 

    [Header("VFX Pool")]
    [SerializeField] private Transform _vfxContainer;
    [Space]
    [SerializeField] private VFX _vfxExplosionPfb;
    [SerializeField] private VFX _vfxBulletCollisionPfb;
    [SerializeField] private VFX _vfxBulletTakeDamagePfb;
    [SerializeField] private VFX _vfxFlagBaseCapturedPfb;
    [SerializeField] private VFX _vfxHealdPfb;
    [SerializeField] private VFX _vfxRocketCollisionPfb;


    private List<VFX> _vfxExplosionList = new List<VFX>();
    private List<VFX> _vfxBulletCollisionList = new List<VFX>();
    private List<VFX> _vfxBulletTakeDamageList = new List<VFX>();
    private List<VFX> _vfxFlagBaseCapturedList = new List<VFX>();
    private List<VFX> _vfxHealedList = new List<VFX>();
    private List<VFX> _vfxRocketCollisionList = new List<VFX>();

    private void GenerateVFXListsHandler()
    {
        GenerateVFXList(_vfxExplosionList, _vfxExplosionPfb, 2);
        GenerateVFXList(_vfxRocketCollisionList, _vfxRocketCollisionPfb, 2);
        GenerateVFXList(_vfxBulletCollisionList, _vfxBulletCollisionPfb, 3);
        GenerateVFXList(_vfxBulletTakeDamageList, _vfxBulletTakeDamagePfb, 3);
        GenerateVFXList(_vfxFlagBaseCapturedList, _vfxFlagBaseCapturedPfb, 1);
        GenerateVFXList(_vfxHealedList, _vfxHealdPfb, 2);
    }
    private void GenerateVFXList(List<VFX> vfxList, VFX vfx, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            VFX vfxInstance = Instantiate(vfx, _vfxContainer);
            vfxList.Add(vfxInstance);
            vfxInstance.gameObject.SetActive(false);
        }
    }

    public VFX RequestVFXExplosion(Vector3 position, Quaternion rotation)
    {
        VFX vfxToReturn = null;
        foreach (VFX vfx in _vfxExplosionList)
        {
            if (vfx.gameObject.activeSelf) { continue; }

            vfxToReturn = vfx;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;

            vfxToReturn.gameObject.SetActive(true);
            break;
        }
        if (vfxToReturn == null)
        {
            VFX vfxInstance = Instantiate(_vfxExplosionPfb, _vfxContainer);
            _vfxExplosionList.Add(vfxInstance);
            vfxToReturn = vfxInstance;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;
        }
        return vfxToReturn;
    }
    public VFX RequestVFXBulletCollison(Vector3 position, Quaternion rotation)
    {
        VFX vfxToReturn = null;
        foreach (VFX vfx in _vfxBulletCollisionList)
        {
            if (vfx.gameObject.activeSelf) { continue; }

            vfxToReturn = vfx;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;

            vfxToReturn.gameObject.SetActive(true);
            break;
        }
        if (vfxToReturn == null)
        {
            VFX vfxInstance = Instantiate(_vfxBulletCollisionPfb, _vfxContainer);
            _vfxBulletCollisionList.Add(vfxInstance);
            vfxToReturn = vfxInstance;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;
        }
        return vfxToReturn;
    }
    public VFX RequestVFXTakeDamage(Vector3 position, Quaternion rotation)
    {
        VFX vfxToReturn = null;
        foreach (VFX vfx in _vfxBulletTakeDamageList)
        {
            if (vfx.gameObject.activeSelf) { continue; }

            vfxToReturn = vfx;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;

            vfxToReturn.gameObject.SetActive(true);
            break;
        }
        if (vfxToReturn == null)
        {
            VFX vfxInstance = Instantiate(_vfxBulletTakeDamagePfb, _vfxContainer);
            _vfxBulletTakeDamageList.Add(vfxInstance);
            vfxToReturn = vfxInstance;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;
        }
        return vfxToReturn;
    }
    public VFX RequestVFXFlagBaseCaptured(Vector3 position, Quaternion rotation)
    {
        VFX vfxToReturn = null;
        foreach (VFX vfx in _vfxFlagBaseCapturedList)
        {
            if (vfx.gameObject.activeSelf) { continue; }

            vfxToReturn = vfx;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;

            vfxToReturn.gameObject.SetActive(true);
            break;
        }
        if (vfxToReturn == null)
        {
            VFX vfxInstance = Instantiate(_vfxFlagBaseCapturedPfb, _vfxContainer);
            _vfxFlagBaseCapturedList.Add(vfxInstance);
            vfxToReturn = vfxInstance;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;
        }
        return vfxToReturn;
    }
    public VFX RequestVFXHealed(Vector3 position, Quaternion rotation)
    {
        VFX vfxToReturn = null;
        foreach (VFX vfx in _vfxHealedList)
        {
            if (vfx.gameObject.activeSelf) { continue; }

            vfxToReturn = vfx;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;

            vfxToReturn.gameObject.SetActive(true);
            break;
        }
        if (vfxToReturn == null)
        {
            VFX vfxInstance = Instantiate(_vfxHealdPfb, _vfxContainer);
            _vfxHealedList.Add(vfxInstance);
            vfxToReturn = vfxInstance;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;
        }
        return vfxToReturn;
    }

    public VFX RequestVFXRocketCollison(Vector3 position, Quaternion rotation)
    {
        VFX vfxToReturn = null;
        foreach (VFX vfx in _vfxRocketCollisionList)
        {
            if (vfx.gameObject.activeSelf) { continue; }

            vfxToReturn = vfx;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;

            vfxToReturn.gameObject.SetActive(true);
            break;
        }
        if (vfxToReturn == null)
        {
            VFX vfxInstance = Instantiate(_vfxRocketCollisionPfb, _vfxContainer);
            _vfxRocketCollisionList.Add(vfxInstance);
            vfxToReturn = vfxInstance;

            vfxToReturn.transform.position = position;
            vfxToReturn.transform.rotation = rotation;
        }
        return vfxToReturn;
    }

    #endregion

    #region Projectile Handler

    [Header("Bullet")]
    [SerializeField] private Transform _bulletContainer;
    [Space]
    [SerializeField] private Bullet _bulletPfb;

    private List<Bullet> _bulletList = new List<Bullet>();

    [Header("Rocket")]
    [SerializeField] private Transform _rocketContainer;
    [Space]
    [SerializeField] private Rocket _rocketPfb;

    private List<Rocket> _rocketList = new List<Rocket>();

    [Server]
    private void ServerGenerateBulletList(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Bullet bulletInstance = Instantiate(_bulletPfb);
            _bulletList.Add(bulletInstance);
            bulletInstance.transform.parent = _bulletContainer;
            bulletInstance.gameObject.SetActive(false);
        }
    }

    [Server]
    private void ServerGenerateRocktList(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Rocket rocketInstance = Instantiate(_rocketPfb);
            _rocketList.Add(rocketInstance);
            rocketInstance.transform.parent = _rocketContainer;
            rocketInstance.gameObject.SetActive(false);
        }
    }


    [Server]
    public Projectile ServerRequestProjectile(Vector3 position, Quaternion rotation, ProjectileType projectileType, NetworkConnection conn)
    {

        switch (projectileType)
        {

            case ProjectileType.Bullet:
                {
                    return ServerRequestBullet(position, rotation, conn);
                }
            case ProjectileType.Rocket:
                {
                    return ServerRequestRocket(position, rotation, conn);
                }
            default:
                {
                    return ServerRequestBullet(position, rotation, conn);
                }
        }
    }
    [Server]
    private Bullet ServerRequestBullet(Vector3 position, Quaternion rotation, NetworkConnection conn)
    {
        Bullet bulletToReturn = null;
        foreach (Bullet bullet in _bulletList)
        {
            if (bullet.gameObject.activeSelf) { continue; }

            bulletToReturn = bullet;

            bulletToReturn.transform.position = position;
            bulletToReturn.transform.rotation = rotation;

            bulletToReturn.gameObject.SetActive(true);
            break;
        }
        if (bulletToReturn == null)
        {
            Bullet bulletInstance = Instantiate(_bulletPfb, _bulletContainer);
            _bulletList.Add(bulletInstance);
            bulletToReturn = bulletInstance;

            bulletToReturn.transform.position = position;
            bulletToReturn.transform.rotation = rotation;
        }

        NetworkServer.Spawn(bulletToReturn.gameObject, conn);
        return bulletToReturn;
    }
    [Server]
    private Rocket ServerRequestRocket(Vector3 position, Quaternion rotation, NetworkConnection conn)
    {
        Rocket rocketToReturn = null;
        foreach (Rocket rocket in _rocketList)
        {
            if (rocket.gameObject.activeSelf) { continue; }

            rocketToReturn = rocket;

            rocketToReturn.transform.position = position;
            rocketToReturn.transform.rotation = rotation;

            rocketToReturn.gameObject.SetActive(true);
            break;
        }
        if (rocketToReturn == null)
        {
            Rocket rocketInstance = Instantiate(_rocketPfb, _rocketContainer);
            _rocketList.Add(rocketInstance);
            rocketToReturn = rocketInstance;

            rocketToReturn.transform.position = position;
            rocketToReturn.transform.rotation = rotation;
        }

        NetworkServer.Spawn(rocketToReturn.gameObject, conn);
        return rocketToReturn;
    }

    #endregion
}


