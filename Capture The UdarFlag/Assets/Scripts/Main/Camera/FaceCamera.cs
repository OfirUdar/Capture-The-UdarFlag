using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Transform _camTransform;

    private void Awake()
    {
        _camTransform = Camera.main.transform;
    }
    private void Update()
    {
        transform.LookAt(transform.position + _camTransform.rotation * Vector3.forward,
            _camTransform.rotation * Vector3.up);
    }
}
