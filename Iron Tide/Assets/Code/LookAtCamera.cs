using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    void Update()
    {
        transform.LookAt(2 * transform.position - Camera.main.transform.position);
    }
}
