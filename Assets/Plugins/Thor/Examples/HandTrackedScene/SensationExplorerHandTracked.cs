using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Leap;
using Leap.Unity;

public class SensationExplorerHandTracked : MonoBehaviour
{
    [Header("Hand tracking")]
    public bool TrackHand;

    [Header("Sensations")]
    public List<Sensation> sensations = new List<Sensation>();
    public ForceFieldController forceFieldController;

    public Sensation CurrentSensation => sensations.ElementAt(SensationIndex);

    private int _sensationIndex;
    private int SensationIndex
    {
        get
        {
            return _sensationIndex;
        }
        set
        {
            _sensationIndex = (value < 0 ? sensations.Count + value : value) % sensations.Count;
        }
    }

    void Start()
    {
        GetComponent<DeviceManager>()?.SetSensation(sensations.First());
    }

    void Update()
    {
        DeviceManager device = GetComponent<DeviceManager>();

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            SensationIndex++;
            device.SetSensation(sensations.ElementAt(SensationIndex));
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            SensationIndex--;
            device.SetSensation(sensations.ElementAt(SensationIndex));
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            TrackHand = !TrackHand;
            if (!TrackHand)
            {
                device.GetSensation().RestoreTransform();
            }
        }

        if (TrackHand
            && !forceFieldController.IsActive)
        {
            Frame frame = Hands.Provider.CurrentFrame;
            if (frame.Hands.Count > 0)
            {
                Hand hand = frame.Hands[0];
                device.GetSensation().position = hand.PalmPosition;
                device.GetSensation().rotation = Quaternion.LookRotation(hand.DistalAxis(), hand.PalmarAxis()).eulerAngles;

                device.maxIntensity = 1;
            }
            else
            {
                device.GetSensation().RestoreTransform();
                device.maxIntensity = 0;
            }
        }
        else
        {
            Frame frame = Hands.Provider.CurrentFrame;
            if (frame.Hands.Count == 0)
            {
                device.maxIntensity = 0;
            }
            else
            {
                device.maxIntensity = 1;
            }
        }
    }
}