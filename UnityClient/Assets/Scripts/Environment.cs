using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/*
Nothing works. At all. But it's not because it's wrong. Just slow and
glitching the editor. Need to either port all this into a new project
or start reducing stuff from this project. Probably latter first quickly
The tile packer map is maybe? part of this... but I think it's just unity */
 
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class Environment: MonoBehaviour
{
    //public static Dictionary<int, Texture2D> tiles   = new Dictionary<int, Texture2D>();
    public Dictionary<string, int> matIdxs           = new Dictionary<string, int>();
    public Dictionary<int, string> idxMats           = new Dictionary<int, string>();
    public Dictionary<int, GameObject> idxPrefabs    = new Dictionary<int, GameObject>();
    public Dictionary<string, GameObject> matPrefabs = new Dictionary<string, GameObject>();

    public Dictionary<string, GameObject>[,] env = new Dictionary<string, GameObject>[mapSz, mapSz];
    public int[,] vals = new int[mapSz, mapSz];
    public Texture2D values;

    public static List<List<GameObject>> terrain = new List<List<GameObject>>();
    public static int mapSz = 80;
    public int[,] oldPacket = new int[mapSz, mapSz];
    public GameObject[,] objs = new GameObject[mapSz, mapSz];

    public int TileWidth = 128;
    public int TileHeight = 128;
    public int NumTilesX = 8;
    public int NumTilesZ = 8;
    public int TileGridWidth = 64;
    public int TileGridHeight = 64;
    public int DefaultTileX = 0;
    public int DefaultTileZ = 0;

    public int tick = 0;
    public Texture2D Tex1;
    public Texture2D Tex2;

    string cmd = "";

    Shader shader;

    GameObject cubePrefab;
    GameObject forestPrefab;
    GameObject nuPrefab;
    GameObject resources;
    Console    console;

    bool first = true;

    Material cubeMatl;
    Material material;
    Material overlayMatl;

    MeshRenderer renderer;

    void addPair(string name, int idx) {
      GameObject prefab = this.addMat(name);
      this.matIdxs[name] = idx;
      this.idxMats[idx]  = name;
      this.idxPrefabs[idx]  = prefab;
      this.addMat(name);
    }

    GameObject addMat(string name) {
      GameObject prefab = Resources.Load("Prefabs/Tiles/" + name) as GameObject;
      this.matPrefabs[name]  = prefab;
      return prefab;
    }


    void OnEnable()
    {
      GameObject root   = GameObject.Find("Client/Environment/Terrain");
      this.resources    = GameObject.Find("Client/Environment/Terrain/Resources");
      this.cubeMatl     = Resources.Load("Prefabs/Tiles/CubeMatl") as Material;
      this.values       = new Texture2D(80, 80);
      this.cubePrefab   = Resources.Load("Prefabs/Cube") as GameObject;
      this.forestPrefab = Resources.Load("LowPoly Style/Free Rocks and Plants/Prefabs/Reed") as GameObject;
      this.console      = GameObject.Find("Console").GetComponent<Console>();
      this.shader       = Shader.Find("Standard");

      this.addPair("Lava", 0);
      this.addPair("Sand", 1);
      this.addPair("Grass", 2);
      this.addPair("Scrub", 3);
      this.addPair("Forest", 4);
      this.addPair("Stone0a", 5);

      this.addMat("Stone0a");
      this.addMat("Stone1a");
      this.addMat("Stone2a");
      this.addMat("Stone2b");
      this.addMat("Stone2c");
      this.addMat("Stone3a");
      this.addMat("Stone3b");
      this.addMat("Stone3c");
      this.addMat("Stone4a");
      this.addMat("Stone4b");
      this.addMat("Stone4c");
      this.addMat("Stone4d");
      this.addMat("Stone4e");
      this.addMat("Stone4f");

      this.addMat("Sand0a");
      this.addMat("Sand1a");
      this.addMat("Sand2a");
      this.addMat("Sand2b");
      this.addMat("Sand2c");
      this.addMat("Sand3a");
      this.addMat("Sand3b");
      this.addMat("Sand3c");
      this.addMat("Sand4a");
      this.addMat("Sand4b");
      this.addMat("Sand4c");
      this.addMat("Sand4d");
      this.addMat("Sand4e");
      this.addMat("Sand4f");

      for(int r=0; r<mapSz; r++) {
         for(int c=0; c<mapSz; c++) {
            this.env[r, c] = new Dictionary<string, GameObject>();            
         }
      }
 
    }

    public void UpdateMap(Dictionary<string, object> packet) {
      GameObject root  = GameObject.Find("Environment");
      List<object> map     = (List<object>) packet["map"];

      string cmd = this.console.cmd;
      this.cmd = cmd;
      if (this.console.validateCommand(cmd))
      {
         int count = 0;
         List<object> values = (List<object>) packet[cmd];
         Color[] pixels = new Color[80 * 80];
         for (int r = 0; r < mapSz; r++)
         {
            List<object> row = (List<object>)values[r];
            for (int c = 0; c < mapSz; c++)
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

      for (int r=0; r<mapSz; r++) {
         List<object> row = (List<object>) map[r];
         for(int c=0; c<mapSz; c++) {
            int val = System.Convert.ToInt32(row[c]);
            if (this.idxMats[val] == "Forest") {
               this.env[r, c]["Forest"].SetActive(true);
            } else if (this.idxMats[val] == "Scrub") {
               this.env[r, c]["Forest"].SetActive(false);
            }
         }
      }
     //this.values = Texture2D.grayTexture;
    }

    public void UpdateTerrain(Dictionary<string, object> packet)
    {
      List<object> map = (List<object>) packet["map"];
      //makeMap(map, 20, 4);
      makeMap(map, mapSz/8, 8);
    }

    void makeMap(List<object> map, int sz, int n) {
      for(int r=0; r<mapSz; r++) {
        List<object> row = (List<object>) map[r];
        for(int c=0; c<mapSz; c++) {
            this.vals[r, c] = System.Convert.ToInt32(row[c]);
        }
      }
 
      GameObject root = GameObject.Find("Environment/Terrain");
      for(int r=0; r<n; r++) {
         for(int c=0; c<n; c++) {
            makeChunk(root, sz, r*sz, c*sz);
         }
      }
    }

    //-1 = idc, 0 = false, 1 = true
    bool bounds(int val, int r, int c, 
         int r0c0, int r0c1, int r0c2, 
         int r1c0, int r1c1, int r1c2, 
         int r2c0, int r2c1, int r2c2) {

       if (r0c0 != -1 && this.vals[r-1, c-1] != val == (r0c0 != 0)) {
          return false; 
       }
       if (r0c1 != -1 && this.vals[r-1, c] != val == (r0c1 != 0)) {
          return false; 
       }
       if (r0c2 != -1 && this.vals[r-1, c+1] != val == (r0c2 != 0)) {
          return false; 
       }
       if (r1c0 != -1 && this.vals[r, c-1] != val == (r1c0 != 0)) {
          return false; 
       }
       if (r1c1 != -1 && this.vals[r, c] != val == (r1c1 != 0)) {
          return false; 
       }
       if (r1c2 != -1 && this.vals[r, c+1] != val == (r1c2 != 0)) {
          return false; 
       }
       if (r2c0 != -1 && this.vals[r+1, c-1] != val == (r2c0 != 0)) {
          return false; 
       }
       if (r2c1 != -1 && this.vals[r+1, c] != val == (r2c1 != 0)) {
          return false; 
       }
       if (r2c2 != -1 && this.vals[r+1, c+1] != val == (r2c2 != 0)) {
          return false; 
       }

       return true;
    }

    GameObject initBlock(string name, float rotation) {
         GameObject obj = Instantiate(this.matPrefabs[name]) as GameObject;
         obj.transform.eulerAngles = new Vector3(0, rotation, 0);
         return obj;
    }

    GameObject getBlock(int r, int c, string name, int val) {
         if (r == 0 || c == 0 || r == mapSz-1 || c == mapSz-1) {
            return Instantiate(this.matPrefabs[name + "4f"]) as GameObject;
         }

         if (this.bounds(val, r, c, 
               -1, 1, -1, 
               0, 1, 0, 
               -1, 0, -1)) {
            return this.initBlock(name + "1a", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               0, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "1a", 90);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               0, 1, 0, 
               -1, 1, -1)) {
            return this.initBlock(name + "1a", 180);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 0, 
               -1, 0, -1)) {
            return this.initBlock(name + "1a", 270);
         }

         if (this.bounds(val, r, c, 
               0, 1, -1, 
               1, 1, 0, 
               -1, 0, -1)) {
            return this.initBlock(name + "2a", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 1, 0, 
               0, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "2a", 90);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               0, 1, 1, 
               -1, 1, 0)) {
            return this.initBlock(name + "2a", 180);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 0, 
               0, 1, -1)) {
            return this.initBlock(name + "2a", 270);
         }
 

         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "2b", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 1, -1, 
               0, 1, 0, 
               -1, 1, -1)) {
            return this.initBlock(name + "2b", 90);
         }


         if (this.bounds(val, r, c, 
               1, 1, -1, 
               1, 1, 0, 
               -1, 0, -1)) {
            return this.initBlock(name + "2c", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 1, 1, 
               0, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "2c", 90);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               0, 1, 1, 
               -1, 1, 1)) {
            return this.initBlock(name + "2c", 180);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 0, 
               1, 1, -1)) {
            return this.initBlock(name + "2c", 270);
         }
 

         if (this.bounds(val, r, c, 
               0, 1, 0, 
               1, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "3a", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 1, 0, 
               0, 1, 1, 
               -1, 1, 0)) {
            return this.initBlock(name + "3a", 90);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 1, 
               0, 1, 0)) {
            return this.initBlock(name + "3a", 180);
         }
         if (this.bounds(val, r, c, 
               0, 1, -1, 
               1, 1, 0, 
               0, 1, -1)) {
            return this.initBlock(name + "3a", 270);
         }

         if (this.bounds(val, r, c, 
               1, 1, 1, 
               1, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "3c", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 1, 1, 
               0, 1, 1, 
               -1, 1, 1)) {
            return this.initBlock(name + "3c", 90);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 1, 
               1, 1, 1)) {
            return this.initBlock(name + "3c", 180);
         }
         if (this.bounds(val, r, c, 
               1, 1, -1, 
               1, 1, 0, 
               1, 1, -1)) {
            return this.initBlock(name + "3c", 270);
         }


         if (this.bounds(val, r, c, 
               1, 1, -1, 
               1, 1, 1, 
               -1, 0, -1)) {
            return this.initBlock(name + "3b", 0);
         }
         if (this.bounds(val, r, c, 
               -1, 1, 1, 
               0, 1, 1, 
               -1, 1, -1)) {
            return this.initBlock(name + "3b", 90);
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 1, 
               -1, 1, 1)) {
            return this.initBlock(name + "3b", 180);
         }
         if (this.bounds(val, r, c, 
               -1, 1, -1, 
               1, 1, 0, 
               1, 1, -1)) {
            return this.initBlock(name + "3b", 270);
         }


         if (this.bounds(val, r, c, 
               0, 1, 1, 
               1, 1, 1, 
               -1, 0, -1)) {
            GameObject obj = this.initBlock(name + "3b", 0);
            obj.transform.localScale  = new Vector3(1, 1, -1);
            return obj;
         }
         if (this.bounds(val, r, c, 
               -1, 1, 0, 
               0, 1, 1, 
               -1, 1, 1)) {
            GameObject obj = this.initBlock(name + "3b", 90);
            obj.transform.localScale  = new Vector3(1, 1, -1);
            return obj;
         }
         if (this.bounds(val, r, c, 
               -1, 0, -1, 
               1, 1, 1, 
               1, 1, 0)) {
            GameObject obj = this.initBlock(name + "3b", 180);
            obj.transform.localScale  = new Vector3(1, 1, -1);
            return obj;
         }
         if (this.bounds(val, r, c, 
               1, 1, -1, 
               1, 1, 0, 
               0, 1, -1)) {
            GameObject obj = this.initBlock(name + "3b", 270);
            obj.transform.localScale  = new Vector3(1, 1, -1);
            return obj;
         }


        if (this.bounds(val, r, c, 
               0, 1, 0, 
               1, 1, 1, 
               0, 1, 0)) {
            return this.initBlock(name + "4a", 0);
        }


         if (this.bounds(val, r, c, 
               0, 1, 1, 
               1, 1, 1, 
               0, 1, 0)) {
            return this.initBlock(name + "4b", 0);
         }
         if (this.bounds(val, r, c, 
               0, 1, 0, 
               1, 1, 1, 
               0, 1, 1)) {
            return this.initBlock(name + "4b", 90);
         }
         if (this.bounds(val, r, c, 
               0, 1, 0, 
               1, 1, 1, 
               1, 1, 0)) {
            return this.initBlock(name + "4b", 180);
         }
        if (this.bounds(val, r, c, 
               1, 1, 0, 
               1, 1, 1, 
               0, 1, 0)) {
            return this.initBlock(name + "4b", 270);
         }


         if (this.bounds(val, r, c, 
               0, 1, 1, 
               1, 1, 1, 
               0, 1, 1)) {
            return this.initBlock(name + "4c", 90);
         }
         if (this.bounds(val, r, c, 
               0, 1, 0, 
               1, 1, 1, 
               1, 1, 1)) {
            return this.initBlock(name + "4c", 180);
         }
         if (this.bounds(val, r, c, 
               1, 1, 0, 
               1, 1, 1, 
               1, 1, 0)) {
            return this.initBlock(name + "4c", 270);
         }
        if (this.bounds(val, r, c, 
               1, 1, 1, 
               1, 1, 1, 
               0, 1, 0)) {
            return this.initBlock(name + "4c", 0);
         }
 

         if (this.bounds(val, r, c, 
               0, 1, 1, 
               1, 1, 1, 
               1, 1, 0)) {
            return this.initBlock(name + "4d", 0);
         }
         if (this.bounds(val, r, c, 
               1, 1, 0, 
               1, 1, 1, 
               0, 1, 1)) {
            return this.initBlock(name + "4d", 90);
         }


         if (this.bounds(val, r, c, 
               0, 1, 1, 
               1, 1, 1, 
               1, 1, 1)) {
            return this.initBlock(name + "4e", 180);
         }
         if (this.bounds(val, r, c, 
               1, 1, 0, 
               1, 1, 1, 
               1, 1, 1)) {
            return this.initBlock(name + "4e", 270);
         }
         if (this.bounds(val, r, c, 
               1, 1, 1, 
               1, 1, 1, 
               1, 1, 0)) {
            return this.initBlock(name + "4e", 0);
         }
        if (this.bounds(val, r, c, 
               1, 1, 1, 
               1, 1, 1, 
               0, 1, 1)) {
            return this.initBlock(name + "4e", 90);
         }

        if (this.bounds(val, r, c, 
               1, 1, 1, 
               1, 1, 1, 
               1, 1, 1)) {
            return this.initBlock(name + "4f", 0);
         }

         return Instantiate(this.idxPrefabs[val]) as GameObject;
    }

   void makeChunk(GameObject root, int sz, int R, int C) {
      GameObject chunk = new GameObject();
      chunk.transform.SetParent(root.transform);
      chunk.name = "Chunk-R" + R.ToString() + "-C" + C.ToString();

      for(int r=0; r<sz; r++) {
        for(int c=0; c<sz; c++) {
            int val = System.Convert.ToInt32(this.vals[R+r, C+c]);

            //stone
            GameObject cube;
            if (val == 5) {
               cube = this.getBlock(R+r, C+c, "Stone", val);
            } else if (val == 0 || val == 1) {
               cube = this.getBlock(R+r, C+c, "Sand", val);
            } else {
               cube = Instantiate(this.idxPrefabs[val]) as GameObject;
            }

            cube.transform.position = new Vector3(R+r, 0, C+c);
            cube.transform.SetParent(chunk.transform);
            this.env[R+r, C+c]["block"] = cube;

            if (this.idxMats[val] == "Forest" || this.idxMats[val] == "Scrub") { 
               GameObject forest = Instantiate(this.forestPrefab) as GameObject;
               forest.transform.position = cube.transform.position;
               forest.transform.localScale    = new Vector3(0.9f, 0.20f, 0.9f);
               forest.transform.eulerAngles   = new Vector3(0, Random.Range(0, 360), 0);
               forest.transform.SetParent(this.resources.transform);
               this.env[R+r, C+c]["Forest"] = forest;
            }
         }
      }
      //StaticBatchingUtility.Combine(chunk);
      combineMeshes(chunk, this.cubeMatl);
    }

    void combineMeshes(GameObject obj, Material mat) {
      MeshFilter[] meshFilters  = obj.GetComponentsInChildren<MeshFilter>();
      CombineInstance[] combine = new CombineInstance[meshFilters.Length];

      int i = 0;
      while (i < meshFilters.Length)
      {
         combine[i].mesh = meshFilters[i].sharedMesh;
         combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
         meshFilters[i].gameObject.SetActive(false);

         i++;
      }

      Mesh mesh = new Mesh();
      mesh.CombineMeshes(combine, true, true);
      obj.transform.gameObject.AddComponent<MeshRenderer>();
      obj.transform.gameObject.AddComponent<MeshFilter>();
      obj.transform.GetComponent<MeshFilter>().mesh = mesh;
      MeshRenderer renderer = obj.transform.GetComponent<MeshRenderer>();
      this.overlayMatl      = Resources.Load("Prefabs/Tiles/OverlayMaterial") as Material;
      this.renderer = renderer;
      Material[] materials = new Material[2];
      materials[0] = mat;
      materials[1] = this.overlayMatl;
      this.overlayMatl.SetTexture("_Overlay", Texture2D.blackTexture);
      renderer.materials = materials;
      obj.transform.gameObject.SetActive(true);
    }

    void Update()
    {
      //Debug.Log("Updating terrain: " + tick.ToString());
      tick++;
      //GameObject plane = GameObject.Find("Plane");
      //plane.GetComponent<MeshRenderer>().material.SetTexture("_Overlay", this.testMatl);
      if (this.overlayMatl)
      {
         string cmd = this.cmd;
         if (this.console.validateCommand(cmd))
         {
            this.overlayMatl.SetTexture("_Overlay", this.values);
         } else if (cmd == "env")
         {
            this.overlayMatl.SetTexture("_Overlay", Texture2D.blackTexture);
         }
      }
    }

}
