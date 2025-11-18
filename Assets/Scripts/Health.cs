using UnityEngine;

public class Health : MonoBehaviour
{
    public int health;
    public int maxHealth = 3; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth; 

    }

    // Update is called once per frame
    void Update()
    {
        
    }



    public void TakeDamge( int amount)
    {
        health -= amount;

        if(health <= 0)
        {
            Destroy(gameObject);
        }
    }
}
