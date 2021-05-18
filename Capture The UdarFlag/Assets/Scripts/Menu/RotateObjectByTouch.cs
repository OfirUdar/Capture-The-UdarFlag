using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateObjectByTouch : MonoBehaviour
{
    [SerializeField] private float _speed = 1500f;
    [SerializeField] private LayerMask _layerMask;

    private bool _isRotating = false;

    private Camera _camera;


    private void Awake()
    {
        _camera = Camera.main;
    }


    private void OnEnable()
    {
        transform.localRotation = Quaternion.Euler(0, 0, 0);
    }


    private void Update()
    {
        //Detect on mouse
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _layerMask))
            {
                if (hitInfo.transform == transform)
                    _isRotating = true;
            }
        }else if (Input.GetMouseButtonUp(0))
            _isRotating = false;


        
        if (!_isRotating) { return; }
        //Rotate
        transform.rotation = Quaternion.Euler(transform.eulerAngles.x,
           transform.eulerAngles.y - Input.GetAxis("Mouse X") * _speed * Time.deltaTime,
           transform.eulerAngles.z);


    }

}
