using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public PlayerLinks playerLinks;



    public static event Action<int> AuthorityOnDie;
    public static event Action<int> AuthorityOnImprisoner;
    public static event Action AuthorityOnRealse;

    //Teammate Object Location Display
    public static event Action<PlayerManager> ClientOnTeammateConnect;
    public static event Action<PlayerManager> ClientOnTeammateDisconnect;


    private GamePlayer _connPlayer;
    private bool _isPlayerInAuthorityTeam;





    public bool IsTeammate(GamePlayer otherPlayer)
    {
        return (playerLinks.gamePlayer.stats.PlayerTeam.IdTeam == otherPlayer.stats.PlayerTeam.IdTeam);
    }
    public bool IsTeammate(Team otherTeam)
    {
        return (playerLinks.gamePlayer.stats.PlayerTeam.IdTeam == otherTeam.IdTeam);
    }
    public bool IsTeammate(int idTeam)
    {
        return (playerLinks.gamePlayer.stats.PlayerTeam.IdTeam == idTeam);
    }



    #region Server

    public override void OnStartServer()
    {
        GameManager.ServerOnGameOver += ServerHandleGameOver;
        playerLinks.health.ServerOnPlayerDie += ServerHandlePlayerDie;
    }
    public override void OnStopServer()
    {
        GameManager.ServerOnGameOver -= ServerHandleGameOver;
        playerLinks.health.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    public void PlayerPositionToBase()
    {
        Vector3 newPlayerPos = playerLinks.gamePlayer.stats.PlayerTeam.BaseTeam.transform.position;
        newPlayerPos += new Vector3(UnityEngine.Random.Range(-2f, 2f), 0, UnityEngine.Random.Range(-2f, 2f));
        transform.position = newPlayerPos;
    }
    [Server]
    private void ServerRespawn()
    {
        playerLinks.gamePlayer.stats.SetIsAlive(true);
        playerLinks.animManager.SetIsDead(false);
        playerLinks.health.Heal(playerLinks.health.GetMaxHealth());
        playerLinks.movement.enabled = true;
        playerLinks.prisonerCirceFiller.gameObject.SetActive(false);
        playerLinks.prisonerDetector.IsPrisoner = false;

        playerLinks.rigidBody.isKinematic = false;
        playerLinks.coll.enabled = true;
        playerLinks.bagManager.ServerRespawn();

        RpcRespawn();
    }
    [Server]
    private void ServerRespawnBackToBase()
    {
        PlayerPositionToBase();
        ServerRespawn();
    }
    [Server]
    public void ServerImprisoner()// when the player become prisoner after died
    {
        if (playerLinks.gamePlayer.stats.IsAlive()) { return; }

        playerLinks.prisonerDetector.IsPrisoner = !playerLinks.prisonerDetector.IsPrisoner;
        bool isPrisoner = playerLinks.prisonerDetector.IsPrisoner;
        CancelInvoke(nameof(ServerRespawnBackToBase));
        if (isPrisoner)
        {
            //become a prisoner
            int respawnTime = 10;
            Invoke(nameof(ServerRespawnBackToBase), respawnTime);
            RpcHandleImprisoner(respawnTime);
        }
        else
        {
            //Realse
            ServerRespawn(); // respawn at the same point and doesnt go back to the base
            TargetHandleRealse();
        }

    }

    [Server]
    private void ServerHandlePlayerDie()
    {
        playerLinks.animManager.SetIsDead(true);
        playerLinks.movement.enabled = false;
        playerLinks.prisonerCirceFiller.gameObject.SetActive(true);

        playerLinks.rigidBody.isKinematic = true;
        playerLinks.coll.enabled = false;
        playerLinks.bagManager.ServerDie();

        playerLinks.gamePlayer.stats.SetIsAlive(false);
        int respawnTime = 4;
        Invoke(nameof(ServerRespawnBackToBase), respawnTime);
        RpcHandlePlayerDie(respawnTime);
    }
    [Server]
    private void ServerHandleGameOver()
    {
        playerLinks.movement.enabled = false;
    }



    #endregion

    #region Client

    [ClientCallback]
    private void Start()
    {
        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        _isPlayerInAuthorityTeam = IsTeammate(_connPlayer);

        if (hasAuthority) { return; }
        if (!_isPlayerInAuthorityTeam) { return; }

        ClientOnTeammateConnect?.Invoke(this);
    }


    [ClientCallback]
    private void OnDestroy()
    {
        if (hasAuthority) { return; }
        if (!_isPlayerInAuthorityTeam) { return; }

        ClientOnTeammateDisconnect?.Invoke(this);
    }

    public override void OnStartAuthority()
    {
        playerLinks.audioListener.enabled = true;
    }
    public override void OnStartClient()
    {
        GameManager.ClientOnGameOver += ClientHandleGameOver;            
    }
    public override void OnStopClient()
    {
        GameManager.ClientOnGameOver -= ClientHandleGameOver;
    }

    [Client]
    private void ClientHandleGameOver(int idTeamWon)
    {
        playerLinks.movement.enabled = false;
        playerLinks.bagManager.enabled = false;
        playerLinks.activeItem.enabled = false;
    }


    [ClientRpc]
    private void RpcHandlePlayerDie(int respawnTime)
    {
        playerLinks.movement.enabled = false;
        playerLinks.prisonerCirceFiller.gameObject.SetActive(true);
        playerLinks.prisonerDetector.ClientDisplayBackgroundCircleFiller(!_isPlayerInAuthorityTeam);

        playerLinks.rigidBody.isKinematic = true;
        playerLinks.coll.enabled = false;
        if (hasAuthority)
            AuthorityOnDie?.Invoke(respawnTime);
    }
    [ClientRpc]
    private void RpcRespawn()
    {
        playerLinks.movement.enabled = true;
        playerLinks.prisonerCirceFiller.gameObject.SetActive(false);

        playerLinks.rigidBody.isKinematic = false;
        playerLinks.coll.enabled = true;
    }
    [ClientRpc]
    private void RpcHandleImprisoner(int respawnTime)
    {
        playerLinks.prisonerDetector.ClientDisplayBackgroundCircleFiller(_isPlayerInAuthorityTeam);
        if (hasAuthority)
            AuthorityOnImprisoner?.Invoke(respawnTime);
    }
    [TargetRpc]
    private void TargetHandleRealse()
    {
        AuthorityOnRealse?.Invoke();
    }

    #endregion

}
