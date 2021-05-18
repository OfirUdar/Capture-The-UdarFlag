using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Team : NetworkBehaviour
{
    [SyncVar]
    public int IdTeam;

    #region Score
    [SyncVar]
    private int _scoreTeam;
    public int ScoreTeam
    {
        get
        {
            return _scoreTeam;
        }
        set
        {
            _scoreTeam = value;
            OnScoreChanged?.Invoke(this);
        }
    }
    #endregion

    #region Players List
    public List<GamePlayer> Players = new List<GamePlayer>();
    private SyncList<uint> _playersNetID = new SyncList<uint>();
    #endregion

    [SyncVar]
    public Base BaseTeam;
    [SyncVar]
    public Flag FlagTeam;




    public static event Action<Team> OnScoreChanged;

    public static event Action ClientOnPlayersUpdated;
    public bool IsEmptyPlayers()
    {
        return _playersNetID.Count == 0;
    }

    public int GetCountPlayers()
    {
        return _playersNetID.Count;
    }



    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }


    [Server]
    public void AddPlayer(uint playerNetID)
    {
        _playersNetID.Add(playerNetID);
    }
    [Server]
    public void RemovePlayer(uint playerNetID)
    {
        _playersNetID.Remove(playerNetID);
    }




    #region Client

    public override void OnStartClient()
    {
        foreach (uint id in _playersNetID)
            ((GameNetworkManager)(NetworkManager.singleton)).AddGameObjectSyncedList(Players, id, ClientHandleTeamPlayersUpdated);
        
        _playersNetID.Callback += ClientHandlePlayersNetIDListUpdated;
    }
    public override void OnStopClient()
    {
        _playersNetID.Callback -= ClientHandlePlayersNetIDListUpdated;
        ClientHandleTeamPlayersUpdated();
    }

    [Client]
    public bool IsAuthorityTeam()
    {
        foreach (GamePlayer player in Players)
        {
            if (player.hasAuthority)
                return true;
        }
        return false;
    }

    private void ClientHandlePlayersNetIDListUpdated(SyncList<uint>.Operation op, int itemIndex, uint oldItem, uint newItem)
    {
        switch (op)
        {
            case SyncList<uint>.Operation.OP_ADD:
                // index is where it got added in the list
                // item is the new item
                foreach (uint id in _playersNetID)
                    ((GameNetworkManager)(NetworkManager.singleton)).AddGameObjectSyncedList(Players, id, ClientHandleTeamPlayersUpdated);
                break;
            case SyncList<uint>.Operation.OP_CLEAR:
                // list got cleared
                break;
            case SyncList<uint>.Operation.OP_INSERT:
                // index is where it got added in the list
                // item is the new item
                break;
            case SyncList<uint>.Operation.OP_REMOVEAT:
                // index is where it got removed in the list
                // item is the item that was removed
                for (int i = 0; i < Players.Count; i++)
                {
                    if (Players[i] == null || Players[i].netId == oldItem)
                    {
                        Players.RemoveAt(i);
                    }
                }
                ClientHandleTeamPlayersUpdated();
                break;
            case SyncList<uint>.Operation.OP_SET:
                // index is the index of the item that was updated
                // item is the previous item
                break;
        }
    }

    private void ClientHandleTeamPlayersUpdated()
    {
        ClientOnPlayersUpdated?.Invoke();
    }
    #endregion


    private string GetPlayersToString()
    {
        string result = "";
        if (Players == null) { return result; }
        foreach (var player in Players)
        {
            result += player.stats.GetPlayerName() + ", ";
        }
        return result;
    }

    public override string ToString()
    {
        return "Id Team= " + IdTeam + " Players: " + GetPlayersToString() + " BaseTeam= " + BaseTeam + " FlagTeam= " + FlagTeam + " ScoreTeam= " + _scoreTeam;
    }


}