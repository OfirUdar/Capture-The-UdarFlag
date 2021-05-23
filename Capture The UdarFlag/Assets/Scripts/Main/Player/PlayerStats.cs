using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerStats : NetworkBehaviour
{
    [SyncVar]
    private string _playerName = "Udar";

    #region Player Team
    private Team _playerTeam;
    public Team PlayerTeam
    {
        get
        {
            if (_playerTeam == null)
            {
                if (NetworkIdentity.spawned.TryGetValue(_teamID, out NetworkIdentity identity))
                    _playerTeam = identity.GetComponent<Team>();
            }
            return _playerTeam;

        }
    }
    [SyncVar]
    private uint _teamID;
    #endregion

    [SyncVar(hook = nameof(ClientHandleKillsChanged))]
    private int _kills;

    private bool _isAlive = true;



    public static event Action<int> ClientOnKillsChanged;

    public string GetPlayerName()
    {
        return _playerName;
    }
    [Server]
    public void SetTeam(uint teamID)
    {
        _teamID = teamID;
    }
    public int GetKills()
    {
        return _kills;
    }
    public bool IsAlive()
    {
        return _isAlive;
    }

    #region Server  

    [Server]
    public void AddKill()
    {
        _kills++;
    }

    [Server]
    public void SetIsAlive(bool isAlive)
    {
        _isAlive = isAlive;
    }


    [Command]
    public void CmdSetPlayerName(string playerName)
    {
        _playerName = playerName;
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        CmdSetPlayerName(PlayerPrefs.GetString("PlayerName", "Guest"));
    }

    private void ClientHandleKillsChanged(int oldKills, int newKills)
    {
        if (hasAuthority)
            ClientOnKillsChanged?.Invoke(newKills);
    }

    #endregion

}
