using Unity.Netcode;
using UnityEditor.Animations;
using UnityEngine;
using UnityEngine.InputSystem.Processors;
using UnityEngine.UIElements;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField] private Animator _playerAnim;
    [SerializeField] private AnimatorController[] _controllers;

    NetworkVariable<Vector2> Axis = new NetworkVariable<Vector2>(default, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
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
        Debug.Log($"Player {OwnerClientId} Spawned");
        if(IsServer)
        {
            ResetPosition();
        }
        
        _playerAnim.runtimeAnimatorController = _controllers[OwnerClientId];
    }

    void Start()
    {
        input = InputManager.Instance;
        anim = gameObject.GetComponent<Animator>();
    }

    void Update()
    {
        if(!GameManager.Instance.IsPlaying)
        {
            return; 
        }

        if (IsLocalPlayer && IsOwner)
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
        if (!IsLocalPlayer || !IsOwner || !GameManager.Instance.IsPlaying) return;
        // Do everything only then if not hit
        if (!anim.GetBool("hit"))
        {
            ChangeInputFromMultipleKeyPresses();
            ActualyChangingCoordinatesAccordingToInput();
            SetLookingDirection();
            ApplyMovementSound();
        }
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
            else if (input.MoveValue.y < 0) Axis.Value = new Vector2(0, -1);
        }
        Debug.Log("Axis = " + Axis);
    }

    private void ChangeInputFromMultipleKeyPresses()
    {
        // Movement changing when pressing keys for both directions
        if (Axis.Value.x != 0 && Axis.Value.y != 0)
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
        Debug.Log("Input XY = " + input_x + ", " + input_y);
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
        transform.position = GameManager.Instance.GetStartPosition((int)OwnerClientId);
        
        Debug.Log($"Player {OwnerClientId} position set to {transform.position}");
    }
}