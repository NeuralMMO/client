using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using MoreMountains.Tools;

namespace MoreMountains.Tools
{
    /// <summary>
    /// Use this class on a sprite or mesh to have its texture pan according to the specified speed
    /// You can also force a sorting layer name 
    /// </summary>
    public class PanningTexture : MonoBehaviour
    {
        /// the speed at which the texture pans
        public Vector2 Speed = new Vector2(10,10);
        /// the name of the sorting layer to render the texture at
        public string SortingLayerName = "Above";

        protected RawImage _rawImage;
        protected Renderer _renderer;
        protected Vector2 _position = Vector2.zero;

        /// <summary>
        /// On start, grabs the renderer and/or raw image
        /// </summary>
        protected virtual void Start()
        {
            _renderer = GetComponent<Renderer>();
            if ((_renderer != null) && (SortingLayerName != ""))
            {
                _renderer.sortingLayerName = SortingLayerName;
            }            

            _rawImage = GetComponent<RawImage>();
        }

        /// <summary>
        /// On update, moves the texture around according to the specified speed
        /// </summary>
        protected virtual void Update()
        {
            if ((_rawImage == null) && (_renderer == null))
            {
                return;
            }

            _position += (Speed / 300) * Time.deltaTime;

            // position reset
            if (_position.x > 1.0f)
            {
                _position.x -= 1.0f;
            }
            if (_position.y > 1.0f)
            {
                _position.y -= 1.0f;
            }
            
            if (_renderer != null)
            {
                _renderer.material.mainTextureOffset = _position;
            }
            if (_rawImage != null)
            {
                _rawImage.material.mainTextureOffset = _position;
            }

        }
    }
}