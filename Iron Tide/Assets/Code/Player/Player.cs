using NUnit.Framework.Constraints;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

// Code foundation from Brackeys Third Person Movement + First Person Movement videos 

public class Player : MonoBehaviour
{
    InputAction moveAction;
    InputAction sprintAction;
    InputAction jumpAction;
    InputAction shootAction;

    [Header ("Things to attach to player")]
    public CharacterController controller;
    public Transform cam;

    [Header("Movement related fields")]
    public float speed = 6f;
    public float runSpeed = 10f;
    [Tooltip ("By default, gravity is -9.81f")]public float gravity = -9.81f;
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
        // Defines player input to be read, new system still need to learn lol
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        sprintAction = InputSystem.actions.FindAction("Player/Sprint");
    }

    // Update is called once per frame
    void Update()
    {
        
        // Reads in player input and adds it to vector to be used for movement
        Vector2 movement = moveAction.ReadValue<Vector2>();
        Vector3 direction = new Vector3(movement.x, 0, movement.y).normalized;

        if (direction.magnitude >= 0.1f)
        { 
            // Applies smooth rotations to player when looking around and/or moving around
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            // Movement added with correct rotation
            Vector3 movDir = Quaternion.Euler(0,targetAngle,0) * Vector3.forward;

            float moveSpeed;
            if (sprintAction.IsPressed()) { moveSpeed = runSpeed;}
            else { moveSpeed = speed; }

            // Moves the player
            controller.Move(movDir.normalized * moveSpeed * Time.deltaTime);
        }

        // Checks if the player is grounded using a simple empty GO 
        
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        // If the player is grounded, remove downward velocity quicker than it can be added(?)
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2;
        }
        // Jump 
        if (jumpAction.IsPressed() && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }


}
