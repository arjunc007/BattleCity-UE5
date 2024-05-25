using System.Collections;
using UnityEngine;

public class Shooting : MonoBehaviour
{
    public Transform bullet;
    public Transform eagle;
    public bool isNPC;
    public int player;
    public AudioClip shotSound;
    public AudioClip gameOver;

    private Transform tank;
    private Animator anim;
    InputManager input;
    private int alreadyShot = 0;
    private int maxBulletsAtOneTime = 1;

    void Start()
    {
        input = InputManager.Instance;
        tank = gameObject.GetComponent<Transform>();
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (canShoot() && !anim.GetBool("hit") &&
            (((!isNPC && player == 1 && input.Fire) /*|| 
            (!isNPC && player == 2 && Input.GetKeyDown(KeyCode.L))*/)
            || isNPC))
        {
            alreadyShot++;
            if (isNPC) StartCoroutine(DelayShootingFor(0.2f));
            else LaunchBullet();
        }
    }

    private IEnumerator DelayShootingFor(float time)
    {
        yield return new WaitForSeconds(time);
        if (!anim.GetBool("hit")) LaunchBullet();
    }

    private void LaunchBullet()
    {
        float x = anim.GetFloat("input_x");
        float y = anim.GetFloat("input_y");

        // Calculate rotation angle
        float r = 0;
        if (x == 0 && y == 1) r = 270;
        if (x == 1 && y == 0) r = 180;
        if (x == 0 && y == -1) r = 90;
        if (x == -1 && y == 0) r = 0;

        // Creates new bullet
        Vector3 pos = tank.position + new Vector3(x, y, 0);

        Transform newBullet = Instantiate(bullet, pos, tank.rotation, GameManager.Instance.BulletHolder);
        newBullet.eulerAngles += new Vector3(0, 0, r);

        // Passes variables x and y
        Animator a = newBullet.GetComponent<Animator>();
        a.SetFloat("input_x", x);
        a.SetFloat("input_y", y);

        a.gameObject.SendMessage("SetShooterTank", tank);

        // plays a sound

        if (!isNPC)
        {
            AudioManager.Instance.PlayOneShot(shotSound);
        }
    }

    //Message receiver from "Bullet"
    public void SetShooting(bool shouldAddBullet)
    {
        if (shouldAddBullet) alreadyShot++;
        else alreadyShot--;
        if (alreadyShot < 0) alreadyShot = 0;
        if (alreadyShot > maxBulletsAtOneTime) alreadyShot = maxBulletsAtOneTime;
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
            tank.position = new Vector3(120, 20, 0);
            tank.SendMessage("SetLevel", 1);

            ArgsPointer<int> pointer = new ArgsPointer<int>();
            tank.SendMessage("GetLives", pointer);

            if (pointer.Args[0] <= 0)
            {
                StartCoroutine(FinishGameAfter(3));
            }
            else
            {
                this.DoAfter(1.5f, () =>
                {
                    tank.SendMessage("ResetPosition");
                    tank.GetComponent<Animator>().SetBool("hit", false);
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
        eagle.SendMessage("FinishGame");
    }

    private bool canShoot()
    {
        return maxBulletsAtOneTime > alreadyShot;
    }
}
