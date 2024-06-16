using Unity.Netcode;
using UnityEngine;

public class SocialTouchStudySetup : MonoBehaviour
{
    public Transform userTransformA, userTransformB;
    public GameObject userNetworkedPrefab;
    
    // TODO: So far the Interactable and Renderer side of the hands is in one prefab.
    // public GameObject ownedNetworkedPrefab;
    // public GameObject interactableNetworkedPrefab;    
    
    private NetworkManager _networkManager;
    public void Start()
    {   
        _networkManager = NetworkManager.Singleton;
        _networkManager.OnConnectionEvent += ConnectionEventAction;
    }
    public void Destroy()
    {
        _networkManager.OnConnectionEvent -= ConnectionEventAction;
    }
    
    private void ConnectionEventAction(NetworkManager networkManager, ConnectionEventData connectionData)
    {
        switch(connectionData.EventType) 
        {
            case ConnectionEvent.ClientConnected:
                ClientConnectedHandler(networkManager, connectionData);
                break;
            case ConnectionEvent.ClientDisconnected:
                ClientDisconnectedAction();
                break;
            default:
                Debug.Log("Unhandled Connection Event");
                break;
        }
    }

    private void ClientConnectedHandler(NetworkManager networkManager, ConnectionEventData connectionData)
    {
        Debug.Log("Client Connected");
                
        if (networkManager.IsClient) {}

        if (networkManager.IsServer || networkManager.IsHost)
            ClientConnectedServerHandler(connectionData.ClientId);
    }
    
    private void ClientConnectedServerHandler(ulong clientId)
    {
        var spawnTransform = userTransformA;
        if(clientId % 2 != 0)
            spawnTransform = userTransformB;
        
        var instance = Instantiate(userNetworkedPrefab, spawnTransform.position, spawnTransform.rotation);
        var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        instanceNetworkObject.SpawnAsPlayerObject(clientId,true);
    }
    
    private void ClientDisconnectedAction()
    {
        Debug.Log("Client Disconnected");
    }

}
