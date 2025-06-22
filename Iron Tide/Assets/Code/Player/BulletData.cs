using UnityEngine;

public class BulletData : MonoBehaviour
{
    [Tooltip("Select the speed of the projectile")]
    [Range(50f, 300f)] public float velocity;

    [Range(5f, 20f)][SerializeField] float lifeSpan = 10f;
    public GameObject effectOnImpact;
    public int damage;
    private bool effect;

    // Start is called before the first frame update
    void Start()
    {
        if (effectOnImpact == null) effect = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeSpan > 0)
            lifeSpan -= 0.5f * Time.deltaTime;
        else if (lifeSpan <= 0)
            Destroy(gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        if (effect) { Instantiate(effectOnImpact); }
        //ContactPoint contact = collision.contacts[0];
        //Quaternion rotation = Quaternion.FromToRotation(Vector3.up, contact.normal);
        //Vector3 position = contact.point;
        //Instantiate(explosionPrefab, position, rotation);
        Destroy(gameObject);
    }
}
