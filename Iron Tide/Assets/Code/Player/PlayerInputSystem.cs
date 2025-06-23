using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    // Player  movement actions
    public InputAction moveAction;
    public InputAction sprintAction;
    public InputAction jumpAction;
    public InputAction strafeAction;
    public InputAction aimAction;

    // For shooting
    public InputAction fireLeft;
    public InputAction fireRight;

    private void Start()
    {
        // Defines player input to be read, new system still need to learn lol
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        strafeAction = InputSystem.actions.FindAction("Player/Strafe");
        aimAction = InputSystem.actions.FindAction("Player/Aim");
        fireLeft = InputSystem.actions.FindAction("Player/FireLeft");
        fireRight = InputSystem.actions.FindAction("Player/FireRight");
    }
}
