using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{

    public Animator animator;

    private float currentSpeed;
    private Vector3 lastPosition;

    private float speedThreshold = 0.1f;
    private float debounceTime = 0.05f;
    private float lastStateChangeTime;
    private bool isMoving = false;

    private float forwardSpeed;
    private float rightSpeed;
    private Vector3 localVelocity;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lastPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        
        float speed = GetComponent<Player>().GetGroundSpeed();
        float time = Time.time;

        Debug.Log($"F: {speed}");

        if (!isMoving && speed > speedThreshold && time - lastStateChangeTime > debounceTime)
        {
            animator.SetTrigger("Move Forward");
            Debug.Log("Move Forward");
            isMoving = true;
            lastStateChangeTime = time;
        }
        else if (isMoving && speed <= speedThreshold && time - lastStateChangeTime > debounceTime)
        {
            animator.SetTrigger("To Idle");
            isMoving = false;
            lastStateChangeTime = time;
        }
    }
}
