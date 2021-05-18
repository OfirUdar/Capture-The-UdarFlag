using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : NetworkBehaviour
{
    [SerializeField] private PlayerLinks _playerLinks;
    [SerializeField] private Audio _diedAudio;
    [SerializeField] private int _maxHealth = 100;

    [SyncVar(hook = nameof(ClientHandleHealthUpdated))]
    private int _currentHealth;

    public event Action ServerOnPlayerDie;
    public event Action<float> ClientOnHealthUpdated;


    public int GetCurrentHealth()
    {
        return _currentHealth;
    }
    public int GetMaxHealth()
    {
        return _maxHealth;
    }

    #region Server

    public override void OnStartServer()
    {
        _currentHealth = _maxHealth;
    }

    [Server]
    public void TakeDamage(int damage)
    {
        _currentHealth = Mathf.Max(0, _currentHealth - damage);
        if (_currentHealth == 0)
        {
            //DIE
            ServerPlayerDie();
        }
    }
    [Server]
    public void Heal(int amount)
    {
        _currentHealth = Mathf.Min(100, _currentHealth + amount);
        RpcHeal();
    }

    [Server]
    private void ServerPlayerDie()
    {
        ServerOnPlayerDie?.Invoke();
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        ClientOnHealthUpdated?.Invoke((float)_currentHealth / _maxHealth);
    }

    private void ClientHandleHealthUpdated(int oldHealth, int newHealth)
    {
        //Display
        ClientOnHealthUpdated?.Invoke((float)newHealth / _maxHealth);
        if (newHealth <= 0)
            _playerLinks.playerAudio.PlayOneShot(_diedAudio);
    }

    [ClientRpc]
    private void RpcHeal()
    {
        VFX vfx = PoolManager.Instance.RequestVFXHealed(transform.position, transform.rotation);
        vfx.SetTempParent(transform);
    }

    #endregion
}
