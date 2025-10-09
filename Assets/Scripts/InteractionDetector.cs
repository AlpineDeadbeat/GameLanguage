using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null; // Closest Interactable
    public GameObject interactionIcon;

    void Start()
    {
        if (interactionIcon != null) interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        // Only act on "performed" (not started/canceled)
        if (!context.performed) return;

        // Safely ignore if nothing is in range
        if (interactableInRange == null) return;

        try
        {
            interactableInRange.Interact();
        }
        catch (System.Exception e)
        {
            Debug.LogException(e, this);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
        {
            interactableInRange = interactable;
            if (interactionIcon != null) interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable) && interactable == interactableInRange)
        {
            interactableInRange = null;
            if (interactionIcon != null) interactionIcon.SetActive(false);
        }
    }
}
