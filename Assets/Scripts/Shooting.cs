using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour
{
    public Transform bullet;
    public bool isNPC;
    public int player;
    public AudioClip shotSound;
    public AudioClip gameOver;

    private Animator anim;
    InputManager input;
    private int alreadyShot = 0;
    private int maxBulletsAtOneTime = 1;

    private bool CanShoot => maxBulletsAtOneTime > alreadyShot;

    void Start()
    {
        input = InputManager.Instance;
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if (CanShoot && !anim.GetBool("hit") &&
            ((!isNPC && input.Fire)
            || isNPC))
        {
            alreadyShot++;
            if (isNPC)
            {
                StartCoroutine(DelayShootingFor(0.2f));
            }
            else
            {
                LaunchBulletRpc();
            }
        }
    }

    private IEnumerator DelayShootingFor(float time)
    {
        yield return new WaitForSeconds(time);
        if (!anim.GetBool("hit"))
        {
            LaunchBulletRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void LaunchBulletRpc()
    {
        if (!isNPC)
        {
            Debug.Log($"{OwnerClientId} Fire");
        }
        float x = anim.GetFloat("input_x");
        float y = anim.GetFloat("input_y");

        // Calculate rotation angle
        float r = 0;
        if (x == 0 && y == 1)
        {
            r = 270;
        }

        if (x == 1 && y == 0)
        {
            r = 180;
        }

        if (x == 0 && y == -1)
        {
            r = 90;
        }

        if (x == -1 && y == 0)
        {
            r = 0;
        }

        // Creates new bullet
        Vector3 pos = transform.position + new Vector3(x, y, 0);

        Transform newBullet = Instantiate(bullet, pos, transform.rotation, GameManager.Instance.BulletHolder);
        newBullet.eulerAngles += new Vector3(0, 0, r);

        // Passes variables x and y
        Animator a = newBullet.GetComponent<Animator>();
        a.SetFloat("input_x", x);
        a.SetFloat("input_y", y);

        a.gameObject.SendMessage("SetShooterTank", transform);

        // plays a sound

        if (!isNPC)
        {
            AudioManager.Instance.PlayOneShot(shotSound);
        }
    }

    //Message receiver from "Bullet"
    public void SetShooting(bool shouldAddBullet)
    {
        if (shouldAddBullet)
        {
            alreadyShot++;
        }
        else
        {
            alreadyShot--;
        }

        if (alreadyShot < 0)
        {
            alreadyShot = 0;
        }

        if (alreadyShot > maxBulletsAtOneTime)
        {
            alreadyShot = maxBulletsAtOneTime;
        }
    }

    //Message receiver from "Player"
    public void SetBullet(Transform bullet)
    {
        this.bullet = bullet;
    }
    //Message receiver from "Player"
    public void SetMaxBullets(int max)
    {
        maxBulletsAtOneTime = max;
    }


    public void Destroy()
    {
        if (isNPC)
        {
            Destroy(gameObject);
        }
        else if (!isNPC)
        {
            transform.position = new Vector3(120, 20, 0);
            transform.SendMessage("SetLevel", 1);

            ArgsPointer<int> pointer = new ArgsPointer<int>();
            transform.SendMessage("GetLives", pointer);

            if (pointer.Args[0] <= 0)
            {
                StartCoroutine(FinishGameAfter(3));
            }
            else
            {
                this.DoAfter(1.5f, () =>
                {
                    transform.SendMessage("ResetPosition");
                    transform.GetComponent<Animator>().SetBool("hit", false);
                    alreadyShot = 0;
                    SendMessage("SetShield", 6);
                });
            }
        }
    }

    IEnumerator FinishGameAfter(float time)
    {
        yield return new WaitForSeconds(time / 3);
        gameOver.NotNull((t) => AudioManager.Instance.PlayOneShot(t));
        yield return new WaitForSeconds(time / 3 * 2);

        anim.SetBool("hit", false);
        GameManager.Instance.Eagle.SendMessage("FinishGame");
    }
}
