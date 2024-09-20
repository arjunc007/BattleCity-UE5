using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Shooting : NetworkBehaviour
{
    public Transform bullet;
    public int player;
    public AudioClip shotSound;
    public AudioClip gameOver;
    public int AlreadyShot = 0;

    private Player _owner;
    private Animator anim;
    InputManager input;
    private int maxBulletsAtOneTime = 1;

    private bool CanShoot => maxBulletsAtOneTime > AlreadyShot;

    public void Initialize(Player owner)
    {
        if (owner == null)
        {
            _owner = owner;
        }
        else
        {
            Debug.Log("Script already owned by " + owner.gameObject.name);
        }
    }

    void Start()
    {
        input = InputManager.Instance;
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (!IsOwner || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (CanShoot && !anim.GetBool("hit") &&
            ((!_owner.IsNPC && input.Fire)
            || _owner.IsNPC))
        {
            AlreadyShot++;
            if (_owner.IsNPC)
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
        if (!_owner.IsNPC)
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

        Bullet newBullet = Instantiate(bullet, pos, Quaternion.Euler(0.0f, 0.0f, r), GameManager.Instance.BulletHolder).GetComponent<Bullet>();

        // Passes variables x and y
        Animator a = newBullet.GetComponent<Animator>();
        a.SetFloat("input_x", x);
        a.SetFloat("input_y", y);

        newBullet.SetShooterTank(transform);

        // plays a sound

        if (!_owner.IsNPC)
        {
            AudioManager.Instance.PlayOneShot(shotSound);
        }
    }

    //Message receiver from "Bullet"
    public void SetShooting(bool shouldAddBullet)
    {
        if (shouldAddBullet)
        {
            AlreadyShot++;
        }
        else
        {
            AlreadyShot--;
        }

        if (AlreadyShot < 0)
        {
            AlreadyShot = 0;
        }

        if (AlreadyShot > maxBulletsAtOneTime)
        {
            AlreadyShot = maxBulletsAtOneTime;
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

    [Rpc(SendTo.Server)]
    public void DestroyEnemyRpc()
    {
        Destroy(gameObject);
    }

    public IEnumerator FinishGameAfter(float time)
    {
        yield return new WaitForSeconds(time / 3);
        gameOver.NotNull((t) => AudioManager.Instance.PlayOneShot(t));
        yield return new WaitForSeconds(time / 3 * 2);

        anim.SetBool("hit", false);
        GameManager.Instance.FinishGame();
    }
}
