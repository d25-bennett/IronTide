using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{

    InputAction shootAction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        shootAction = InputSystem.actions.FindAction("Player/Shoot");
    }

    // Update is called once per frame
    void Update()
    {
        ScanForEnemies();
    }

    void ScanForEnemies()
    {
        // Checks for closest enemy, if there is one nearby then use melee attack
        // If enemy is far away, use main weapon when using primary attack button
    }

    void RangedAttack(GameObject weapon)
    {
        // Use ranged attack
    }

    void MeleeAttack(GameObject weapon)
    {
        // Use melee attack
    }
}
