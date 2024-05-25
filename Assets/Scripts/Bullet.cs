using UnityEngine;

public class Bullet : MonoBehaviour
{

    private Animator anim;
    private Transform trans;
    private Transform tank;

    private float input_x;
    private float input_y;

    public bool isFriendly;
    public float speed;

    public AudioClip brickHit;
    public AudioClip ironHit;

    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        trans = gameObject.GetComponent<Transform>();

        input_x = anim.GetFloat("input_x");
        input_y = anim.GetFloat("input_y");
    }

    void FixedUpdate()
    {
        bool hit = anim.GetBool("hit");

        if (!hit)
        {
            trans.position += new Vector3(speed * input_x, speed * input_y, 0);
        }
    }

    private void DestroyAfterAnimationFinishes()
    {
        Destroy(gameObject);
    }

    // message receiver from "Shooting" or "Enemy"
    void SetShooterTank(Transform tank)
    {
        this.tank = tank;
    }

    // message receiver from "BulletWallDestroy" or "BulletIronDestroy"
    void PlayBrickHitSound()
    {
        if (isFriendly)
        {
            AudioManager.Instance.PlayOneShot(brickHit);
        }
    }

    // message receiver from "BulletWallDestroy" or "BulletIronDestroy"
    void PlayIronHitSound()
    {
        if (isFriendly)
        {
            AudioManager.Instance.PlayOneShot(ironHit);
        }
    }

    public void OnTriggerEnter2D(Collider2D collider)
    {
        Transform other = collider.GetComponent<Transform>();

        if (other.name.Contains("Bullet"))
        {
            Destroy(gameObject);
            Destroy(other.gameObject);
        }
    }

    void OnDestroy()
    {
        if (tank != null) tank.NotNull((t) => t.gameObject.SendMessage("SetShooting", false));
    }
}
