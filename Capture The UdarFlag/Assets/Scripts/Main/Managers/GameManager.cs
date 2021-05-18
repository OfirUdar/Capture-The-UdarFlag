using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }


    [Tooltip("The amount of flags (score) to win the game")]
    public int flagsAmountTarget = 5;


    private bool _hasWinner;
    private GamePlayer _connPlayer;

    public static event Action<bool, int> ClientOnScoreTeamChanged;
    public static event Action ServerOnGameOver;
    public static event Action<int> ClientOnGameOver;


    public bool HasWinner()
    {
        return _hasWinner;
    }


    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
    }

    #region Server

    public override void OnStartServer()
    {
        Team.OnScoreChanged += ServerHandleScoreTeamChanged;
    }
    public override void OnStopServer()
    {
        Team.OnScoreChanged -= ServerHandleScoreTeamChanged;
    }

    [Server]
    private void ServerHandleScoreTeamChanged(Team team)
    {
        CheckForWinner(team);
        RpcScoreTeamChanged(team.IdTeam, team.ScoreTeam);
    }
    [Server]
    private void CheckForWinner(Team team)
    {
        if (_hasWinner) { return; }
        //it means there is no winner

        if ( team.ScoreTeam >= flagsAmountTarget)
        {
            //this team won!
            WinTheGame(team);
        }
    }

    [Server]
    public void WinTheGame(Team winnerTeam)
    {
        _hasWinner = true;
        ServerOnGameOver?.Invoke();
        RpcGameOver(winnerTeam.IdTeam);
    }
    #endregion


    #region Client

    [ClientRpc]
    private void RpcScoreTeamChanged(int idTeam,int score)
    {
        ClientOnScoreTeamChanged?.Invoke(_connPlayer.IsTeammate(idTeam), score);
    }
    [ClientRpc]
    private void RpcGameOver(int idTeamWon)
    {
        ClientOnGameOver?.Invoke(idTeamWon);
    }


    #endregion
}
