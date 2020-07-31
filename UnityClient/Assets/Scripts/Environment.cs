using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using Unity.Mathematics;
using Unity.Entities.UniversalDelegates;
using System.Linq;

public struct Matrix4x4Component : IComponentData
{
   public Matrix4x4 Value;
}

public class Chunk
{
   public bool active = true;
   private Tuple<int, int> key;
   private Dictionary<Tuple<int, int>, Entity> tiles;
   public Entity terrain;

   public Chunk(Env env, Entity terrain, int r, int c)
   {
      this.tiles      = new Dictionary<Tuple<int, int>, Entity>();
      this.key        = Tuple.Create(r, c);
      this.terrain    = terrain;

      //this.terrain.transform.SetParent(root.transform);
      //this.terrain.name = "Chunk-R" + r.ToString() + "-C" + c.ToString();
   }

   public Entity GetTile(int tileR, int tileC)
   {
      Tuple<int, int> key = Tuple.Create(tileR, tileC);
      return this.tiles[key];
   }

   public bool ContainsTile(int tileR, int tileC) {
      Tuple<int, int> key = Tuple.Create(tileR, tileC);
      return this.tiles.ContainsKey(key);
   }
 
   public void SetTile(int tileR, int tileC, Entity ent)
   {
      Tuple<int, int> key = Tuple.Create(tileR, tileC);
      this.tiles[key] = ent;
   }

   public void SetActive(EntityManager manager, bool active)
   {
      this.active = active;
      manager.SetEnabled(this.terrain, active); 
      foreach(Entity e in this.tiles.Values)
      {
         manager.SetEnabled(e, active); 
      }
   }

   public bool GetIsActive()
   {
      return this.active;
   }

}

public class Env
{
   private Dictionary<Tuple<int, int>, Chunk> chunks;
   public  HashSet<Tuple<int, int>> activeChunks;

   public Env()
   {
      this.chunks       = new Dictionary<Tuple<int, int>, Chunk>();
      this.activeChunks = new HashSet<Tuple<int, int>>();
   }

   public Chunk GetChunk(int chunkR, int chunkC)
   {
      Tuple<int, int> key = Tuple.Create(chunkR, chunkC);
      return this.chunks[key];
   }
   public Chunk GetChunk(Tuple<int, int> key)
   {
      return this.chunks[key];
   }
 
   public void SetChunk(Chunk chunk, int chunkR, int chunkC)
   {
      Tuple<int, int> key = Tuple.Create(chunkR, chunkC);
      this.chunks[key] = chunk;
   }
   public void SetChunk(Chunk chunk, Tuple<int, int> key)
   {
      this.chunks[key] = chunk;
   }
 
   public bool ContainsChunk(int chunkR, int chunkC)
   {
      Tuple<int, int> key = Tuple.Create(chunkR, chunkC);
      return this.chunks.ContainsKey(key);
   }
 
   public Entity GetTile(int tileR, int tileC)
   {
      int chunkR = (int) Math.Floor((float) tileR / Consts.CHUNK_SIZE);
      int chunkC = (int) Math.Floor((float) tileC / Consts.CHUNK_SIZE);

      return this.GetChunk(chunkR, chunkC).GetTile(tileR, tileC);
   }
   public bool ContainsTile(int tileR, int tileC) {
      int chunkR = (int) Math.Floor((float) tileR / Consts.CHUNK_SIZE);
      int chunkC = (int) Math.Floor((float) tileC / Consts.CHUNK_SIZE);

      if (!this.ContainsChunk(chunkR, chunkC))
      {
         return false;
      }
      return this.GetChunk(chunkR, chunkC).ContainsTile(tileR, tileC);
   }
   public void SetActive(EntityManager manager, int r, int c, bool active)
   {
      Tuple<int, int> key = Tuple.Create(r, c);
      this.SetActive(manager, key, active);
  }

   public void SetActive(EntityManager manager, Tuple<int, int> key, bool active)
   {
      Chunk chunk = this.GetChunk(key);
      //Check if already in desired state
      if (chunk.active == active)
      {
         return;
      }

      if (active)
      {
         this.activeChunks.Add(key);
      } else
      {
         this.activeChunks.Remove(key);
      }

      chunk.SetActive(manager, active);
   }
 
   public void UpdateActive(Tuple<int, int> key, bool active)
   {
      bool containsKey = this.activeChunks.Contains(key);
      if (active && !containsKey)
      {
         this.activeChunks.Add(key);
      } else if (!active && containsKey)
      {
         this.activeChunks.Remove(key);
      }
   }
      
 
}

public class EnvMaterials : MonoBehaviour
{
    public  Dictionary<int, string> idxStrs;
    private Dictionary<int, GameObject> idxObjs;
    private Dictionary<string, GameObject> strObjs;

   public EnvMaterials()
   {
    this.idxStrs = new Dictionary<int, string>();
    this.idxObjs = new Dictionary<int, GameObject>();
    this.strObjs = new Dictionary<string, GameObject>();

    //this.AddBlock("Lava", 0, 0);
    this.idxStrs.Add(0, "Sand");
    this.idxStrs.Add(1, "Sand");
    this.AddBlock("Grass", 0, 2);
    this.AddBlock("Scrub", 0, 3);
    this.AddBlock("Forest", 0, 4);
    this.idxStrs.Add(5, "Stone");

    string matKeys = "0a 1a 2a 2b 2c 3a 3b 3c 4a 4b 4c 4d 4e 4f";
    foreach(string key in matKeys.Split(' '))
    {
         this.AddBlock("Stone" + key, 0);
         this.AddBlock("Sand" + key, 0);
    }
   }
    public void AddBlock(string name, float rotation, int idx=-1) {
         GameObject prefab = Resources.Load("Prefabs/Tiles/" + name) as GameObject;
         GameObject obj = Instantiate(prefab) as GameObject;
         obj.transform.eulerAngles = new Vector3(0, rotation, 0);
         if (!this.strObjs.ContainsKey(name))
         {
            this.strObjs.Add(name, obj);
         }
         if (idx != -1)
         {
            this.idxObjs.Add(idx, obj);
         print("Add String: " + name);
         this.idxStrs.Add(idx, name);
         }
    }

   public GameObject GetObj(string key)
   {
      return this.strObjs[key];
   }

   public GameObject GetObj(int key)
   {
      return this.idxObjs[key];
   }
   public string GetName(int key)
   {
      return this.idxStrs[key];
   }
}

public class Tile
{
   public string name;
   public int rot;
   public bool flip;

   public Tile(string name, int rot, bool flip)
   {
      this.name = name;
      this.rot  = rot;
      this.flip = flip;
   }
}

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Environment: MonoBehaviour
{
    //public static Dictionary<int, Texture2D> tiles   = new Dictionary<int, Texture2D>();
    GameObject root;
    GameObject cameraAnchor;
    public Env env = new Env();
    public int[,] vals;
    public Texture2D values;
    Dictionary<string, object> overlays;
    Dictionary<byte, Tile> meshHash;
    int overlayR;
    int overlayC;


    public EnvMaterials envMaterials;

    EntityManager entityManager;
    EntityArchetype chunkArchetype;
    EntityArchetype scrubArchetype;
    EntityArchetype containerArchetype;

    public static List<List<GameObject>> terrain = new List<List<GameObject>>();
    public int[,] oldPacket = new int[Consts.MAP_SIZE, Consts.MAP_SIZE];
    public GameObject[,] objs = new GameObject[Consts.MAP_SIZE, Consts.MAP_SIZE];

    Dictionary<Tuple<int, int>, GameObject> chunks = new Dictionary<Tuple<int, int>, GameObject>();
    Queue<Tuple<int, int>> loadedChunks   = new Queue<Tuple<int, int>>();
    Queue<Tuple<int, int>> unloadedChunks = new Queue<Tuple<int, int>>();

    public int tick = 0;
    string cmd = "";

    Shader shader;

    GameObject cubePrefab;
    GameObject forestPrefab;
    GameObject resources;
    Console    console;

    bool first = true;

    Material cubeMatl;
    Material overlayMatl;

    MeshRenderer renderer;

   GameObject forest;
   Material scrubMaterial;
   Mesh scrubMesh;


    void OnEnable()
    {
      this.root         = GameObject.Find("Environment/Terrain");
      this.resources    = GameObject.Find("Client/Environment/Terrain/Resources");
      this.cubeMatl     = Resources.Load("Prefabs/Tiles/CubeMatl") as Material;
      this.values       = new Texture2D(2*Consts.TILE_RADIUS, 2*Consts.TILE_RADIUS);
      this.cubePrefab   = Resources.Load("Prefabs/Cube") as GameObject;
      this.forestPrefab = Resources.Load("LowPoly Style/Free Rocks and Plants/Prefabs/Reed") as GameObject;
      this.console      = GameObject.Find("Console").GetComponent<Console>();
      this.shader       = Shader.Find("Standard");
      this.cameraAnchor = GameObject.Find("CameraAnchor");

      this.envMaterials = new EnvMaterials();

      this.entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
      this.scrubArchetype = this.entityManager.CreateArchetype(
         typeof(Translation),
         typeof(Rotation),
         typeof(NonUniformScale),
         typeof(RenderMesh),
         typeof(RenderBounds),
         typeof(LocalToWorld)
      );

      this.chunkArchetype = this.entityManager.CreateArchetype(
         typeof(Translation),
         typeof(RenderMesh),
         typeof(RenderBounds),
         typeof(LocalToWorld)
      );

      this.containerArchetype = this.entityManager.CreateArchetype(
         typeof(Child),
         typeof(Translation),
         typeof(LocalToWorld)
      ); ;

      this.forest = Instantiate(this.forestPrefab) as GameObject;
      this.scrubMaterial = forest.GetComponent<MeshRenderer>().material;
      this.scrubMesh = forest.GetComponent<MeshFilter>().sharedMesh;

      int R = Consts.CHUNKS / 2;
      int x = 0;
      int y = 0;
      int temp = 0;
      int dx = 0;
      int dy = -1;
      //for (int i = 0; i < (Consts.CHUNKS+1)*(Consts.CHUNKS+1); i++)
      for (int i = 0; i < Consts.CHUNKS*Consts.CHUNKS; i++)
      {
         //Debug.Log(y.ToString() + ", " + x.ToString());
         if (-R <= x && x <= R && -R <= y && y <= R)
         {
            int RR = y + R - 1;
            int CC = x + R - 1;
            unloadedChunks.Enqueue(Tuple.Create(y+R-1, x+R-1));
         }
         if (x == y || (x < 0 && x == -y) || (x > 0 && x == 1 - y))
         {
            temp = dx;
            dx = -dy;
            dy = temp;
         }
         x = x + dx;
         y = y + dy;
      }

      this.makeMeshHash();

   }

    public void UpdateMap(Dictionary<string, object> packet) {
      GameObject root  = GameObject.Find("Environment");
      List<object> map = (List<object>) packet["map"];
      this.overlays    = (Dictionary<string, object>) packet["overlay"];
      this.overlayMatl = Resources.Load("Prefabs/Tiles/OverlayMaterial") as Material;
      this.overlayMatl.SetTexture("_Overlay", Texture2D.blackTexture);
      List<object> overlayPos = (List<object>) packet["pos"];

      string cmd = this.console.cmd;
      this.cmd = cmd;
      if (this.overlays.ContainsKey(cmd))
      {
         int count = 0;
         List<object> values = (List<object>) this.overlays[cmd];
         Color[] pixels = new Color[2*2*Consts.TILE_RADIUS*Consts.TILE_RADIUS];
         for (int r = 0; r < 2*Consts.TILE_RADIUS; r++)
         {
            List<object> row = (List<object>)values[r];
            for (int c = 0; c < 2*Consts.TILE_RADIUS; c++)
            {
               List<object> col = (List<object>)row[c];
               Color value = new Color();
               for (int i = 0; i < 3; i++)
               {
                  value[i] = System.Convert.ToSingle(col[i]);
               }
               value.a = 0f;
               pixels[count] = value;
               count++;
            }
        }
        this.values.SetPixels(pixels);
        this.values.Apply(false);
      }

      int cameraR = (int) Math.Floor(this.cameraAnchor.transform.position.x / Consts.CHUNK_SIZE) * Consts.CHUNK_SIZE;
      int cameraC = (int) Math.Floor(this.cameraAnchor.transform.position.z / Consts.CHUNK_SIZE) * Consts.CHUNK_SIZE;

      this.overlayR = System.Convert.ToInt32(overlayPos[0]);
      this.overlayC = System.Convert.ToInt32(overlayPos[1]);

      for (int r=-Consts.TILE_RADIUS; r<Consts.TILE_RADIUS; r++) {
         if (r + cameraR < 0 || r + cameraR >= Consts.MAP_SIZE)
         {
            continue;
         }
         //List<object> row = (List<object>) map[r+cameraR];
         List<object> row = (List<object>) map[r+Consts.TILE_RADIUS];
         for (int c=-Consts.TILE_RADIUS; c<Consts.TILE_RADIUS; c++) {
            if (c + cameraC < 0 || c + cameraC >= Consts.MAP_SIZE)
            {
               continue;
            }
            if (!this.env.ContainsTile(r+cameraR, c+cameraC))
            {
               continue;
            }
            //int val = System.Convert.ToInt32(row[c+cameraC]);
            int val = System.Convert.ToInt32(row[c+Consts.TILE_RADIUS]);
            Entity tile = this.env.GetTile(r + cameraR, c + cameraC);
            string name = this.envMaterials.idxStrs[val];
            if (name == "Forest" ) {
               this.entityManager.SetEnabled(tile, true);
            } else if (name == "Scrub") {
               this.entityManager.SetEnabled(tile, false);
            }
         }
      }
    }

   byte makeByteRepr(
           int r0c0, int r0c1, int r0c2,
           int r1c0,           int r1c2,
           int r2c0, int r2c1, int r2c2) { 
        byte byteRepr = 0;
        byteRepr += (byte) (r0c0 << 7);
        byteRepr += (byte) (r0c1 << 6);
        byteRepr += (byte) (r0c2 << 5);
        byteRepr += (byte) (r1c0 << 4);
        byteRepr += (byte) (r1c2 << 3);
        byteRepr += (byte) (r2c0 << 2);
        byteRepr += (byte) (r2c1 << 1);
        byteRepr += (byte) (r2c2 << 0);
        return byteRepr;
   }
 
    void makeMeshHash()
    {
        this.meshHash = new Dictionary<byte, Tile>();

        this.addTemplate(meshHash, "0a", false, true, false, false, false,
             -1, -1, -1,
             -1, -1, -1,
             -1, -1, -1);
     
        this.addTemplate(meshHash, "4f", false,
            true, false, false, false,
             1,  1,  1, 
             1,  1,  1, 
             1,  1,  1);

        this.addTemplate(meshHash, "4e", false,
            true, true, true, true,
             1,  1,  1, 
             1,  1,  1, 
             1,  1,  0);

        this.addTemplate(meshHash, "4d", false,
            true, true, false, false,
             0,  1,  1, 
             1,  1,  1, 
             1,  1,  0);

        this.addTemplate(meshHash, "4c", false,
            true, true, true, true,
             1,  1,  1, 
             1,  1,  1, 
             0,  1,  0);

        this.addTemplate(meshHash, "4b", false,
            true, true, true, true,
             0,  1,  1, 
             1,  1,  1, 
             0,  1,  0);

        this.addTemplate(meshHash, "4a", false,
            true, false, false, false,
             0,  1,  0, 
             1,  1,  1, 
             0,  1,  0);

        this.addTemplate(meshHash, "3b", false,
            true, true, true, true,
             1,  1, -1, 
             1,  1,  1, 
            -1,  0, -1);

        this.addTemplate(meshHash, "3b", true,
            true, true, true, true,
             0,  1,  1, 
             1,  1,  1, 
            -1,  0, -1);

        this.addTemplate(meshHash, "3c", false,
            true, true, true, true,
             1,  1,  1, 
             1,  1,  1, 
            -1,  0, -1);

        this.addTemplate(meshHash, "3a", false,
            true, true, true, true,
             0,  1,  0, 
             1,  1,  1, 
            -1,  0, -1);

        this.addTemplate(meshHash, "2c", false,
            true, true, true, true,
             1,  1, -1,
             1,  1,  0,
            -1,  0, -1);

        this.addTemplate(meshHash, "2b", false,
            true, true, false, false,
            -1,  0, -1, 
             1,  1,  1, 
            -1,  0, -1);

        this.addTemplate(meshHash, "2a", false,
            true, true, true, true,
             0,  1, -1,
             1,  1,  0,
            -1,  0, -1); 

        this.addTemplate(meshHash, "1a", false,
            true, true, true, true,
            -1,  1, -1,
             0,  1,  0,
            -1,  0, -1);

      this.meshHash = meshHash;
    }

   void addTemplate(Dictionary<byte, Tile> meshHash, string meshName, bool flip,
            bool rot0, bool rot90, bool rot180, bool rot270,
            int r0c0, int r0c1, int r0c2, 
            int r1c0, int r1c1, int r1c2, 
            int r2c0, int r2c1, int r2c2) {

      if (rot0) {
         this.addWildcards(meshHash, meshName, 0, flip, new List<int>{
            r0c0, r0c1, r0c2,
            r1c0,       r1c2, 
            r2c0, r2c1, r2c2});
      }
      if (rot90) {
         this.addWildcards(meshHash, meshName, 90, flip, new List<int>{
            r2c0, r1c0, r0c0,
            r2c1,       r0c1, 
            r2c2, r1c2, r0c2});
      }
      if (rot180) {
         this.addWildcards(meshHash, meshName, 180, flip, new List<int>{
            r2c2, r2c1, r2c0,
            r1c2,       r1c0, 
            r0c2, r0c1, r0c0});
      }
      if (rot270) {
         this.addWildcards(meshHash, meshName, 270, flip, new List<int>{
            r0c2, r1c2, r2c2,
            r0c1,       r2c1, 
            r0c0, r1c0, r2c0});
      }
    }

    Tuple<Mesh, Matrix4x4> mapBlock(int rOff, int cOff, int r, int c, int val)
    {
         Entity cube;
         int rot = 0;
         bool flip = false;

         string name = this.envMaterials.idxStrs[val];
         if (r == 0 || c == 0 || r == Consts.MAP_SIZE-1 || c == Consts.MAP_SIZE-1)
         {
            name = "Sand4f";
         } else  if (name == "Stone" || name == "Sand") {
            byte byteRepr = makeByteRepr(
               (val == this.vals[r - 1, c - 1]) ? 1 : 0,
               (val == this.vals[r - 1, c])     ? 1 : 0,
               (val == this.vals[r - 1, c + 1]) ? 1 : 0,
               (val == this.vals[r, c - 1])     ? 1 : 0,
               (val == this.vals[r, c + 1])     ? 1 : 0,
               (val == this.vals[r + 1, c - 1]) ? 1 : 0,
               (val == this.vals[r + 1, c])     ? 1 : 0,
               (val == this.vals[r + 1, c + 1]) ? 1 : 0);

            Tile tile = this.meshHash[byteRepr];
            rot = tile.rot;
            flip = tile.flip;
            string suffix = tile.name;
            name += suffix;
         }

         GameObject obj = this.envMaterials.GetObj(name);

         Vector3    translate = new Vector3(rOff, 0, cOff);
         Quaternion rotate    = Quaternion.Euler(new Vector3(0, rot, 0));
         Vector3    scale     = new Vector3(1, 1, 1);
         
         Mesh mesh = obj.GetComponentInChildren<MeshFilter>().sharedMesh;

         if (flip) {
            scale = new Vector3(1, 1, -1);
         }

         Matrix4x4 transform = Matrix4x4.TRS(translate, rotate, scale);
         Tuple<Mesh, Matrix4x4> ret = Tuple.Create(mesh, transform);
         return ret;

    }

    //Recursively fill wilcards with 0s or 1s, adding all possible binary strings to meshHash
    void addWildcards(Dictionary<byte, Tile> meshHash, string meshName, int rot, bool flip, List<int> repr) {
         int wildcard = repr.IndexOf(-1);
         if (wildcard == -1) {
            byte byteRepr = makeByteRepr(repr[0], repr[1], repr[2], repr[3], repr[4], repr[5], repr[6], repr[7]);
            if (meshHash.ContainsKey(byteRepr))
            {
               meshHash.Remove(byteRepr);
            }
            Tile meshRepr = new Tile(meshName, rot, flip);
            meshHash.Add(byteRepr, meshRepr);
            return;
         }

         List<int> replaceZero = new List<int>(repr);
         replaceZero[wildcard] = 0;
         this.addWildcards(meshHash, meshName, rot, flip, replaceZero);

         List<int> replaceOne = new List<int>(repr);
         replaceOne[wildcard] = 1;
         this.addWildcards(meshHash, meshName, rot, flip, replaceOne);
    }
 
   void makeChunk(GameObject root, int sz, int R, int C) {
      R = R * sz;
      C = C * sz;

      int flatIdx = 0;
      Tuple<Mesh, Matrix4x4>[] cubes = new Tuple<Mesh, Matrix4x4>[256];
      for(int r=0; r<sz; r++) {
        for(int c=0; c<sz; c++) {
            int val = System.Convert.ToInt32(this.vals[R+r, C+c]);
            cubes[flatIdx] = this.mapBlock(r, c, R+r, C+c, val);
            flatIdx++;

        }
      }
      Entity terrain = combineMeshes(cubes, this.cubeMatl, R, C);
      Chunk chunk = new Chunk(this.env, terrain, R, C);
      this.env.SetChunk(chunk, R/sz, C/sz);
 
      for (int r = 0; r < sz; r++)
      {
         for (int c = 0; c < sz; c++)
         {
            Entity entity = this.entityManager.CreateEntity(this.scrubArchetype);
            this.entityManager.SetEnabled(entity, false);
            int val = System.Convert.ToInt32(this.vals[R + r, C + c]);
            string name = this.envMaterials.idxStrs[val];
            if (name == "Forest" || name == "Scrub") { 
               this.entityManager.AddComponentData(entity, new Translation      { Value = new float3(R + r, 0, C + c) } );
               this.entityManager.AddComponentData(entity, new Rotation         { Value = quaternion.Euler(new float3(0, UnityEngine.Random.Range(0, 360), 0))} );
               this.entityManager.AddComponentData(entity, new NonUniformScale  { Value = new float3(0.9f, 0.2f, 0.9f)} );
               this.entityManager.AddSharedComponentData(entity, new RenderMesh {mesh = scrubMesh, material = scrubMaterial} );

               chunk.SetTile(R + r, C + c, entity);
            }
         }
      }
      chunk.SetActive(this.entityManager, false); 
    }

    Entity combineMeshes(Tuple<Mesh, Matrix4x4>[] ents, Material mat, int R, int C) {
      CombineInstance[] combine = new CombineInstance[ents.Length];

      int i = 0;
      while (i < ents.Length)
      {
         Tuple<Mesh, Matrix4x4> ent = ents[i];

         combine[i].mesh = ent.Item1;
         combine[i].transform = ent.Item2;
         //meshFilters[i].gameObject.SetActive(false);
         //Destroy(meshFilters[i].gameObject);
         //Destroy(cubes[i]);
         i++;
      }


      Mesh mesh = new Mesh();
      mesh.CombineMeshes(combine, true, true);
      mesh.bounds.SetMinMax(new Vector3(0, 0, 0), new Vector3(Consts.CHUNK_SIZE, 2, Consts.CHUNK_SIZE));

      /*
      Transform transform = obj.GetTransform();
      transform.gameObject.AddComponent<MeshRenderer>();
      transform.gameObject.AddComponent<MeshFilter>();
      transform.GetComponent<MeshFilter>().mesh = mesh;
      MeshRenderer renderer = transform.GetComponent<MeshRenderer>();
      this.renderer = renderer;
      Material[] materials = new Material[2];
      materials[0] = mat;
      materials[1] = this.overlayMatl;
      renderer.materials = materials;
      */

      //Parent entity
      //Entity chunk = this.entityManager.CreateEntity(this.containerArchetype);
      //this.entityManager.SetName(chunk, "Chunk-R"+R.ToString()+"-C"+C.ToString());

      float3 translation = transform.position;
      translation = new float3(R, 0, C);

      Entity terrain = this.entityManager.CreateEntity(this.chunkArchetype);
      this.entityManager.AddComponentData(terrain, new Translation       { Value = translation} );
      this.entityManager.AddComponentData(terrain, new RenderBounds      { Value = mesh.bounds.ToAABB()} );
      this.entityManager.AddSharedComponentData(terrain, new RenderMesh  {mesh = mesh, material = mat, subMesh=0} );

      Entity overlays = this.entityManager.CreateEntity(this.chunkArchetype);
      this.entityManager.AddComponentData(overlays, new Translation      { Value = translation} );
      this.entityManager.AddComponentData(overlays, new RenderBounds     { Value = mesh.bounds.ToAABB()} );
      this.entityManager.AddSharedComponentData(overlays, new RenderMesh {mesh = mesh, material = this.overlayMatl, subMesh=1} );


      return terrain;

    }
    public void initTerrain(Dictionary<string, object> packet)
   {
      List<object> map = (List<object>) packet["map"];
      if (this.vals == null)
      {
         this.vals = new int[Consts.MAP_SIZE, Consts.MAP_SIZE];
         Debug.Log("Setting val map");
         for(int r=0; r<Consts.MAP_SIZE; r++) {
           List<object> row = (List<object>) map[r];
           for(int c=0; c<Consts.MAP_SIZE; c++) {
               this.vals[r, c] = System.Convert.ToInt32(row[c]);
           }
         }
      }
   }

    public void loadNextChunk()
   {
      Tuple<int, int> pos = unloadedChunks.Dequeue();
      loadedChunks.Enqueue(pos);
      int r = pos.Item1;
      int c = pos.Item2;

      //Debug.Log(r.ToString() + ", " + c.ToString());
      makeChunk(this.root, Consts.CHUNK_SIZE, r, c);
   }

    void Update()
    {

     if (this.vals != null && unloadedChunks.Count != 0){
         this.loadNextChunk();
      }


      int cameraR = (int) Math.Floor(this.cameraAnchor.transform.position.x / Consts.CHUNK_SIZE);
      int cameraC = (int) Math.Floor(this.cameraAnchor.transform.position.z / Consts.CHUNK_SIZE);

      //Activate inactive chunks in view
      HashSet<Tuple<int, int>> activeChunks = new HashSet<Tuple<int, int>>();
      for (int r = cameraR - Consts.CHUNK_RADIUS; r < cameraR + Consts.CHUNK_RADIUS; r++)
      {
         for (int c = cameraC - Consts.CHUNK_RADIUS; c < cameraC + Consts.CHUNK_RADIUS; c++)
         {
            if (!this.env.ContainsChunk(r, c))
            {
               continue;
            }
            Tuple<int, int> key = Tuple.Create(r, c);
            this.env.SetActive(this.entityManager, key, true);
            activeChunks.Add(key);
         }
      }

      //Deactivate active chunks outside view
      foreach (Tuple<int, int> key in this.env.activeChunks.ToList())
         {
            if(!activeChunks.Contains(key))
            {
               this.env.SetActive(this.entityManager, key, false);
            }
         }

      //Debug.Log("Updating terrain: " + tick.ToString());
      tick++;
      //GameObject plane = GameObject.Find("Plane");
      //plane.GetComponent<MeshRenderer>().material.SetTexture("_Overlay", this.testMatl);
      if (this.overlayMatl)
      {
         string cmd = this.cmd;
         if (this.overlays.ContainsKey(cmd))
         {
            this.overlayMatl.SetTexture("_Overlay", this.values);
         } else if (cmd == "env")
         {
            this.overlayMatl.SetTexture("_Overlay", Texture2D.blackTexture);
         }
         this.overlayMatl.SetVector("_PanParams", new Vector4(cameraR*Consts.CHUNK_SIZE, cameraC*Consts.CHUNK_SIZE, this.overlayR, this.overlayC));
         this.overlayMatl.SetVector("_SizeParams", new Vector4(Consts.TILE_RADIUS, 0, 0, 0));
      }
    }

}
