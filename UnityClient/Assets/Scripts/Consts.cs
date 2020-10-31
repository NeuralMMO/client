using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consts : MonoBehaviour
{
    public static int MAP_SIZE;
    public static int BORDER;

    public static int CHUNK_RADIUS = 4;
    public static int LOW_DETAIL_HEIGHT = 128;

    public static int CHUNK_SIZE()
    {
      return MAP_SIZE / 8;
    }
    public static int TILE_RADIUS()
    {
      return CHUNK_RADIUS * Consts.CHUNK_SIZE();
    }
    public static int CHUNKS()
    {
      return MAP_SIZE / Consts.CHUNK_SIZE();
    }
}
