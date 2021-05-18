using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SprintStaminaDisplay_UI : MonoBehaviour
{
    [SerializeField] private Image _sprintStaminaInnerBar;

    private float _progress = 1;
    private void OnEnable()
    {
        PlayerMovement.AuthorityOnSprintStaminaChanged += AuthorityHandleSprintStaminaChanged;
    }

    private void OnDisable()
    {
        PlayerMovement.AuthorityOnSprintStaminaChanged -= AuthorityHandleSprintStaminaChanged;
    }

    private void Update()
    {
        if (_sprintStaminaInnerBar.fillAmount != _progress)
            _sprintStaminaInnerBar.fillAmount = Mathf.Lerp(_sprintStaminaInnerBar.fillAmount, _progress, 0.15f);
    }

    private void AuthorityHandleSprintStaminaChanged(float progress)
    {
        _progress = progress;
    }

}
