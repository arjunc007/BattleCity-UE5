using UnityEngine;

public class Eagle : MonoBehaviour
{

    public AudioClip eagleDestroy;
    public AudioClip gameOver;

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
            AudioManager.Instance.PlayOneShot(eagleDestroy);

            this.DoAfter(1, () => AudioManager.Instance.PlayOneShot(gameOver));
        }
    }

    public void SetDestroyed(bool destroy)
    {
        _animator.SetBool("isDestroyed", destroy);
    }
}
