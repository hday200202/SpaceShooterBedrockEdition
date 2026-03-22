using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(PolygonCollider2D), typeof(MeshFilter), typeof(MeshRenderer))]
public class MapRenderer : MonoBehaviour {
    private PolygonCollider2D poly;
    private MeshFilter meshFilter;

    void OnValidate() { GenerateMapMesh(); }
    void Start() { GenerateMapMesh(); }

    public void GenerateMapMesh() {
        if (poly == null) poly = GetComponent<PolygonCollider2D>();
        if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();

        Mesh mesh = poly.CreateMesh(true, true);

        meshFilter.sharedMesh = mesh;
    }

    void OnDrawGizmosSelected() {
        GenerateMapMesh();
    }
}