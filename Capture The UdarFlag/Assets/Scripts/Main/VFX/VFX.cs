using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VFX : MonoBehaviour
{
    [SerializeField] private float _vfxLifetime = 2f;
    [SerializeField] private bool _stopOnDisable = false;

    private Transform _parent;
    private void Start()
    {
        _parent = transform.parent;
    }

    private void OnEnable()
    {
        Invoke(nameof(HandleStopVFX), _vfxLifetime);
    }

    private void HandleStopVFX()
    {
        if (_stopOnDisable)
        {
            if (_parent != null)
                transform.parent = _parent;
            this.gameObject.SetActive(false);
            return;
        }
        Destroy(this.gameObject);
    }

    public void SetTempParent(Transform parent)
    {
        transform.parent = parent;
    }
}
