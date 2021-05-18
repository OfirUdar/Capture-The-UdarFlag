using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Mirror;
public class ScoreDisplay_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _authorityTeamScoreText;
    [SerializeField] private TextMeshProUGUI _opponentTeamScoreText;


    private void Start()
    {
        _authorityTeamScoreText.text = "" + 0;
        _authorityTeamScoreText.color = TeamsManager.Instance.teamAuthorityColor;
        _opponentTeamScoreText.text = "" + 0;
        _opponentTeamScoreText.color = TeamsManager.Instance.teamOpponentColor;

    }

    private void OnEnable()
    {
        GameManager.ClientOnScoreTeamChanged += ClientHandleScoreTeamChanged;
    }
    private void OnDisable()
    {
        GameManager.ClientOnScoreTeamChanged -= ClientHandleScoreTeamChanged;

    }


    private void ClientHandleScoreTeamChanged(bool AuthorityTeam, int score)
    {
        if (AuthorityTeam)
            _authorityTeamScoreText.text = score.ToString();
        else
            _opponentTeamScoreText.text = score.ToString();
    }

}
