using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class SocialTouchNetworkManager : NetworkManager
{
    public Transform userTransformA, userTransformB;
    public GameObject ownedNetworkedPrefab;
    public GameObject interactableNetworkedPrefab;    
    
    public void Awake()
    {
        this.OnClientStarted += ClientStartedAction;
        this.OnServerStarted += ServerStartedAction;
    }
    public void Destroy()
    {
        this.OnClientStarted -= ClientStartedAction;
        this.OnServerStarted -= ServerStartedAction;
    }
    
    private void ServerStartedAction()
    {
        Debug.Log("Server started");
        // var instance = Instantiate(myPrefab);
        // var instanceNetworkObject = instance.GetComponent<NetworkObject>();
        // instanceNetworkObject.Spawn();
    }

    private void ClientStartedAction()
    {
        Debug.Log("Client started");
        
        
    }
}
