using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UIElements;

public class PlayerMovement : NetworkBehaviour
{
    public NetworkVariable<Vector2> Axis  = new NetworkVariable<Vector2>();
    public float MaxSpeed = 0.10f;

    private Animator anim;
    public int player;

    private bool isMoving;

    private float input_x = 0;
    private float input_y = 1;

    private float look_x = 0;
    private float look_y = 1;

    public AudioClip notMovingSound;
    public AudioClip movingSound;

    private InputManager input;

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            SetStartPosition();
        }
    }

    void Start()
    {
        input = InputManager.Instance;
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if (IsOwner)
        {
            CalculateAxis();
        }

        anim.SetBool("isMoving", isMoving);
        anim.SetFloat("input_x", input_x);
        anim.SetFloat("input_y", input_y);

        if (anim.GetBool("hit")) anim.SetBool("isMoving", false);
    }

    public void FixedUpdate()
    {
        // Do everything only then if not hit
        if (!anim.GetBool("hit"))
        {
            ChangeInputFromMultipleKeyPresses();
            ActualyChangingCoordinatesAccordingToInput();
            SetLookingDirection();
            ApplyMovementSound();
        }
    }

    private void SetStartPosition()
    {
        Vector3 positionXY;
        if (NetworkManager.Singleton.IsServer)
        {
            positionXY = GameManager.Instance.GetStartPosition(0);
        }
        else
        {
            positionXY = GameManager.Instance.GetStartPosition(1);
        }

        Vector3 pos = new Vector3(positionXY.x, positionXY.y, transform.position.z);
        transform.position = pos;
    }

    private void CalculateAxis()
    {
        if (input.MoveValue == Vector2.zero)
        {
            Axis.Value = Vector2.zero;
        }
        if (Mathf.Abs(input.MoveValue.x) > Mathf.Abs(input.MoveValue.y))
        {
            if (input.MoveValue.x > 0) Axis.Value = new Vector2(1, 0);
            else if (input.MoveValue.x < 0) Axis.Value = new Vector2(-1, 0);
        }
        else
        {
            if (input.MoveValue.y > 0) Axis.Value = new Vector2(0, 1);
            else if (input.MoveValue.y < 0) Axis.Value = new Vector2(0, 1);
        }
    }

    private void ChangeInputFromMultipleKeyPresses()
    {
        // Movement changing when pressing keys for both directions
        if (Axis.Value != Vector2.zero)
        {
            if (input_x == 0)
            {
                input_x = Axis.Value.x;
                input_y = 0;
            }
            if (input_y == 0)
            {
                input_x = 0;
                input_y = Axis.Value.y;
            }
        }
        // If at least one key pressed
        else if (Axis.Value.x != 0 || Axis.Value.y != 0)
        {
            input_x = Axis.Value.x;
            input_y = Axis.Value.y;
        }
    }

    private void ActualyChangingCoordinatesAccordingToInput()
    {
        // Movement when pressing a key
        if (Axis.Value.x != 0 || Axis.Value.y != 0)
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
    }

    private void SetLookingDirection()
    {
        look_x = input_x;
        look_y = input_y;
    }

    private void ApplyMovementSound()
    {
        if (player == 1)
        {
            // Sounds moving and not moving
            if (IsSomethingPressed() && !AudioManager.Instance.IsPlaying(movingSound))
            {
                AudioManager.Instance.PlaySFX(movingSound);
            }
            else if (!IsSomethingPressed() && !AudioManager.Instance.IsPlaying(notMovingSound))
            {
                AudioManager.Instance.PlaySFX(notMovingSound);
            }
        }
    }

    private bool IsSomethingPressed()
    {
        return Axis.Value != Vector2.zero;
    }


    // message receiver from "BulletTankDestroy" and "loadmap"
    public void ResetPosition()
    {
        if (player == 1)
        {
            transform.position = new Vector3(-4, -12, 0);
        }
        else if (player == 2)
        {
            transform.position = new Vector3(4, -12, 0);
        }
    }
}
