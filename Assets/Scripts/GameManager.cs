using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    [SerializeField] private MapLoad _mapLoader;
    [SerializeField] private NetworkManager _netManager;
    [SerializeField] private EnemySpawning _spawnManager;

    [SerializeField] private Eagle _eagle;
    private List<Player> _players = new();
    [SerializeField] private Transform[] _spawnPositions;
    [SerializeField] private Transform _powerupPrefab;

    public bool IsMultiplayer = false;
    public bool IsPlaying = false;

    [Header("UI")]
    [SerializeField] private GameObject _mainMenu;
    [SerializeField] private LobbyManager _lobbyMenu;
    [SerializeField] private TextMeshProUGUI _myIP;
    [SerializeField] private TMP_InputField _targetIP;

    public EnemySpawning EnemySpawner => _spawnManager;
    public List<Enemy> Enemies => _spawnManager.Enemies;
    public Transform Eagle => _eagle.transform;
    public Transform BulletHolder => _mapLoader.generatedBulletFolder;
    public Transform WallsHolder => _mapLoader.generatedWallFolder;

    public PowerUp PowerUp { get; private set; }
    private bool _connected = false;

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
        _myIP.text = GetLocalIPv4();
    }

    public void EnterLobby(bool asHost)
    {
        if (_connected)
        {
            Debug.Log("Already connected. Failed to create lobby.");
            return;
        }

        UnityTransport transport = _netManager.NetworkConfig.NetworkTransport as UnityTransport;
        transport.ConnectionData.Address = _targetIP.text;

        if (asHost)
        {
            _netManager.StartHost();
            _lobbyMenu.Initialise(true);
        }
        else
        {
            if (!NetworkManager.Singleton.StartClient())
            {
                Debug.LogError("Failed to start client.");
                _connected = false;
                return;
            }

            _lobbyMenu.Initialise(false);
        }
        _lobbyMenu.gameObject.SetActive(true);
        _connected = true;
        IsMultiplayer = true;
    }

    public void RegisterPlayer(Player player)
    {
        if (!_players.Contains(player))
        {
            _players.Add(player);
        }
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
            var spawner = Instantiate(_spawnManager.gameObject);
            spawner.GetComponent<NetworkObject>().Spawn();
            StartClientGameRpc();
        }
    }

    public void ResetPlayers()
    {
        foreach (var player in _players)
        {
            player.Reset();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void StartClientGameRpc()
    {
        Debug.Log("Start Game");
        PowerUp = Instantiate(_powerupPrefab).GetComponent<PowerUp>();
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
        foreach (var player in _players)
        {
            player.SetLevel(1);
            player.SetLives(3);
        }

        _eagle.SetDestroyed(false);
        _mapLoader.LoadMap(1);
    }

    private string GetLocalIPv4()
    {
        return Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(
        f => f.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
        .ToString();
    }
    public void ExitLobby()
    {
        _netManager.Shutdown();
        StartCoroutine(WaitForDisconnect());
        _lobbyMenu.CleanUp();
    }

    private IEnumerator WaitForDisconnect()
    {
        while (_netManager.ShutdownInProgress)
        {
            yield return null;
        }

        _connected = false;
    }
}
