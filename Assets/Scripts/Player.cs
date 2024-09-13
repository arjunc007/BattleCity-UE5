using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private Shooting _shooting;
    [SerializeField] private PlayerMovement _movement;
    public int level = 1;
    public int lives = 3;
    public Transform bulletWeak;
    public Transform bulletFast;
    public Transform bulletStrong;

    public Animator shieldAnim;

    public int shieldTime = 0;

    private Animator _anim;

    void Start()
    {
        level = 1;
        _anim = GetComponent<Animator>();
        _shooting.Initialize(this);
        GameManager.Instance.RegisterPlayer(this);
    }

    void Update()
    {
        if (level == 1)
        {
            _shooting.SetBullet(bulletWeak);
            _shooting.SetMaxBullets(1);
        }
        else if (level == 2)
        {
            _shooting.SetBullet(bulletFast);
            _shooting.SetMaxBullets(1);
        }
        else if (level == 3)
        {
            _shooting.SetBullet(bulletFast);
            _shooting.SetMaxBullets(2);
        }
        else if (level == 4)
        {
            _shooting.SetBullet(bulletStrong);
            _shooting.SetMaxBullets(2);
        }

        _anim.SetInteger("level", level);
    }

    // Bonus taken
    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<PowerUp>(out var powerup))
        {
            int bonus = Mathf.RoundToInt(powerup.GetComponent<Animator>().GetFloat("bonus"));

            if (bonus == 1)
            {
                level++;
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
        if (shieldTime <= 0)
        {
            shieldAnim.SetBool("isOn", false);
            _anim.SetBool("shield", false);
        }
    }

    // message receiver from "load map"
    public void SetLevel(int level)
    {
        this.level = level;
    }

    // message receiver from "BulletTankDestroy"
    public void Hit()
    {
        lives--;
    }

    // message receiver from "BulletTankDestroy"
    public void GetLives(ArgsPointer<int> pointer)
    {
        pointer.Args = new int[] { lives };
    }

    // message receiver from "load map"
    public void SetLives(int lives)
    {
        this.lives = lives;
    }

    public void Reset()
    {
        _movement.ResetPosition();
        _anim.SetBool("hit", false);
        _shooting.SetShooting(false);
        SetShield(6);
    }

    public void ResetPosition()
    {
        _movement.ResetPosition();
    }
}
