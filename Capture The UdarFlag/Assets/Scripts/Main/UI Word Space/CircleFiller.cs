using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CircleFiller : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Collider _collider;
    [SerializeField] private NetworkIdentity _parentIdentity; // identity for the collect item or something
    [Space]
    [SerializeField] private float _fillTime = 1.5f;


    [Header("Circle Fill Display")]
    [SerializeField] private GameObject _circleFillDisplay;
    [SerializeField] private Image _backgroundFillCircle;
    [SerializeField] private Image _fillCircle;


    public float GetFillTime()
    {
        return _fillTime;
    }
    public void DisplayCircleFiller(bool isDisplay)
    {
        _circleFillDisplay.SetActive(isDisplay);
    }
    public void DisplayBackgroundCircleFiller(bool isDisplay)
    {
        _backgroundFillCircle.gameObject.SetActive(isDisplay);
    }
    public void DisplayFillCircleFiller(bool isDisplay)
    {
        _fillCircle.gameObject.SetActive(isDisplay);
    }
    public void SetFillAmount(float fillAmount)
    {
        _fillCircle.fillAmount = fillAmount;
    }
    public NetworkIdentity GetParentIdentity()
    {
        return _parentIdentity;
    }
    public Collider GetCollider()
    {
        return _collider;
    }



}
