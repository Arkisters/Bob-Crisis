using System.Collections;
using UnityEditor.Rendering.Analytics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth = 5;
    public int deathTimer = 3;
    public bool takingDamage = false;
    Vector2 startPos;

    private SpriteRenderer Player_0;

    private void Awake()
    {
        Player_0 = GetComponent<SpriteRenderer>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        health = maxHealth;

        startPos = transform.position; 

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
            Die();
        }
    }
    
            

    void Die()
    {
        StartCoroutine(Respawn(0.5f));
    }

    IEnumerator Respawn(float duration)
    {
        Player_0.enabled = false;
        yield return new WaitForSeconds(duration);
        transform.position = startPos;
        health = maxHealth;
        Player_0.enabled = true;
    }

    void Respawn()
    {
        transform.position = startPos;

        health = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        health -= amount;
        takingDamage = true;
        StartCoroutine(Pain(1.0f));
    }
    IEnumerator Pain(float duration)
    {    
        Player_0.color = Color.red;
        yield return new WaitForSeconds (.5f);
        yield return null;
        Player_0.color = Color.white;
        yield return null;
    }
}
