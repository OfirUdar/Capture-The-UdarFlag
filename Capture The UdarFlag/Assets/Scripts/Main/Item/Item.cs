using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smooth;
using System;

public class Item : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] protected Rigidbody _rigidBody;
    [SerializeField] protected Collider _itemCollider;
    [SerializeField] protected Transform _itemGFX;
    [SerializeField] protected SmoothSyncMirror _smoothSyncMirror;
    [SerializeField] private CircleFiller _circleFiller;
    [SerializeField] protected LayerMask _whatIsGround;

    [Header("Features")]
    [SerializeField] private int _itemAnimationLayer = 1;
    [SerializeField] private Sprite _itemSprite;

    [Header("Holding Features")]
    [SerializeField] private Vector3 _itemGFXStartPosition = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 _itemPosition = new Vector3(0, 0, -0.1f);
    [SerializeField] private Vector3 _itemRotation = new Vector3(0.153f, 89.795f, 89.79f);

    [Header("Aiming Properties")]
    [SerializeField] protected AimingProperties _aimingProperties;

    private Camera _mainCamera;



    protected bool _isEquiped = false;
    private Vector3 _startScale;


    [SyncVar]
    protected PlayerLinks _playerHolding;


    public event Action<Item> ServerOnItemRemoved;
    public static event Action AuthorityOnItemUsed;
    public static event Action AuthorityOnItemRemoved;


    public CircleFiller GetCircleFiller()
    {
        return _circleFiller;
    }
    public int GetItemAnimationLayer()
    {
        return _itemAnimationLayer;
    }
    public Sprite GetItemSprite()
    {
        return _itemSprite;
    }
    public bool IsEquiped()
    {
        return _isEquiped;
    }

    public AimingProperties GetAimingProperties()
    {
        return _aimingProperties;
    }

    protected virtual void Start()
    {
        _startScale = transform.localScale;
    }



    #region Server

    [Server]
    public void ServerCollect(Transform itemHolder, NetworkConnection conn)
    {
        _isEquiped = true;
        _rigidBody.isKinematic = true;
        _rigidBody.useGravity = false;
        _itemCollider.enabled = false;
        _smoothSyncMirror.enabled = false;


        transform.SetParent(itemHolder);
        transform.localPosition = _itemPosition;
        transform.localRotation = Quaternion.Euler(_itemRotation);
        _itemGFX.localRotation = Quaternion.Euler(Vector3.zero);
        _itemGFX.GetChild(0).localPosition = Vector3.zero;

        _playerHolding = conn.identity.GetComponent<GamePlayer>().playerManager.playerLinks;
        this.netIdentity.AssignClientAuthority(conn);
    }
    [Server]
    public virtual void ServerRemove(bool isThrow = false)
    {
        _isEquiped = false;
        transform.SetParent(null);
        _smoothSyncMirror.enabled = true;
        _rigidBody.isKinematic = false;
        _rigidBody.useGravity = true;
        _itemCollider.enabled = true;
        transform.localScale = _startScale;
        transform.localRotation = Quaternion.Euler(Vector3.zero);
        _itemGFX.GetChild(0).localPosition = _itemGFXStartPosition;
        Invoke(nameof(ActivateCircleFillerCollider), 0.3f);

        RpcRemove(isThrow);
        _playerHolding = null;
        this.netIdentity.RemoveClientAuthority();
        if (!isThrow)
            _smoothSyncMirror.teleportOwnedObjectFromOwner();


        ServerOnItemRemoved?.Invoke(this);
    }
    [Server]
    private void ActivateCircleFillerCollider()
    {
        _circleFiller.GetCollider().enabled = true;
    }

    [Command]
    public void CmdRemove()
    {
        ServerRemove();
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        _mainCamera = Camera.main;
    }

    [ClientCallback]
    protected virtual void Update()
    {
        if (!_isEquiped)
        {
            _itemGFX.Rotate(Vector3.up * 50f * Time.deltaTime);
            return;
        }
    }


    [Client]
    public virtual void ClientCollect(Transform itemHolder)
    {
        if (!isClientOnly) { return; }

        _isEquiped = true;
        _rigidBody.isKinematic = true;
        _rigidBody.useGravity = false;
        _itemCollider.enabled = false;
        _smoothSyncMirror.enabled = false;

        transform.SetParent(itemHolder);
        transform.localPosition = _itemPosition;
        transform.localRotation = Quaternion.Euler(_itemRotation);
        _itemGFX.localRotation = Quaternion.Euler(Vector3.zero);
        _itemGFX.GetChild(0).localPosition = Vector3.zero;
    }

    [ClientRpc]
    protected virtual void RpcRemove(bool isThrow)
    {
        if (hasAuthority)
            AuthorityOnItemRemoved?.Invoke();

        if (!isClientOnly) { return; }

        _isEquiped = false;
        transform.SetParent(null);
        _smoothSyncMirror.enabled = true;
        _rigidBody.isKinematic = false;
        _rigidBody.useGravity = true;
        transform.localScale = _startScale;

        _itemGFX.GetChild(0).localPosition = _itemGFXStartPosition;

        if (!isThrow)
            _itemCollider.enabled = true;

    }


    [Client]
    public virtual void Use(bool isUsing, bool isOnceUsing)
    {
        if (hasAuthority)
            AuthorityOnItemUsed?.Invoke();
    }

    [Client]
    public virtual void StopUse()
    {

    }

    #endregion

}

