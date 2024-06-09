using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private MapLoad _mapLoader;
    [SerializeField] private NetworkManager _netManager;

    [SerializeField] private Eagle _eagle;
    [SerializeField] private Transform[] _players;
    [SerializeField] private Transform[] _spawnPositions;

    public bool IsMultiplayer = false;
    public bool IsPlaying = false;

    [Header("UI")]
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private LobbyManager _lobbyMenu;
    public Transform BulletHolder => _mapLoader.generatedBulletFolder;
    public Transform EnemyHolder => _mapLoader.generatedEnemyFolder;
    public Transform WallsHolder => _mapLoader.generatedWallFolder;

    public override void OnNetworkSpawn()
    {
        Debug.Log("Lobby Network Spawn");
        if (IsServer)
        {
            //Server will be notified when a client connects
            NetworkManager.OnClientConnectedCallback += OnClientConnectedCallback;
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        Debug.Log($"{clientId} is connected.");
        if (IsServer)
        {
            _lobbyMenu.AddClient();
        }
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.antiAliasing = 0;
        QualitySettings.shadowCascades = 0;
        QualitySettings.vSyncCount = 1;
        QualitySettings.SetQualityLevel(2);
        _mainMenu.SetActive(true);
    }

    public void EnterLobby(bool isHost)
    {
        if(isHost)
        {
            _netManager.StartHost();
            _lobbyMenu.Initialise(true);
        }
        else
        {
            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Failed to start client.");
                return;
            }
            
            _lobbyMenu.Initialise(false);
        }
        _lobbyMenu.gameObject.SetActive(true);
        IsMultiplayer = true;
    }

    [Rpc(SendTo.ClientsAndHost)]
    public void ReadyPlayerRpc(ulong clientId)
    {
        _lobbyMenu.TogglePlayerReady(clientId);
    }

    public void StartGame()
    {
        if (IsServer)
        {
            StartClientGameRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartClientGameRpc()
    {
        Debug.Log("Start Game");
        _mapLoader.StartGame(IsMultiplayer);
        _lobbyMenu.gameObject.SetActive(false);
        IsPlaying = true;
    }

    public Vector2 GetStartPosition(int playerId)
    {
        return new Vector2(_spawnPositions[playerId].position.x, _spawnPositions[playerId].position.y);
    }

    public void FinishGame()
    {

        _players[0].SendMessage("SetLevel", 1);
        _players[0].SendMessage("SetLives", 3);
        //player2.SendMessage("SetLevel", 1);
        //player2.SendMessage("SetLives", 3);
        _eagle.SetDestroyed(false);
        _mapLoader.LoadMap(1);
    }
}
