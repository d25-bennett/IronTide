using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputSystem : MonoBehaviour
{
    public InputAction moveAction;
    public InputAction sprintAction;
    public InputAction jumpAction;
    public InputAction shootAction;
    public InputAction strafeAction;
    public InputAction aimAction;

    private void Start()
    {
        // Defines player input to be read, new system still need to learn lol
        moveAction = InputSystem.actions.FindAction("Player/Move");
        jumpAction = InputSystem.actions.FindAction("Player/Jump");
        sprintAction = InputSystem.actions.FindAction("Player/Sprint");
        strafeAction = InputSystem.actions.FindAction("Player/Strafe");
        aimAction = InputSystem.actions.FindAction("Player/Aim");
    }
}
