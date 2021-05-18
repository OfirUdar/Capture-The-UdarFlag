using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WaitForPlayersDisplay_UI : MonoBehaviour
{
    [SerializeField] private CanvasGroup _canvasGroup;
    [SerializeField] private TextMeshProUGUI _text;

    private void Awake()
    {
        _canvasGroup.alpha = 1;
    }

    private void OnEnable()
    {
        GamePlayer.ClientOnJoinToMainScene += UpdateText;
    }
    private void OnDisable()
    {
        GamePlayer.ClientOnJoinToMainScene -= UpdateText;
    }

    private void UpdateText(int restPlayers)
    {
        if (restPlayers == 0)
            StartCoroutine(DisableWithFadeOut());
        else
            _text.text = "Waiting for the " + restPlayers + " players to join...";
    }

    private IEnumerator DisableWithFadeOut()
    {
        _text.text = "Starting Game!";
        yield return new WaitForSeconds(1f);
        while (_canvasGroup.alpha>0)
        {
            _canvasGroup.alpha -= 0.01f;
            yield return null;
        }
        this.gameObject.SetActive(false);
    }
}

