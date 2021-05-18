using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AimingProperties
{

    public AimingType aimingType;
    [Space]
    [Header("Aiming")]
    public float maxViewDistance = 5f;
    public float fieldOfView = 90f;
    [Space]
    [Header("Aiming To Throw")]
    public float launchForceMin = 5f;
    public float launchForceMax = 15f;
    public float radiusThrowing = 2;
}
public enum AimingType
{
    Aiming,
    AimingToThrow
}

public class AimingManager : MonoBehaviour
{
    public static AimingManager Instance { get; private set; }
    [SerializeField] private VisualAimingFieldOfView _aimingFieldOfView;
    [SerializeField] private VisualAimingThrowing _aimingThrowing;

    private AimingType _currentAimingType;

    [HideInInspector]
    public bool isAiming;

    private void Awake()
    {
        Instance = this;
    }
    public void SetIsAim(bool isAiming)
    {
        this.isAiming = isAiming;
        switch (_currentAimingType)
        {
            case AimingType.Aiming:
                {
                    _aimingFieldOfView.gameObject.SetActive(isAiming);
                    break;
                }
            case AimingType.AimingToThrow:
                {
                    if (isAiming) // activate the visual with some delay 
                        StartCoroutine(ActivateObjectDelay(_aimingThrowing.gameObject, isAiming, .2f));
                    else
                    {
                        StopAllCoroutines();
                        _aimingThrowing.gameObject.SetActive(isAiming);
                    }
                    break;
                }
        }
    }
    public void SetProperties(AimingProperties aimingProperties)
    {
        _currentAimingType = aimingProperties.aimingType;

        switch (_currentAimingType)
        {
            case AimingType.Aiming:
                {
                    _aimingFieldOfView.SetAimingProperties(aimingProperties);
                    break;
                }
            case AimingType.AimingToThrow:
                {
                    _aimingThrowing.SetRadius(aimingProperties.radiusThrowing);
                    break;
                }
        }

    }

    //Throwing
    public void SetVisualThrowing(Vector3 startPosition, Vector3 launchDirection, float launchForce)
    {
        _aimingThrowing.SetVisualThrowing(startPosition, launchDirection, launchForce);
    }




    private IEnumerator ActivateObjectDelay(GameObject gameObject, bool active, float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(active);
    }

}
