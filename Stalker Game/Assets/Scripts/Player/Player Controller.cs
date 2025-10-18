using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Player Stats Config")] 
    [SerializeField] float health;
    [SerializeField] float staminaRecovery = 1.0f;
    [SerializeField] float stamina = 1.0f;
    [SerializeField] float bonusMoveSpeed = 1.0f;
    [SerializeField] float bulletResistance = 1.0f;
    
    [Header("Player Speed Config")]
    [SerializeField] float moveSpeed;
    [SerializeField] float maxHorizontalSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float sprintSpeedMultiplier;
    [SerializeField] float slideSpeedMultiplier;
    float defaultDrag;
    
    [Header("Player Gravity Config")] 
    [SerializeField] float smoothInputSpeed = 0.2f;
    
    Rigidbody rb;
    Camera cam;
    InputManager inputManager;

    bool readyToJump;
    bool isSprinting; 
    bool isGrounded = false;
    bool isCrouching = false;
    bool quitCrouchThisFrame = false;
    bool isSliding = false;
    bool readyToSlide = false;

    Vector2 inputMovement;
    Vector2 currentInputVector;
    Vector2 smoothInputVelocity;

    Vector3 currentMovement;
    
    // Start is called before the first frame update
    void Start()
    {
        // setup
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        inputManager = InputManager.Instance;
        defaultDrag = rb.linearDamping;
        
        Cursor.lockState = CursorLockMode.Locked;
    }

    // use update to get input every frame 
    void Update()
    {
        
        isGrounded = Physics.Raycast(transform.position, -transform.up, 2);
        
        if (inputManager.IsJumping() && isGrounded)
            readyToJump = true;
        
        inputMovement = inputManager.GetPlayerMovement();
        isSprinting = inputManager.IsSprinting();
        isCrouching = inputManager.IsCrouching();
        quitCrouchThisFrame = inputManager.DoneCrouching();
        
        rb.linearDamping = isGrounded ? defaultDrag : 0.5f;
    }

    void FixedUpdate()
    {
        if(!isSliding)
            currentInputVector = Vector2.SmoothDamp(currentInputVector, isSprinting ? inputMovement * sprintSpeedMultiplier : inputMovement, ref smoothInputVelocity, smoothInputSpeed);
        
        currentMovement = new Vector3(currentInputVector.x, 0f, currentInputVector.y);
        currentMovement = transform.forward * currentMovement.z + transform.right * currentMovement.x;
        currentMovement.y = 0;

        // restrict horizontal input/movement in the air
        if (!isGrounded)
            currentMovement *= 0.35f;

        if (quitCrouchThisFrame) // the player stood up
            readyToSlide = false;
        else if (isCrouching && isSprinting && isGrounded && !readyToSlide) // slide, if sprinting
            StartCoroutine(Slide());
        else if(isCrouching && isGrounded && !isSliding) // move slower if crouched is still being held after sliding speed has diminished
            currentMovement *= 0.5f;
        
        // clamp the horizontal speed of the player
        Vector3 vel = rb.linearVelocity;
        Vector3 horizontalVel = new Vector3(vel.x, 0, vel.z);
        
        if (readyToJump) // jump key was pressed
        {
            readyToJump = false;
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }

        // restrict max move speed
            // drag on the rigidbody kinda does this for us. but is still good to have when the speed stat gets higher
        if (isSliding && horizontalVel.magnitude > maxHorizontalSpeed * 2)
        {
            horizontalVel = horizontalVel.normalized * maxHorizontalSpeed;
            rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        }
        else if (horizontalVel.magnitude > maxHorizontalSpeed && !isSliding)
        {
            horizontalVel = horizontalVel.normalized * maxHorizontalSpeed;
            rb.linearVelocity = new Vector3(horizontalVel.x, rb.linearVelocity.y, horizontalVel.z);
        }
        else
            rb.AddForce(currentMovement * (moveSpeed * bonusMoveSpeed * 2.0f) , ForceMode.Force);
        
        transform.eulerAngles = new Vector3(0, cam.transform.eulerAngles.y, 0); 
    }

    IEnumerator Slide()
    {
        float elapsedTime = 0f;
        isSliding = true;
        readyToSlide = true;

        while (!quitCrouchThisFrame && isGrounded && currentInputVector.magnitude > 0.3f)
        {
            currentMovement *= slideSpeedMultiplier;
            currentInputVector = Vector3.Lerp(currentInputVector, Vector3.zero, elapsedTime);
            
            elapsedTime += 0.01f * Time.deltaTime;
            yield return null;
        }

        isSliding = false;
    }
}
