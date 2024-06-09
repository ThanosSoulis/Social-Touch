using UnityEngine;

public class DistributerChild : MonoBehaviour
{
    public int SphereID;
    public bool Disable;
    void OnTriggerEnter(Collider other)
    {
        if (Disable == true) {
            return;
        }

        if (other.TryGetComponent(out IMammothInteractable interactable)) {
            interactable.AddContactPoint(transform, 0f);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (Disable == true) {
            return;
        }

        if (other.TryGetComponent(out IMammothInteractable interactable))
        {
            interactable.RemoveContactPoint(transform);
        }
    }

}
