using System.Collections;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.UI.GridLayoutGroup;

public class Enemy : NetworkBehaviour, ITank
{
    public Transform bullet;
    public float MaxSpeed = 0.10f;
    public NetworkVariable<int> bonus = new NetworkVariable<int>();
    private NetworkVariable<int> lives = new NetworkVariable<int>(1);

    private Animator _anim;

    private NetworkVariable<float> input_x = new NetworkVariable<float>(0);
    private NetworkVariable<float> input_y = new NetworkVariable<float>(-1);
    private bool isMoving;
    private bool changingPos;

    public int AlreadyShot = 0;
    private int maxBulletsAtOneTime = 1;
    private bool CanShoot => maxBulletsAtOneTime > AlreadyShot;

    private System.Random r = new System.Random();

    public override void OnNetworkSpawn()
    {
        //transform.parent = GameManager.Instance.EnemyHolder;
    }

    void Start()
    {
        _anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (!GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (CanShoot && !_anim.GetBool("hit"))
        {
            AlreadyShot++;
            StartCoroutine(DelayShootingFor(0.2f));
        }

        _anim.SetFloat("input_x", input_x.Value);
        _anim.SetFloat("input_y", input_y.Value);
        _anim.SetInteger("bonus", bonus.Value);
        _anim.SetInteger("lives", lives.Value);

        _anim.SetBool("isMoving", isMoving);
    }

    public void FixedUpdate()
    {
        if (!GameManager.Instance.IsPlaying)
        {
            return;
        }

        if (!_anim.GetBool("hit"))
        {
            // AI

            if (!changingPos)
            {
                StartCoroutine(ChangePostition());
            }

            //Movement
            if (input_x.Value != 0 || input_y.Value != 0)
            {
                // Move object
                isMoving = true;
                transform.position += new Vector3(MaxSpeed * input_x.Value, MaxSpeed * input_y.Value, 0);

                // Align to cells
                if (input_x.Value == 0)
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, 0);
                }
                if (input_y.Value == 0)
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0);
                }
            }
            else
            {
                isMoving = false;
            }

            _anim.SetFloat("input_x", input_x.Value);
            _anim.SetFloat("input_y", input_y.Value);

        }
    }

    private IEnumerator DelayShootingFor(float time)
    {
        yield return new WaitForSeconds(time);
        if (!_anim.GetBool("hit"))
        {
            LaunchBulletRpc();
        }
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void LaunchBulletRpc()
    {
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
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsServer)
        {
            if ((transform.position.y < -11.5f && input_y.Value < 0) || (transform.position.y > 11.5f && input_y.Value > 0))
            {
                input_x.Value = (r.Next(50) % 3) - 1;
                if (input_x.Value == 0)
                {
                    input_y.Value = -input_y.Value;
                }
                else
                {
                    input_y.Value = 0;
                }
            }
            else if ((transform.position.x < -11.5f && input_x.Value < 0) || (transform.position.x > 11.5f && input_x.Value > 0))
            {
                input_y.Value = (r.Next(50) % 3) - 1;
                if (input_y.Value == 0)
                {
                    input_x.Value = -input_x.Value;
                }
                else
                {
                    input_x.Value = 0;
                }
            }
        }

        _anim.SetFloat("input_x", input_x.Value);
        _anim.SetFloat("input_y", input_y.Value);
    }

    public void SetMovingAnim(bool value)
    {
        _anim.SetBool("isMoving", value);
    }

    public void Hit()
    {
        _anim.SetBool("hit", true);
    }

    private IEnumerator ChangePostition()
    {
        changingPos = true;

        yield return new WaitForSeconds(3f);

        SetRandomValues();

        changingPos = false;
    }

    private void SetRandomValues()
    {
        if (!IsServer)
        {
            return;
        }

        input_x.Value = (r.Next(50) % 3) - 1;
        input_y.Value = (r.Next(50) % 3) - 1;

        if ((input_x.Value == 0 && input_y.Value == 0) || (input_y.Value != 0 && input_x.Value != 0))
        {
            SetRandomValues();
        }
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
    public void SetBonus(int bonus)
    {
        if (IsServer)
        {
            this.bonus.Value = bonus;
        }
        _anim.SetInteger("bonus", bonus);
    }

    public int GetLives()
    {
        return lives.Value;
    }

    //Message receiver from "BulletTankDestroy"
    public void SetLives(int lives)
    {
        if (IsServer)
        {
            this.lives.Value = lives;
        }
    }

    public void Destroy()
    {
        if (IsServer)
        {
            Destroy(gameObject);
        }
        else
        {
            DestroyEnemyRpc();
        }
    }

    [Rpc(SendTo.Server)]
    public void DestroyEnemyRpc()
    {
        Destroy(gameObject);
    }
}
