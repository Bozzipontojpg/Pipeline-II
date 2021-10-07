using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshObjects : MonoBehaviour
{
    public List<GameObject> assets = new List<GameObject>();
    public MeshFilter terrainMesh;

    private List<Vector3> verticesPos = new List<Vector3>();

    [Range(10, 500)]
    public int amountToSpawn;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public Vector3 GetVertexWorldPosition(Vector3 vertex, Transform owner)
    {
        return owner.localToWorldMatrix.MultiplyPoint3x4(vertex);
    }

    public void GenerateAssets()
    {
        var localToWorld = transform.localToWorldMatrix;
        var vertices = terrainMesh.sharedMesh.vertices;

        foreach(var v in  vertices)
        {
            verticesPos.Add(GetVertexWorldPosition(v, terrainMesh.transform));
        }
        

        for(int i = 0; i< amountToSpawn; i++)
        {
            GameObject obj = Instantiate(assets[Random.Range(0,assets.Count)], verticesPos[Random.Range(0, verticesPos.Count)], Quaternion.identity);
            obj.transform.parent = terrainMesh.transform;
        }
    }

    
}