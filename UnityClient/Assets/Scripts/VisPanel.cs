using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class VisPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 lastMousePosition;
    bool isActive;
    Character character;

    // Start is called before the first frame update
    void Start()
    {
        isActive = true;
        //Fill in character's name and level
        Transform header = transform.Find("Header");
        header.GetComponent<TextMeshProUGUI>().text = character.overheads.playerName.text;
    }

    // Update is called once per frame
    void Update()
    {
        foreach (KeyValuePair<string, Resource> rs in character.resources.resources){
            //Debug.Log("resource type: " + rs.Key);
            string name = char.ToUpper(rs.Key[0]) + rs.Key.Substring(1);
            UpdateResource(name, rs.Value);
        }

        foreach (KeyValuePair<string, Skill> sk in character.skills.skills){
            string name = char.ToUpper(sk.Key[0]) + sk.Key.Substring(1);
            UpdateSkill(name, sk.Value);
        }
    }

    public void Init(Character character){
        this.character = character;
    }

    public void UpdateResource(string name, Resource resource){
        Transform stat = transform.Find(name); //e.g. Health, Food, or Water
        if (stat != null){
            TextMeshProUGUI tm = stat.GetComponent<TextMeshProUGUI>();
            tm.text = name + ": " + resource.value.ToString();
        }
    }

    public void UpdateSkill(string name, Skill skill){
        Transform stat = transform.Find(name); //e.g. melee, defense, range, fishing, hunting, mage
        if (stat != null){
            TextMeshProUGUI tm = stat.GetComponent<TextMeshProUGUI>();
            tm.text = name + ": Lvl " + skill.level.ToString();
        }
    }

    public void Toggle(){
        isActive = !isActive;
        gameObject.SetActive(isActive);
    }
    
    public void OnBeginDrag(PointerEventData e){
        lastMousePosition = e.position;
    } 

    public void OnDrag(PointerEventData e){
        Vector2 currentMousePosition = e.position;
        Vector2 diff = currentMousePosition - lastMousePosition;
        RectTransform rect = GetComponent<RectTransform>();

        Vector3 newPosition = rect.position + new Vector3(diff.x, diff.y, transform.position.z);
        Vector3 oldPosition = rect.position;
        rect.position = newPosition;
        if(!IsRectTransformInsideScreen(rect)){
            Debug.Log("Not draggable");
            rect.position = oldPosition;
        }
        lastMousePosition = currentMousePosition;
    }

    public void OnEndDrag(PointerEventData e){

    }

    public void DestroyPanel(){
        Destroy(gameObject);
    }

    private bool IsRectTransformInsideScreen(RectTransform rectTransform){
        bool isInside = false;
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        int visibleCorners = 0;
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        foreach(Vector3 corner in corners){
            if(rect.Contains(corner)) visibleCorners++;
        }
        if(visibleCorners == 4) isInside = true;
        return isInside;
    }
}
