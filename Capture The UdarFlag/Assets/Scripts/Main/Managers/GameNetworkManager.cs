using Mirror;
using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameNetworkManager : NetworkManager
{
    [Space(2)]
    [Header("-------Steam-------")]
    //Steam
    public bool useSteam = false;
    [SerializeField] private Transport _steamTransport;
    [SerializeField] private SteamManager _steamManager;
    [SerializeField] private Transport _defualtTransport;
    [Header("-------GameSettings-------")]
    public GameSettings gameSettings;
    [Header("-------Scenes-------")]
    [Scene] public string mainScene = "Main_Scene";
    [Scene] public string menuScene = "Menu_Scene";
    [Scene] public string tutorialScene = "Tutorial_Scene";
    [Header("-------Prefabs-------")]
    [SerializeField] private TeamsManager _teamsManagerPfb;
    [SerializeField] private PoolManager _poolManagerPfb;
    [SerializeField] private GameManager _gameManagerPfb;
    [SerializeField] private Base _basePfb;
    [SerializeField] private Flag _flagPfb;


    private NetworkStartPosition[] _basePoints;

    private int _joinedPlayers = 0; // the num player that has joined to the Main Scene

    //Steam
    [HideInInspector] public CSteamID currentLobbyID;


    public static event Action ClientOnConnect;
    public static event Action ClientOnDisconnect;
    public static event Action ClientOnStop;

    public bool CanStartGame()
    {
        // if (numPlayers <= 1) { return false; }

        foreach (Team team in TeamsManager.Instance.Teams)
        {
            foreach (GamePlayer player in team.Players)
            {
                if (!player.IsReady()) { return false; }
            }
        }
        return true;
    }
    public void StartGame()
    {
        ScreenChanger.Instance.LoadScene(mainScene);
    }



    public void StartTutorial()
    {
        ScreenChanger.Instance.LoadScene(tutorialScene);
    }


    public override void Awake()
    {
        base.Awake();
        if (useSteam)
        {
            transport = _steamTransport;
            _defualtTransport.enabled = false;
        }
        else
        {
            transport = _defualtTransport;
            _steamTransport.enabled = false;
            _steamManager.enabled = false;
        }
    }


    #region Server

    public override void OnServerConnect(NetworkConnection conn)
    {
        if (numPlayers >= gameSettings.GetMaxPlayers())
        {
            conn.Disconnect();
        }
        if (!SceneManager.GetActiveScene().path.Equals(menuScene))
        {
            conn.Disconnect();
        }

        if (numPlayers == 0)
        {
            _joinedPlayers = 0;
            TeamsManager teamsManager = Instantiate(_teamsManagerPfb);
            NetworkServer.Spawn(teamsManager.gameObject);
        }
    }
    public override void OnServerDisconnect(NetworkConnection conn)
    {        
        if (conn.identity!=null&&conn.identity.TryGetComponent(out GamePlayer playerDisconnect))
        {
            if (playerDisconnect.playerManager != null)
            {
                playerDisconnect.playerManager.playerLinks.bagManager.RemoveAll(); // remove all the items
            }

            //remove player from team or if team is empty - remove team
            TeamsManager.Instance.ServerRemovePlayer(playerDisconnect);
        }

        base.OnServerDisconnect(conn);

    }

    public override void OnStopServer()
    {
        ScreenChanger.Instance.LoadScene(menuScene);
    }

    public override void OnServerAddPlayer(NetworkConnection conn)
    {
        base.OnServerAddPlayer(conn);

        GamePlayer gamePlayer = conn.identity.GetComponent<GamePlayer>();

        gamePlayer.SetIsLeader(numPlayers == 1);

        TeamsManager.Instance.ServerAddPlayer(gamePlayer);

    }

    public override void OnServerChangeScene(string newSceneName)
    {
        base.OnServerChangeScene(newSceneName);
        if (NetworkClient.active)
            ScreenChanger.Instance.ClientLoadScene();
    }

    [Server]
    public int AddPlayerJoinedTheMainScene()
    {
        _joinedPlayers++;

        if (_joinedPlayers == numPlayers) // when all the players join to the Main Scene create the game!
        {
            Invoke(nameof(CreateGame), .2f);
        }
        return numPlayers - _joinedPlayers; // return the rest players
    }

    #region CreateGame

    [Server]
    private void CreateGame()
    {
        CreateItems();
        CreateGameManager();
        CreatePoolManager();
        _basePoints = FindObjectsOfType<NetworkStartPosition>();
        for (int i = 0; i < TeamsManager.Instance.Teams.Count; i++)
        {
            Base baseTeam = CreateBase(TeamsManager.Instance.Teams[i]);
            Flag flag = CreateFlag(baseTeam);

            TeamsManager.Instance.Teams[i].BaseTeam = baseTeam;
            TeamsManager.Instance.Teams[i].FlagTeam = flag;

            foreach (GamePlayer player in TeamsManager.Instance.Teams[i].Players)
            {
                player.SpawnCharacter();
            }
        }

    }
    [Server]
    private void CreateItems()
    {
        ItemsSpawner.Instance.CreateItems();
    }
    [Server]
    private void CreateGameManager()
    {
        GameManager gameManagerInstance = Instantiate(_gameManagerPfb);
        NetworkServer.Spawn(gameManagerInstance.gameObject);
    }
    [Server]
    private void CreatePoolManager()
    {
        PoolManager poolManagerInstance = Instantiate(_poolManagerPfb);
        NetworkServer.Spawn(poolManagerInstance.gameObject);
    }

    #endregion

    #region Create Teams that include (Base,Flag)

    [Server]
    private Flag CreateFlag(Base baseTeam)
    {
        Flag flagInstance = Instantiate(_flagPfb, baseTeam.transform.position, Quaternion.identity);
        NetworkServer.Spawn(flagInstance.gameObject);
        flagInstance.SetTeam(baseTeam.GetTeam());
        return flagInstance;
    }
    [Server]
    private Base CreateBase(Team team)
    {
        Base baseInstance = Instantiate(_basePfb, _basePoints[TeamsManager.Instance.Teams.IndexOf(team)].transform.position, _basePoints[TeamsManager.Instance.Teams.IndexOf(team)].transform.rotation);
        NetworkServer.Spawn(baseInstance.gameObject);
        baseInstance.SetTeam(team);
        return baseInstance;
    }

    #endregion

    #endregion


    #region Client

    public override void OnClientDisconnect(NetworkConnection conn)
    {
        base.OnClientDisconnect(conn);

        ClientOnDisconnect?.Invoke();
    }

    public override void OnClientConnect(NetworkConnection conn)
    {
        base.OnClientConnect(conn);

        ClientOnConnect?.Invoke();
    }

    public override void OnStopClient()
    {
        if (!SceneManager.GetActiveScene().path.Equals(menuScene))
            ScreenChanger.Instance.LoadScene(menuScene, false);

        ClientOnStop?.Invoke();

        if (useSteam)
            SteamMatchmaking.LeaveLobby(currentLobbyID);

    }
    public override void OnClientChangeScene(string newSceneName, SceneOperation sceneOperation, bool customHandling)
    {
        base.OnClientChangeScene(newSceneName, sceneOperation, customHandling);

        ScreenChanger.Instance.ClientLoadScene();
    }
    public override void OnClientSceneChanged(NetworkConnection conn)
    {
        base.OnClientSceneChanged(conn);

        if (conn.identity.hasAuthority && !SceneManager.GetActiveScene().path.Equals(menuScene))
            conn.identity.GetComponent<GamePlayer>().CmdPlayerJoinedToMainScene();
    }


    #endregion


    #region SyncVarHelper
    public void SetGameObjectSyncedVar<T>(T t, uint id)
    {
        StartCoroutine(IEnumeratorSetGameObjectSyncedVar<T>(t, id));
    }
    public void AddGameObjectSyncedList<T>(List<T> tList, uint id, Action OnAdded = null)
    {
        StartCoroutine(IEnumeratorAddGameObjectSyncedList<T>(tList, id, OnAdded));
    }

    private IEnumerator IEnumeratorSetGameObjectSyncedVar<T>(T t, uint id)
    {
        while (t == null)
        {
            if (NetworkIdentity.spawned.TryGetValue(id, out NetworkIdentity identity))
                t = identity.GetComponent<T>();
            yield return null;
        }
    }
    private IEnumerator IEnumeratorAddGameObjectSyncedList<T>(List<T> tList, uint id, Action OnAdded = null)
    {
        NetworkIdentity identity;

        while (!NetworkIdentity.spawned.TryGetValue(id, out identity))
        {
            yield return null;
        }
        T t = identity.GetComponent<T>();
        if (!tList.Contains(t))
        {
            tList.Add(t);
            OnAdded?.Invoke();
        }

    }

    #endregion


}
[System.Serializable]
public class GameSettings
{
    public int numTeams = 2;
    public int numPlayersInTeam = 1;
    [Tooltip("The amount of flags (score) to win the game")]
    public int flagsAmountTarget = 3;
    public int GetMaxPlayers()
    {
        return numTeams * numPlayersInTeam;
    }
}

