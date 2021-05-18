using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ItemsDisplay_UI : MonoBehaviour
{
    [SerializeField] private Image[] _backgroundItemImages;
    [SerializeField] private Image[] _itemImages;
    private int _currentItemIndex = 0;

    [Header("Selected")]
    [SerializeField] private Vector3 _selectedScale = 1.1f * Vector3.one;
    [SerializeField] private Vector3 _unSelectedScale = Vector3.one;
    [SerializeField] private Color _selectedBackgroundColor = Color.black;
    [SerializeField] private Color _selectedColor = Color.white;

    [Header("Unselected")]
    [SerializeField] private Color _unSelectedBackgroundColor = Color.black;
    [SerializeField] private Color _unSelectedColor = Color.white;



    private void Start()
    {
        UpdateItemImages();
    }


    private void OnEnable()
    {
        BagManager.AuthorityOnItemSwitched += AuthorityHandleCurretItemChanged;
        BagManager.AuthorityOnAddedItem += AuthorityHandleAddedItem;
        BagManager.AuthorityOnRemovedItem += AuthorityHandleRemovedItem;
        BagManager.AuthorityOnItemReplaced += AuthorityHandleReplacedItem;
    }



    private void OnDisable()
    {
        BagManager.AuthorityOnItemSwitched -= AuthorityHandleCurretItemChanged;
        BagManager.AuthorityOnAddedItem -= AuthorityHandleAddedItem;
        BagManager.AuthorityOnRemovedItem -= AuthorityHandleRemovedItem;
        BagManager.AuthorityOnItemReplaced -= AuthorityHandleReplacedItem;

    }

    private void AuthorityHandleCurretItemChanged(List<Item> items, int newCurrentIndex)
    {
        if (_currentItemIndex < items.Count) // the old item is active
        {
            _backgroundItemImages[_currentItemIndex].color = _unSelectedBackgroundColor;
            _itemImages[_currentItemIndex].color = _unSelectedColor;
            _backgroundItemImages[_currentItemIndex].transform.localScale = _unSelectedScale;
        }

        _currentItemIndex = newCurrentIndex;

        _backgroundItemImages[_currentItemIndex].color = _selectedBackgroundColor;
        _itemImages[_currentItemIndex].color = _selectedColor;
        _backgroundItemImages[_currentItemIndex].transform.localScale = _selectedScale;
    }

    private void AuthorityHandleAddedItem(List<Item> items)
    {
        UpdateItemImages(items);
    }

    private void AuthorityHandleRemovedItem(List<Item> items)
    {
        UpdateItemImages(items);
    }

    private void AuthorityHandleReplacedItem(Item newItem, int index)
    {
        _itemImages[index].sprite = newItem.GetItemSprite();
    }

    private void UpdateItemImages(List<Item> items = null)
    {
        int i = 0;
        if (items != null&&items.Count>0)
        {
            for (; i < items.Count; i++)
            {
                _itemImages[i].sprite = items[i].GetItemSprite();
                if (i == _currentItemIndex)
                {
                    _itemImages[_currentItemIndex].color = _selectedColor;
                    _backgroundItemImages[_currentItemIndex].color = _selectedBackgroundColor;
                    _backgroundItemImages[_currentItemIndex].transform.localScale = _selectedScale;
                }
                else
                {
                    _itemImages[i].color = _unSelectedColor;
                    _backgroundItemImages[i].color = _unSelectedBackgroundColor;
                    _backgroundItemImages[i].transform.localScale = _unSelectedScale;
                }
            }

        }
        for (; i < _itemImages.Length; i++)
        {
            _itemImages[i].sprite = null;
            _itemImages[i].color = Color.clear;
            _backgroundItemImages[i].color = new Color(0, 0, 0, .1f);
            _backgroundItemImages[i].transform.localScale = _unSelectedScale;
        }


    }




}
