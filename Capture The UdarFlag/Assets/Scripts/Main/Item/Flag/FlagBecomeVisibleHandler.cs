using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagBecomeVisibleHandler : MonoBehaviour
{
    [SerializeField] private Flag _flag;
    public static event Action<bool> ClientOnFlagInvisible;
    public static event Action<bool> ClientOnFlagVisible;
    public static event Action<bool, Flag> ClientOnFlagSetTeam;

    private PlayerManager _connPlayer;
    private bool _isTeamOwnerFlag;
    private bool _isVisible;


    private void Start()
    {
        _connPlayer = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        _isTeamOwnerFlag = _connPlayer.IsTeammate(_flag.GetTeam());
        ClientOnFlagSetTeam?.Invoke(_isTeamOwnerFlag, _flag);
        if (_isVisible)
            ClientOnFlagVisible?.Invoke(_isTeamOwnerFlag);
        else
            ClientOnFlagInvisible?.Invoke(_isTeamOwnerFlag);

    }
    private void OnDisable()
    {
        if (!_isTeamOwnerFlag)
            ClientOnFlagVisible?.Invoke(_isTeamOwnerFlag);
    }
    private void OnDestroy()
    {
        ClientOnFlagVisible?.Invoke(_isTeamOwnerFlag);
    }


    private void OnBecameInvisible()
    {
        _isVisible = false;
        ClientOnFlagInvisible?.Invoke(_isTeamOwnerFlag);
    }

    private void OnBecameVisible()
    {
        _isVisible = true;
        ClientOnFlagVisible?.Invoke(_isTeamOwnerFlag);
    }

}
