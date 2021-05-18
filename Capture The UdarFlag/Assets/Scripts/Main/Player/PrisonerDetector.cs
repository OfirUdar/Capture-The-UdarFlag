using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PrisonerDetector : CircleFillerDetector
{
    [SerializeField] private PlayerLinks _playerLinks;

    public bool IsPrisoner { get; set; }


    private void Start()
    {
        this.IsPrisoner = false;
    }




    [Server]
    protected override void StartFilling(CircleFiller circleFiller)
    {
        PlayerManager otherPlayer = circleFiller.GetParentIdentity().GetComponent<PlayerManager>();
        if (otherPlayer == null) { return; }

        PrisonerDetector otherPrisonerDetector = otherPlayer.playerLinks.prisonerDetector;

        if (otherPlayer == _playerLinks.playerManager) { return; }// cannot imprison or save himself 
        if (_playerLinks.playerManager.IsTeammate(otherPlayer.playerLinks.gamePlayer) && !otherPrisonerDetector.IsPrisoner) { return; }// cannot be friend of team who save who he is not prisoner
        if (!_playerLinks.playerManager.IsTeammate(otherPlayer.playerLinks.gamePlayer) && otherPrisonerDetector.IsPrisoner) { return; }// cannot be enemy who imprison who he is prisoner


        base.StartFilling(circleFiller);
    }
    [Server]
    protected override void ServerOnFillEnd(NetworkIdentity parentIdentity)
    {
        base.ServerOnFillEnd(parentIdentity);

        PlayerManager otherPlayer = parentIdentity.GetComponent<PlayerManager>();

        otherPlayer.ServerImprisoner();

        SetActiveCollider(true);

        RpcOnFillEnd();
    }

    [ClientRpc]
    private void RpcOnFillEnd()
    {
        _playerLinks.prisonerCirceFiller.DisplayFillCircleFiller(false);
        _playerLinks.prisonerCirceFiller.DisplayBackgroundCircleFiller(false);
        if (hasAuthority)
            AudioManager.Instance.PlayOneShot("Collect");
    }

    [Client]
    public void ClientDisplayBackgroundCircleFiller(bool isDisplay)
    {
        _playerLinks.prisonerCirceFiller.DisplayBackgroundCircleFiller(isDisplay);
    }

    protected override void AuthorityDisplayCircleFiller(bool isDisplay)
    {
        _circleFiller.DisplayFillCircleFiller(isDisplay);
    }


}
