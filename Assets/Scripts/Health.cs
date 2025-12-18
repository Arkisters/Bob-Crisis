using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Health : MonoBehaviour
{
    public float health;
    public float maxHealth = 5;
    public int deathTimer = 3;
    public bool takingDamage = false;
    Vector2 checkpointPos;

    private SpriteRenderer Player_0;

    private void Awake()
    {
        Player_0 = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        health = maxHealth;
        checkpointPos = transform.position;
    }

    private void Update()
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

    public void Updatecheckpoint(Vector2 pos)
    {
        checkpointPos = pos;
        health = maxHealth;
    }

    void Die()
    {
        StartCoroutine(Respawn(0.5f));
    }

    IEnumerator Respawn (float duration)
    {
        Player_0.enabled = false;
        yield return new WaitForSeconds(duration);
        transform.position = checkpointPos;
        health = maxHealth;
        Player_0.enabled = true;
    }

    void Respawn()
    {
        transform.position = checkpointPos;
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
        yield return new WaitForSeconds(.5f);
        yield return null;
        Player_0.color = Color.white;
        yield return null;
    }
}