using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RightClickButton: MonoBehaviour
{
    Button button;
    TextMeshProUGUI content;

    public int offset = 0;
    public int sz     = 32;

    // Start is called before the first frame update
    void OnEnable()
    {
        this.button  = this.GetComponent<Button>();
        this.content = this.GetComponentInChildren<TextMeshProUGUI>();
    }

    public void UpdateSelf(string text) {
      Vector3 trans = new Vector3(4, -this.offset*this.sz -4, 0);
      this.transform.localPosition = trans;
      this.content.text = text;
      this.button.Select();
    }

    public void OnMouseOver() 
    {
      this.button.Select();
    }

   }