using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOverDisplay_UI : MonoBehaviour
{
    [SerializeField] private GameObject _gameOverDisplay;
    [SerializeField] private TextMeshProUGUI _text;


    private GamePlayer _connPlayer;

    private void OnEnable()
    {
        GameManager.ClientOnGameOver += ClientHandleGameOver;
    }
    private void OnDisable()
    {
        GameManager.ClientOnGameOver -= ClientHandleGameOver;
    }

    private void Start()
    {
        _connPlayer = NetworkClient.connection.identity.GetComponent<GamePlayer>();
    }

    private void ClientHandleGameOver(int idTeamWon)
    {
        Team teamWon = TeamsManager.Instance.Teams[idTeamWon];
        string teamWonText = "";

        for (int i = 0; i < teamWon.Players.Count; i++)
        {
            if (i == 0)
            {
                teamWonText = teamWon.Players[i].stats.GetPlayerName();
                continue;
            }
            if (i == teamWon.Players.Count - 1)
            {
                teamWonText += " and " + teamWon.Players[i].stats.GetPlayerName();
                continue;
            }
            teamWonText += "," + teamWon.Players[i].stats.GetPlayerName();
        }
        teamWonText += "\nwon the game!";

        StartCoroutine(DisplayGameOverDelay(0.5f, teamWonText, idTeamWon));
    }

    private IEnumerator DisplayGameOverDelay(float delay, string textToDisplay, int idTeamWon)
    {
        yield return new WaitForSeconds(delay);
        DisplayGameOver(textToDisplay);
        if (_connPlayer.IsTeammate(idTeamWon))
            AudioManager.Instance.PlayOneShot("GameOverAuthorityWon");
        else
            AudioManager.Instance.PlayOneShot("GameOverOpponentWon");


    }
    private void DisplayGameOver(string textToDisplay)
    {
        _text.text = textToDisplay;
        _gameOverDisplay.SetActive(true);
    }



    public void BackToMenu()
    {
        if (NetworkServer.active)
            NetworkManager.singleton.StopHost();
        else
            NetworkManager.singleton.StopClient();
    }


}
