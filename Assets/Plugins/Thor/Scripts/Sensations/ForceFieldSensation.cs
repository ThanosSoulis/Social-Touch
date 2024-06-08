using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Force Field", menuName = "Sensations/Force Field")]
public class ForceFieldSensation : SpeedConstrainableSensation
{
    public float length;
    public Vector3 trackedObjectPosition;
    public float beforeThreshold;
    public float afterThreshold;

    public override bool IsClosedShape { get { return false; } }

    public override (Vector3 p, float intensity) EvaluateAt(double seconds)
    {
        float intensity = 1;
        if (trackedObjectPosition.z < position.z - beforeThreshold || trackedObjectPosition.z > position.z + afterThreshold) {
            intensity = 0;
        }
        return (Lerp(GetFraction(seconds)), intensity);
    }

    public override Vector3 Lerp(double fraction)
    {
        fraction = fraction <= 0.5 ? fraction * 2 : 1 - ((fraction - 0.5) * 2);
        return Shape.LineAt(fraction, new Vector3(length / 2, 0, 0), new Vector3(-length / 2, 0, 0));
    }

    protected override float ComputeFrequency()
    {
        return speedTravel / (length * 2);
    }
}
