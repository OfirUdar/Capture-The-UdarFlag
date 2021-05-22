using Mirror;
using Steamworks;
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
    [SerializeField] private Button _joinButtonPanel;
    [SerializeField] private TMP_InputField _ipInputField;
    [SerializeField] private TMP_InputField _playerNameInputField;


    private bool _isTutorialMade;



    protected Callback<LobbyCreated_t> _lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> _lobbyJoinRequested;
    protected Callback<LobbyEnter_t> _lobbyEntered;

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
        Setup();
        if (((GameNetworkManager)NetworkManager.singleton).useSteam)
            SteamSetup();
    }

    private void Setup()
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
        _joinButtonPanel.interactable = false;
    }
    public void CancelJoin()
    {
        NetworkManager.singleton.StopClient();
        _joinButtonPanel.interactable = true;
    }
    public void StartHost()
    {
        if (((GameNetworkManager)NetworkManager.singleton).useSteam)
        {
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,
                ((GameNetworkManager)NetworkManager.singleton).gameSettings.GetMaxPlayers());
            return;
        }
        NetworkManager.singleton.networkAddress = "localHost";
        NetworkManager.singleton.StartHost();
    }
    public void CloseApplication()
    {
        Application.Quit();
    }

    private void HandleClientOnConnect()
    {
        if (_isTutorialMade)
        {
            _joinPanel.SetActive(false);
            _joinButtonPanel.interactable = true;
            ScreenChanger.Instance.LoadPanel(this.gameObject, _lobbyPanel);
        }
        else
        {
            ((GameNetworkManager)(NetworkManager.singleton)).StartTutorial();
        }

    }
    private void HandleClientOnDisconnect()
    {
        _joinButtonPanel.interactable = true;
    }



    //Steam

    private void SteamSetup()
    {
        _lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        _lobbyJoinRequested = Callback<GameLobbyJoinRequested_t>.Create(OnLobbyJoinRequested);
        _lobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        _joinButton.interactable = false;
    }


    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) { return; }

        NetworkManager.singleton.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
            "HostAdress",
            SteamUser.GetSteamID().ToString());
    }
    private void OnLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }
    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CSteamID lobbyID = new CSteamID(callback.m_ulSteamIDLobby);
        ((GameNetworkManager)NetworkManager.singleton).currentLobbyID = lobbyID;

        if (NetworkServer.active) { return; }

        string hostAdress = SteamMatchmaking.GetLobbyData(lobbyID
            , "HostAdress");
        NetworkManager.singleton.networkAddress = hostAdress;
        NetworkManager.singleton.StartClient();
    }


}
