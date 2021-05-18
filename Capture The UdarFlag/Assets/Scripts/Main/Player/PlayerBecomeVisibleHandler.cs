using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBecomeVisibleHandler : MonoBehaviour
{
    [SerializeField] private PlayerLinks _playerLinks;
    public static event Action<PlayerManager> ClientOnTeammateInvisible;
    public static event Action<PlayerManager> ClientOnTeammateVisible;



    private GamePlayer _connPlayer;
    private bool _canCheck = false;
    private bool _isVisible=true;

    private void Start()
    {
        if (_playerLinks.playerManager.hasAuthority)
        {
            this.enabled = false;
            return; 
        }
        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        _canCheck = true;
    }


    private void OnDestroy()
    {
        if (_playerLinks.playerManager.hasAuthority || !_canCheck) { return; }

        if (_connPlayer.IsTeammate(_playerLinks.gamePlayer))
            ClientOnTeammateVisible?.Invoke(_playerLinks.playerManager);
    }

    private void OnBecameInvisible()
    {
        if (_playerLinks.playerManager.hasAuthority||!_canCheck) { return; }

        if (!_connPlayer.IsTeammate(_playerLinks.gamePlayer) || !_isVisible) { return; }
        
            ClientOnTeammateInvisible?.Invoke(_playerLinks.playerManager);
            _isVisible = false;
        
    }

    private void OnBecameVisible()
    {
        if (_playerLinks.playerManager.hasAuthority || !_canCheck) { return; }

        if (!_connPlayer.IsTeammate(_playerLinks.gamePlayer) || _isVisible) { return; }

        ClientOnTeammateVisible?.Invoke(_playerLinks.playerManager);
        _isVisible = true;
    }
}
