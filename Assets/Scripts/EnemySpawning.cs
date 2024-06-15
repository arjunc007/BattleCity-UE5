﻿using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class EnemySpawning : NetworkBehaviour
{
    private NetworkList<int> tanks;
    private Animator anim;
    System.Random r;

    public int next;
    public Transform eagle;
    public Transform generatedEnemyFolder;
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
        next = 0;
    }

    void Update()
    {
        if (!IsServer || !GameManager.Instance.IsPlaying)
        {
            return;
        }
        int tankCount = generatedEnemyFolder.GetComponentsInChildren<Transform>().Length;
        bool isMultiPlayer = GameManager.Instance.IsMultiplayer;

        // 4 tanks and 1 folder also counts, (if multiplayer, 6 tanks can be on screen)
        if (next < 20 && (tankCount < 5 && !isMultiPlayer || tankCount < 7 && isMultiPlayer))
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
        anim.SetBool("spawn", false);

        Transform t = null;

        if (tanks[next] == 1)
        {
            t = Instantiate(easyTank, transform.position, easyTank.rotation , generatedEnemyFolder);
        }
        else if (tanks[next] == 2)
        {
            t = Instantiate(fastTank, transform.position, fastTank.rotation, generatedEnemyFolder);
        }
        else if (tanks[next] == 3)
        {
            t = Instantiate(mediumTank, transform.position, mediumTank.rotation, generatedEnemyFolder);
        }
        else if (tanks[next] == 4)
        {
            t = Instantiate(strongTank, transform.position, strongTank.rotation, generatedEnemyFolder);
            t.SendMessage("SetLives", 5);
        }

        PushPosition();

        // every four enemies, one get bonus 
        if ((next + 1) % 4 == 0)
        {
            t.SendMessage("SetBonus", r.Next(50) % 5 + 1);
        }

        next++;
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
