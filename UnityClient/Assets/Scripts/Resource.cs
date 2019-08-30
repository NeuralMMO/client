using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MonoBehaviorExtension;

public class Resource: UnityModule 
{
    public static List<Resource> Active = new List<Resource>();
    public Slider bar;

    public int value;
    public int max;

    public float Percentage() {
        return (float) value / (float) max;
    }

    void OnEnable() {
        this.bar = this.GetComponentInChildren<Slider>();
        Active.Add(this);
    }

    void OnDisable() {
        Active.Remove(this);
    }

    public void UpdateResource(object resource)
    {
       this.max   = Convert.ToInt32(Unpack("max", resource));
       this.value = Convert.ToInt32(Unpack("val", resource));
       this.bar.value = this.Percentage();
    }
}

