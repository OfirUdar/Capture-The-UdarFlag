using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeamsManager : NetworkBehaviour
{
    public static TeamsManager Instance { get; private set; }

    [SerializeField] private Team _pfbTeam;

    public List<Team> Teams = new List<Team>();
    private SyncList<uint> _teamsNetID = new SyncList<uint>();

    public Color teamAuthorityColor;
    public Color teamOpponentColor;



    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }


    #region Server

    [Server]
    private bool TryGetTeamWithLessPlayers(out Team outTeam)
    {
        int minPlayers = 10000;
        bool hasTeamWithLessPlayers = false ;
        outTeam = default;
        foreach (Team team in Teams)
        {
            int teamPlayers = team.GetCountPlayers();
            if (teamPlayers < ((GameNetworkManager)NetworkManager.singleton).gameSettings.numPlayersInTeam)
            {
                if (teamPlayers < minPlayers)
                {
                    outTeam = team;
                    minPlayers = teamPlayers;
                    hasTeamWithLessPlayers = true;
                }
            }
        }
        return hasTeamWithLessPlayers; //No teams has capacity or there is no teams
    }


    [Server]
    public void ServerAddPlayer(GamePlayer player)
    {
        if(Teams.Count< 
            ((GameNetworkManager)NetworkManager.singleton).gameSettings.numTeams)
        {
            CreateTeam(player);
        }
        else
        {
            if (TryGetTeamWithLessPlayers(out Team team))
            {
                team.AddPlayer(player.netId);
                player.stats.SetTeam(team.netId);
            }
        }
        
    }
    [Server]
    public void ServerRemovePlayer(GamePlayer playerDisconnect)
    {
        Team team = playerDisconnect.stats.PlayerTeam;
        team.RemovePlayer(playerDisconnect.netId);

        if (team.IsEmptyPlayers())
        {

            _teamsNetID.Remove(team.netId);

            if (playerDisconnect.playerManager != null)
            {
                NetworkServer.Destroy(team.BaseTeam.gameObject);
                NetworkServer.Destroy(team.FlagTeam.gameObject);

                if (Teams.Count == 1 && !GameManager.Instance.HasWinner())
                    GameManager.Instance.WinTheGame(Teams[0]);
            }

            NetworkServer.Destroy(team.gameObject);
        }
    }

    [Server]
    private Team CreateTeam(GamePlayer player)
    {
        Team team = Instantiate(_pfbTeam);
        NetworkServer.Spawn(team.gameObject);
        team.IdTeam = Teams.Count;
        team.AddPlayer(player.netId);
        _teamsNetID.Add(team.netId);
        player.stats.SetTeam(team.netId);
        return team;
    }

    #endregion

    #region Client

    public override void OnStartClient()
    {
        foreach (uint id in _teamsNetID)
            ((GameNetworkManager)(NetworkManager.singleton)).AddGameObjectSyncedList(Teams, id);

        _teamsNetID.Callback += ClientHandleTeamsNetIDUpdated;
    }
    public override void OnStopClient()
    {
        _teamsNetID.Callback -= ClientHandleTeamsNetIDUpdated;

    }
    [Client]
    public Team GetAuthorityTeam()
    {
        foreach (Team team in Teams)
        {
            foreach (GamePlayer player in team.Players)
            {
                if (player.hasAuthority)
                    return team;
            }
        }
        return null;
    }
    [Client]
    public Team GetOpponentTeam()
    {
        foreach (Team team in Teams)
        {
            if (team.Players == null) { return null; }

            bool flagIsOpponentTeam = true;
            foreach (GamePlayer player in team.Players)
            {
                if (player.hasAuthority)
                    flagIsOpponentTeam = false;
            }
            if (flagIsOpponentTeam)
                return team;

        }
        return null;
    }


    private void ClientHandleTeamsNetIDUpdated(SyncList<uint>.Operation op, int itemIndex, uint oldItem, uint newItem)
    {
        switch (op)
        {
            case SyncList<uint>.Operation.OP_ADD:
                // index is where it got added in the list
                // item is the new item
                foreach (uint id in _teamsNetID)
                    ((GameNetworkManager)(NetworkManager.singleton)).AddGameObjectSyncedList(Teams, id);
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
                for (int i = 0; i < Teams.Count; i++)
                {
                    if (Teams[i] == null || Teams[i].netId == oldItem)
                    {
                        Teams.RemoveAt(i);
                    }
                }
                break;
            case SyncList<uint>.Operation.OP_SET:
                // index is the index of the item that was updated
                // item is the previous item
                break;
        }
    }


    #endregion




}


