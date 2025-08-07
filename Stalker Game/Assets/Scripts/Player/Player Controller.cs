using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Config")] 
    [SerializeField] float health;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpForce;
    [Range(1, 2)] 
    [SerializeField] float sprintSpeedMultiplier;
    
    [Header("Player Restrictions")] 
    [SerializeField] float maxSpeed;
    [SerializeField] float smoothInputSpeed = 0.2f;
    [SerializeField] float gravityValue = 8f;
    
    CharacterController characterController;
    Camera cam;
    InputManager inputManager;

    bool readyToJump;
    bool isSprinting; 
    bool isGrounded = false;
    float currentSpeed;

    Vector2 inputMovement;
    Vector2 currentInputVector;
    Vector2 smoothInputVelocity;

    Vector3 jumpVelocity;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        characterController = GetComponent<CharacterController>();
        inputManager = InputManager.Instance;

        Cursor.lockState = CursorLockMode.Locked;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = Physics.Raycast(transform.position, -transform.up, characterController.height);
        if (inputManager.IsJumping() && isGrounded)
            readyToJump = true;
        inputMovement = inputManager.GetPlayerMovement();
        isSprinting = inputManager.IsSprinting();
    }

    void FixedUpdate()
    {
        if (isGrounded)
        {
            jumpVelocity.y = 0f;
        }
        else
        {
            jumpVelocity.y += gravityValue * Time.deltaTime;
        }
        
        transform.eulerAngles = new Vector3(0f, cam.transform.eulerAngles.y, 0f);
        currentInputVector = Vector2.SmoothDamp(currentInputVector, isSprinting ? inputMovement * sprintSpeedMultiplier : inputMovement, ref smoothInputVelocity, smoothInputSpeed);
        
        Vector3 movement = new Vector3(currentInputVector.x, 0f, currentInputVector.y);
        movement = transform.forward * movement.z + transform.right * movement.x;
        movement.y = 0;
        
        characterController.Move(movement * (Time.deltaTime * moveSpeed));

        if (readyToJump)
        {
            jumpVelocity.y += Mathf.Sqrt(jumpForce * -3.0f * gravityValue);
            readyToJump = false;
        }
        
        characterController.Move(jumpVelocity * Time.deltaTime);
    }
}
