using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddSphereIDs : MonoBehaviour
{
    public bool SetIds;

    private DistributerChild[] _distributers; 

    void OnValidate()
    {
        if (SetIds == true) {
            SetIds = false;

            _distributers = FindObjectsByType<DistributerChild>(FindObjectsSortMode.None);
            Debug.Log(_distributers.Length);

            for (int i = 0; i < _distributers.Length; i++) {
                _distributers[i].SphereID = i+1;
            }
        }

    }
}
