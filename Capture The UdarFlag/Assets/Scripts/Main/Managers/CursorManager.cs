using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CursorManager : MonoBehaviour
{
    public static CursorManager Instance { get; private set; }

    [SerializeField] private Texture2D _cursorTextureUI;
    [SerializeField] private Texture2D _cursorMainGame;

    private string _menuScene;
    private string _activeScene;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
            Destroy(this.gameObject);
    }
    
    private void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }
    private void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }
    private void Start()
    {
        _menuScene = ((GameNetworkManager)(NetworkManager.singleton)).menuScene;
        CursorHandler();
    }
    private void OnActiveSceneChanged(Scene current, Scene next)
    {
        _activeScene = next.path;
        CursorHandler();
    }

    public void CursorHandler(bool isUIActive = false) // set ui cursor if UI interactive or if menu scene is active
    {
        if(isUIActive|| _activeScene.Equals(_menuScene))
            Cursor.SetCursor(_cursorTextureUI, Vector2.zero, CursorMode.Auto);
        else
            Cursor.SetCursor(_cursorMainGame, Vector2.zero, CursorMode.Auto);
    }


}
