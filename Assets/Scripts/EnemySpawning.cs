using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawning : NetworkBehaviour
{
    private NetworkList<int> tanks;
    private Animator anim;
    System.Random r;

    public NetworkVariable<int> next = new();
    public Transform easyTank;
    public Transform fastTank;
    public Transform mediumTank;
    public Transform strongTank;


    public override void OnNetworkSpawn()
    {
        if(!IsServer) { return; }

        r = new System.Random();

        for (int i = 0; i < 20; i++)
        {
            tanks.Add(r.Next(50) % 4 + 1);
        }
    }

    private void Awake()
    {
        tanks = new NetworkList<int>();
    }

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();

        Reset();
    }

    public void Reset()
    {
        transform.position = new Vector3(-12, 12, 0);
        if(IsServer) next.Value = 0;
    }

    void Update()
    {
        if (!IsServer || !GameManager.Instance.IsPlaying)
        {
            return;
        }
        int tankCount = GameManager.Instance.EnemyHolder.GetComponentsInChildren<Transform>().Length;
        bool isMultiPlayer = GameManager.Instance.IsMultiplayer;

        // 4 tanks and 1 folder also counts, (if multiplayer, 6 tanks can be on screen)
        if (next.Value < 20 && (tankCount < 5 && !isMultiPlayer || tankCount < 7 && isMultiPlayer))
        {
            SpawnEnemyRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void SpawnEnemyRpc()
    {
        Debug.Log($"{OwnerClientId} Start Enemy Spawn");
        anim.SetBool("spawn", true);
    }

    // Called from animation event
    private void SpawnEnemy()
    {
        if(!IsServer) { return; }

        anim.SetBool("spawn", false);

        Enemy enemy = null;

        if (tanks[next.Value] == 1)
        {
            enemy = Instantiate(easyTank, transform.position, easyTank.rotation).GetComponent<Enemy>();
        }
        else if (tanks[next.Value] == 2)
        {
            enemy = Instantiate(fastTank, transform.position, fastTank.rotation).GetComponent<Enemy>();
        }
        else if (tanks[next.Value] == 3)
        {
            enemy = Instantiate(mediumTank, transform.position, mediumTank.rotation).GetComponent<Enemy>().GetComponent<Enemy>();
        }
        else if (tanks[next.Value] == 4)
        {
            enemy = Instantiate(strongTank, transform.position, strongTank.rotation).GetComponent<Enemy>();
            enemy.SetLives(5);
        }

        PushPosition();

        // every four enemies, one get bonus 
        if ((next.Value + 1) % 4 == 0)
        {
            enemy.SetBonus(r.Next(50) % 5 + 1);
        }

        enemy.GetComponent<NetworkObject>().Spawn();

        next.Value++;
    }

    private void PushPosition()
    {
        transform.position += new Vector3(12, 0, 0);
        if (transform.position.x > 12) transform.position = new Vector3(-12, 12, 0);
    }

    public override void OnDestroy()
    {
        tanks?.Dispose();
    }
}
