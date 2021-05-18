using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Smooth;
public class GamePlayer : NetworkBehaviour
{
    public PlayerStats stats;
    [HideInInspector]
    public PlayerManager playerManager;


    private CharacterSO _characterToSpawn;

    [SyncVar]
    private bool _isLeader;

    [SyncVar(hook = nameof(ClientHandleIsReadyChanged))]
    private bool _isReady;


    public static event Action ClientOnReadyChanged;
    public static event Action<int> ClientOnJoinToMainScene;


    public bool IsReady()
    {
        return _isReady;
    }
    public bool IsLeader()
    {
        return _isLeader;
    }

    public bool IsTeammate(GamePlayer otherPlayer)
    {
        return (stats.PlayerTeam.IdTeam == otherPlayer.stats.PlayerTeam.IdTeam);
    }
    public bool IsTeammate(Team otherTeam)
    {
        return (stats.PlayerTeam.IdTeam == otherTeam.IdTeam);
    }
    public bool IsTeammate(int idTeam)
    {
        return (stats.PlayerTeam.IdTeam == idTeam);
    }


    private void Start()
    {
        DontDestroyOnLoad(this);
    }

    #region Server


    [Server]
    public void SetIsLeader(bool isLeader)
    {
        _isLeader = isLeader;
    }

    [Server]
    public void SpawnCharacter()
    {
        playerManager = Instantiate(_characterToSpawn.prefab).GetComponent<PlayerManager>();
        NetworkServer.Spawn(playerManager.gameObject, connectionToClient);
        playerManager.playerLinks.gamePlayer = this;
        RpcSpawnedCharacter(playerManager);
    }

    [Command]
    public void CmdSetCharacter(int index)
    {
        _characterToSpawn = CharacterSelection.Instance.GetCharacter(index);
    }

    [Command]
    private void CmdStartGame()
    {
        GameNetworkManager gameNetworkManager = ((GameNetworkManager)(NetworkManager.singleton));
        if (!_isLeader) { return; }
        if (!gameNetworkManager.CanStartGame()) { return; }
        gameNetworkManager.StartGame();
    }
    [Command]
    private void CmdSetIsReady(bool isReady)
    {
        _isReady = isReady;
    }
    [Command]
    private void CmdPlayerJoinedToMainScene()
    {
        int restPlayers = ((GameNetworkManager)NetworkManager.singleton).AddPlayerJoinedTheMainScene();
        RpcClientChangedToMainScene(restPlayers);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        CmdSetCharacter(CharacterSelection.Instance.GetCurrentIndexCharacter());
    }

    [Client]
    public void StartGame()
    {
        CmdStartGame();
    }
    [Client]
    public void ChangeSignReady()//ready or unready
    {
        CmdSetIsReady(!_isReady);
    }

    [Client]
    public void AuthorityChangedToMainScene()
    {
        CmdPlayerJoinedToMainScene();
    }

    [ClientRpc]
    private void RpcClientChangedToMainScene(int restPlayers)
    {
        ClientOnJoinToMainScene?.Invoke(restPlayers);
    }
    [ClientRpc]
    private void RpcSpawnedCharacter(PlayerManager playerManager)
    {
        this.playerManager = playerManager;
        this.playerManager.playerLinks.gamePlayer = this;
    }

    private void ClientHandleIsReadyChanged(bool oldReady, bool newReady)
    {
        ClientHandleReadyChanged();
    }

    private void ClientHandleReadyChanged()
    {
        ClientOnReadyChanged?.Invoke();
    }

    #endregion






}


