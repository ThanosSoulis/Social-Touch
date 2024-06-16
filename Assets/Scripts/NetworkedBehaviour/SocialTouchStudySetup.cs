using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
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
        // _networkManager.OnConnectionEvent += ConnectionEventAction;
        _networkManager.OnClientConnectedCallback += ClientConnectedServerHandler;
        _networkManager.OnClientDisconnectCallback += ClientDisconnectedServerHandler;
    }
    public void Destroy()
    {
        // _networkManager.OnConnectionEvent -= ConnectionEventAction;
        _networkManager.OnClientConnectedCallback -= ClientConnectedServerHandler;
        _networkManager.OnClientDisconnectCallback -= ClientDisconnectedServerHandler;
    }
    
    private void ConnectionEventAction(NetworkManager networkManager, ConnectionEventData connectionData)
    {
        switch(connectionData.EventType) 
        {
            case ConnectionEvent.ClientConnected:
                ClientConnectedHandler(networkManager, connectionData);
                break;
            case ConnectionEvent.ClientDisconnected:
                ClientDisconnectedHandler(networkManager, connectionData);
                break;
            default:
                Debug.Log("[Social Touch] Unhandled Connection Event");
                break;
        }
    }

    private void ClientConnectedHandler(NetworkManager networkManager, ConnectionEventData connectionData)
    {
        Debug.Log("[Social Touch] Client Connected");

        if (networkManager.IsClient)
        {
            Debug.Log("[Social Touch] No Spawning for Client");
        }

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
        
        Debug.Log($"[Social Touch] Spawned User Networked Prefab for ClientID=|{clientId}|");
    }
    
    private void ClientDisconnectedHandler(NetworkManager networkManager, ConnectionEventData connectionData)
    {
        ClientDisconnectedServerHandler(connectionData.ClientId);
    }

    private void ClientDisconnectedServerHandler(ulong clientId)
    {
        Debug.Log($"[Social Touch] Client {clientId} Disconnected");
    }

}
