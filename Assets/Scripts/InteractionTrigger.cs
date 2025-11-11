using UnityEngine;
using UnityEngine.Events;

public class InteractionTrigger : MonoBehaviour, IInteractable
{
    [Header("Interaction Settings")]
    public bool canBeReused = true;
    public GameObject targetObject;
    
    private bool hasBeenUsed = false;


    public void Interact(PlayerController player)
    {
        if (hasBeenUsed && !canBeReused)
        {
            return;
        }
        
        hasBeenUsed = true;
        
        if (targetObject != null)
        {
            IInteractable interactable = targetObject.GetComponent<IInteractable>();
            if (interactable != null)
            {
                interactable.Interact(player);
            }
        }
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
