using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAiming : NetworkBehaviour
{
    [SerializeField] private PlayerLinks _playerLinks;

    private Item _activeItem;

    private Camera _mainCamera;
    private bool _isStartToAim = false;


    private float _currentLaunchForce;
    private Vector3 _currentLaunchDirection;

    public static event Action AuthorityOnAiming;

    #region Server

    [Server]
    public void SlerpRotation(Vector3 direction)
    {
        Quaternion lookRotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 0.3f);
    }

    [Command]
    private void CmdRotateToDirection(Vector3 direction)
    {
        transform.forward = direction.normalized;
    }

    [Command]
    private void CmdSetAiming(bool isAiming,bool isItemThrow) //setting the movement and the animating to AIMING MODE
    {
        _playerLinks.movement.SetMovementAiming(isAiming);
        if (isItemThrow)
        {
            _playerLinks.animManager.SetIsAimingThrow(isAiming); // throwing aim
        }
        else
        {
            _playerLinks.animManager.SetIsAiming(isAiming); //  aiminig aim (like weapon)
        }
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        _mainCamera = Camera.main;
        ActiveItem.AuthorityOnActiveItemChanged += AuthorityHandleActiveItemChanged;
    }
    public override void OnStopAuthority()
    {
        ActiveItem.AuthorityOnActiveItemChanged -= AuthorityHandleActiveItemChanged;
    }


    [Client]
    private Vector3? Aim()
    {
        Vector3? hitPoint = GetHitPoint();
        if (hitPoint != null)
            AimTowardsPoint((Vector3)hitPoint);
        return hitPoint;
    }

    [Client]
    private void AimTowardsPoint(Vector3 point)
    {
        Vector3 direction = (point - transform.position);
        direction.y = 0;
        if (direction.sqrMagnitude > .15f)// minmum distance to prevent fast self rotate 
            CmdRotateToDirection(direction);
    }
    [Client]
    private Vector3? GetHitPoint()
    {
        Plane plane = new Plane(Vector3.up, Vector3.zero);
        Ray ray = _mainCamera.ScreenPointToRay(Input.mousePosition);

        if (plane.Raycast(ray, out float point))
        {
            Vector3 hitPoint = ray.GetPoint(point);
            return hitPoint;
        }
        return null;
    }





    [Client]
    private void AuthorityStartAim()
    {
        _isStartToAim = true;
        AimingManager.Instance.SetIsAim(true);
        CmdSetAiming(true, _activeItem as ItemThrow);
        AuthorityOnAiming?.Invoke();
    }
    [Client]
    private void AuthorityStopAim()
    {
        if (!_isStartToAim) { return; }

        _isStartToAim = false;
        AimingManager.Instance.SetIsAim(false);
        CmdSetAiming(false, _activeItem as ItemThrow);
    }


    #region Weapon Aiming
    [Client]
    public void WeaponAim(bool isAiming)
    {
        if (isAiming)
        {
            Aim();
            if (!_isStartToAim)
                AuthorityStartAim();
        }
        else
        {
            AuthorityStopAim();
        }
    }


    #endregion

    #region Aiming To Throw

    [Client]
    public virtual void HandleInputThrow(bool inputStart, bool inputThrow, bool inputCancel)
    {
        AimingProperties aimingProperties = _activeItem.GetAimingProperties();
        float launchForceMin = aimingProperties.launchForceMin;
        float launchForceMax = aimingProperties.launchForceMax;
        if (inputStart)
        {
            ItemThrowAim(launchForceMax);
            if (!_isStartToAim && _currentLaunchForce >= launchForceMin)
            {
                //StartAim
                AuthorityStartAim();
            }
        }
        if (_currentLaunchForce < launchForceMin || inputCancel)
        {
            //Cancel Aim
            AuthorityStopAim();
        }
        if (_isStartToAim && inputThrow)
        {
            //Throw
            if (_currentLaunchForce >= launchForceMin)
            {
                ((ItemThrow)_activeItem).StartThrow(_currentLaunchDirection, _currentLaunchForce);
                AimingManager.Instance.SetIsAim(false);
            }
            else
                AuthorityStopAim();
        }
    }

    [Client]
    private void ItemThrowAim(float launchForceMax)
    {
        Vector3? hitPoint = Aim();
        if (hitPoint == null) { return; }

        _currentLaunchDirection = ((Vector3)hitPoint - transform.position);
        _currentLaunchForce = _currentLaunchDirection.sqrMagnitude * .4f;
        _currentLaunchForce = Mathf.Min(_currentLaunchForce, launchForceMax);


        AimingManager.Instance.SetVisualThrowing(_activeItem.transform.position,
            _currentLaunchDirection,
            _currentLaunchForce);

    }

    #endregion



    private void AuthorityHandleActiveItemChanged(Item activeItem)
    {
        AuthorityStopAim();
        _activeItem = activeItem;
        if (_activeItem != null)
            AimingManager.Instance.SetProperties(_activeItem.GetAimingProperties());
    }

    #endregion
}
