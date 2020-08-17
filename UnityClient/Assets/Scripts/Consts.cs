using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consts : MonoBehaviour
{
    public static int MAP_SIZE = 1024;
    public static int CHUNK_SIZE = 128;
    public static int CHUNK_RADIUS = 4;

    public static int TILE_RADIUS = CHUNK_RADIUS * CHUNK_SIZE;
    public static int CHUNKS = MAP_SIZE / CHUNK_SIZE;
}
