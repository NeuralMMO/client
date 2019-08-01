using UnityEngine;
using UnityEngine.EventSystems;

public class RightClickMenu : MonoBehaviour, IPointerExitHandler
{
   delegate void triggerFunc(PointerEventData data);

   public Player playerCache; 
   public Player player; 

   RightClickButton examine;
   RightClickButton follow;

   RectTransform bounds;
   GameObject target;
   UI ui;

   OrbitCamera camera;

   void Start()
   {
      GameObject prefab = Resources.Load("Prefabs/RightClickButton") as GameObject;
      GameObject cam    = GameObject.Find("Client/CameraAnchor/OrbitCamera");
      this.target       = GameObject.Find("Client/CameraAnchor");
      this.camera       = cam.GetComponent<OrbitCamera>();

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

      if (this.playerCache == null) {
         return; 
      }

      this.target.transform.SetParent(this.playerCache.transform);
      this.target.transform.localPosition = new Vector3(0,0,0);
      this.camera.distance = this.camera.distanceMin;
      Debug.Log("Following");
   }

   public void OnExamine(PointerEventData data)
   {
      this.gameObject.SetActive(false);

      if (this.playerCache == null) {
         return; 
      }

      this.player = this.playerCache;
      Debug.Log("Examining");
   }

   public void UpdateSelf(UI ui, Player player) {
      this.examine.UpdateSelf("Examine: " + player.name);
      this.follow.UpdateSelf("Follow: " + player.name);
      this.transform.position = Input.mousePosition + new Vector3(-10, 10, 0);

      this.gameObject.SetActive(true);
      this.playerCache = player;
   }

   public void OnPointerExit(PointerEventData eventData)
   {
      this.gameObject.SetActive(false);
   }
}
