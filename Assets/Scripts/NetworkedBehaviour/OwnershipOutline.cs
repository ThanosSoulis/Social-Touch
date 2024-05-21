using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Serialization;

public class OwnershipOutline : NetworkBehaviour
{
    [SerializeField]
    private Material outlineMaterial;
    [SerializeField]
    private SkinnedMeshRenderer[] handSkinnedMeshRenderers;
    
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        if(!IsOwner) return;

        Debug.Log("Changing material");
        foreach (var skinnedMeshRenderer in handSkinnedMeshRenderers)
        {
            skinnedMeshRenderer.materials[0] = outlineMaterial;
        }
    }
}
