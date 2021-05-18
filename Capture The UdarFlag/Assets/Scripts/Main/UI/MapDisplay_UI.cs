using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay_UI : MonoBehaviour
{
    [SerializeField] private Panel _mapPanel;
    [SerializeField] private GameObject _mapCamera;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            if (_mapPanel.isActiveAndEnabled)
                CloseMap();
            else
                OpenMap();
        }

    }

    public void CloseMap()// call from edtior 
    {
        _mapPanel.ClosePanel();
        _mapCamera.SetActive(false);
    }

    public void OpenMap() // call from edtior 
    {
        _mapCamera.SetActive(true);
        _mapPanel.gameObject.SetActive(true);
    }
}
