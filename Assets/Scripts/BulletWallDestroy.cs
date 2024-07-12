using UnityEngine;

public class BulletWallDestroy : MonoBehaviour
{
    private Animator _animator;
    private Transform _wall;

    private void Start()
    {
        _animator = gameObject.GetComponent<Animator>();
    }


    public void OnTriggerEnter2D(Collider2D collider)
    {
        _wall = collider.GetComponent<Transform>();

        if ((_wall.name.Contains("Wall") || _wall.name.Contains("Iron")) && !_animator.GetBool("hit"))
        {
            _animator.SetBool("hit", true);
            DestroyWallsAccordingToCoordinates(Mathf.Round(transform.position.x), Mathf.Round(transform.position.y));
            PlaySound();
        }
    }

    private void PlaySound()
    {
        if (_wall.name.Contains("Iron"))
        {
            gameObject.SendMessage("PlayIronHitSound");
        }
        if (_wall.name.Contains("Wall"))
        {
            gameObject.SendMessage("PlayBrickHitSound");
        }
    }

    private void DestroyWallsAccordingToCoordinates(float x, float y)
    {
        Transform[] ts = GameManager.Instance.WallsHolder.GetComponentsInChildren<Transform>();

        float input_x = _animator.GetFloat("input_x");
        float input_y = _animator.GetFloat("input_y");

        // Horizontal shot
        if (input_y == 0)
        {
            if (input_x == -1)
            {
                x -= 1;
            }

            PartiallyDestroy(ts.GetByNameAndCoords("Wall", x, y), _animator);
            PartiallyDestroy(ts.GetByNameAndCoords("Wall", x, y - 1), _animator);
        }

        // Vertical shot
        if (input_x == 0)
        {
            if (input_y == -1)
            {
                y -= 1;
            }

            PartiallyDestroy(ts.GetByNameAndCoords("Wall", x, y), _animator);
            PartiallyDestroy(ts.GetByNameAndCoords("Wall", x - 1, y), _animator);
        }
    }

    public static void PartiallyDestroy(Transform obj, Animator bulletAnim)
    {
        float input_x = bulletAnim.GetFloat("input_x");
        float input_y = bulletAnim.GetFloat("input_y");

        obj.NotNull((t) =>
        {
            Wall wall = t.GetComponent<Wall>();

            float curr = wall.CurrentState;
            Debug.Log("Partially Destroy wall " + curr);

            // The tyniest piece of wall left
            if (curr.IsIn(1, 3, 7, 9))
            {
                Debug.Log("Destroy wall");
                Destroy(t.gameObject);
            }
            // Vertical shot
            else if (input_x == 0)
            {
                if (curr.IsIn(2, 8))
                {
                    Debug.Log("Destroy wall");
                    Destroy(t.gameObject);
                }
                else if (curr.IsIn(4, 5, 6))
                {
                    wall.UpdateWall(curr + (input_y * 3));
                }
            }
            // Horizontal shot
            else if (input_y == 0)
            {
                if (curr.IsIn(4, 6))
                {
                    Debug.Log("Destroy wall");
                    Destroy(t.gameObject);
                }
                else if (curr.IsIn(2, 5, 8))
                {
                    wall.UpdateWall(curr + input_x);
                }
            }
        });
    }
}
