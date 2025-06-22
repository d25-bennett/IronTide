using System.Diagnostics.Contracts;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{

    public InputAction fireLeft;
    public InputAction fireRight;
     
    public RangedWeaponData weaponLeft;
    public RangedWeaponData weaponRight;

    public float cooldownRight;
    public float cooldownLeft;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fireLeft = InputSystem.actions.FindAction("Player/FireLeft");
        fireRight = InputSystem.actions.FindAction("Player/FireRight");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //ScanForEnemies();

        if (fireRight.IsPressed() && cooldownRight <= 0) { RangedAttack(weaponRight); cooldownRight = weaponRight.bulletCooldown; }
        if (fireLeft.IsPressed() && cooldownLeft <= 0) { RangedAttack(weaponLeft); cooldownLeft = weaponLeft.bulletCooldown; }

        if (cooldownRight > 0)
            cooldownRight -= Time.deltaTime; 

        if (cooldownLeft > 0)
            cooldownLeft -= Time.deltaTime;
    }

    void ScanForEnemies()
    {
        // Checks for closest enemy, if there is one nearby then use melee attack
        // If enemy is far away, use main weapon when using primary attack button
    }

    void RangedAttack(RangedWeaponData weapon)
    {
        // Use ranged attack
        Debug.Log("Pew Pew");
        Rigidbody instantiatedProjectile = Instantiate(weapon.projectile, weapon.firingPoint.transform.position, weapon.firingPoint.transform.rotation) as Rigidbody;
        instantiatedProjectile.linearVelocity = transform.TransformDirection(new Vector3(0, 0, weapon.velocity));
        //if (audioClip != null)
       // { AudioSource.PlayClipAtPoint(audioClip, transform.position); }

    }

    void MeleeAttack(GameObject weapon)
    {
        // Use melee attack
    }
}
