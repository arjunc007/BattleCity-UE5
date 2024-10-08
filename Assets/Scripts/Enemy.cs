﻿using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Enemy : NetworkBehaviour
{
    public float MaxSpeed = 0.10f;
    public NetworkVariable<int> bonus = new NetworkVariable<int>();
    private NetworkVariable<int> lives = new NetworkVariable<int>(1);

    private Animator _anim;

    private NetworkVariable<float> input_x = new NetworkVariable<float>(0);
    private NetworkVariable<float> input_y = new NetworkVariable<float>(-1);
    private bool isMoving;
    private bool changingPos;

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

    public void SetBonus(int bonus)
    {
        if (IsServer)
        {
            this.bonus.Value = bonus;
        }
    }

    //Message receiver from "BulletTankDestroy"
    public void SetLives(int lives)
    {
        if (IsServer)
        {
            this.lives.Value = lives;
        }
    }
}
