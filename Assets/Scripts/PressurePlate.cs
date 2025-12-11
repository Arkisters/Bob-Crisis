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

    private int objectsOnPlate = 0; // Track how many objects are on the trigger
    private Coroutine unlockCoroutine = null;
    private Coroutine lockCoroutine = null;

    private void Awake()
    {
        if (doorRenderer == null)
        {
            doorRenderer = door.GetComponent<SpriteRenderer>();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Box"))
        {
            objectsOnPlate++;
            Open();
        }
    }

    void Open()
    {
        // Stop any running lock coroutine when opening
        if (lockCoroutine != null)
        {
            StopCoroutine(lockCoroutine);
            lockCoroutine = null;
        }

        // Only start unlock if it's not already running
        if (unlockCoroutine == null)
        {
            unlockCoroutine = StartCoroutine(Unlock(1.0f));
        }
    }

    IEnumerator Unlock (float duration)
    {
        // Disable the door's collider so the player can pass through
        Collider2D doorCollider = door.GetComponent<Collider2D>();
        if (doorCollider != null)
            doorCollider.enabled = false;
        doorRenderer.color = openColor;
        yield return new WaitForSeconds(duration);
        unlockCoroutine = null;
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Box"))
        {
            Open();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") || collision.CompareTag("Box"))
        {
            objectsOnPlate--;
            // Only close if NO objects remain on the plate
            if (objectsOnPlate <= 0)
            {
                objectsOnPlate = 0; // Clamp to 0 to prevent negative values
                Close();
            }
        }
    }

    void Close()
    {
        // Stop any running unlock coroutine when closing
        if (unlockCoroutine != null)
        {
            StopCoroutine(unlockCoroutine);
            unlockCoroutine = null;
        }

        // Only start lock if it's not already running
        if (lockCoroutine == null)
        {
            lockCoroutine = StartCoroutine(Lock(1.0f));
        }
    }

    IEnumerator Lock (float duration)
    {
        // Re-enable the door’s collider when player leaves the plate
        Collider2D doorCollider = door.GetComponent<Collider2D>();
        yield return new WaitForSeconds(duration);
        if (doorCollider != null)
        {
            doorCollider.enabled = true;
            doorRenderer.color = closedColor;
        }
        lockCoroutine = null;
    }
}

