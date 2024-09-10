using UnityEngine;
using System.Collections;
using System;

public class PowerUp : MonoBehaviour {

    public AudioClip powerUpTaken;
    public AudioClip powerUpShowUp;
    public int bonus = 1;

    private System.Random r;
    public int freezeTime = 0;

    void Start()
    {
        r = new System.Random();

        Reset();
    }

    void Update()
    {
        gameObject.GetComponent<Animator>().SetFloat("bonus", bonus);

        Transform[] ts = GameManager.Instance.EnemyHolder.GetComponentsInChildren<Transform>();

        if (freezeTime > 0)
        {
            foreach (var t in ts)
            {
                if (!t.gameObject.name.Contains("Generated"))
                {
                    t.GetComponent<Animator>().SetBool("isMoving", false);
                }
            }
        }
    }

    // Message receiver from "MapLoad"
    public void Reset()
    {
        transform.position = new Vector3(0, 100, 0);
        freezeTime = -100;
    }

    // Message receiver from "Player"
    public void HidePowerUp()
    {
        AudioManager.Instance.PlayOneShot(powerUpTaken);
        transform.position = new Vector3(0, 100, 0);
    }

    // Message receiver from "BulletTankDestroy"
    public void ShowPowerUp(int bonus)
    {
        if (bonus > 0)
        {
            this.bonus = bonus;
            AudioManager.Instance.PlayOneShot(powerUpShowUp);
            transform.position = new Vector3(GetRanCoord(), GetRanCoord(), 0);
        }
    }

    // Message receiver from "Player" (PowerUp)
    public void DestroyAllTanks()
    {
        foreach (var enemy in GameManager.Instance.Enemies)
        {
            enemy.Hit();
        }
    }

    // Message receiver from "Player" (PowerUp)
    public void FreezeTime()
    {
        if (freezeTime <= 0)
        {
            freezeTime = 15;
            StartCoroutine(FreezeEnumerator());
        }

        freezeTime = 15;
    }

    IEnumerator FreezeEnumerator()
    {
        while (freezeTime > 0)
        {
            yield return new WaitForSeconds(1);
            freezeTime--;
        }
        if (freezeTime <= 0)
        {
            Transform[] ts = GameManager.Instance.EnemyHolder.GetComponentsInChildren<Transform>();
            foreach (var t in ts)
            {
                if (!t.gameObject.name.Contains("Generated"))
                {
                    t.GetComponent<Animator>().SetBool("isMoving", true);
                }
            }
        }
    }

    private float GetRanCoord()
    {
        return (r.Next(-120, 120) / 10f);
    }
}
