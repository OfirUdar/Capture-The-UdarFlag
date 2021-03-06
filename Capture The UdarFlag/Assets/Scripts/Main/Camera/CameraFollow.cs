using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Vector3 _offset;
    [SerializeField] private bool _startOffset = false;
    [SerializeField] private float _smoothTime = .5f;
    public Transform targetFollow;


    private void Start()
    {  
        if (_startOffset)
            _offset = transform.position;
    }

    private void LateUpdate()
    {     
        if (ReferenceEquals(targetFollow, null)) { return; }

        Vector3 newPos = targetFollow.position + _offset;

        transform.position = Vector3.Lerp(transform.position, newPos, _smoothTime);
    }

    public void SetupCamera(Transform targetFollow)
    {
        StartCoroutine(ChangeTheSmoothTime(_smoothTime));
        this.targetFollow = targetFollow; 
    }
    private IEnumerator ChangeTheSmoothTime(float targetSmoothTime)
    {
        _smoothTime = 0;
        while (_smoothTime<targetSmoothTime)
        {
            _smoothTime = Mathf.Lerp(_smoothTime, targetSmoothTime, .02f);
            yield return null;
        }
    }
}
