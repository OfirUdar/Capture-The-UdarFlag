using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
public class Rocket : Projectile
{
    [Header("Rocket")]
    [SerializeField] private RendererVisibleHandler _rendererVisibleHandler;
    [SerializeField] private float _shakePower = 0.2f;
    [SerializeField] private float _shakeDuration = 0.2f;


    private bool _isVisible = true;



    public override void OnStartClient()
    {
        base.OnStartClient();
        _rendererVisibleHandler.OnVisibleChanged += (bool isVisible) => _isVisible = isVisible;
    }

    public override void OnStopClient()
    {
        OnStop();
        _rendererVisibleHandler.OnVisibleChanged -= (bool isVisible) => _isVisible = isVisible;
    }

    [ClientCallback]
    protected override void StartEffect()
    {
        PoolManager.Instance.RequestVFXRocketCollison(transform.position, Quaternion.identity);
        if (_isVisible)
            CameraShake.Instance.Shake(_shakeDuration, _shakePower);
    }

}
