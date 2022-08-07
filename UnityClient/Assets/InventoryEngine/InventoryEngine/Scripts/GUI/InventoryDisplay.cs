using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using System.Collections.Generic;
using UnityEngine.UI;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.EventSystems;
#if UNITY_EDITOR
	using UnityEditor;
#endif

namespace MoreMountains.InventoryEngine
{	
	[SelectionBase]
	/// <summary>
	/// A component that handles the visual representation of an Inventory, allowing the user to interact with it
	/// </summary>
	public class InventoryDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[Header("Binding")]
		/// the name of the inventory to display
		[MMInformation("An InventoryDisplay is a component that will handle the visualization of the data contained in an Inventory. Start by specifying the name of the inventory you want to display.",MMInformationAttribute.InformationType.Info,false)]
		public string TargetInventoryName = "MainInventory";

		protected Inventory _targetInventory = null;

		/// <summary>
		/// Grabs the target inventory based on its name
		/// </summary>
		/// <value>The target inventory.</value>
		public Inventory TargetInventory;
		/*
		public Inventory TargetInventory 
		{ 
			get 
			{ 
				if (TargetInventoryName==null)
				{
					return null;
				}
				if (_targetInventory == null)
				{
					foreach (Inventory inventory in UnityEngine.Object.FindObjectsOfType<Inventory>())
					{
						if (inventory.name == TargetInventoryName)
						{
							_targetInventory = inventory;
						}
					}	
				}
				return _targetInventory;
			}
		}
		*/

		[Header("Inventory Size")]
		/// the number of rows to display
		[MMInformation("An InventoryDisplay presents an inventory's data in slots containing one item each, and displayed in a grid. Here you can set how many rows and columns of slots you want. Once you're happy with your settings, you can press the 'auto setup' button at the bottom of this inspector to see your changes.",MMInformationAttribute.InformationType.Info,false)]
		public int NumberOfRows = 3;
		/// the number of columns to display
		public int NumberOfColumns = 2;

		/// the total number of slots in this inventory
		public int InventorySize { get { return NumberOfRows * NumberOfColumns; } set {} }		

		[Header("Equipment")]
		[MMInformation("If this displays the contents of an Equipment Inventory, you should bind here a Choice Inventory. A Choice Inventory is the inventory in which you'll pick items for your equipment. Usually the Choice Inventory is the Main Inventory. Again, if this is an equipment inventory, you can specify what class of items you want to authorize.",MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay TargetChoiceInventory;
		public ItemClasses ItemClass;

		[Header("Behaviour")]
		/// if this is true, we'll draw slots even if they don't contain an object. Otherwise we don't draw them
		[MMInformation("If you set this to true, empty slots will be drawn, otherwise they'll be hidden from the player.",MMInformationAttribute.InformationType.Info,false)]
		public bool DrawEmptySlots=true;

		[Header("Inventory Padding")]
		[MMInformation("Here you can define the padding between the borders of the inventory panel and the slots.",MMInformationAttribute.InformationType.Info,false)]
		/// The internal margin between the top of the inventory panel and the first slots
		public int PaddingTop=20;
		/// The internal margin between the right of the inventory panel and the last slots
		public int PaddingRight=20;
		/// The internal margin between the bottom of the inventory panel and the last slots
		public int PaddingBottom=20;
		/// The internal margin between the left of the inventory panel and the first slots
		public int PaddingLeft=20;

		[Header("Slots")]
		[MMInformation("When pressing the auto setup button at the bottom of this inventory, the InventoryDisplay will fill itself with slots ready to display your inventory's contents. Here you can define the slot's size, margins, and define the images to use when the slot is empty, filled, etc.",MMInformationAttribute.InformationType.Info,false)]
		// the horizontal and vertical size of the slots
		public Vector2 SlotSize = new Vector2(50,50);
		// the size of the icon in each slot
		public Vector2 IconSize = new Vector2(30,30);
		// the horizontal and vertical margin to apply between slots rows and columns
		public Vector2 SlotMargin = new Vector2(5,5);
		/// The image to set as the background of each slot when the slot is empty
		public Sprite EmptySlotImage;
		/// The image to set as the background of each slot when the slot is not empty
		public Sprite FilledSlotImage;
		/// The image to set as the background of each slot when the slot is highlighted
		public Sprite HighlightedSlotImage;
		/// The image to set as the background of each slot when the slot is pressed
		public Sprite PressedSlotImage;
		/// The image to set as the background of each slot when the slot is disabled
		public Sprite DisabledSlotImage;
		/// The image to set as the background of each slot when the item in the slot is being moved around
		public Sprite MovedSlotImage;

		/// The type of the image (sliced, normal, tiled...)
		public Image.Type SlotImageType;

		[Header("Navigation")]
		[MMInformation("Here you can decide whether or not you want to use the built-in navigation system (allowing the player to move from slot to slot using keyboard arrows or a joystick), and whether or not this inventory display panel should be focused whent the scene starts. Usually you'll want your main inventory to get focus.",MMInformationAttribute.InformationType.Info,false)]
		/// if true, the engine will automatically create bindings to navigate through the different slots using keyboard or gamepad.
		public bool EnableNavigation = true;
		/// if this is true, this inventory display will get the focus on start
		public bool GetFocusOnStart = false;

		[Header("Title Text")]
		[MMInformation("Here you can decide to display (or not) a title next to your inventory display panel. For it you can specify the title, font, font size, color etc.",MMInformationAttribute.InformationType.Info,false)]
		/// if true, will display the panel's title
		public bool DisplayTitle=true;
		/// the title for the inventory that will be displayed
		public string Title;
		/// the font used to display the quantity
		public Font TitleFont;
		/// the font size to use 
		public int TitleFontSize=20;
		/// the color to display the quantity in
		public Color TitleColor = Color.black;
		/// the padding (distance to the slot's edge)
		public Vector3 TitleOffset=Vector3.zero;
		/// where the quantity should be displayed
		public TextAnchor TitleAlignment = TextAnchor.LowerRight;

		[Header("Quantity Text")]
		[MMInformation("If your inventory contains stacked items (more than one item of a certain sort in a single slot, like coins or potions maybe) you'll probably want to display the quantity next to the item's icon. For that, you can specify here the font to use, the color, and position of that quantity text.",MMInformationAttribute.InformationType.Info,false)]
		/// the font used to display the quantity
		public Font QtyFont;
		/// the font size to use 
		public int QtyFontSize=12;
		/// the color to display the quantity in
		public Color QtyColor = Color.black;
		/// the padding (distance to the slot's edge)
		public float QtyPadding=10f;
		/// where the quantity should be displayed
		public TextAnchor QtyAlignment = TextAnchor.LowerRight;

		[Header("Extra Inventory Navigation")]
		[MMInformation("The InventoryInputManager comes with controls allowing you to go from one inventory panel to the next. Here you can define what inventory the player should go to from this panel when pressing the previous or next inventory button.",MMInformationAttribute.InformationType.Info,false)]
		public InventoryDisplay PreviousInventory;
		public InventoryDisplay NextInventory;

		/// the grid layout used to display the inventory in rows and columns
		public GridLayoutGroup InventoryGrid { get; protected set; }
		/// the gameobject used to display the inventory's name
		public InventoryDisplayTitle InventoryTitle { get; protected set; }
		/// the main panel
		public RectTransform InventoryRectTransform { get { return GetComponent<RectTransform>(); }}
		/// an internal list of slots
		public List<GameObject> SlotContainer { get; protected set; }	
		/// the inventory the focus should return to after an action
		public InventoryDisplay ReturnInventory { get; protected set; }	

		/// the item currently being moved
		[MMHidden]
		public int CurrentlyBeingMovedItemIndex=-1;

		protected bool _inventoryWindowIsOpen;
		protected List<InventoryItem> _contentLastUpdate;		
		protected List<int> _changes;		
		protected List<int> _comparison;	
		protected SpriteState _spriteState = new SpriteState();
		protected InventorySlot _currentlySelectedSlot;

		protected GameObject _slotPrefab = null;

		/// <summary>
		/// Creates and sets up the inventory display (usually called via the inspector's dedicated button)
		/// </summary>
		public virtual void SetupInventoryDisplay()
		{
			/*
			if (TargetInventoryName == "")
			{
				Debug.LogError("The " + this.name + " Inventory Display doesn't have a TargetInventoryName set. You need to set one from its inspector, matching an Inventory's name.");
				return;
			}
			*/

			if (TargetInventory == null)
			{
				Debug.LogError("The " + this.name + " Inventory Display couldn't find a TargetInventory. You either need to create an inventory with a matching inventory name (" + TargetInventoryName + "), or set that TargetInventoryName to one that exists.");
				return;
			}

			// if we also have a sound player component, we set it up too
			if (this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer>() != null)
			{
				this.gameObject.MMGetComponentNoAlloc<InventorySoundPlayer> ().SetupInventorySoundPlayer ();
			}

			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryTitle();
			ResizeInventoryDisplay ();
			DrawInventoryContent();
		}

		/// <summary>
		/// On Awake, initializes the various lists used to keep track of the content of the inventory
		/// </summary>
		protected virtual void Awake()
		{
			_contentLastUpdate = new List<InventoryItem>();		
			SlotContainer = new List<GameObject>() ;		
			_comparison = new List<int>();
			if (!TargetInventory.Persistent)
			{
				RedrawInventoryDisplay (); 	
			}
		}

		/// <summary>
		/// Redraws the inventory display's contents when needed (usually after a change in the target inventory)
		/// </summary>
		protected virtual void RedrawInventoryDisplay()
		{
			InitializeSprites();
			AddGridLayoutGroup();
			DrawInventoryContent();		
			FillLastUpdateContent();	
		}

		/// <summary>
		/// Initializes the sprites.
		/// </summary>
		protected virtual void InitializeSprites()
		{
			// we create a spriteState to specify our various button states
			_spriteState.disabledSprite= DisabledSlotImage;
			_spriteState.highlightedSprite= HighlightedSlotImage;
			_spriteState.pressedSprite= PressedSlotImage;
		}

		/// <summary>
		/// Adds and sets up the inventory title child object
		/// </summary>
		protected virtual void DrawInventoryTitle()
		{
			if (!DisplayTitle)
			{
				return;
			}
			if (GetComponentInChildren<InventoryDisplayTitle>()!=null)
			{
				if (Application.isEditor)
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						DestroyImmediate(title.gameObject);
					}
				}
				else
				{
					foreach (InventoryDisplayTitle title in GetComponentsInChildren<InventoryDisplayTitle>())
					{
						Destroy(title.gameObject);
					}
				}
			}
			GameObject inventoryTitle = new GameObject();
			InventoryTitle = inventoryTitle.AddComponent<InventoryDisplayTitle>();
			inventoryTitle.name="InventoryTitle";
			inventoryTitle.GetComponent<RectTransform>().SetParent(this.transform);
			inventoryTitle.GetComponent<RectTransform>().sizeDelta=GetComponent<RectTransform>().sizeDelta;
			inventoryTitle.GetComponent<RectTransform>().localPosition=TitleOffset;
			inventoryTitle.GetComponent<RectTransform>().localScale=Vector3.one;
			InventoryTitle.text=Title;
			InventoryTitle.color=TitleColor;
			InventoryTitle.font=TitleFont;
			InventoryTitle.fontSize=TitleFontSize;
			InventoryTitle.alignment=TitleAlignment;
			InventoryTitle.raycastTarget=false;
		}

		/// <summary>
		/// Adds a grid layout group if there ain't one already
		/// </summary>
		protected virtual void AddGridLayoutGroup()
		{
			if (GetComponentInChildren<InventoryDisplayGrid>() == null)
			{
				GameObject inventoryGrid=new GameObject("InventoryDisplayGrid");
				inventoryGrid.transform.parent=this.transform;
				inventoryGrid.transform.position=transform.position;
				inventoryGrid.transform.localScale=Vector3.one;
				inventoryGrid.AddComponent<InventoryDisplayGrid>();
				InventoryGrid = inventoryGrid.AddComponent<GridLayoutGroup>();
			}
			if (InventoryGrid == null)
			{
				InventoryGrid = GetComponentInChildren<GridLayoutGroup>();
			}
			InventoryGrid.padding.top = PaddingTop;
			InventoryGrid.padding.right = PaddingRight;
			InventoryGrid.padding.bottom = PaddingBottom;
			InventoryGrid.padding.left = PaddingLeft;
			InventoryGrid.cellSize = SlotSize;
			InventoryGrid.spacing = SlotMargin;
		}

		/// <summary>
		/// Resizes the inventory panel, taking into account the number of rows/columns, the padding and margin
		/// </summary>
		protected void ResizeInventoryDisplay()
		{

			float newWidth = PaddingLeft + SlotSize.x * NumberOfColumns + SlotMargin.x * (NumberOfColumns-1) + PaddingRight;
			float newHeight = PaddingTop + SlotSize.y * NumberOfRows + SlotMargin.y * (NumberOfRows-1) + PaddingBottom;

			TargetInventory.ResizeArray(NumberOfRows * NumberOfColumns);	

			Vector2 newSize= new Vector2(newWidth,newHeight);
			InventoryRectTransform.sizeDelta = newSize;
			InventoryGrid.GetComponent<RectTransform>().sizeDelta = newSize;
		}

		/// <summary>
		/// Draws the content of the inventory (slots and icons)
		/// </summary>
		protected virtual void DrawInventoryContent ()             
		{            
			if (SlotContainer!=null)
			{
				SlotContainer.Clear();
			}
			else
			{
				SlotContainer=new List<GameObject>();
			}
			// we initialize our sprites 
			if (EmptySlotImage==null)
			{
				InitializeSprites();
			}
			// we remove all existing slots
			foreach (InventorySlot slot in transform.GetComponentsInChildren<InventorySlot>())
			{	 			
				if (Application.isEditor)
				{
					DestroyImmediate (slot.gameObject);
				}
				else
				{
					Destroy(slot.gameObject);
				}				
			}
			// for each slot we create the slot and its content
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{    
				DrawSlot(i);
			}	
			DestroyImmediate (_slotPrefab);

			if (EnableNavigation)
			{
				SetupSlotNavigation();
			}
		}

		/// <summary>
		/// If the content has changed, we draw our inventory panel again
		/// </summary>
		protected virtual void ContentHasChanged()
		{
			if (!(Application.isPlaying))
			{
				AddGridLayoutGroup();
				DrawInventoryContent () ;
				#if UNITY_EDITOR
					EditorUtility.SetDirty(gameObject);
				#endif
			}
			else
			{
				UpdateInventoryContent();
			}
		}

		/// <summary>
		/// Fills the last content of the update.
		/// </summary>
		protected virtual void FillLastUpdateContent()		
		{		
			_contentLastUpdate.Clear();		
			_comparison.Clear();
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 		
			{  		
				if (!InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					_contentLastUpdate.Add(TargetInventory.Content[i].Copy());	
				}
				else
				{
					_contentLastUpdate.Add(null);	
				}	
			}	
		}

		/// <summary>
		/// Draws the content of the inventory (slots and icons)
		/// </summary>
		protected virtual void UpdateInventoryContent ()             
		{      
			if (_contentLastUpdate == null || _contentLastUpdate.Count == 0)
			{
				FillLastUpdateContent();
			}

			// we compare our current content with the one in storage to look for changes
			for (int i = 0; i < TargetInventory.Content.Length; i ++) 
			{
				if ((TargetInventory.Content[i] == null) && (_contentLastUpdate[i] != null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i] == null))
				{
					_comparison.Add(i);
				}
				if ((TargetInventory.Content[i] != null) && (_contentLastUpdate[i] != null))
				{
					if ((TargetInventory.Content[i].ItemID != _contentLastUpdate[i].ItemID) || (TargetInventory.Content[i].Quantity != _contentLastUpdate[i].Quantity))
					{
						_comparison.Add(i);
					}
				}
			}
			if (_comparison.Count>0)
			{
				foreach (int comparison in _comparison)
				{
					UpdateSlot(comparison);
				}
			} 	    
			FillLastUpdateContent();
		}

		/// <summary>
		/// Updates the slot's content and appearance
		/// </summary>
		/// <param name="i">The index.</param>
		protected virtual void UpdateSlot(int i)
		{
			if (SlotContainer.Count < i)
			{
				Debug.LogWarning ("It looks like your inventory display wasn't properly initialized. If you're not triggering any Load events, you may want to mark your inventory as non persistent in its inspector. Otherwise, you may want to reset and empty saved inventories and try again.");
			}
			
			if (SlotContainer[i] == null)
			{
				return;
			}
			// we update the slot's bg image
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				SlotContainer[i].GetComponent<Image>().sprite = FilledSlotImage;   
			}
			else
			{
				SlotContainer[i].GetComponent<Image>().sprite = EmptySlotImage;    	
			}
			// we remove potential child objects
			foreach(Transform child in SlotContainer[i].transform)
			{
				GameObject.Destroy(child.gameObject);
			}
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				// we redraw the icon
				SlotContainer[i].GetComponent<InventorySlot>().DrawIcon(TargetInventory.Content[i],i);
			}			   
		}

		/// <summary>
		/// Creates the slot prefab to use in all slot creations
		/// </summary>
		protected virtual void InitializeSlotPrefab()
		{
			_slotPrefab = new GameObject();
			_slotPrefab.AddComponent<RectTransform>();

			_slotPrefab.AddComponent<Image> ();
			_slotPrefab.MMGetComponentNoAlloc<Image> ().raycastTarget = true;

			_slotPrefab.AddComponent<InventorySlot> ();
			_slotPrefab.MMGetComponentNoAlloc<InventorySlot> ().transition = Selectable.Transition.SpriteSwap;

			Navigation explicitNavigation = new Navigation ();
			explicitNavigation.mode = Navigation.Mode.Explicit;
			_slotPrefab.GetComponent<InventorySlot> ().navigation = explicitNavigation;

			_slotPrefab.MMGetComponentNoAlloc<InventorySlot> ().interactable = true;

			_slotPrefab.AddComponent<CanvasGroup> ();
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().alpha = 1;
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().interactable = true;
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().blocksRaycasts = true;
			_slotPrefab.MMGetComponentNoAlloc<CanvasGroup> ().ignoreParentGroups = false;

			_slotPrefab.name = "SlotPrefab";
		}

		/// <summary>
		/// Draws the slot and its content (icon, quantity...).
		/// </summary>
		/// <param name="i">The index.</param>
		protected virtual void DrawSlot(int i)
		{
			if (!DrawEmptySlots)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					return;
				}
			}

			if (_slotPrefab == null)
			{
				InitializeSlotPrefab ();
			}

			GameObject theSlot = (GameObject)Instantiate(_slotPrefab);

			theSlot.transform.SetParent(InventoryGrid.transform);
			theSlot.GetComponent<RectTransform>().localScale=Vector3.one;
			theSlot.transform.position = transform.position;
			theSlot.name="Slot "+i;

			// we add the background image
			if (!InventoryItem.IsNull(TargetInventory.Content[i]))
			{
				theSlot.GetComponent<Image>().sprite = FilledSlotImage;   
			}
			else
			{
				theSlot.GetComponent<Image>().sprite = EmptySlotImage;      	
			}
			theSlot.GetComponent<Image>().type = SlotImageType;
			theSlot.GetComponent<InventorySlot>().spriteState=_spriteState;
			theSlot.GetComponent<InventorySlot>().MovedSprite=MovedSlotImage;
			theSlot.GetComponent<InventorySlot>().ParentInventoryDisplay = this;
			theSlot.GetComponent<InventorySlot>().Index=i;

			SlotContainer.Add(theSlot);	

			theSlot.SetActive(true)	;

			theSlot.GetComponent<InventorySlot>().DrawIcon(TargetInventory.Content[i],i);
		}

		/// <summary>
		/// Setups the slot navigation using Unity's GUI built-in system, so that the user can move using the left/right/up/down arrows
		/// </summary>
		protected virtual void SetupSlotNavigation()
		{
			if (!EnableNavigation)
			{
				return;
			}

			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (SlotContainer[i]==null)
				{
					return;
				}
				Navigation navigation = SlotContainer[i].GetComponent<InventorySlot>().navigation;
				// we determine where to go when going up
				if (i-NumberOfColumns >= 0) 
				{
					navigation.selectOnUp = SlotContainer[i-NumberOfColumns].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnUp=null;
				}
				// we determine where to go when going down
				if (i+NumberOfColumns < SlotContainer.Count) 
				{
					navigation.selectOnDown = SlotContainer[i+NumberOfColumns].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnDown=null;
				}
				// we determine where to go when going left
				if ((i%NumberOfColumns != 0) && (i>0))
				{
					navigation.selectOnLeft = SlotContainer[i-1].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnLeft=null;
				}
				// we determine where to go when going right
				if (((i+1)%NumberOfColumns != 0)  && (i<SlotContainer.Count - 1))
				{
					navigation.selectOnRight = SlotContainer[i+1].GetComponent<InventorySlot>();
				}
				else
				{
					navigation.selectOnRight=null;
				}
				SlotContainer[i].GetComponent<InventorySlot>().navigation = navigation;
			}
		}

		/// <summary>		
		/// Sets the focus on the first item of the inventory		
		/// </summary>		
		public virtual void Focus()		
		{
			if (!EnableNavigation)
			{
				return;
			}
			if (transform.GetComponentInChildren<InventorySlot> () != null) 		
			{		
				transform.GetComponentInChildren<InventorySlot> ().Select ();	

				if (EventSystem.current.currentSelectedGameObject == null) 		
				{	
					EventSystem.current.SetSelectedGameObject (transform.GetComponentInChildren<InventorySlot> ().gameObject);		
				}		
			}					
		}

		/// <summary>
		/// Returns the currently selected inventory slot
		/// </summary>
		/// <returns>The selected inventory slot.</returns>
		public virtual InventorySlot CurrentlySelectedInventorySlot()
		{
			return _currentlySelectedSlot;
		}

		/// <summary>
		/// Sets the currently selected slot
		/// </summary>
		/// <param name="slot">Slot.</param>
		public virtual void SetCurrentlySelectedSlot(InventorySlot slot)
		{
			_currentlySelectedSlot = slot;
		}

		/// <summary>
		/// Goes to the previous (-1) or next (1) inventory, based on the int direction passed in parameter.
		/// </summary>
		/// <param name="direction">Direction.</param>
		public virtual InventoryDisplay GoToInventory(int direction)
		{
			if (direction==-1)
			{
				if (PreviousInventory==null)
				{
					return null;
				}
				PreviousInventory.Focus();
				return PreviousInventory;
			}
			else
			{
				if (NextInventory==null)
				{
					return null;
				}
				NextInventory.Focus();	
				return NextInventory;			
			}
		}

		/// <summary>
		/// Sets the return inventory display
		/// </summary>
		/// <param name="inventoryDisplay">Inventory display.</param>
		public virtual void SetReturnInventory(InventoryDisplay inventoryDisplay)
		{
			ReturnInventory = inventoryDisplay;
		}

		/// <summary>
		/// If possible, returns the focus to the current return inventory focus (after equipping an item, usually)
		/// </summary>
		public virtual void ReturnInventoryFocus()
		{
			if (ReturnInventory==null)
			{
				return;
			}
			else
			{
				ResetDisabledStates();
				ReturnInventory.Focus();
				ReturnInventory=null;
			}
		}

		/// <summary>
		/// Disables all the slots in the inventory display, except those from a certain class
		/// </summary>
		/// <param name="itemClass">Item class.</param>
		public virtual void DisableAllBut(ItemClasses itemClass)
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				if (InventoryItem.IsNull(TargetInventory.Content[i]))
				{
					continue;
				}
				if (TargetInventory.Content[i].ItemClass!=itemClass)
				{
					SlotContainer[i].GetComponent<InventorySlot>().DisableSlot();
				}
			}
		}

		/// <summary>
		/// Enables back all slots (usually after having disabled some of them)
		/// </summary>
		public virtual void ResetDisabledStates()
		{
			for (int i=0; i<SlotContainer.Count;i++)
			{
				SlotContainer[i].GetComponent<InventorySlot>().EnableSlot();
			}
		}

		/// <summary>
		/// Catches MMInventoryEvents and acts on them
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			// if this event doesn't concern our inventory display, we do nothing and exit
			if (inventoryEvent.TargetInventoryName != this.TargetInventoryName)
			{
				return;
			}

			switch (inventoryEvent.InventoryEventType)
			{
				case MMInventoryEventType.Select:
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Click:
					ReturnInventoryFocus ();
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
					break;

				case MMInventoryEventType.Move:
					this.ReturnInventoryFocus();
                    UpdateSlot(inventoryEvent.Index);

                    break;

				case MMInventoryEventType.ItemUsed:
					this.ReturnInventoryFocus();
					break;
				
				case MMInventoryEventType.EquipRequest:
					if (this.TargetInventory.InventoryType == Inventory.InventoryTypes.Equipment)
					{
						// if there's no target inventory set we do nothing and exit
						if (TargetChoiceInventory == null)
						{
							Debug.LogWarning ("InventoryEngine Warning : " + this + " has no choice inventory associated to it.");
							return;
						}
						// we disable all the slots that don't match the right type
						TargetChoiceInventory.DisableAllBut (this.ItemClass);
						// we set the focus on the target inventory
						TargetChoiceInventory.Focus ();
						// we set the return focus inventory
						TargetChoiceInventory.SetReturnInventory (this);
					}
					break;
				
				case MMInventoryEventType.ItemEquipped:
					ReturnInventoryFocus();
					break;

				case MMInventoryEventType.Drop:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.ItemUnEquipped:
					this.ReturnInventoryFocus ();
					break;

				case MMInventoryEventType.InventoryOpens:
					Focus();
					CurrentlyBeingMovedItemIndex = -1;
					_inventoryWindowIsOpen=true;
					EventSystem.current.sendNavigationEvents=true;
					break;

				case MMInventoryEventType.InventoryCloses:
					CurrentlyBeingMovedItemIndex = -1;
					_inventoryWindowIsOpen = false;
					EventSystem.current.sendNavigationEvents=false;
					SetCurrentlySelectedSlot (inventoryEvent.Slot);
				break;

				case MMInventoryEventType.ContentChanged:
					ContentHasChanged ();
					break;

				case MMInventoryEventType.Redraw:
					RedrawInventoryDisplay ();
					break;

				case MMInventoryEventType.InventoryLoaded:
					RedrawInventoryDisplay ();
					if (GetFocusOnStart)
					{
						Focus();
					}
					break;
			}
		}

		/// <summary>
		/// On Enable, we start listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}

		/// <summary>
		/// On Disable, we stop listening for MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}
