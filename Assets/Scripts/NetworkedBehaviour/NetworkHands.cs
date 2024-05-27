// Credits to https://docs.ultraleap.com/xr-and-tabletop/xr/unity/plugin/features/networking-hands.html
using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;

using UnityEngine;
using Unity.Netcode;

public class NetworkHands : NetworkBehaviour
{
    [SerializeField]
    private HandModelBase leftModel = null, rightModel = null;

    private LeapProvider leapProvider;

    private VectorHand leftVector = new VectorHand(), rightVector = new VectorHand();
    private Hand leftHand = new Hand(), rightHand = new Hand();

    private byte[] leftBytes = new byte[VectorHand.NUM_BYTES], rightBytes = new byte[VectorHand.NUM_BYTES];

    private bool leftTracked, rightTracked;

    private void Awake()
    {
        // Find the most suitable LeapProvider in the scene automatically
        leapProvider = Hands.Provider;
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // We own the hands, so we will be sending the data across the network
            leapProvider.OnUpdateFrame += OnUpdateFrame;
            //Destroy(leftModel?.gameObject);
            //Destroy(rightModel?.gameObject);
        }
        else
        {
            // We are going to be sent hand data for these hands.
            // We should control the hands directly, not from a LeapProvider
            leftModel.leapProvider = null;
            rightModel.leapProvider = null;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            // We no longer need this event as we have disconnected from the network
            leapProvider.OnUpdateFrame -= OnUpdateFrame;
        }
    }

    private void OnUpdateFrame(Frame frame)
    {
        // Find the left hand index and use it if it exists
        int ind = frame.Hands.FindIndex(x => x.IsLeft);
        if(ind != -1)
        {
            // The left hand exists, encode the vector hand for it and fill the byte[] with data
            leftTracked = true;
            leftVector.Encode(frame.Hands[ind]);
            leftVector.FillBytes(leftBytes);
        }
        else
        {
            leftTracked = false;
        }

        ind = frame.Hands.FindIndex(x => !x.IsLeft);
        if(ind != -1)
        {
            // The right hand exists, encode the vector hand for it and fill the byte[] with data
            rightTracked = true;
            rightVector.Encode(frame.Hands[ind]);
            rightVector.FillBytes(rightBytes);
        }
        else
        {
            rightTracked = false;
        }

        // Send any data we have generated to the server to be disributed across the network
        UpdateHandServerRpc(NetworkManager.LocalClientId, leftTracked, rightTracked, leftBytes, rightBytes);
    }

    [ServerRpc]
    private void UpdateHandServerRpc(ulong clientId, bool leftTracked, bool rightTracked, byte[] leftHand, byte[] rightHand)
    {
        if (!IsServer) return;

        // As the server, we should directly Load the data we were given
        LoadHandsData(leftTracked, rightTracked, leftHand, rightHand);

        // Send the data on to all clients for use on their hands
        UpdateHandClientRpc(clientId, leftTracked, rightTracked, leftHand, rightHand);
    }

    [ClientRpc]
    private void UpdateHandClientRpc(ulong clientId, bool leftTracked, bool rightTracked, byte[] leftHand, byte[] rightHand)
    {
        // If we own this object, we do not need to load the hand data, we produced it!
        if (IsOwner) return;

        // Load the other client's hand data into our copy of their hands
        LoadHandsData(leftTracked, rightTracked, leftHand, rightHand);
    }

    private void LoadHandsData(bool leftTracked, bool rightTracked, byte[] leftHand, byte[] rightHand)
    {
        if (leftModel != null)
        {
            leftModel.gameObject.SetActive(leftTracked);

            if (leftTracked)
            {
                // Read the new data into the vector hand and then decode it into a Leap.Hand to be send to the hand model
                leftVector.ReadBytes(leftHand);
                leftVector.Decode(this.leftHand);
                leftModel?.SetLeapHand(this.leftHand);
                leftModel?.UpdateHand();
            }
        }

        if(rightModel != null)
        {
            rightModel.gameObject.SetActive(rightTracked);

            if (rightTracked)
            {
                // Read the new data into the vector hand and then decode it into a Leap.Hand to be send to the hand model
                rightVector.ReadBytes(rightHand);
                rightVector.Decode(this.rightHand);
                rightModel?.SetLeapHand(this.rightHand);
                rightModel?.UpdateHand();
            }
        }
    }
}
