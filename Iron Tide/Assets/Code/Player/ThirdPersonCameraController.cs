using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonCameraController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera freeCam;
    [SerializeField] private CinemachineCamera aimCam;

    public GameObject crosshair;

    InputAction aimAction; 
    bool isAiming;

    private void Start()
    {
        aimAction = InputSystem.actions.FindAction("Player/Aim");
    }

    void Update()
    {
        isAiming = aimAction.IsPressed();

        aimCam.Priority = isAiming ? 20 : 0;
        freeCam.Priority = isAiming ? 0 : 20;

        // TODO: place this in a UI script instead, just a quick test for the time being 
        crosshair.SetActive(isAiming);
    }
}
