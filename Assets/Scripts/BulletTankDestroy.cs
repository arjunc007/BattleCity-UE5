using UnityEngine;

public class BulletTankDestroy : MonoBehaviour
{
    [SerializeField] private Animator _bulletAnim;
    public bool isFriendly;
    public AudioClip tankDestroy;
    public AudioClip ironHit;


    public void OnTriggerEnter2D(Collider2D collider)
    {
        Transform tank = collider.GetComponent<Transform>();
        Animator tankAnim = collider.GetComponent<Animator>();

        collider.TryGetComponent<Enemy>(out var enemy);
        collider.TryGetComponent<Player>(out var player);

        // Show power up if was red
        if (enemy != null && isFriendly
            && !_bulletAnim.GetBool("hit") && !tankAnim.GetBool("hit"))
        {
            GameManager.Instance.PowerUp.ShowPowerUp(tankAnim.GetInteger("bonus"));
            enemy.SetBonus(0);
        }

        // Destroy tank and bullet
        if (((tank.name.Contains("Tank") && isFriendly) || (tank.name.Contains("Player") && !isFriendly))
            && !_bulletAnim.GetBool("hit") && !tankAnim.GetBool("hit"))
        {
            _bulletAnim.SetBool("hit", true);

            if (!tank.name.Contains("Player") || !tankAnim.GetBool("shield"))
            {
                // player
                if (tank.name.Contains("Player"))
                {
                    tank.SendMessage("Hit");
                    tankAnim.SetBool("hit", true);
                    AudioManager.Instance.PlayOneShot(tankDestroy);
                }
                // not player
                else if (tankAnim.GetInteger("lives") <= 1)
                {
                    tankAnim.SetBool("hit", true);
                    GameManager.Instance.EnemySpawner.RemoveEnemy(collider.GetComponent<Enemy>());
                    AudioManager.Instance.PlayOneShot(tankDestroy);
                }
                else
                {
                    tank.SendMessage("SetLives", tankAnim.GetInteger("lives") - 1);
                    AudioManager.Instance.PlayOneShot(ironHit);
                }
            }
        }
    }
}
