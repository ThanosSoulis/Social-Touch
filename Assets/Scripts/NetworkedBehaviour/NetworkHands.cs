// Credits to https://docs.ultraleap.com/xr-and-tabletop/xr/unity/plugin/features/networking-hands.html
using Leap;
using Leap.Unity;
using Leap.Unity.Encoding;

using UnityEngine;
using Unity.Netcode;

public class NetworkHands : NetworkBehaviour
{
    [SerializeField] private bool isAutoInstantiated = true;
    [SerializeField] private HandModelBase leftModel = null, rightModel = null;
    
    private LeapProvider _leapProvider;

    private VectorHand _leftVector = new VectorHand(), _rightVector = new VectorHand();
    private Hand _leftHand = new Hand(), _rightHand = new Hand();

    private byte[] _leftBytes = new byte[VectorHand.NUM_BYTES], _rightBytes = new byte[VectorHand.NUM_BYTES];

    private bool _leftTracked, _rightTracked;

    private void Awake()
    {
        // Find the most suitable LeapProvider in the scene automatically
        _leapProvider = Hands.Provider;
    }
    
    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            // We own the hands, so we will be sending the data across the network
            _leapProvider.OnUpdateFrame += OnUpdateFrame;
            
            //Destroy model if it is not instantiated by the NetworkManager
            if (!isAutoInstantiated)
            {
                Destroy(leftModel?.gameObject);
                Destroy(rightModel?.gameObject);   
            }
        }
        else
        {
            // We are going to be sent hand data for these hands.
            // We should control the hands through network, not from a LeapProvider
            leftModel.leapProvider = null;
            rightModel.leapProvider = null;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            // We no longer need this event as we have disconnected from the network
            _leapProvider.OnUpdateFrame -= OnUpdateFrame;
        }
    }

    private void OnUpdateFrame(Frame frame)
    {
        int ind = frame.Hands.FindIndex(x => x.IsRight);
        if(ind != -1)
        {
            // The right hand exists, encode the vector hand for it and fill the byte[] with data
            _rightTracked = true;
            _rightVector.Encode(frame.Hands[ind]);
            _rightVector.FillBytes(_rightBytes);
        }
        else
        {
            _rightTracked = false;
        }
        
        // Find the left hand index and use it if it exists
        ind = frame.Hands.FindIndex(x => x.IsLeft);
        if(ind != -1)
        {
            // The left hand exists, encode the vector hand for it and fill the byte[] with data
            _leftTracked = true;
            _leftVector.Encode(frame.Hands[ind]);
            _leftVector.FillBytes(_leftBytes);
        }
        else
        {
            _leftTracked = false;
        }

        // Send any data we have generated to the server to be distributed across the network
        UpdateHandServerRpc(NetworkManager.LocalClientId, _leftTracked, _rightTracked, _leftBytes, _rightBytes);
    }
    
    [Rpc(SendTo.Server)]
    private void UpdateHandServerRpc(ulong clientId, bool leftTracked, bool rightTracked, byte[] leftHand, byte[] rightHand)
    {
        if (!IsServer) return;

        // As the server, we should directly Load the data we were given
        LoadHandsData(leftTracked, rightTracked, leftHand, rightHand);

        // Send the data on to all clients for use on their hands
        UpdateHandClientRpc(clientId, leftTracked, rightTracked, leftHand, rightHand);
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void UpdateHandClientRpc(ulong clientId, bool leftTracked, bool rightTracked, byte[] leftHand, byte[] rightHand)
    {
        // If we own this object, we do not need to load the hand data, we produced it!
        if (IsOwner) return;

        // Load the other client's hand data into our copy of their hands
        LoadHandsData(leftTracked, rightTracked, leftHand, rightHand);
    }

    private void LoadHandsData(bool leftTracked, bool rightTracked, byte[] leftHand, byte[] rightHand)
    {
        if(rightModel != null)
        {
            rightModel.gameObject.SetActive(rightTracked);

            if (rightTracked)
            {
                // Read the new data into the vector hand and then decode it into a Leap.Hand to be send to the hand model
                _rightVector.ReadBytes(rightHand);
                _rightVector.Decode(this._rightHand);
                rightModel?.SetLeapHand(this._rightHand);
                rightModel?.UpdateHand();
            }
        }
        
        if (leftModel != null)
        {
            leftModel.gameObject.SetActive(leftTracked);

            if (leftTracked)
            {
                // Read the new data into the vector hand and then decode it into a Leap.Hand to be send to the hand model
                _leftVector.ReadBytes(leftHand);
                _leftVector.Decode(this._leftHand);
                leftModel?.SetLeapHand(this._leftHand);
                leftModel?.UpdateHand();
            }
        }
    }
}
