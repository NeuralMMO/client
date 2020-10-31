using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ScreenSpaceCanvas : MonoBehaviour
{
    HashSet<Overheads> panelsHash = new HashSet<Overheads>();
    List<Overheads> panels = new List<Overheads>();

    void Start()
    {
       panels.Clear();
        
    }

    void Update()
    {
       Sort(); 
    }

    public void AddToCanvas(Overheads overheads) {
       panelsHash.Add(overheads);
    }

    public void RemoveFromCanvas(Overheads overheads)
    {
      panelsHash.Remove(overheads);
    }

    void Sort() {
        panels = panelsHash.ToList<Overheads>();
        panels.Sort( (x, y) => x.depth.CompareTo(y.depth));
        for (int i=0; i<panels.Count; i++) {
            panels[i].transform.SetSiblingIndex(i);
        }
    }
}
