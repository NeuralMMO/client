using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using MoreMountains.Tools;
using System.Collections.Generic;

namespace MoreMountains.Tools
{
    public class MMPSBToUIConverter : MonoBehaviour
    {
        [Header("Target")]
        public Canvas TargetCanvas;
        public float ScaleFactor = 100f;
        public bool ReplicateNesting = false;

        [Header("Size")]
        public float TargetWidth = 2048;
        public float TargetHeight = 1152;

        [Header("Conversion")]
        [MMInspectorButton("ConvertToCanvas")]
        public bool ConvertToCanvasButton;
        public Vector3 ChildImageOffset = new Vector3(-1024f, -576f, 0f);

        protected Transform _topLevel;
        protected Dictionary<Transform, int> _sortingOrders;

        public virtual void ConvertToCanvas()
        {
            Screen.SetResolution((int)TargetWidth, (int)TargetHeight, true);

            _sortingOrders = new Dictionary<Transform, int>();

            // remove existing canvas if found
            foreach (Transform child in TargetCanvas.transform)
            {
                if (child.name == this.name)
                {
                    child.MMDestroyAllChildren();
                    DestroyImmediate(child.gameObject);
                }
            }

            // force size on canvas scaler
            CanvasScaler canvasScaler = TargetCanvas.GetComponent<CanvasScaler>();
            if (canvasScaler != null)
            {
                canvasScaler.referenceResolution = new Vector2(TargetWidth, TargetHeight);
            }

            // create a parent in the target canvas
            GameObject newRoot = new GameObject(this.name, typeof(RectTransform));
            newRoot.transform.SetParent(TargetCanvas.transform);
            RectTransform newRootRect = newRoot.GetComponent<RectTransform>();
            SetupForStretch(newRootRect);

            _topLevel = newRoot.transform;
            CreateImageForChildren(this.transform, newRoot.transform);

            // apply sorting orders
            foreach (KeyValuePair<Transform, int> pair in _sortingOrders)
            {
                pair.Key.SetSiblingIndex(pair.Value);
            }
        }

        /// <summary>
        /// Recursively goes through the children of the specified "root" Transform, and parents them to the specified "parent" 
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        protected virtual void CreateImageForChildren(Transform root, Transform parent)
        {
            foreach (Transform child in root)
            {
                GameObject imageGO = new GameObject(child.name, typeof(RectTransform));
                imageGO.transform.localPosition = ScaleFactor * child.transform.localPosition;
                if (ReplicateNesting)
                {
                    imageGO.transform.SetParent(parent);
                }
                else
                {
                    imageGO.transform.SetParent(_topLevel);
                    Vector3 newLocalPosition = imageGO.transform.localPosition;
                    newLocalPosition.x = newLocalPosition.x + TargetWidth / 2f;
                    imageGO.transform.localPosition = newLocalPosition;
                }

                SpriteRenderer spriteRenderer = child.gameObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    Image image = imageGO.AddComponent<Image>();
                    image.sprite = spriteRenderer.sprite;
                    _sortingOrders.Add(image.transform, spriteRenderer.sortingOrder);
                    image.SetNativeSize();

                    RectTransform imageGoRect = imageGO.GetComponent<RectTransform>();
                    Vector3 newPosition = imageGoRect.localPosition;
                    newPosition += ChildImageOffset;
                    newPosition.z = 0f;
                    imageGoRect.localPosition = newPosition;
                }
                else
                {
                    imageGO.name += " - NODE";
                    RectTransform imageGoRect = imageGO.GetComponent<RectTransform>();
                    imageGoRect.sizeDelta = new Vector2(TargetWidth, TargetHeight);
                    imageGoRect.localPosition = Vector3.zero;
                }
                imageGO.GetComponent<RectTransform>().localScale = Vector3.one;
                CreateImageForChildren(child, imageGO.transform);
            }
        }

        protected virtual void SetupForStretch(RectTransform rect)
        {
            rect.localPosition = Vector3.zero;
            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localScale = Vector3.one;
        }

    }
}
