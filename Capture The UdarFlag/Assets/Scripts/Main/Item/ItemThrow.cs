using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class ItemThrow : Item
{
    [Header("Audio")]
    [SerializeField] private Audio _throwAudio;

    protected bool _isThrowing = false;


    #region Server

    [Server]
    protected void ServerThrow(Vector3 direction, float launchForce)
    {
        _rigidBody.velocity = direction.normalized * launchForce;
    }
    [Server]
    protected virtual void ThrowPointAnimation(Vector3 launchDirection, float launchForce)
    {
        ServerRemove(true);
        ServerThrow(launchDirection, launchForce);
    }

    [Command]
    private void CmdThrow(Vector3 launchDirection, float launchForce)
    {
        _playerHolding.animManager.TriggerThrow(() =>
        {
            ThrowPointAnimation(launchDirection, launchForce);
        });

        RpcThrow();
        _smoothSyncMirror.teleportOwnedObjectFromOwner();
        _smoothSyncMirror.enabled = true;
    }

    #endregion

    #region Client


    #region ThrowingInputHandler

    public override void OnStopAuthority()
    {
        _isThrowing = false;
    }

    [Client]
    public void StartThrow(Vector3 launchDirection, float launchForce)
    {        
        if (!_isThrowing)
            CmdThrow(launchDirection, launchForce);
        _isThrowing = true;
    }


    [ClientRpc]
    protected virtual void RpcThrow()
    {
        _smoothSyncMirror.enabled = true;
        _itemCollider.enabled = true;
        _playerHolding.activeItem.itemAudio.PlayOneShot(_throwAudio);
    }

    #endregion

    #endregion
}
