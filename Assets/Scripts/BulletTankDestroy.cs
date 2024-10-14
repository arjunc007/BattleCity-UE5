using UnityEngine;

public class BulletTankDestroy : MonoBehaviour
{
    [SerializeField] private Animator _bulletAnim;
    public bool isFriendly;
    public AudioClip tankDestroy;
    public AudioClip ironHit;


    public void OnTriggerEnter2D(Collider2D collider)
    {
        collider.TryGetComponent<ITank>(out var tank);
        Animator tankAnim = collider.GetComponent<Animator>();

        collider.TryGetComponent<Enemy>(out var enemy);
        collider.TryGetComponent<Player>(out var player);

        // Show power up if was red
        if (enemy != null && isFriendly
            && !_bulletAnim.GetBool("hit") && !tankAnim.GetBool("hit"))
        {
            GameManager.Instance.PowerUp.ShowPowerUp(enemy.bonus.Value);
            enemy.SetBonus(0);
        }

        // Destroy tank and bullet
        if (((enemy != null && isFriendly) || (player != null && !isFriendly))
            && !_bulletAnim.GetBool("hit") && !tankAnim.GetBool("hit"))
        {
            _bulletAnim.SetBool("hit", true);

            if (player != null || !tankAnim.GetBool("shield"))
            {
                // player
                if (player != null)
                {
                    player.Hit();
                    AudioManager.Instance.PlayOneShot(tankDestroy);
                }
                // not player
                else if (tank.GetLives() <= 1)
                {
                    tankAnim.SetBool("hit", true);
                    GameManager.Instance.EnemySpawner.RemoveEnemy(collider.GetComponent<Enemy>());
                    AudioManager.Instance.PlayOneShot(tankDestroy);
                }
                else
                {
                    tank.SetLives(tank.GetLives() - 1);
                    AudioManager.Instance.PlayOneShot(ironHit);
                }
            }
        }
    }
}
