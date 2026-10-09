using System.Collections.Generic;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    private readonly List<IInteractable> inRange = new();
    private IInteractable current;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable) && !inRange.Contains(interactable))
            inRange.Add(interactable);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.TryGetComponent<IInteractable>(out var interactable))
        {
            inRange.Remove(interactable);
            interactable.SetHighlight(false);
        }
    }
    private void Update()
    {
        // Limpia objetos destruidos (ej. un pickup recogido)
        inRange.RemoveAll(i => (i as UnityEngine.Object) == null);

        IInteractable nearest = null;
        float minDist = float.MaxValue;

        foreach (var i in inRange)
        {
            if (!i.CanInteract) continue;

            float d = Vector2.Distance(transform.position, ((Component)i).transform.position);
            if (d < minDist)
            {
                minDist = d;
                nearest = i;
            }
        }
        if (nearest != current)
        {
            if (current != null && (current as UnityEngine.Object) != null)
                current.SetHighlight(false);

            current = nearest;

            if (current != null)
                current.SetHighlight(true);
        }
    }

    public void TryInteract()
    {
        if (current != null && current.CanInteract)
            current.Interact(gameObject);
    }
}