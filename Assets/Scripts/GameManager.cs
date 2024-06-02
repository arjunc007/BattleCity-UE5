using Unity.Netcode;
using UnityEngine;

public class GameManager : MonoBehaviour
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
            _lobbyMenu.gameObject.SetActive(true);
            _lobbyMenu.Initialise(true);
        }
        else
        {
            _netManager.StartClient();
        }
        
        IsMultiplayer = true;
    }

    public void StartGame()
    {
        _mapLoader.StartGame(IsMultiplayer);
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
