using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInput : MonoBehaviour
{
    private PlayerInputActions playerInputActions;
    public event EventHandler OnJumpActionStarted;
    public event EventHandler OnJumpAction;

    public event EventHandler OnDashActionStarted;
    public event EventHandler OnDashAction;

    public bool JumpInputDown;
    public bool JumpInputUp;

    public bool DashInputDown;
    public bool DashInputUp;

    private void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable(); // Player is the action map defined in the asset

        playerInputActions.Player.Jump.started += Jump_started;
        playerInputActions.Player.Jump.performed += Jump_performed;

        playerInputActions.Player.Jump.started += Dash_started;
        playerInputActions.Player.Dash.performed += Dash_performed;

    }

    private void Update()
    {
        JumpInputDown = playerInputActions.Player.Jump.IsPressed();
        JumpInputUp = playerInputActions.Player.Jump.WasReleasedThisFrame();

        DashInputDown = playerInputActions.Player.Dash.IsPressed();
        DashInputUp = playerInputActions.Player.Dash.WasReleasedThisFrame();
    }

    private void Jump_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnJumpActionStarted?.Invoke(this, EventArgs.Empty);
    }
    private void Jump_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnJumpAction?.Invoke(this, EventArgs.Empty);
    }

    private void Dash_started(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnDashActionStarted?.Invoke(this, EventArgs.Empty);
    }
    private void Dash_performed(UnityEngine.InputSystem.InputAction.CallbackContext obj)
    {
        OnDashAction?.Invoke(this, EventArgs.Empty);
    }

    public Vector2 GetMovementVectorNormalized()
    {
        Vector2 inputVector = playerInputActions.Player.Move.ReadValue<Vector2>();
        inputVector = inputVector.normalized;
        return inputVector;
    }
}
