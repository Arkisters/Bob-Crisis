using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    public GameObject door; // Assign in Inspector
    public SpriteRenderer doorRenderer;
    public Color closedColor = Color.red;
    public Color openColor = Color.green;

    private void Awake()
    {
        if (doorRenderer == null)
        {
            doorRenderer = door.GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Open();
        }
        if (collision.CompareTag("Box"))
        {
            Open();
        }
    }

    void Open()
    {
        StartCoroutine(Unlock(1.0f));
    }

    IEnumerator Unlock (float duration)
    {
        // Disable the door's collider so the player can pass through
            Collider2D doorCollider = door.GetComponent<Collider2D>();
            if (doorCollider != null)
                doorCollider.enabled = false;
                doorRenderer.color = openColor;
                yield return new WaitForSeconds(duration);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Close();
        }
        if (collision.CompareTag("Box"))
        {
            Close();
        }
    }

    void Close()
    {
        StartCoroutine(Lock(1.0f));
    }

    IEnumerator Lock (float duration)
    {
        // Re-enable the door’s collider when player leaves the plate
            Collider2D doorCollider = door.GetComponent<Collider2D>();
            if (doorCollider != null)
                yield return new WaitForSeconds(duration);
                doorCollider.enabled = true;
                doorRenderer.color = closedColor;
    }
}

