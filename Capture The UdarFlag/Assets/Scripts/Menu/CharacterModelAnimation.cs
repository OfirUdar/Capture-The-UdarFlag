using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterModelAnimation : MonoBehaviour
{
    [SerializeField] private Animator _anim;   
    [SerializeField] private LayerMask _layerMask;

    private Camera _camera;

    private int _danceHash = Animator.StringToHash("Macarena_Dance");
    private int _waveHash = Animator.StringToHash("Wave");


    private void Awake()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _layerMask))
            {
                if (hitInfo.transform == transform)
                    AnimateRandom();
            }
        }
    }


    private void AnimateRandom()
    {
        int randomNum = Random.Range(0, 2);
        switch(randomNum)
        {
            case 0:
                {
                    Dance();
                    break;
                }
            case 1:
                {
                    Wave();
                    break;
                }
        }
    }

    private void Dance()
    {
        _anim.SetTrigger(_danceHash);
    }
    private void Wave()
    {
        _anim.SetTrigger(_waveHash);
    }
}
