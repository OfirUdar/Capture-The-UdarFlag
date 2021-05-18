using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapIcon : MonoBehaviour
{
    [SerializeField] private PlayerManager _playerManager;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Color _teamateColor;


    private void Start()
    {
        GamePlayer connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();

        if (connPlayer.IsTeammate(_playerManager.playerLinks.gamePlayer))
        {
            if (!_playerManager.hasAuthority)
                _spriteRenderer.color = _teamateColor;
            this.enabled = false;
        }
        else
            gameObject.SetActive(false);
    }
}

