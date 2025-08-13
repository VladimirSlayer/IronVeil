using UnityEngine;
using UnityEngine.AI;

public class NavMeshCleaner : MonoBehaviour
{
    void Start()
    {
        NavMesh.RemoveAllNavMeshData();
    }
}
