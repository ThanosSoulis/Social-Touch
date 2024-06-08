using System.Linq;
using UnityEngine;
using Leap;
using Leap.Unity;

public class ForceFieldController : MonoBehaviour
{
    public ForceFieldSensation sensation;
    public SensationExplorerHandTracked sensationExplorerHandTracked;
    public bool IsActive { get; private set; }

    void Update()
    {
        if (sensationExplorerHandTracked.CurrentSensation != sensation)
        {
            IsActive = false;
            return;
        }

        IsActive = true;

        Hand hand = Hands.Provider.CurrentFrame.Hands.FirstOrDefault();
        if (hand != null)
        {
            sensation.trackedObjectPosition = hand.PalmPosition;
            sensation.position.x = sensation.trackedObjectPosition.x;
            sensation.position.y = sensation.trackedObjectPosition.y;
            sensation.rotation = Vector3.zero;
        }
        else
        {
            sensation.trackedObjectPosition = Vector3.negativeInfinity;
        }
    }
}
