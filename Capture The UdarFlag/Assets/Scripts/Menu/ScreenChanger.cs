using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenChanger : MonoBehaviour
{
    public static ScreenChanger Instance { get; private set; }

    [SerializeField] private Animator _anim;
    [SerializeField] private bool _startWithAnimation = true;
    [SerializeField] private GameObject _loadingBarOB;
    [SerializeField] private Image _loadingBarFillImage;
    [SerializeField] private TextMeshProUGUI _loadingPresentageText;


    private int _startLoadHash = Animator.StringToHash("Start");
    private int _endLoadHash = Animator.StringToHash("End");

    private event Action _OnLoaded;

    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        if (_startWithAnimation)
            EndLoad();
    }
    public void LoadScene(string sceneName, bool isNetworked = true)
    {
        if (_OnLoaded != null) { return; }

        _OnLoaded = () =>
        {
            if (isNetworked)
                NetworkManager.singleton.ServerChangeScene(sceneName);
            else
                SceneManager.LoadScene(sceneName);
            _OnLoaded = null;
        };
        StartLoad();
    }

    public void LoadScene(int sceneIndex, bool isNetworked = true)
    {
        if (_OnLoaded != null) { return; }

        _OnLoaded = () =>
        {
            if (isNetworked)
                NetworkManager.singleton.ServerChangeScene(SceneManager.GetSceneByBuildIndex(sceneIndex).name);
            else
                SceneManager.LoadScene(sceneIndex);
            _OnLoaded = null;
        };

        StartLoad();
    }


    public void ClientLoadScene()
    {
        if (!NetworkServer.active)// if it is client only
        {
            _OnLoaded = () => StartCoroutine(IClientLoadScene());
            StartLoad();
        }
        else
        {
            StartCoroutine(IClientLoadScene());
        }

    }
    private IEnumerator IClientLoadScene()
    {
        _loadingBarOB.SetActive(true);
        yield return null;
        AsyncOperation operation = NetworkManager.loadingSceneAsync;
        while (!operation.isDone)
        {
            float progressBar = Mathf.Clamp01(operation.progress / 0.9f);
            _loadingBarFillImage.fillAmount = progressBar;
            _loadingPresentageText.text = "Loading... " + Mathf.RoundToInt(progressBar * 100)+"%";
            yield return null;
        }
        _loadingBarOB.SetActive(false);
    }

    public void LoadPanel(GameObject currentPanel, GameObject panelToLoad)
    {
        if (_OnLoaded != null) { return; }

        _OnLoaded = () =>
        {
            currentPanel.SetActive(false);
            panelToLoad.SetActive(true);
            _anim.Play(_endLoadHash);
            _OnLoaded = null;
        };
        StartLoad();
    }

    private void StartLoad()
    {
        _anim.Play(_startLoadHash);
    }
    private void EndLoad()
    {
        _anim.Play(_endLoadHash);
    }
    public void OnLoaded()// Call form the animator
    {
        _OnLoaded?.Invoke();
    }
}
