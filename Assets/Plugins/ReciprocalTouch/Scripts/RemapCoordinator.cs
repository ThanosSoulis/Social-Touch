using System.Collections.Generic;
using UnityEngine;

public class RemapCoordinator : MonoBehaviour
{
    public bool SetIds = false;
    public bool EnableDisable = false;

    private DistributerChild[] _unsortedDistributerChildren;
    public DistributerChild[] DistributerChildren;

    public List<Transform> GetRemappedTransforms (List<Transform> staticTransforms, int count) {
        List<Transform> transforms = new List<Transform>();

        for (int i = 0; i < count; i++) {
            transforms.Add(DistributerChildren[staticTransforms[i].GetComponent<DistributerChild>().SphereID - 1].transform);
        }
        return transforms;
    }
    public Transform GetTransformFromID(int id) {
        return DistributerChildren[id-1].transform;
    }

    void OnValidate() {
        if (SetIds == true) {
            SetIds = false;

            _unsortedDistributerChildren = FindObjectsByType<DistributerChild>(FindObjectsSortMode.None);
            DistributerChildren = new DistributerChild[_unsortedDistributerChildren.Length];


            for (int i = 0; i < _unsortedDistributerChildren.Length; i++) {
                DistributerChildren[_unsortedDistributerChildren[i].SphereID - 1] = _unsortedDistributerChildren[i];
            }
        }

        if (EnableDisable == true) {
            EnableDisable = false;
            
            for (int i = 0; i < DistributerChildren.Length; i++)
            {
                DistributerChildren[i].Disable = !DistributerChildren[i].Disable;
            }
        }
    }
}