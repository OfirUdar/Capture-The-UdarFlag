using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class KillsDisplay_UI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _killsText;

    private void OnEnable()
    {
        PlayerStats.ClientOnKillsChanged += ClientHandleKillsChanged;
    }   
    private void OnDisable()
    {
        PlayerStats.ClientOnKillsChanged -= ClientHandleKillsChanged;
    }



    private void ClientHandleKillsChanged(int kills)
    {
        _killsText.text = " - " + kills;
    }
}
