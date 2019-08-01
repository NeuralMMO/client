using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StatBar: MonoBehaviour {
  public static List<StatBar> Active = new List<StatBar>();
  public Slider bar;

  public int val;
  public int max;

  public float width = 80;
  public float opacity = 1f;
  public float xOffset = -17.5f;
  public float yOffset = 1.5f;
  public int barIdx = 0;

  public Color posColor = Color.green;
  public Color negColor = Color.red;

  public float Percentage() {
     return (float) val / (float) max;
  }

  public void UpdateBar(Color color) {
    this.bar.value = this.Percentage();
    this.bar.image.color = color;
    this.bar.fillRect.GetComponent<Image>().color = color;
    this.bar.GetComponentInChildren<Image>().color = Color.red;
  }

  void OnEnable() {
    Active.Add(this);
  }

  void OnDisable() {
    Active.Remove(this);
  }
}
