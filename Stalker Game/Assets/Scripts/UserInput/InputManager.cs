using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    private UserInputAction playerControls;
    private static InputManager instance;

    public static InputManager Instance => instance;

    void Awake()
    {
        if (instance != null && instance == this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
        playerControls = new UserInputAction();
    }

    void OnEnable()
    {
        playerControls.Enable();
    }

    void OnDisable()
    {
        playerControls.Disable();
    }

    public bool IsSprinting()
    {
        return playerControls.PlayerMovement.Sprinting.IsPressed();
    }


    public Vector2 GetCameraMovement()
    {
        return playerControls.PlayerCameraMovement.MouseLook.ReadValue<Vector2>();
    }

    public Vector2 GetPlayerMovement()
    {
        return playerControls.PlayerMovement.Move.ReadValue<Vector2>();
    }

    public bool IsJumping()
    {
        return playerControls.PlayerMovement.Jump.IsPressed();
    }

    public bool IsCrouching()
    {
        return playerControls.PlayerMovement.Crouch.IsPressed();
    }

    public bool DoneCrouching()
    {
        return playerControls.PlayerMovement.Crouch.WasReleasedThisFrame();
    }
}
