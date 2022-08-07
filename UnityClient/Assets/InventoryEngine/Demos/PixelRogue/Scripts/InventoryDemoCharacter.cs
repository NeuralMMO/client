using UnityEngine;
using System.Collections;
using MoreMountains.Tools;

namespace MoreMountains.InventoryEngine
{	

	[RequireComponent(typeof(Rigidbody2D))]
	/// <summary>
	/// Demo character controller, very basic stuff
	/// </summary>
	public class InventoryDemoCharacter : MonoBehaviour, MMEventListener<MMInventoryEvent>
	{
		[MMInformation("A very basic demo character controller, that makes the character move around on the xy axis. Here you can change its speed and bind sprites and equipment inventories.",MMInformationAttribute.InformationType.Info,false)]

		/// the character speed
	    public float CharacterSpeed = 10f;
		/// the sprite used to show the current weapon
	    public SpriteRenderer WeaponSprite;
		/// the armor inventory
		public Inventory ArmorInventory;
		/// the weapon inventory
		public Inventory WeaponInventory;

	    protected int _currentArmor=0;
	    protected int _currentWeapon=0;
	    protected float _horizontalMove = 0f;
	    protected float _verticalMove = 0f;
	    protected Vector2 _movement;
	    protected Animator _animator;
	    protected Rigidbody2D _rigidBody2D;
	    protected bool _isFacingRight = true;

		/// <summary>
		/// On Start, we store the character's animator and rigidbody
		/// </summary>
	    protected virtual void Start()
	    {
	        _animator = GetComponent<Animator>();
	        _rigidBody2D = GetComponent<Rigidbody2D>();
	    }

		/// <summary>
		/// On fixed update we move the character and update its animator
		/// </summary>
	    protected virtual void FixedUpdate()
	    {
	        Movement();
	        UpdateAnimator();
	    }

		/// <summary>
		/// Updates the character's movement values for this frame
		/// </summary>
		/// <param name="movementX">Movement x.</param>
		/// <param name="movementY">Movement y.</param>
	    public virtual void SetMovement(float movementX, float movementY)
		{
			_horizontalMove = movementX;
	    	_verticalMove = movementY;
		}

		/// <summary>
		/// Sets the horizontal move value
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void SetHorizontalMove(float value)
		{
			_horizontalMove = value;
		}

		/// <summary>
		/// Sets the vertical move value
		/// </summary>
		/// <param name="value">Value.</param>
		public virtual void SetVerticalMove(float value)
		{
			_verticalMove = value;
		}

		/// <summary>
		/// Acts on the rigidbody's velocity to move the character based on its current horizontal and vertical values
		/// </summary>
	    protected virtual void Movement()
	    {
	        if (_horizontalMove > 0.1f)
	        {
	            if (!_isFacingRight)
	                Flip();
	        }
	        // If it's negative, then we're facing left
	        else if (_horizontalMove < -0.1f)
	        {
	            if (_isFacingRight)
	                Flip();
	        }
	        _movement = new Vector2(_horizontalMove, _verticalMove);
	        _movement *= CharacterSpeed;
	        _rigidBody2D.velocity = _movement;
	    }
	    
	    /// <summary>
		/// Flips the character and its dependencies (jetpack for example) horizontally
		/// </summary>
		protected virtual void Flip()
	    {
	        // Flips the character horizontally
	        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
	        _isFacingRight = transform.localScale.x > 0;
	    }

		/// <summary>
		/// Updates the animator's parameters
		/// </summary>
	    protected virtual void UpdateAnimator()
	    {
	        if (_animator != null)
	        {
	            _animator.SetFloat("Speed", _rigidBody2D.velocity.magnitude);
                _animator.SetInteger("Armor", _currentArmor);
	        }
	    }

		/// <summary>
		/// Sets the current armor.
		/// </summary>
		/// <param name="index">Index.</param>
	    public virtual void SetArmor(int index)
	    {
	    	_currentArmor = index;
	    }

		/// <summary>
		/// Sets the current weapon sprite
		/// </summary>
		/// <param name="newSprite">New sprite.</param>
		/// <param name="item">Item.</param>
	    public virtual void SetWeapon(Sprite newSprite, InventoryItem item)
	    {
			WeaponSprite.sprite = newSprite;
	    }

		/// <summary>
		/// Catches MMInventoryEvents and if it's an "inventory loaded" one, equips the first armor and weapon stored in the corresponding inventories
		/// </summary>
		/// <param name="inventoryEvent">Inventory event.</param>
		public virtual void OnMMEvent(MMInventoryEvent inventoryEvent)
		{
			if (inventoryEvent.InventoryEventType == MMInventoryEventType.InventoryLoaded)
			{
				if (inventoryEvent.TargetInventoryName == "RogueArmorInventory")
				{
					if (ArmorInventory != null)
					{
						if (!InventoryItem.IsNull(ArmorInventory.Content [0]))
						{
							ArmorInventory.Content [0].Equip ();	
						}
					}
				}
				if (inventoryEvent.TargetInventoryName == "RogueWeaponInventory")
				{
					if (WeaponInventory != null)
					{
						if (!InventoryItem.IsNull (WeaponInventory.Content [0]))
						{
							WeaponInventory.Content [0].Equip ();
						}
					}
				}
			}
		}

		/// <summary>
		/// On Enable, we start listening to MMInventoryEvents
		/// </summary>
		protected virtual void OnEnable()
		{
			this.MMEventStartListening<MMInventoryEvent>();
		}


		/// <summary>
		/// On Disable, we stop listening to MMInventoryEvents
		/// </summary>
		protected virtual void OnDisable()
		{
			this.MMEventStopListening<MMInventoryEvent>();
		}
	}
}