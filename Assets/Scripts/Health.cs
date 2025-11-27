using UnityEditor.Rendering.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth = 5;
    public int deathTimer = 3;
    public bool takingDamage = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth; 

    }

    // Update is called once per frame
    void Update()
    {
        if (takingDamage)
        {
            health -= Time.deltaTime;
        }
        if (health <= 0)
        {
            Destroy(gameObject);
            SceneManager.LoadScene("LeyoBareLeyo");
        }
    }



    public void TakeDamage(float amount)
    {
        health -= amount;
        takingDamage = true;
    }
}
