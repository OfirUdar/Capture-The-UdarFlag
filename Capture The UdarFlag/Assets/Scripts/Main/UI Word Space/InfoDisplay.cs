using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror;

public class InfoDisplay : MonoBehaviour
{

    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private Image _circleFlagImage;
    [Header("Health Display")]
    [SerializeField] private Image _innerHealthBar;
    [SerializeField] private Image _tempInnerHealthBar;
    [SerializeField] private Color _damageColor;
    [SerializeField] private Color _healColor;
    [Space]
    [SerializeField] private PlayerLinks _playerLinks;

    private void Start()
    {
       Setup();
    }

    private void OnEnable()
    {
        _playerLinks.health.ClientOnHealthUpdated += ClientHandleHealthUpdated;
        _playerLinks.bagManager.ClientOnFlagCollected += ClientHandleFlagCollected;
        _playerLinks.bagManager.ClientOnFlagRemoved += ClientHandleFlagRemoved;
        _circleFlagImage.gameObject.SetActive(false);
    }
    private void OnDisable()
    {
        _playerLinks.health.ClientOnHealthUpdated -= ClientHandleHealthUpdated;
        _playerLinks.bagManager.ClientOnFlagCollected -= ClientHandleFlagCollected;
        _playerLinks.bagManager.ClientOnFlagRemoved -= ClientHandleFlagRemoved;
    }

    private void Setup()
    {
        _nameText.text = _playerLinks.gamePlayer.stats.GetPlayerName();
    }

    private void ClientHandleHealthUpdated(float progress)
    {
        StopAllCoroutines();
        if (progress == 0)
        {
            _tempInnerHealthBar.fillAmount = progress;
            _innerHealthBar.fillAmount = progress;
        }
        else
        {
            StartCoroutine(SetTempHealthBar(0.4f, progress));
        }
    }

    private void ClientHandleFlagCollected()
    {
        _circleFlagImage.gameObject.SetActive(true);
    }
    private void ClientHandleFlagRemoved()
    {
        _circleFlagImage.gameObject.SetActive(false);
    }



    private IEnumerator SetTempHealthBar(float startDelay, float endProgress)
    {
        float startProgress = _tempInnerHealthBar.fillAmount;
        bool isDamage = (startProgress > endProgress);
        float fillAmountPerFrame = 0.008f;

        if (isDamage)
        {
            //Damage
            _tempInnerHealthBar.color = _damageColor;
            _innerHealthBar.fillAmount = endProgress;
            fillAmountPerFrame *= -1;

            yield return new WaitForSeconds(startDelay);

            while (_tempInnerHealthBar.fillAmount - endProgress > 0)
            {
                _tempInnerHealthBar.fillAmount += fillAmountPerFrame;
                yield return null;
            }
            _tempInnerHealthBar.fillAmount = endProgress;
        }
        else
        {
            //Heal
            _tempInnerHealthBar.color = _healColor;
            _tempInnerHealthBar.fillAmount = endProgress;
            fillAmountPerFrame *= 1;

            yield return new WaitForSeconds(startDelay);

            while (endProgress - _innerHealthBar.fillAmount > 0)
            {
                _innerHealthBar.fillAmount += fillAmountPerFrame;
                yield return null;
            }
            _innerHealthBar.fillAmount = endProgress;
        }

    }

}
