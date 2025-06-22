using UnityEngine;

// Class to be applied to a weapon prefab. Details all the information for firing the weapon (bullets, firing point, ect.)
public class RangedWeaponData : MonoBehaviour
{
    [Header("Weapon Properties")]

    [Tooltip("Use this to select a GameObject as the point of instantiation")]
    public GameObject firingPoint;

    [Tooltip("What type of ammunition does the weapon uses")]
    public Rigidbody projectile;

    [HideInInspector] public float velocity; // Takes velocity data from projectile, but hidden from being edited in editor

    [Tooltip("How long before the next projectile can be fired from the weapons. Uses deltaTime so range is in seconds")]
    [Range(0.1f, 9f)] public float bulletCooldown;

    // TODO: Might move this check into the main player attack script
    [Tooltip("Warm-up/charging period for weapons")]
    public float weaponCharge;
    public bool chargingWeapon;

    private void Awake()
    {
        velocity = projectile.GetComponent<BulletData>().velocity;
    }

}
