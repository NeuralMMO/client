using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

public class NNInventory : MonoBehaviour
{
    public Inventory items; 
    public Inventory ammunition; 
    public Inventory hat; 
    public Inventory top; 
    public Inventory bottom; 
    public Inventory held; 

    // Start is called before the first frame update
    void Awake()
    {
        this.items       = this.gameObject.transform.Find("Items").GetComponent<Inventory>();
        this.ammunition  = this.gameObject.transform.Find("Ammunition").GetComponent<Inventory>();
        this.hat         = this.gameObject.transform.Find("Hat").GetComponent<Inventory>();
        this.top         = this.gameObject.transform.Find("Top").GetComponent<Inventory>();
        this.bottom      = this.gameObject.transform.Find("Bottom").GetComponent<Inventory>();
        this.held        = this.gameObject.transform.Find("Held").GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
