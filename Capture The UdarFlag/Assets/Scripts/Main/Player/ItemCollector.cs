using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCollector : CircleFillerDetector
{
    [SerializeField] private PlayerLinks _playerLinks;
    [SerializeField] private Transform _bagOB;

    private GamePlayer _connPlayer;
    private Transform _rightHand;

    public static Action<bool, bool> ClientOnFlagCollected;


    private void Start()
    {
        _rightHand = _playerLinks.animManager.animator.GetBoneTransform(HumanBodyBones.RightHand);
        _bagOB.SetParent(_rightHand);
        _bagOB.localPosition = Vector3.zero;
        _bagOB.localRotation = Quaternion.Euler(Vector3.zero);
    }

    [Server]
    protected override void StartFilling(CircleFiller circleFiller)
    {
        Item item = circleFiller.GetParentIdentity().GetComponent<Item>();
        if (item == null) { return; }
        if (item.IsEquiped()) { return; } // if the item is equiped - you cannot pickup it
        if (item as Flag &&
           _playerLinks.playerManager.IsTeammate(((Flag)item).GetTeam()))// if the flag is owner to the team are collecting - return to the base
        {
            if (((Flag)item).IsFlagOnBase()) { return; }
        }

        base.StartFilling(circleFiller);
    }

    [Server]
    protected override void ServerOnFillEnd(NetworkIdentity itemIdentity)
    {
        Item item = itemIdentity.GetComponent<Item>();
        if (item.IsEquiped()) { return; }

        if (item as Flag)
        {
            bool isAuthorityFlag = _playerLinks.playerManager.IsTeammate(((Flag)item).GetTeam());
            RpcFlagCollected(((Flag)item).GetTeam().IdTeam, isAuthorityFlag);
            if (isAuthorityFlag) // if the flag is owner to the team are collecting - return to the base)
            {
                ((Flag)item).FlagPositionToBase();// flag recovered
                return;
            }
        }
        base.ServerOnFillEnd(itemIdentity);
        item.ServerCollect(_bagOB, connectionToClient);
        _playerLinks.bagManager.ServerAddItem(item);
        RpcCollect(item);
    }


    public override void OnStartClient()
    {
        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
    }

    [ClientRpc]
    private void RpcCollect(Item item)
    {
        item.ClientCollect(_bagOB);
        if (hasAuthority)
            AudioManager.Instance.PlayOneShot("Collect");
    }

    [ClientRpc]
    private void RpcFlagCollected(int flagTeamID, bool isFlagRecovered)
    {
        bool isAuthorityTeam = _connPlayer.IsTeammate(flagTeamID);
        ClientOnFlagCollected?.Invoke(isAuthorityTeam, isFlagRecovered);
    }

}
