using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public TankInputActions playerControls;
    public Vector2 MoveValue { get; private set; }
    public bool Fire { get; private set; }

    private void Awake()
    {
        playerControls = new TankInputActions();
    }

    private void OnEnable()
    {
        playerControls.Player.Enable();
        playerControls.Player.Move.performed += ctx => MoveValue = ctx.ReadValue<Vector2>();
        playerControls.Player.Move.canceled += ctx => MoveValue = Vector2.zero;

        playerControls.Player.Shoot.performed += ctx => Fire = ctx.ReadValueAsButton();
        playerControls.Player.Shoot.canceled += ctx => Fire = false;
    }

    private void OnDisable()
    {
        playerControls.Player.Disable();
    }
}
