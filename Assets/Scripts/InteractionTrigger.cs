using UnityEngine;
using UnityEngine.Events;

public class InteractionTrigger : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    public string interactionPrompt = "Press E to interact";
    public bool canBeReused = true;
    
    [Header("Events")]
    public UnityEvent onInteract;
    
    private bool hasBeenUsed = false;


    public void Interact(PlayerController player)
    {
        if (hasBeenUsed && !canBeReused)
        {
            Debug.Log($"{gameObject.name} has already been used.");
            return;
        }
        
        hasBeenUsed = true;
        onInteract?.Invoke();
        
        Debug.Log($"Player interacted with {gameObject.name}");
    }


    public void ResetTrigger()
    {
        hasBeenUsed = false;
    }


    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);
        }
    }
}
