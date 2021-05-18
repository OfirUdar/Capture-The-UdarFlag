using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class AidKit : ItemThrow
{
    [Header("AidKit")]
    [SerializeField] private int _healAmount = 30;

    private bool _isHealed = false;


    [Command]
    private void CmdHeal()
    {
        if (_isHealed) { return; }

        Health healthPlayer = connectionToClient.identity.GetComponent<GamePlayer>().playerManager.playerLinks.health;
        if (healthPlayer.GetCurrentHealth() >= healthPlayer.GetMaxHealth()) { return; }


        healthPlayer.Heal(_healAmount);
        _isHealed = true;
        ServerRemove();
    }
    [Server]
    public override void ServerRemove(bool isThrow = false)
    {
        base.ServerRemove(isThrow);
        if (_isHealed)
        {
            _isEquiped = true;
            Invoke(nameof(DestroyAidKit), 0.1f);
        }
    }

    [Server]
    private void DestroyAidKit()
    {
        NetworkServer.Destroy(this.gameObject);
    }

    public override void Use(bool isUsing, bool isOnceUsing)
    {
        base.Use(isUsing, isOnceUsing);
        if (!_isThrowing)
            CmdHeal();
    }
}
