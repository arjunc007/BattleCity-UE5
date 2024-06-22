using UnityEngine;

public class BulletTankDestroy : MonoBehaviour
{
    public bool isFriendly;
    public AudioClip tankDestroy;
    public AudioClip ironHit;
    public Transform powerUp;

    public void OnTriggerEnter2D(Collider2D collider)
    {
        Animator bulletAnim = gameObject.GetComponent<Animator>();

        Transform tank = collider.GetComponent<Transform>();
        Animator tankAnim = collider.GetComponent<Animator>();


        // Show power up if was red
        if (tank.name.Contains("Tank") && isFriendly
            && !bulletAnim.GetBool("hit") && !tankAnim.GetBool("hit"))
        {
            powerUp.SendMessage("ShowPowerUp", tankAnim.GetInteger("bonus"));
            tank.SendMessage("SetBonus", 0, 0);
            tankAnim.SetInteger("bonus", 0);
        }

        // Destroy tank and bullet
        if (((tank.name.Contains("Tank") && isFriendly) || (tank.name.Contains("Player") && !isFriendly))
            && !bulletAnim.GetBool("hit") && !tankAnim.GetBool("hit"))
        {
            bulletAnim.SetBool("hit", true);

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
