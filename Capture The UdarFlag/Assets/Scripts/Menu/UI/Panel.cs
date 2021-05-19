using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Panel : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private int _closePanelHash = Animator.StringToHash("Move_Out");



    private void OnEnable()
    {
        CursorManager.Instance.CursorHandler(true);
    }
    private void OnDisable()
    {
        CursorManager.Instance.CursorHandler(false);
    }
    public void ClosePanel()
    {
        _anim.Play(_closePanelHash);
    }
    public void DisablePanel()
    {
        this.gameObject.SetActive(false);
    }
}
