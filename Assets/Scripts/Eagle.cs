using UnityEngine;
using System.Collections;
using System;

public class Eagle : MonoBehaviour {

    public AudioSource eagleDestroy;
    public AudioSource gameOver;

    private Animator _animator;

    private void Start()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        Transform other = collider.GetComponent<Transform>();

        if (other.name.Contains("Bullet") && !other.GetComponent<Animator>().GetBool("hit") &&
            !_animator.GetBool("isDestroyed"))
        {
            other.GetComponent<Animator>().SetBool("hit", true);
            SetDestroyed(true);
            eagleDestroy.Play();
            
            this.DoAfter(1, () => gameOver.Play());
        }
    }

    public void SetDestroyed(bool destroy)
    {
        _animator.SetBool("isDestroyed", destroy);
    }
}
