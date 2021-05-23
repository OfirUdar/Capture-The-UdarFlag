using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BagManager : NetworkBehaviour
{
    public int maxItems = 4;
    private List<Item> _items = new List<Item>();

    private int _currentItemIndex = 0;
    private bool _isReplacing = false;


    public event Action<List<Item>, int> ServerOnItemSwitched;

    public static event Action<List<Item>, int> AuthorityOnItemSwitched;
    public static event Action<List<Item>> AuthorityOnAddedItem;
    public static event Action<List<Item>> AuthorityOnRemovedItem;
    public static event Action<Item, int> AuthorityOnItemReplaced;

    public event Action ClientOnFlagCollected;
    public event Action ClientOnFlagRemoved;

    private bool _isDie = false;


    #region Server

    [Server]
    public void RemoveAll()
    {
        while (_items.Count > 0)
        {
            Item item = _items[0];
            item.gameObject.SetActive(true);
            item.ServerRemove();
        }
    }
    [Server]
    public void ServerDie() // when player die - remove flag, disable the current item, and disable input for switching items
    {
        TargetSetDie(true);
        RemoveFlag();
        ServerSetActiveCurrentItem(false);
    }
    [Server]
    public void ServerRespawn()// when server respawn - activate the current item, activate the input for switching items
    {
        TargetSetDie(false);
        ServerSetActiveCurrentItem(true);
    }
    [Server]
    private void ServerSetActiveCurrentItem(bool active)
    {
        if (active)
        {
            ServerSwitchToItem(_currentItemIndex);
        }
        else
        {
            ServerSwitchToItem(-1);
        }
    }
    [Server]
    private void RemoveFlag()
    {
        foreach (Item item in _items)
        {
            if (item as Flag)
            {
                item.gameObject.SetActive(true);
                item.ServerRemove();
                return;
            }
        }
    }

    [Server]
    public void ServerAddItem(Item item)
    {                                                                               
        if (_items.Count >= maxItems)
        {
            //need to replace!
            _isReplacing = true;
            ServerReplaceItem(_currentItemIndex, item);
            _isReplacing = false;
        }
        else
        {
            _items.Add(item);
            item.gameObject.SetActive(false);
            RpcAddItem(item);
            if (item as Flag || _items.Count == 1)
                ServerSwitchToItem(_items.Count - 1);
        }
        if (isClient && item as Flag)
            ClientOnFlagCollected?.Invoke();
        item.ServerOnItemRemoved += ServerHandleItemRemoved;
    }
    [Server]
    public void ServerRemoveItem(Item item)
    {
        int itemIndex = _items.IndexOf(item);
        if (isClient && item as Flag)
            ClientOnFlagRemoved?.Invoke();
        _items.RemoveAt(itemIndex);
        RpcRemoveItem(itemIndex);
        item.ServerOnItemRemoved -= ServerHandleItemRemoved;


        if (_items.Count > 0 && _currentItemIndex >= _items.Count)
            _currentItemIndex = _items.Count - 1;
        ServerSwitchToItem(_currentItemIndex);

    }
    [Server]
    private void ServerSwitchToItem(int newItemIndex)
    {
        if (_currentItemIndex >= 0 && _currentItemIndex < _items.Count)
            _items[_currentItemIndex].gameObject.SetActive(false);
        if (newItemIndex >= 0 && newItemIndex < _items.Count)
        {
            _currentItemIndex = newItemIndex;
            _items[_currentItemIndex].gameObject.SetActive(true);
        }
        ServerOnItemSwitched?.Invoke(_items, newItemIndex);
        RpcSwitchToItem(newItemIndex);
    }
    [Server]
    private void ServerReplaceItem(int index, Item itemToReplace)
    {
        if (index < 0 || index >= _items.Count) { return; }

        Item itemToRemove = _items[index];
        itemToRemove.ServerRemove();
        _items[index] = itemToReplace;
        RpcReplaceItem(itemToReplace, index);
        ServerSwitchToItem(index);
    }
    [Server]
    private void ServerHandleItemRemoved(Item item)
    {
        if (!_isReplacing)
            ServerRemoveItem(item);
        item.ServerOnItemRemoved -= ServerHandleItemRemoved;
    }


    [Command]
    private void CmdSwitchItem(int newItemIndex)
    {
        if (_currentItemIndex == newItemIndex) { return; }

        ServerSwitchToItem(newItemIndex);
    }

    #endregion


    #region Client

    [ClientCallback]
    private void Update()
    {
        if (!hasAuthority) { return; }
        if (_isDie) { return; }
        if (_items.Count == 0) { return; }
        HandleKeyboardSwitcher();
        HandleMouseSwitcher();
    }


    [Client]
    private void HandleKeyboardSwitcher()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) { CmdSwitchItem(0); }
        if (Input.GetKeyDown(KeyCode.Alpha2)) { CmdSwitchItem(1); }
        if (Input.GetKeyDown(KeyCode.Alpha3)) { CmdSwitchItem(2); }
        if (Input.GetKeyDown(KeyCode.Alpha4)) { CmdSwitchItem(3); }
    }
    [Client]
    private void HandleMouseSwitcher()
    {
        if (Input.mouseScrollDelta.y > 0)
        {
            if (_currentItemIndex - 1 >= 0)
                CmdSwitchItem(_currentItemIndex - 1);
            else
                CmdSwitchItem(_items.Count - 1);
        }
        else if (Input.mouseScrollDelta.y < 0)
        {
            if (_currentItemIndex + 1 < _items.Count)
                CmdSwitchItem(_currentItemIndex + 1);
            else
                CmdSwitchItem(0);
        }
    }

    [TargetRpc]
    private void TargetSetDie(bool isDie) // set _isDie variable to prevent input
    {
        _isDie = isDie;
    }

    [ClientRpc]
    private void RpcReplaceItem(Item itemToReplace, int index)
    {
        if (isClientOnly)
        {
            _items[index] = itemToReplace;
        }

        if (hasAuthority)
            AuthorityOnItemReplaced?.Invoke(itemToReplace, index);
    }

    [ClientRpc]
    public void RpcAddItem(Item item)
    {
        if (isClientOnly)
        {
            _items.Add(item);
            item.gameObject.SetActive(false);
            if (item as Flag)
                ClientOnFlagCollected?.Invoke();
        }
        if (hasAuthority)
            AuthorityOnAddedItem?.Invoke(_items);
    }

    [ClientRpc]
    public void RpcRemoveItem(int index)
    {
        if (isClientOnly)
        {
            if (_items[index] as Flag)
                ClientOnFlagRemoved?.Invoke();
            _items[index].gameObject.SetActive(true);
            _items.Remove(_items[index]);
        }
        if (hasAuthority)
            AuthorityOnRemovedItem?.Invoke(_items);
    }

    [ClientRpc]
    private void RpcSwitchToItem(int newItemIndex)
    {
        if (isClientOnly)
        {
            if (_currentItemIndex >= 0 && _currentItemIndex < _items.Count)
                _items[_currentItemIndex].gameObject.SetActive(false);
            if (newItemIndex >= 0 && newItemIndex < _items.Count)
            {
                _currentItemIndex = newItemIndex;
                _items[_currentItemIndex].gameObject.SetActive(true);

            }
        }

        if (hasAuthority && newItemIndex >= 0 && newItemIndex < _items.Count)
            AuthorityOnItemSwitched?.Invoke(_items, newItemIndex);
    }

    #endregion



}
