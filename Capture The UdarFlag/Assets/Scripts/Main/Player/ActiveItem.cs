using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.EventSystems;

public class ActiveItem : NetworkBehaviour
{
    [SerializeField] private PlayerLinks _playerLinks;
    public AudioOB itemAudio; // On the bag object


    [SyncVar(hook = nameof(ClientHandleActiveItemChanged))]
    private Item _activeItem;


    public static event Action<Item> AuthorityOnActiveItemChanged;



    public Item GetActiveItem()
    {
        return _activeItem;
    }

    #region Server

    public override void OnStartServer()
    {
        _playerLinks.bagManager.ServerOnItemSwitched += ServerHandleItemSwitched;

    }
    public override void OnStopServer()
    {
        _playerLinks.bagManager.ServerOnItemSwitched -= ServerHandleItemSwitched;

    }

    [Server]
    private void ServerHandleItemSwitched(List<Item> items, int currentItemIndex)
    {
        bool isBagEmpty = currentItemIndex < 0 || currentItemIndex >= items.Count;
        if (_activeItem != null)
        {
            if (isBagEmpty || (!isBagEmpty && items[currentItemIndex].GetItemAnimationLayer() != _activeItem.GetItemAnimationLayer()))
                _playerLinks.animManager.LerpActivateLayer(_activeItem.GetItemAnimationLayer(), false);
        }

        if (!isBagEmpty)
        {
            _activeItem = items[currentItemIndex];
            _playerLinks.animManager.LerpActivateLayer(_activeItem.GetItemAnimationLayer(), true);
        }
        else
            _activeItem = null;


    }


    #endregion


    #region Client


    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority || _activeItem == null) { return; }
        if (EventSystem.current.IsPointerOverGameObject()) { return; }// when clicking on UI it doesnt response

        bool isRemoving = Input.GetKeyDown(KeyCode.T);

        bool isUsing = Input.GetMouseButton(0);
        bool isOnceUsing = Input.GetMouseButtonDown(0);
        bool isStopUsing = Input.GetMouseButtonUp(0);
        bool isAiming = Input.GetMouseButton(1);
        bool isStopAiming = Input.GetMouseButtonUp(1);

        bool isReloading = Input.GetKeyDown(KeyCode.R);

        if (isRemoving)
        {
            _activeItem.CmdRemove();
            return;
        }       
        if (isAiming || isStopAiming)
        {
            if (_activeItem as Weapon)
                _playerLinks.playerAiming.WeaponAim(isAiming);
            else if (_activeItem as ItemThrow)
                AimToThrow(isAiming, isOnceUsing, isStopAiming);
        }
        if (isUsing || isOnceUsing)
        {
            _activeItem.Use(isUsing, isOnceUsing);
        }
        if (isStopUsing)
        {
            _activeItem.StopUse();
        }
        if (isReloading)
        {
            if (_activeItem as Weapon)
                ((Weapon)_activeItem).CmdReload();
        }
    }

    //ItemToThrow
    [Client]
    private void AimToThrow(bool isAiming, bool isOnceUsing, bool isStopAiming)
    {
        bool inputAim = isAiming;
        bool inputThrow = isOnceUsing;
        bool inputCancelAim = isStopAiming;

        _playerLinks.playerAiming.HandleInputThrow(inputAim, inputThrow, inputCancelAim);
    }

    private void ClientHandleActiveItemChanged(Item oldItem, Item newItem)
    {
        if (!hasAuthority) { return; }

        AuthorityOnActiveItemChanged?.Invoke(newItem);
    }


    #endregion
}
