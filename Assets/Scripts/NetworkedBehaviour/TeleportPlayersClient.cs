using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

public class TeleportPlayersClient : NetworkBehaviour
{
    [Rpc(SendTo.Server)]
    public void TeleportPlayerRPC(ulong clientID, Vector3 teleportPos, Quaternion teleportRot)
    {
        if(OwnerClientId != clientID)
            return;
        
        var oldPosition = transform.position;
        var oldRotation = transform.rotation;
        transform.position = teleportPos;
        transform.rotation = teleportRot;
        var newPosition = transform.position;
        var newRotation = transform.rotation;
        
        print($"{nameof(TeleportPlayerRPC)}() -> {nameof(OwnerClientId)}: {OwnerClientId} --- {nameof(oldPosition)}: {oldPosition} || {nameof(oldRotation)}: {oldRotation}" +
              $" --- {nameof(newPosition)}: {newPosition} || {nameof(newRotation)}: {newRotation}");
    }
}
