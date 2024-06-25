using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class TeleportPlayersManager : MonoBehaviour
{
    public Transform userTransformA, userTransformB;
    
    private NetworkManager _networkManager;
    private void Start()
    {
        _networkManager = NetworkManager.Singleton;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T)) TeleportPlayers();
    }

    private void TeleportPlayers()
    {
        // Only allow the server to teleport players
        if(_networkManager.IsClient)
            return;
        
        foreach (var clientPair in _networkManager.ConnectedClients)
        {
            var clientID = clientPair.Key;
            var client = clientPair.Value;

            var spawnTransform = userTransformA;
            if (clientID % 2 != 0)
                spawnTransform = userTransformB;

            if (client.PlayerObject.TryGetComponent(out TeleportPlayersClient teleportPlayersClient))
                teleportPlayersClient.TeleportPlayerRPC(clientID, spawnTransform.position, spawnTransform.rotation);
        }
    }
}
