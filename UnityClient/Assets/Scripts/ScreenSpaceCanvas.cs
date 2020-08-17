using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenSpaceCanvas : MonoBehaviour
{
    List<Overheads> panels = new List<Overheads>();

    void Start()
    {
        this.panels.Clear();
        
    }

    void Update()
    {
        this.Sort(); 
    }

    public void AddToCanvas(Overheads overheads) {
       panels.Add(overheads);
    }

    void Sort() {
        panels.RemoveAll(x => x == null);
        panels.Sort( (x, y) => x.depth.CompareTo(y.depth));
        for (int i=0; i<panels.Count; i++) {
            panels[i].transform.SetSiblingIndex(i);
        }
    }
}
