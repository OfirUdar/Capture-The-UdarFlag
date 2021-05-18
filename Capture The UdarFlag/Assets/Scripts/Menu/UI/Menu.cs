using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    [SerializeField] private GameObject _lobbyPanel;
    [SerializeField] private GameObject _joinPanel;
    [SerializeField] private Panel _exitPanel;
    [SerializeField] private Button _joinButton;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _playerNameInputField;


    private bool _isTutorialMade;

  
    private void OnEnable()
    {       
        GameNetworkManager.ClientOnConnect += HandleClientOnConnect;
        GameNetworkManager.ClientOnDisconnect += HandleClientOnDisconnect;
    }
    private void OnDisable()
    {
        GameNetworkManager.ClientOnConnect -= HandleClientOnConnect;
        GameNetworkManager.ClientOnDisconnect -= HandleClientOnDisconnect;
    }
    private void Start()
    {
        _playerNameInputField.text = PlayerPrefs.GetString("PlayerName", "");
        _isTutorialMade = PlayerPrefs.GetInt("IsTutorialMade", 0) == 1;

        if (!_isTutorialMade)
            StartHost();
        Cursor.visible = true;
    }



    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (_exitPanel.isActiveAndEnabled)
                _exitPanel.ClosePanel();
            else
                _exitPanel.gameObject.SetActive(true);
        }
    }

    public void OnEditPlayerName()
    {
        PlayerPrefs.SetString("PlayerName", _playerNameInputField.text);
    }

    public void Join()// Join as a Client
    {
        if (String.IsNullOrEmpty(_ipInputField.text))
        {
            _ipInputField.text = "localhost";
        }
        NetworkManager.singleton.networkAddress = _ipInputField.text;
        NetworkManager.singleton.StartClient();
        _joinButton.interactable = false;
    }
    public void CancelJoin()
    {
        NetworkManager.singleton.StopClient();
        _joinButton.interactable = true;
    }
    public void StartHost()
    {
        NetworkManager.singleton.networkAddress = "localHost";
        NetworkManager.singleton.StartHost();
    }
    public void CloseApplication()
    {
        Application.Quit();
    }

    private void HandleClientOnConnect()
    {
        if(_isTutorialMade)
        {
            _joinPanel.SetActive(false);
            _joinButton.interactable = true;
            ScreenChanger.Instance.LoadPanel(this.gameObject, _lobbyPanel);
        }
        else
        {
            ((GameNetworkManager)(NetworkManager.singleton)).StartTutorial();
        }
       
    }
    private void HandleClientOnDisconnect()
    {
        _joinButton.interactable = true;
    }

    
}
