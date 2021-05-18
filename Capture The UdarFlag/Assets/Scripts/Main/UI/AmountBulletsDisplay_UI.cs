using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AmountBulletsDisplay_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _amountBulletsText;


    private void Start()
    {
        _amountBulletsText.enabled = false;
    }

    private void OnEnable()
    {
        Weapon.AuthorityOnAmountBulletChanged += AuthorityHandleShoot;
        BagManager.AuthorityOnItemSwitched += AuthorityHandleItemSwitched;
        BagManager.AuthorityOnRemovedItem += AuthorityHandleRemovedItem;
    }



    private void OnDisable()
    {
        Weapon.AuthorityOnAmountBulletChanged -= AuthorityHandleShoot;
        BagManager.AuthorityOnItemSwitched -= AuthorityHandleItemSwitched;
        BagManager.AuthorityOnRemovedItem -= AuthorityHandleRemovedItem;
    }

    

    private void AuthorityHandleShoot(int currentAmountBullets, int amountBulletsMagazine)
    {
        _amountBulletsText.enabled = true;
        UpdateAmountBulletsText(currentAmountBullets, amountBulletsMagazine);
    }

    private void AuthorityHandleItemSwitched(List<Item> items, int currentIndex)
    {
        if (items[currentIndex] as Weapon)
        {
            _amountBulletsText.enabled = true;
            Weapon weapon = ((Weapon)items[currentIndex]);
            UpdateAmountBulletsText(weapon.GetCurrentBulletAmount(),
                weapon.GetBulletAmountMagazine());
        }
        else
            _amountBulletsText.enabled = false;
    }
    private void AuthorityHandleRemovedItem(List<Item> items)
    {
       if(items.Count==0)
            _amountBulletsText.enabled = false;
    }
    private void UpdateAmountBulletsText(int currentAmountBullets, int amountBulletsMagazine)
    {
        _amountBulletsText.text = currentAmountBullets + "/" + amountBulletsMagazine;
    }

}
