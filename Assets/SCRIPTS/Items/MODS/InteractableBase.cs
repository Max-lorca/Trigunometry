using UnityEngine;
using UnityEngine.Events;

public class InteractableBase : MonoBehaviour, IInteractable
{
    [SerializeField] private GameObject promptUI;
    public UnityEvent<GameObject> onInteract;

    public bool CanInteract => true;

    void Start()
    {
        if (promptUI != null) promptUI.SetActive(false);
    }

    public void SetHighlight(bool active)
    {
        if (promptUI != null) promptUI.SetActive(active);
    }

    public void Interact(GameObject interactor) => onInteract.Invoke(interactor);
}