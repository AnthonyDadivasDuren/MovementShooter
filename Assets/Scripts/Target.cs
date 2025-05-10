using UnityEngine;

public class Target : MonoBehaviour
{
    public float health = 50f;
    public GameObject destroyedVersion;
    public void TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            Die();
        }
    }


    void Die()
    {
        if (destroyedVersion != null)
        {
            Instantiate(destroyedVersion, transform.position, transform.rotation);
        }
        Destroy(gameObject);
    }
}
