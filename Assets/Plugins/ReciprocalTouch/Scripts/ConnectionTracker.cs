using UnityEngine;

public class ConnectionTracker : MonoBehaviour
{
    public GameObject[] PrimaryHands;

    public MammothRenderer[] MammothRenderers;

    private bool _isEnabled = true;

    void LateUpdate()
    {
        for (int i = 0; i < PrimaryHands.Length; i++)
        {
            if (PrimaryHands[i].activeSelf == false)
            {
                Disconnect();
                break;
            }
        }
    }

    private void Disconnect()
    {
        if (_isEnabled == false) {
            return;
        }

        for (int i = 0; i < MammothRenderers.Length; i++)
        {
            MammothRenderers[i].Disconnect();
        }
    }

    public void Enabled(bool isEnabled) {
        _isEnabled = isEnabled;
    }
}
