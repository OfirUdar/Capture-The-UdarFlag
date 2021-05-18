using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObject : MonoBehaviour
{
    [SerializeField] private bool _isRotateToRight = true;
    [SerializeField] private float _speed = 20f;


    private void Start()
    {
        _speed *= _isRotateToRight ? 1 : -1;
    }

    private void Update()
    {
        transform.Rotate(Vector3.forward, _speed * Time.deltaTime);
    }
}
