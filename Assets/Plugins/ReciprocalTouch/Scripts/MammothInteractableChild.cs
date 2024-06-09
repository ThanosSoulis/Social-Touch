using UnityEngine;

public class MammothInteractableChild : MonoBehaviour, IMammothInteractable
{
    public MammothInteractable MammothInteractable;
    
    [Range(0f, 1f)]
    public float Intensity = 1f;

    public void AddContactPoint(Transform childTransform, float intensity)
    {
        MammothInteractable.AddContactPoint(childTransform, Intensity);
    }

    public void RemoveContactPoint(Transform childTransform)
    {
        MammothInteractable.RemoveContactPoint(childTransform);
    }
}
