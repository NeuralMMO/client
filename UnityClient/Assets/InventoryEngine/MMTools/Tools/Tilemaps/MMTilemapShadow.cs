using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.Tools;
using System;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

namespace MoreMountains.Tools
{
    /// <summary>
    /// A class to put on a tilemap so it acts as a shadow/copy of another reference tilemap.
    /// Useful for wall shadows for example.
    /// Offsetting the tilemap and changing its sorting order etc is done via the regular components
    /// </summary>
    [ExecuteAlways]
    [RequireComponent(typeof(Tilemap))]
    public class MMTilemapShadow : MonoBehaviour
    {        
        /// the tilemap to copy
        public Tilemap ReferenceTilemap;
        
        [MMInspectorButton("UpdateShadows")]
        public bool UpdateShadowButton;

        protected Tilemap _tilemap;

        /// <summary>
        /// This method will copy the reference tilemap into the one on this gameobject
        /// </summary>
        protected virtual void UpdateShadows()
        {
            if (ReferenceTilemap == null)
            {
                return;
            }

            _tilemap = this.gameObject.GetComponent<Tilemap>();
            List<Vector3Int> referenceTilemapPositions = new List<Vector3Int>();
            
            // we grab all filled positions from the ref tilemap
            foreach (var pos in ReferenceTilemap.cellBounds.allPositionsWithin)
            {
                Vector3Int localPlace = new Vector3Int(pos.x, pos.y, pos.z);
                if (ReferenceTilemap.HasTile(localPlace))
                {
                    referenceTilemapPositions.Add(localPlace);
                }
            }
            
            // we turn our list into an array
            Vector3Int[] positions = new Vector3Int[referenceTilemapPositions.Count];
            TileBase[] allTiles = new TileBase[referenceTilemapPositions.Count];
            int i = 0;
            foreach(Vector3Int tilePosition in referenceTilemapPositions)
            {
                positions[i] = tilePosition;
                allTiles[i] = ReferenceTilemap.GetTile(tilePosition);
                i++;
            }

            // we clear our tilemap and resize it
            _tilemap.ClearAllTiles();
            _tilemap.RefreshAllTiles();
            _tilemap.size = ReferenceTilemap.size;
            _tilemap.origin = ReferenceTilemap.origin;
            _tilemap.ResizeBounds();
            
            // we feed it our positions
            _tilemap.SetTiles(positions, allTiles);
        }
		
	}
}
