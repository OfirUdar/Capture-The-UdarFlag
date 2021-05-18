using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DieDisplay_UI : MonoBehaviour
{
    [SerializeField] private GameObject _dieDisplay;
    [SerializeField] private TextMeshProUGUI _text;

    private bool _isRespawning;
    private int _respawnTime;
    private float _timer;
    private string _textToDisplay;

    private void OnEnable()
    {
        PlayerManager.AuthorityOnDie += AuthorityHandlePlayerDie;
        PlayerManager.AuthorityOnImprisoner += AuthorityHandlePlayerImprisoner;
        PlayerManager.AuthorityOnRealse += AuthorityHandlePlayerRealsed;
    }

    private void OnDisable()
    {
        PlayerManager.AuthorityOnDie -= AuthorityHandlePlayerDie;
        PlayerManager.AuthorityOnImprisoner -= AuthorityHandlePlayerImprisoner;
        PlayerManager.AuthorityOnRealse -= AuthorityHandlePlayerRealsed;
    }


    private void Update()
    {
        if(_isRespawning)
        {
            _timer += Time.deltaTime;
            _text.text = _textToDisplay+"\nRespawn in " + Mathf.RoundToInt(_respawnTime - _timer) + " seconds...";
            if(_timer>=_respawnTime)
            {
                RespawnTimeEnd();
            }
        }
    }

    private void AuthorityHandlePlayerDie(int respawnTime)
    {
        _dieDisplay.SetActive(true);
        _respawnTime = respawnTime;
        _isRespawning = true;
        _textToDisplay = "";
    }
    private void AuthorityHandlePlayerImprisoner(int respawnTime)
    {
        _timer = 0;
        _respawnTime = respawnTime;
        _isRespawning = true;
        _textToDisplay = "You are a prisoner! wait for respawn or you will realsed by your friend";
    }
    private void AuthorityHandlePlayerRealsed()
    {
        RespawnTimeEnd();
    }

    private void RespawnTimeEnd()
    {
        _isRespawning = false;
        _timer = 0;
        _dieDisplay.SetActive(false);
    }
}
