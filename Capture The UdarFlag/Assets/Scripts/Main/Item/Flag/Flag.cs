using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class Flag : ItemThrow
{
    [Header("Flag")]
    [SerializeField] private string _baseTag;

    [SyncVar]
    private Team _flagTeam;
    private GamePlayer _connPlayer;
    private bool _isTeamOwnerFlag;

    //Object Location Display
    public static event Action<bool, Flag> ClientOnFlagSetTeam;
    public static event Action<bool> ClientOnFlagCaptured;



    public Team GetTeam()
    {
        return _flagTeam;
    }

    protected override void Start()
    {
        base.Start();

        if (!isClient) { return; }

        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
        _isTeamOwnerFlag = _connPlayer.IsTeammate(_flagTeam);
        ClientOnFlagSetTeam?.Invoke(_isTeamOwnerFlag, this);
    }
    private void OnDestroy()
    {
        if (!isClient) { return; }

        ClientOnFlagSetTeam?.Invoke(_isTeamOwnerFlag, null);
    }


    #region Server

    [Server]
    public void SetTeam(Team team)
    {
        _flagTeam = team;
    }

    [ServerCallback]
    private void OnTriggerEnter(Collider other)
    {
        if (_isEquiped) { return; }

        if (!other.CompareTag(_baseTag)) { return; }

        Base collideBase = other.GetComponent<Base>();

        if (collideBase.GetTeam().IdTeam == _flagTeam.IdTeam) { return; }


        // it means the flag reach to the target base!! 
        // add score and update the position of the flag to the authority base

        RpcOnFlagCaptured(other.transform.position);
        _isEquiped = true;// make sure that the no one can pickup the flag this time
        collideBase.GetTeam().ScoreTeam++;// adding the score (amount flags)
        StartCoroutine(SetPosition(_flagTeam.BaseTeam.transform.position, 2f));// setting back to the position of the authority base with some delay
    }

    IEnumerator SetPosition(Vector3 pos, float delay)
    {
        yield return new WaitForSeconds(delay);

        transform.position = pos;
        _isEquiped = false;
    }
    [Server]
    public void FlagPositionToBase()
    {
        transform.position = _flagTeam.BaseTeam.transform.position;
    }

    [Server]
    public bool IsFlagOnBase()
    {
        return (transform.position - _flagTeam.BaseTeam.transform.position).sqrMagnitude < 2f;
    }


    #endregion

    #region Client

    [Client]
    public override void Use(bool isUsing, bool isOnceUsing)
    {
        base.Use(isUsing, isOnceUsing);

        if (!isOnceUsing)
        {
            bool inputAim = true;
            bool inputThrow = false;

            _playerHolding.playerAiming.HandleInputThrow(inputAim, inputThrow, false); //aim
        }

    }
    [Client]
    public override void StopUse()
    {
        base.StopUse();

        if (!_isThrowing)
        {
            bool inputAim = false;
            bool inputThrow = true;

            _playerHolding.playerAiming.HandleInputThrow(inputAim, inputThrow, false); //throw
        }
    }


    [ClientRpc]
    private void RpcOnFlagCaptured(Vector3 basePosition)
    {
        bool isAuthorityTeam = _connPlayer.IsTeammate(_flagTeam);
        PoolManager.Instance.RequestVFXFlagBaseCaptured(basePosition, Quaternion.identity);
        ClientOnFlagCaptured?.Invoke(isAuthorityTeam);
    }


    #endregion
}
