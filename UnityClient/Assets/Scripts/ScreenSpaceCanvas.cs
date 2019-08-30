using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceCanvas : MonoBehaviour
{
    List<UIDepth> panels = new List<UIDepth>();

    void Start()
    {
        this.panels.Clear();
        
    }

    void Update()
    {
        this.Sort(); 
    }

    public void AddToCanvas(GameObject obj) {
        panels.Add(obj.GetComponent<UIDepth>());
    }

    void Sort() {
        panels.RemoveAll(x => x == null);
        panels.Sort( (x, y) => x.depth.CompareTo(y.depth));
        for (int i=0; i<panels.Count; i++) {
            panels[i].transform.SetSiblingIndex(i);
        }
    }
}
