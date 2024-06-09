using UnityEngine;

public class EnemySpawning : MonoBehaviour
{

    private int[] tanks;
    private Animator anim;
    System.Random r;

    public int next;
    public Transform eagle;
    public Transform generatedEnemyFolder;
    public Transform easyTank;
    public Transform fastTank;
    public Transform mediumTank;
    public Transform strongTank;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        r = new System.Random();

        tanks = new int[20];

        for (int i = 0; i < 20; i++)
        {
            tanks[i] = r.Next(50) % 4 + 1;
        }

        Reset();
    }

    public void Reset()
    {
        transform.position = new Vector3(-12, 12, 0);
        next = 0;
    }

    void Update()
    {
        return;
        if (!GameManager.Instance.IsPlaying)
        {
            return;
        }
        int tankCount = generatedEnemyFolder.GetComponentsInChildren<Transform>().Length;
        bool isMultiPlayer = GameManager.Instance.IsMultiplayer;

        // 4 tanks and 1 folder also counts, (if multiplayer, 6 tanks can be on screen)
        if (next < 20 && (tankCount < 5 && !isMultiPlayer || tankCount < 7 && isMultiPlayer))
        {
            anim.SetBool("spawn", true);
        }
    }

    // Called from animation event
    private void SpawnEnemy()
    {
        return;
        anim.SetBool("spawn", false);

        Transform t = null;

        if (tanks[next] == 1)
        {
            t = Instantiate(easyTank, transform.position, easyTank.rotation) as Transform;
        }
        else if (tanks[next] == 2)
        {
            t = Instantiate(fastTank, transform.position, fastTank.rotation) as Transform;
        }
        else if (tanks[next] == 3)
        {
            t = Instantiate(mediumTank, transform.position, mediumTank.rotation) as Transform;
        }
        else if (tanks[next] == 4)
        {
            t = Instantiate(strongTank, transform.position, strongTank.rotation) as Transform;
            t.SendMessage("SetLives", 5);
        }

        PushPosition();

        t.parent = generatedEnemyFolder;

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
}
