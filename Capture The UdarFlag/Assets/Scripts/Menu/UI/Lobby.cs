using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Lobby : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] _authorityPlayerStatusText;
    [SerializeField] private TextMeshProUGUI[] _opponentPlayerStatusText;
    [Space]
    [SerializeField] private GameObject _buttonStartGame;
    [SerializeField] private Button _buttonReady;
    [SerializeField] private TextMeshProUGUI _buttonReadyText;
    [SerializeField] private Color _readyColor = Color.green;
    [SerializeField] private Color _unreadylColor = Color.gray;
    [Space]
    [SerializeField] private GameObject _menuPanel;

    private GamePlayer _connPlayer;

    private void OnEnable()
    {
        Team.ClientOnPlayersUpdated += ClientHandleTeamPlayersUpdated;
        GamePlayer.ClientOnReadyChanged += UpdatePlayersStatus;
        GameNetworkManager.ClientOnStop += HandleOnStopClient;
        Setup();
    }
    private void OnDisable()
    {
        Team.ClientOnPlayersUpdated -= ClientHandleTeamPlayersUpdated;
        GamePlayer.ClientOnReadyChanged -= UpdatePlayersStatus;
        GameNetworkManager.ClientOnStop -= HandleOnStopClient;
        CancelInvoke(nameof(UpdatePlayersStatus));
    }
    private void Setup()
    {
        if (NetworkClient.active)
        {
            _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
            ChangeColorButtonReady(false);
            UpdatePlayersStatus();
        }
        else
            Invoke(nameof(HandleOnStopClient), 0.3f);
    }

    private void ClientHandleTeamPlayersUpdated()
    {
        Invoke(nameof(UpdatePlayersStatus), 0.5f);
    }
    private void UpdatePlayersStatus()
    {
        Team authorityTeam = TeamsManager.Instance.GetAuthorityTeam();
        Team opponentTeam = TeamsManager.Instance.GetOpponentTeam();

        UpdatePlayersStatus(authorityTeam, _authorityPlayerStatusText);
        UpdatePlayersStatus(opponentTeam, _opponentPlayerStatusText);
        if (_connPlayer.IsLeader() && ((GameNetworkManager)(NetworkManager.singleton)).CanStartGame())
        {
            _buttonReady.gameObject.SetActive(false);
            _buttonStartGame.SetActive(true);
        }
        else
        {
            _buttonReady.gameObject.SetActive(true);
            _buttonStartGame.SetActive(false);
        }
    }
    private void UpdatePlayersStatus(Team team, TextMeshProUGUI[] playerStatusTexts)
    {
        for (int i = 0; i < playerStatusTexts.Length; i++)
        {
            if (team != null && i < team.Players.Count)
            {
                GamePlayer player = team.Players[i];
                if (player.IsReady())
                {
                    playerStatusTexts[i].text =
                        player.stats.GetPlayerName() + "- <color=green> Ready";
                }
                else
                {
                    playerStatusTexts[i].text =
                       player.stats.GetPlayerName() + "- Not Ready";
                }
            }
            else
            {
                playerStatusTexts[i].text = "Waiting for player...";
            }
        }

    }

    private void ChangeColorButtonReady(bool isReady)
    {
        if (isReady)
        {
            _buttonReadyText.text = "Unready";
            _buttonReady.image.color = _unreadylColor;
        }
        else
        {
            _buttonReadyText.text = "Ready";
            _buttonReady.image.color = _readyColor;
        }
    }

    public void Ready()//call from editor
    {
        bool isReady = !_connPlayer.IsReady();
        ChangeColorButtonReady(isReady);
        _connPlayer.ChangeSignReady();
    }
    public void StartGame()//call from editor
    {
        _connPlayer.StartGame();
    }
    public void Exit()//call from editor
    {
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else if (NetworkClient.active)
            NetworkManager.singleton.StopClient();
    }

    private void HandleOnStopClient()
    {
        ScreenChanger.Instance.LoadPanel(this.gameObject, _menuPanel);
    }
}
