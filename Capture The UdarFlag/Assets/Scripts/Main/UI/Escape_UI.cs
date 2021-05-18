using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Escape_UI : MonoBehaviour
{
    [SerializeField] private Panel _panelEscape;

    private bool _isGameOver = false;

    private void OnEnable()
    {
        GameManager.ClientOnGameOver += (int idTeamWon) => _isGameOver = true;
    }
    private void OnDisable()
    {
        GameManager.ClientOnGameOver -= (int idTeamWon) => _isGameOver = true;
    }
    private void Update()
    {
        if (!_isGameOver && Input.GetKeyDown(KeyCode.Escape))
        {
            if (_panelEscape.isActiveAndEnabled)
                _panelEscape.ClosePanel();
            else
                _panelEscape.gameObject.SetActive(true);
        }
    }

    public void BackToMenu()
    {
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
    }
}
