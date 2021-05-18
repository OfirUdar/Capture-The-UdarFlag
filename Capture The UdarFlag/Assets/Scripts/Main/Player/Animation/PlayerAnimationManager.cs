using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class PlayerAnimationManager : MonoBehaviour
{
    [SerializeField] private Animator _anim;
    [SerializeField] private NetworkAnimator _networkAnim;


    private int _isAimingHash = Animator.StringToHash("IsAiming");
    private int _isAimingThrowHash = Animator.StringToHash("IsAimingThrow");
    private int _throwHash = Animator.StringToHash("Throw");
    private int _velocityZHash = Animator.StringToHash("VelocityZ");
    private int _velocityXHash = Animator.StringToHash("VelocityX");
    private int _dashHash = Animator.StringToHash("Dash");
    private int _isDeadHash = Animator.StringToHash("IsDead");
    private int _shootHash = Animator.StringToHash("Shoot");
    private int _isReloadingHash = Animator.StringToHash("IsReloading");


    private event Action _OnThrowPoint;
    private event Action _OnRealodComplete;
    private event Action _OnShootStarted;




    public void ActivateLayer(int layer, bool active)
    {
        _anim.SetLayerWeight(layer, active ? 1 : 0);
    }
    public void LerpActivateLayer(int layer, bool active)
    {
        StartCoroutine(LerpingActivateLayer(layer, active ? 1 : 0));
    }
    private IEnumerator LerpingActivateLayer(int layer, float value)
    {
        float currentValue = _anim.GetLayerWeight(layer);
        while (Mathf.Abs(currentValue - value) > 0.05f)
        {
            currentValue = Mathf.Lerp(currentValue, value, 0.1f);
            _anim.SetLayerWeight(layer, currentValue);
            yield return null;
        }
        _anim.SetLayerWeight(layer, value);
    }

    public void SetVelocites(float velZ, float velX)
    {
        _anim.SetFloat(_velocityZHash, velZ, 0.03f, Time.deltaTime);
        _anim.SetFloat(_velocityXHash, velX, 0.03f, Time.deltaTime);
    }
    public void Dash()
    {
        _networkAnim.SetTrigger(_dashHash);
    }
    public void SetAnimatorSpeed(float speed)
    {
        _anim.speed = speed;
    }
    public void SetIsAiming(bool isAiming)
    {
        _anim.SetBool(_isAimingHash, isAiming);
    }
    public void SetIsAimingThrow(bool isAimingThrow)
    {
        _anim.SetBool(_isAimingThrowHash, isAimingThrow);
    }
    public void TriggerThrow(Action OnThrowPoint)
    {
        this._OnThrowPoint = OnThrowPoint;
        _networkAnim.SetTrigger(_throwHash);
    }

    public void ThrowPoint()//call from animaton clip event
    {
        _OnThrowPoint?.Invoke();
    }

    public void SetIsDead(bool isDead)
    {
        _anim.SetBool(_isDeadHash, isDead);
    }


    //Weapon
    public void TriggerShoot(Action OnShootStarted)
    {
        this._OnShootStarted = OnShootStarted;
        _networkAnim.SetTrigger(_shootHash);
    }

    public void ShootStarted()//called from event animation
    {
        this._OnShootStarted?.Invoke();
        this._OnShootStarted = null;
    }


    public void StartReload(Action OnReloadComplete)
    {
        _OnRealodComplete = OnReloadComplete;
        _anim.SetBool(_isReloadingHash, true);
    }
    public void StopReload()
    {
        _anim.SetBool(_isReloadingHash, false);
    }

    public void ReloadComplete()//called from event animation
    {
        _OnRealodComplete?.Invoke();
        StopReload();
    }

}
