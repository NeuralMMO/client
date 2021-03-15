using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using MoreMountains.InventoryEngine;

public class NNInventory : MonoBehaviour
{
    public Inventory equipment; 
    public Inventory ammunition; 
    public Inventory consumables; 
    public Inventory loot; 
    public Inventory hat; 
    public Inventory top; 
    public Inventory bottom; 
    public Inventory weapon; 

    // Start is called before the first frame update
    void Awake()
    {
        this.ammunition  = this.gameObject.transform.Find("Ammunition").GetComponent<Inventory>();
        this.consumables = this.gameObject.transform.Find("Consumables").GetComponent<Inventory>();
        this.loot        = this.gameObject.transform.Find("Loot").GetComponent<Inventory>();
        this.hat         = this.gameObject.transform.Find("Hat").GetComponent<Inventory>();
        this.top         = this.gameObject.transform.Find("Top").GetComponent<Inventory>();
        this.bottom      = this.gameObject.transform.Find("Bottom").GetComponent<Inventory>();
        this.weapon      = this.gameObject.transform.Find("Weapon").GetComponent<Inventory>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
