using UnityEngine;
using UnityEngine.EventSystems;
using MoreMountains.InventoryEngine;

public class RightClickMenu : MonoBehaviour, IPointerExitHandler
{
   delegate void triggerFunc(PointerEventData data);

   public Character characterCache; 
   public Character character; 

   RightClickButton examine;
   RightClickButton follow;

   RectTransform bounds;
   GameObject target;
   UI ui;

   InventoryDisplay itemDisplay;
   InventoryDisplay ammunitionDisplay;
   InventoryDisplay hatDisplay;
   InventoryDisplay topDisplay;
   InventoryDisplay bottomDisplay;
   InventoryDisplay heldDisplay;

   OrbitCamera camera;

   void Start()
   {
      GameObject prefab = Resources.Load("Prefabs/RightClickButton") as GameObject;
      GameObject cam    = GameObject.Find("Client/CameraAnchor/OrbitCamera");
      this.target       = GameObject.Find("Client/CameraAnchor");
      this.camera       = cam.GetComponent<OrbitCamera>();

      this.itemDisplay        = GameObject.Find("Client/UI/Canvas/InventoryPanel/Items").GetComponent<InventoryDisplay>();
      this.ammunitionDisplay  = GameObject.Find("Client/UI/Canvas/InventoryPanel/Ammunition").GetComponent<InventoryDisplay>();
      this.hatDisplay         = GameObject.Find("Client/UI/Canvas/InventoryPanel/Hat").GetComponent<InventoryDisplay>();
      this.topDisplay         = GameObject.Find("Client/UI/Canvas/InventoryPanel/Top").GetComponent<InventoryDisplay>();
      this.bottomDisplay      = GameObject.Find("Client/UI/Canvas/InventoryPanel/Bottom").GetComponent<InventoryDisplay>();
      this.heldDisplay        = GameObject.Find("Client/UI/Canvas/InventoryPanel/Held").GetComponent<InventoryDisplay>();

      this.follow  = this.MakeButton(prefab, this.OnFollow, 0);
      this.examine = this.MakeButton(prefab, this.OnExamine, 1);

      this.gameObject.SetActive(false);
      this.bounds = this.GetComponent<RectTransform>();
      this.bounds.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 2*32 + 8);
      this.bounds.ForceUpdateRectTransforms();
   }

   RightClickButton MakeButton(GameObject prefab, triggerFunc func, int offset) {
      GameObject obj = Instantiate(prefab) as GameObject;
      obj.transform.SetParent(this.transform);
      RightClickButton button = obj.GetComponent<RightClickButton>();
      button.offset = offset;
      this.AddTrigger(button, func);
      return button;
   }

   void AddTrigger(RightClickButton button, triggerFunc func) {
      EventTrigger trigger = button.GetComponent<EventTrigger>();
      EventTrigger.Entry entry = new EventTrigger.Entry();
      entry.eventID = EventTriggerType.PointerDown;
      entry.callback.AddListener((data) => { func((PointerEventData)data); });
      trigger.triggers.Add(entry);
   }
 
   public void OnFollow(PointerEventData data)
   {
      this.gameObject.SetActive(false);

      if (this.characterCache == null) {
         return; 
      }

      this.target.transform.SetParent(this.characterCache.transform);
      this.target.transform.localPosition = new Vector3(0,0,0);
      this.camera.distance = this.camera.distanceMin;
      Debug.Log("Following");
   }

   public void OnExamine(PointerEventData data)
   {
      this.gameObject.SetActive(false);

      if (this.characterCache == null) {
         return; 
      }

      this.character = this.characterCache;

      this.itemDisplay.TargetInventory        = this.character.inventory.items;         
      this.ammunitionDisplay.TargetInventory  = this.character.inventory.ammunition;         
      this.hatDisplay.TargetInventory         = this.character.inventory.hat;         
      this.topDisplay.TargetInventory         = this.character.inventory.top;         
      this.bottomDisplay.TargetInventory      = this.character.inventory.bottom;         
      this.heldDisplay.TargetInventory        = this.character.inventory.held;         

      this.itemDisplay.SetupInventoryDisplay();
      this.ammunitionDisplay.SetupInventoryDisplay();
      this.hatDisplay.SetupInventoryDisplay();
      this.topDisplay.SetupInventoryDisplay();
      this.bottomDisplay.SetupInventoryDisplay();
      this.heldDisplay.SetupInventoryDisplay();

      Debug.Log("Examining");
   }

   public void UpdateSelf(UI ui, Character character) {
      this.examine.UpdateSelf("Examine: " + character.name);
      this.follow.UpdateSelf("Follow: " + character.name);
      this.transform.position = Input.mousePosition + new Vector3(-20, 20, 0);

      this.gameObject.SetActive(true);
      this.characterCache = character;
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      this.gameObject.SetActive(false);
   }
}
