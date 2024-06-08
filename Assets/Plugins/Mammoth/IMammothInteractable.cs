using UnityEngine;

public interface IMammothInteractable
{
    public void AddContactPoint(Transform childTransform, float intensity);
    public void RemoveContactPoint(Transform childTransform);
}
