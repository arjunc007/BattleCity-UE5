using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [SerializeField] private MapLoad _mapLoader;

    [SerializeField] private Eagle _eagle;
    [SerializeField] private Transform[] _players;

    public bool IsMultiplayer = false;
    
    [Header("UI")]
    [SerializeField] private GameObject _mainMenu;
    public Transform BulletHolder => _mapLoader.generatedBulletFolder;
    public Transform EnemyHolder => _mapLoader.generatedEnemyFolder;
    public Transform WallsHolder => _mapLoader.generatedWallFolder;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this) 
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

    public void StartGame(bool multiplayer)
    {
        IsMultiplayer = multiplayer;
        _mapLoader.StartGame(multiplayer);
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
