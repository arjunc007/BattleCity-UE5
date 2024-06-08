﻿using UnityEngine;
using System.Collections;
using System;

public class Enemy : MonoBehaviour
{
    public float MaxSpeed = 0.10f;
    public int bonus = 0;
    public int lives = 1;

    private Animator anim;

    private float input_x = 0;
    private float input_y = -1;
    private bool isMoving;
    private bool changingPos;

    private System.Random r = new System.Random();

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        anim.SetFloat("input_x", input_x);
        anim.SetFloat("input_y", input_y);
        anim.SetInteger("bonus", bonus);
        anim.SetInteger("lives", lives);

        anim.SetBool("isMoving", isMoving);
    }

    public void FixedUpdate()
    {
        if (!anim.GetBool("hit"))
        {
            // AI
            
            if (!changingPos)
            {
                StartCoroutine(ChangePostition());
            }
            
            //Movement
            if (input_x != 0 || input_y != 0)
            {
                // Move object
                isMoving = true;
                transform.position += new Vector3(MaxSpeed * input_x, MaxSpeed * input_y, 0);

                // Align to cells
                if (input_x == 0)
                {
                    transform.position = new Vector3(Mathf.Round(transform.position.x), transform.position.y, 0);
                }
                if (input_y == 0)
                {
                    transform.position = new Vector3(transform.position.x, Mathf.Round(transform.position.y), 0);
                }
            }
            else
            {
                isMoving = false;
            }

            anim.SetFloat("input_x", input_x);
            anim.SetFloat("input_y", input_y);

        }
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (transform.position.y < -11.5f && input_y < 0 || transform.position.y > 11.5f && input_y > 0)
        {
            input_x = r.Next(50) % 3 - 1;
            if (input_x == 0) input_y = -input_y;
            else input_y = 0;
        }
        else if (transform.position.x < -11.5f && input_x < 0 || transform.position.x > 11.5f && input_x > 0)
        {
            input_y = r.Next(50) % 3 - 1;
            if (input_y == 0) input_x = -input_x;
            else input_x = 0;
        }

        anim.SetFloat("input_x", input_x);
        anim.SetFloat("input_y", input_y);
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
        input_x = r.Next(50) % 3 - 1;
        input_y = r.Next(50) % 3 - 1;

        if ((input_x == 0 && input_y == 0) || (input_y != 0 && input_x != 0))
        {
            SetRandomValues();
        }
    }

    //Message receiver from "EnemySpawning"
    public void SetBonus(int bonus)
    {
        this.bonus = bonus;
    }

    //Message receiver from "BulletTankDestroy"
    public void SetLives(int lives)
    {
        this.lives = lives;
    }
}
