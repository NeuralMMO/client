using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishSize : MonoBehaviour
{
    public Unit unit = Unit.Metre;
    public float size = 1;

    private void Start() {
        switch (unit) {
            case Unit.Metre: {
                    this.gameObject.transform.localScale = new Vector3(size, size, size);
                    break;
                }
            case Unit.Centimetre: {
                    this.gameObject.transform.localScale = new Vector3(size / 100, size / 100, size / 100);
                    break;
                }
        }
    }
}

public enum Unit {
    Metre = 1,
    Centimetre = 2
}