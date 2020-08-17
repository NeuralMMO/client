using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Deprecated: Code integrated directly into Overheads
public class UIDepth: MonoBehaviour
{
    public float depth;
    private CanvasGroup canvasGroup;

    void Start()
    {
       this.canvasGroup = this.GetComponent<CanvasGroup>(); 
    }

    void Update()
    {
       float alpha = 1 - Mathf.Clamp(-this.depth/64 - 1, 0, 1);
       this.SetAlpha(alpha);
    }

    public void SetAlpha(float alpha) {
        this.canvasGroup.alpha = alpha;
        
        if (alpha <= 0) {
            this.gameObject.SetActive(false);
        } else {
            this.gameObject.SetActive(true);
        }

    }
}
