using UnityEngine;

public interface IInteractable
{
    bool CanInteract { get; }
    void Interact(GameObject interactor);
    void SetHighlight(bool active); // mostrar u ocultar el "E" flotante
}