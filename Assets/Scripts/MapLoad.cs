using UnityEngine;

public class MapLoad : MonoBehaviour
{
    public Transform generatedWallFolder;
    public Transform generatedBulletFolder;
    public Transform spawnLocation;
    public Transform powerUpPrefab;

    public int level;

    public Transform wall;
    public Transform iron;
    public Transform bush;
    public Transform ice;
    public Transform water;

    public AudioClip levelStarting;

    private bool _multiplayer = false;
    private int currentLevel;

    public void StartGame(bool multiplayer)
    {
        _multiplayer = multiplayer;
        LoadMap(level);
    }

    public void LoadMap(bool won)
    {
        if (won)
        {
            LoadMap(++level);
        }
    }

    public void LoadMap(int lev)
    {
        currentLevel = lev;
        level = lev;

        // Reset data
        DeleteChilds(generatedWallFolder);
        DeleteChilds(generatedBulletFolder);

        //player1.SendMessage("ResetPosition");
        //player1.GetComponent<Animator>().SetBool("hit", false);
        //player1.SendMessage("SetShooting", false);
        //player1.SendMessage("SetShooting", false);
        //player1.SendMessage("SetShield", 6);

        //if (_multiplayer)
        //{
        //    player2.SendMessage("ResetPosition");
        //    player2.GetComponent<Animator>().SetBool("hit", false);
        //    player2.SendMessage("SetShooting", false);
        //    player2.SendMessage("SetShooting", false);
        //    player2.SendMessage("SetShield", 6);
        //}
        //else
        //{
        //    player2.GetComponent<Transform>().position = new Vector3(0, -155, 0);
        //}


        // Enemy spawning reset
        GameManager.Instance.EnemySpawner.Reset();

        // Read map file
        string[] m = System.IO.File.ReadAllLines(@"Assets/Maps/map" + currentLevel + ".txt");
        GenerateObjects(m);

        // powerUp reset
        GameManager.Instance.PowerUp.Reset();

        // play a sound
        AudioManager.Instance.PlayOneShot(levelStarting);

    }

    private void DeleteChilds(Transform folder)
    {
        Transform[] ts = folder.GetComponentsInChildren<Transform>();

        foreach (var t in ts)
        {
            if (!t.gameObject.name.Contains("Generated"))
            {
                Destroy(t.gameObject);
            }
        }
    }

    private void GenerateObjects(string[] m)
    {
        for (int i = 0; i < 26; i++)
        {
            for (int j = 0; j < 26; j++)
            {
                Transform t = null;
                if (m[i][j] == 'o')
                {
                    t = Instantiate(wall, new Vector3(j - 13, 13 - (i + 1), 0), wall.rotation);
                }
                else if (m[i][j] == 'Q')
                {
                    t = Instantiate(iron, new Vector3(j - 13, 13 - (i + 1), 0), wall.rotation);
                }
                else if (m[i][j] == 'b')
                {
                    t = Instantiate(bush, new Vector3(j - 13, 13 - (i + 1), 0), wall.rotation);
                }
                else if (m[i][j] == 'i')
                {
                    t = Instantiate(ice, new Vector3(j - 13, 13 - (i + 1), 0), wall.rotation);
                }
                else if (m[i][j] == 'w')
                {
                    t = Instantiate(water, new Vector3(j - 13, 13 - (i + 1), 0), wall.rotation);
                }
                if (m[i][j] != '.')
                {
                    t.parent = generatedWallFolder;
                }
            }
        }
    }
}
