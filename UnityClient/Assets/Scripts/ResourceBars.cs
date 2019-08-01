using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ResourceBars : MonoBehaviour
{
    public StatBar health;
    public StatBar food;
    public StatBar water;

    public GameObject prefab;
    public Canvas canvas;
    public GameObject panel;

    // Start is called before the first frame update
    void OnEnable()
    {
       //this.health.posColor = Color.green;
       //this.health.barIdx = 0;
       //this.health.bar = panel.GetComponentsInChildren<Slider>()[0];

       //this.food.posColor = Color.yellow;
       //this.food.barIdx = 1;
       //this.food.bar = panel.GetComponentsInChildren<Slider>()[1];

       //this.water.posColor = Color.blue;
       //this.water.barIdx = 2;        
       //this.water.bar = panel.GetComponentsInChildren<Slider>()[2];
    }

    // Update is called once per frame
    public void UpdateBars()
    {
       this.health.UpdateBar(Color.green);
       this.food.UpdateBar(Color.yellow);
       this.water.UpdateBar(Color.blue);
    }
}
