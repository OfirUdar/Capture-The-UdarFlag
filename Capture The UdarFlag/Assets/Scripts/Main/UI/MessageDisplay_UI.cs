using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;




public class MessageDisplay_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _messageText;
    [SerializeField] private float _timeToDisplay = 3f;
    [Space]
    [SerializeField] private string _authorityTeamStoleFlag = "Your Team Has Stole The Enemy Flag";
    [SerializeField] private string _authorityTeamCapturedFlag = "Enemy Flag Successfully Captured";
    [SerializeField] private string _authorityTeamRecoverdFlag = "Your Team Has Recovered Your Flag";

    [SerializeField] private string _opponentTeamStoleFlag = "Your Enemy Has Stole Your Flag";
    [SerializeField] private string _opponentTeamCapturedFlag = "Your Flag Successfully Captured";
    [SerializeField] private string _opponentTeamRecoverdFlag = "The Enemy Has Recovered Their Flag";




    private struct Message
    {
        public bool isAuthorityTeam;
        public string messageText;

        public Message(bool isAuthorityTeam, string messageText)
        {
            this.isAuthorityTeam = isAuthorityTeam;
            this.messageText = messageText;
        }
    }
    private Queue<Message> _messagesQueue = new Queue<Message>();

    private Color _teamAuthorityColor;
    private Color _teamOpponentColor;

    private void Start()
    {
        _teamAuthorityColor = TeamsManager.Instance.teamAuthorityColor;
        _teamOpponentColor = TeamsManager.Instance.teamOpponentColor;
    }

    private void OnEnable()
    {
        ItemCollector.ClientOnFlagCollected += HandleOnFlagCollected;
        Flag.ClientOnFlagCaptured += HandleOnFlagCaptured;
    }
    private void OnDisable()
    {
        ItemCollector.ClientOnFlagCollected -= HandleOnFlagCollected;
        Flag.ClientOnFlagCaptured -= HandleOnFlagCaptured;
    }
    private void DisplayMessage()
    {
        Message message = _messagesQueue.Dequeue();

        _messageText.color = message.isAuthorityTeam ? _teamAuthorityColor : _teamOpponentColor;
        _messageText.text = message.messageText;
        _messageText.gameObject.SetActive(true);


        Invoke(nameof(StopDisplayMessage), _timeToDisplay);
    }
    private void StopDisplayMessage()
    {
        if (_messagesQueue.Count != 0)
            DisplayMessage();
        else
            _messageText.gameObject.SetActive(false);

    }

    private void HandleOnFlagCollected(bool isTeamAuthority, bool isRecovered)
    {
        if (isRecovered)
        {
            if (isTeamAuthority)
            {
                _messagesQueue.Enqueue(new Message(isTeamAuthority, _authorityTeamRecoverdFlag));
                AudioManager.Instance.PlayOneShot("AuthorityStoleOrRecoverdFlag");
            }
            else
            {
                _messagesQueue.Enqueue(new Message(isTeamAuthority, _opponentTeamRecoverdFlag));
                AudioManager.Instance.PlayOneShot("OpponentStoleOrRecoverdFlag");
            }
        }
        else
        {
            if (isTeamAuthority)
            {
                _messagesQueue.Enqueue(new Message(!isTeamAuthority, _opponentTeamStoleFlag));
                AudioManager.Instance.PlayOneShot("AuthorityStoleOrRecoverdFlag");
            }
            else
            {
                _messagesQueue.Enqueue(new Message(!isTeamAuthority, _authorityTeamStoleFlag));
                AudioManager.Instance.PlayOneShot("OpponentStoleOrRecoverdFlag");
            }
        }

        if (_messagesQueue.Count != 0)
            DisplayMessage();
    }
    private void HandleOnFlagCaptured(bool isTeamAuthority)
    {
        if (isTeamAuthority)
        {
            _messagesQueue.Enqueue(new Message(!isTeamAuthority, _opponentTeamCapturedFlag));
            AudioManager.Instance.PlayOneShot("OpponentCaptureFlag");
        }
        else
        {
            _messagesQueue.Enqueue(new Message(!isTeamAuthority, _authorityTeamCapturedFlag));
            AudioManager.Instance.PlayOneShot("AuthorityCaptureFlag");
        }
        if (_messagesQueue.Count != 0)
            DisplayMessage();
    }

}