using UnityEngine;

public class EnemyClass : MonoBehaviour
{

    [Header("Enemy Stats")]
    public int maxHealth;
    public EnemyHealthUI healthUI;

    private int currentHealth;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthUI.UpdateHealthbar((float)currentHealth / (float)maxHealth);
        Debug.Log("Ouch");

        if (currentHealth <= 0)
        {
            Debug.Log("Dead lol");
            Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "Bullet")
        {
            int damage = col.gameObject.GetComponent<BulletData>().damage;
            //TODO: Add in code to reduce damage by armour amount here
            //if (damage <= 0)
            //{
            //    return;
            //}
            TakeDamage(damage);

        }
    }
}
