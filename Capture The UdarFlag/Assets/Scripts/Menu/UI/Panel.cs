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
        ((GameNetworkManager)(NetworkManager.singleton)).CursorHandler(true);
    }
    private void OnDisable()
    {
        ((GameNetworkManager)(NetworkManager.singleton)).CursorHandler(false);
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
