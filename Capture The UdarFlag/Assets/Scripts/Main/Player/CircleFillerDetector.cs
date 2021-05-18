using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CircleFillerDetector : NetworkBehaviour
{


    protected CircleFiller _circleFiller; //the circleFiller we are collecting

    private bool _isFilling;
    private float _fillTime;
    private float _timer;



    private void Update()
    {
        if (_isFilling)
        {
            if (isServer || hasAuthority)
                _timer += Time.deltaTime;

            if (hasAuthority)
            {
                AuthorityUpdateCircleFiller(_timer / _fillTime);
                if (_timer >= _fillTime)
                    CmdTryFillEnd();
            }
        }
    }


    #region Server


    [ServerCallback]
    private void OnTriggerStay(Collider other)
    {
        if (!_isFilling && other.TryGetComponent(out CircleFiller circleFiller))
        {
            StartFilling(circleFiller);
        }
    }
    [ServerCallback]
    private void OnTriggerExit(Collider other)
    {
        if (_isFilling && other.TryGetComponent(out CircleFiller circleFiller) && _circleFiller == circleFiller)
        {
            StopFilling();
        }
    }

    [Server]
    protected virtual void StartFilling(CircleFiller circleFiller)
    {
        _isFilling = true;
        _circleFiller = circleFiller;
        _timer = 0;
        _fillTime = _circleFiller.GetFillTime();
        if (isClient && hasAuthority)
        {
            AuthorityUpdateCircleFiller(0);
            AuthorityDisplayCircleFiller(true);
        }
        TargetStartFilling(_circleFiller.GetParentIdentity());
    }
    [Server]
    private void StopFilling()
    {
        if (isClient && hasAuthority)
            AuthorityDisplayCircleFiller(false);
        _isFilling = false;

        TargetStopFilling();
    }
    [Server]
    protected virtual void ServerOnFillEnd(NetworkIdentity parentIdentity)
    {
        SetActiveCollider(false);
    }

    [Command]
    private void CmdTryFillEnd()
    {
        if (_isFilling && _timer >= _fillTime && enabled)
        {
            ServerOnFillEnd(_circleFiller.GetParentIdentity());
            StopFilling();
        }
    }

    [Server]
    protected void SetActiveCollider(bool active)
    {
        _circleFiller.GetCollider().enabled = active;
    }

    #endregion

    #region Client


    [Client]
    protected virtual void AuthorityDisplayCircleFiller(bool isDisplay)
    {
        _circleFiller.DisplayCircleFiller(isDisplay);
    }

    [Client]
    private void AuthorityUpdateCircleFiller(float fillAmount)
    {
        _circleFiller.SetFillAmount(fillAmount);
    }


    [TargetRpc]
    private void TargetStartFilling(NetworkIdentity parentIdentity)
    {
        if (!isClientOnly) { return; }

        _isFilling = true;
        _circleFiller = parentIdentity.GetComponentInChildren<CircleFiller>();
        _timer = 0;
        _fillTime = _circleFiller.GetFillTime();


        AuthorityUpdateCircleFiller(0);
        AuthorityDisplayCircleFiller(true);
    }
    [TargetRpc]
    private void TargetStopFilling()
    {
        if (!isClientOnly) { return; }

        _isFilling = false;
        AuthorityDisplayCircleFiller(false);
    }



    #endregion

}
