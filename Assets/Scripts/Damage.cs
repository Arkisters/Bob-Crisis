using UnityEngine;

public class Damage : MonoBehaviour
{
    public float damage = 1f;
    public Health playerHealth;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            if (playerHealth == null)
            {
             playerHealth = collision.gameObject.GetComponent<Health>();

            }
            playerHealth.TakeDamage(damage);
            
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        playerHealth.takingDamage = false;
    }
}
