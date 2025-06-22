using NUnit.Framework.Constraints;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Code foundation from Brackeys Third Person Movement + First Person Movement videos 

public class Player : MonoBehaviour
{

    private PlayerInputSystem pInputSys;

    [Header("Things to attach to player")]
    public CharacterController controller;
    public Transform cam;

    private bool isAiming;

    [Header("Movement related fields")]
    public float speed = 6f;
    public float runSpeed = 10f;
    [Tooltip("By default, gravity is -9.81f")] public float gravity = -9.81f;
    public float jumpHeight = 3f;
    Vector3 velocity;
    public float turnSmoothTime = 0.1f;
    float turnSmoothVelocity;

    [Header("Grounded stuff")]
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    bool isGrounded;


    private void Start()
    {
        pInputSys = GetComponent<PlayerInputSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        isAiming = pInputSys.aimAction.IsPressed();

        Movement();
        Jump();
    }

    void Movement()
    {
        // Read player input
        Vector2 movement = pInputSys.moveAction.ReadValue<Vector2>();

        // Use the camera's orientation to move relative to it
        Vector3 camForward = cam.forward;
        Vector3 camRight = cam.right;

        // Flatten y-axis to prevent moving up/down
        //camForward.y = 0;
        //camRight.y = 0;
        //camForward.Normalize();
        //camRight.Normalize();

        // Calculate movement direction relative to the camera
        Vector3 moveDirection = camForward * movement.y + camRight * movement.x;

        Vector3 direction = new Vector3(movement.x, 0, movement.y).normalized;

        if (moveDirection.magnitude >= 0.1f)
        {
            // Choose speed (walking or sprinting)
            float moveSpeed = pInputSys.sprintAction.IsPressed() ? runSpeed : speed;

            if (isAiming)
            {
                // Aiming mode - strafe and rotate toward camera
                float targetAngle = cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
            }
            else
            {
                // Character's forward movement is tied to the camera's direction
                float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
                float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.rotation = Quaternion.Euler(0f, angle, 0f);

                // Movement added with correct rotation
                Vector3 movDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;
                controller.Move(movDir.normalized * moveSpeed * Time.deltaTime);
            }
        }
        

    }

    void Jump()
    {
        // Checks if the player is grounded using a simple empty GO 

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // If the player is grounded, remove downward velocity quicker than it can be added(?)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }
        // Jump 
        if (pInputSys.jumpAction.IsPressed() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}





//// Reads in player input and adds it to vector to be used for movement
//Vector2 movement = moveAction.ReadValue<Vector2>();
//Vector3 direction = new Vector3(movement.x, 0, movement.y).normalized;

//if (direction.magnitude >= 0.1f)
//{
//    // Applies smooth rotations to player when looking around and/or moving around
//    float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
//    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
//    transform.rotation = Quaternion.Euler(0f, angle, 0f);

//    // Movement added with correct rotation
//    Vector3 movDir = Quaternion.Euler(0, targetAngle, 0) * Vector3.forward;

//    float moveSpeed;
//    float moveSpeed = sprintAction.IsPressed() ? runSpeed : speed;

//    // Moves the player
//    controller.Move(movDir.normalized * moveSpeed * Time.deltaTime);
//}