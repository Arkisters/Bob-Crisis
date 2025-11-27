
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Timeline;

public class Checkpoint : MonoBehaviour
{
    Health health;

    private void Awake()
    {
        health = GameObject.FindGameObjectWithTag("Player").GetComponent<Health>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            health.Updatecheckpoint(transform.position);
        }
    }
}