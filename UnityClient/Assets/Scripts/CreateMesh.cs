using System.Collections.Generic;
using UnityEngine;
 
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class CreateMesh : MonoBehaviour
{
    public int TileWidth = 16;
    public int TileHeight = 16;
    public int NumTilesX = 16;
    public int NumTilesZ = 16;
    public int TileGridWidth = 100;
    public int TileGridHeight = 100;
    public int DefaultTileX;
    public int DefaultTileZ;
    public Texture2D Tex1;
    public Texture2D Tex2;
 
    void OnEnable()
    {
        CreatePlane(TileWidth, TileHeight, TileGridWidth, TileGridHeight);
   
    }
 
    void Update()
    {
        var tileColumn = Random.Range(0, NumTilesX);
        var tileRow = Random.Range(0, NumTilesZ);
 
        var x = Random.Range(0, TileGridWidth);
        var z = Random.Range(0, TileGridHeight);
 
        UpdateGrid(new Vector2(x, z), new Vector2(tileColumn, tileRow), TileWidth, TileHeight, TileGridWidth);
    }
 
    public void UpdateGrid(Vector2 gridIndex, Vector2 tileIndex, int tileWidth, int tileHeight, int gridWidth)
    {
        var mesh = GetComponent<MeshFilter>().mesh;
        var uvs = mesh.uv;
 
        var tileSizeX = 1.0f / NumTilesX;
        var tileSizeZ = 1.0f / NumTilesZ;
 
        mesh.uv = uvs;
 
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 0] = new Vector2(tileIndex.x * tileSizeX, tileIndex.y * tileSizeZ);
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 1] = new Vector2((tileIndex.x + 1) * tileSizeX, tileIndex.y * tileSizeZ);
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 2] = new Vector2((tileIndex.x + 1) * tileSizeX, (tileIndex.y + 1) * tileSizeZ);
        uvs[(int)(gridWidth * gridIndex.x + gridIndex.y) * 4 + 3] = new Vector2(tileIndex.x * tileSizeX, (tileIndex.y + 1) * tileSizeZ);
 
        mesh.uv = uvs;
    }
 
    void CreatePlane(int tileHeight, int tileWidth, int gridHeight, int gridWidth)
    {
        var mesh = new Mesh();
        var mf = GetComponent<MeshFilter>();
        mf.GetComponent<Renderer>().material.SetTexture("_MainTex", Tex1);
        mf.mesh = mesh;
 
        var tileSizeX = 1.0f / NumTilesX;
        var tileSizeZ = 1.0f / NumTilesZ;
 
        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var normals = new List<Vector3>();
        var uvs = new List<Vector2>();
 
        var index = 0;
        for (var x = 0; x < gridWidth; x++) {
            for (var z = 0; z < gridHeight; z++) {
                AddVertices(tileHeight, tileWidth, z, x, vertices);
                index = AddTriangles(index, triangles);
                AddNormals(normals);
                AddUvs(DefaultTileX, tileSizeZ, tileSizeX, uvs, DefaultTileZ);
            }
        }
 
        mesh.vertices = vertices.ToArray();
        mesh.normals = normals.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();
    }
 
    private static void AddVertices(int tileHeight, int tileWidth, int z, int x, ICollection<Vector3> vertices)
    {
        vertices.Add(new Vector3((x * tileWidth), 0, (z * tileHeight)));
        vertices.Add(new Vector3((x * tileWidth) + tileWidth, 0, (z * tileHeight)));
        vertices.Add(new Vector3((x * tileWidth) + tileWidth, 0, (z * tileHeight) + tileHeight));
        vertices.Add(new Vector3((x * tileWidth), 0, (z * tileHeight) + tileHeight));
    }
 
    private static int AddTriangles(int index, ICollection<int> triangles)
    {
        triangles.Add(index + 2);
        triangles.Add(index + 1);
        triangles.Add(index);
        triangles.Add(index);
        triangles.Add(index + 3);
        triangles.Add(index + 2);
        index += 4;
        return index;
    }
 
    private static void AddNormals(ICollection<Vector3> normals)
    {
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
        normals.Add(Vector3.forward);
    }
 
    private static void AddUvs(int tileRow, float tileSizeZ, float tileSizeX, ICollection<Vector2> uvs, int tileColumn)
    {
        uvs.Add(new Vector2(tileColumn * tileSizeX, tileRow * tileSizeZ));
        uvs.Add(new Vector2((tileColumn + 1) * tileSizeX, tileRow * tileSizeZ));
        uvs.Add(new Vector2((tileColumn + 1) * tileSizeX, (tileRow + 1) * tileSizeZ));
        uvs.Add(new Vector2(tileColumn * tileSizeX, (tileRow + 1) * tileSizeZ));
    }
}
