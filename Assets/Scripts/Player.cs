using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Player : NetworkBehaviour, ITank
{
    public Transform bullet;
    public AudioClip shotSound;
    public AudioClip gameOver;
    [SerializeField] private PlayerMovement _movement;
    public int level = 1;
    public int lives = 3;
    public Transform bulletWeak;
    public Transform bulletFast;
    public Transform bulletStrong;

    public Animator shieldAnim;

    public bool IsShieldActive => shieldTime > 0;

    private int shieldTime = 0;

    private Animator _anim;

    private InputManager input;

    public int AlreadyShot = 0;
    private int maxBulletsAtOneTime = 1;
    private bool CanShoot => maxBulletsAtOneTime > AlreadyShot;

    void Start()
    {
        input = InputManager.Instance;
        _anim = GetComponent<Animator>();
        SetLevel(1);
        GameManager.Instance.RegisterPlayer(this);
    }

    void Update()
    {
        if (!IsOwner || !GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (CanShoot && !_anim.GetBool("hit") && input.Fire)
        {
            AlreadyShot++;

            LaunchBulletRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void LaunchBulletRpc()
    {
        Debug.Log($"{OwnerClientId} Fire");

        float x = _anim.GetFloat("input_x");
        float y = _anim.GetFloat("input_y");

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

        newBullet.SetShooterTank(this);

        // plays a sound
        AudioManager.Instance.PlayOneShot(shotSound);
    }

    // Bonus taken
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PowerUp>(out var powerup))
        {
            int bonus = Mathf.RoundToInt(powerup.GetComponent<Animator>().GetFloat("bonus"));

            if (bonus == 1)
            {
                SetLevel(level + 1);
            }
            if (bonus == 2)
            {
                powerup.DestroyAllTanks();
            }
            if (bonus == 3)
            {
                powerup.FreezeTime();
            }
            if (bonus == 4)
            {
                SetShield(15);
            }
            if (bonus == 5)
            {
                lives++;
            }

            powerup.HidePowerUp();
        }
    }

    // Message receiver from "Map load" and/ "this"
    public void SetShield(int time)
    {
        if (shieldTime <= 0)
        {
            shieldTime = time;
            StartCoroutine(ShieldEnumerator());
        }

        shieldTime = time;
        shieldAnim.SetBool("isOn", true);
        _anim.SetBool("shield", true);
    }

    IEnumerator ShieldEnumerator()
    {
        while (shieldTime > 0)
        {
            yield return new WaitForSeconds(1);
            shieldTime--;
        }

        var shielded = shieldTime > 0;
        shieldAnim.SetBool("isOn", shielded);
        _anim.SetBool("shield", shielded);
    }

    // message receiver from "load map"
    public void SetLevel(int level)
    {
        this.level = level;

        if (level == 1)
        {
            SetBullet(bulletWeak);
            SetMaxBullets(1);
        }
        else if (level == 2)
        {
            SetBullet(bulletFast);
            SetMaxBullets(1);
        }
        else if (level == 3)
        {
            SetBullet(bulletFast);
            SetMaxBullets(2);
        }
        else if (level == 4)
        {
            SetBullet(bulletStrong);
            SetMaxBullets(2);
        }

        _anim.SetInteger("level", level);
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

    // message receiver from "BulletTankDestroy"
    public void Hit()
    {
        lives--;
        _anim.SetBool("hit", true);
    }

    // message receiver from "BulletTankDestroy"
    public void GetLives(ArgsPointer<int> pointer)
    {
        pointer.Args = new int[] { lives };
    }

    public int GetLives()
    {
        return lives;
    }

    // message receiver from "load map"
    public void SetLives(int lives)
    {
        this.lives = lives;
    }

    public void Destroy()
    {
        ResetPosition();
        SetLevel(1);

        ArgsPointer<int> pointer = new();
        GetLives(pointer);

        if (pointer.Args[0] <= 0)
        {
            StartCoroutine(FinishGameAfter(3));
        }
        else
        {
            this.DoAfter(1.5f, () =>
            {
                ResetPosition();
                _anim.SetBool("hit", false);
                AlreadyShot = 0;
                SetShield(6);
            });
        }
    }

    public void Reset()
    {
        _movement.ResetPosition();
        _anim.SetBool("hit", false);
        SetShooting(false);
        SetShield(6);
    }

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

    public void ResetPosition()
    {
        _movement.ResetPosition();
    }

    public IEnumerator FinishGameAfter(float time)
    {
        yield return new WaitForSeconds(time / 3);
        gameOver.NotNull((t) => AudioManager.Instance.PlayOneShot(t));
        yield return new WaitForSeconds(time / 3 * 2);

        _anim.SetBool("hit", false);
        GameManager.Instance.FinishGame();
    }
}
