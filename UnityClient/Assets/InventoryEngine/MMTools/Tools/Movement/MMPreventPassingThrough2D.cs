using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	/// <summary>
	/// Prevents fast moving objects from going through colliders by casting a ray backwards after each movement
	/// </summary>
	public class MMPreventPassingThrough2D : MonoBehaviour 
	{
		/// the layer mask to search obstacles on
		public LayerMask ObstaclesLayerMask; 
		/// the bounds adjustment variable
		public float SkinWidth = 0.1f;

		protected float _smallestBoundsWidth; 
		protected float _adjustedSmallestBoundsWidth; 
		protected float _squaredBoundsWidth; 
		protected Vector2 _positionLastFrame; 
		protected Rigidbody2D _rigidbody;
		protected Collider2D _collider;
		protected Vector2 _lastMovement;
		protected float _lastMovementSquared;

		/// <summary>
		/// On Start we initialize our object
		/// </summary>
		protected virtual void Start() 
		{ 
			Initialization ();
		} 

		/// <summary>
		/// Grabs the rigidbody and computes the bounds width
		/// </summary>
		protected virtual void Initialization()
		{
			_rigidbody = GetComponent<Rigidbody2D>();
			_positionLastFrame = _rigidbody.position; 

			_collider = GetComponent<Collider2D>();

			_smallestBoundsWidth = Mathf.Min(Mathf.Min(_collider.bounds.extents.x, _collider.bounds.extents.y), _collider.bounds.extents.z); 
			_adjustedSmallestBoundsWidth = _smallestBoundsWidth * (1.0f - SkinWidth); 
			_squaredBoundsWidth = _smallestBoundsWidth * _smallestBoundsWidth; 
		}

		/// <summary>
		/// On Enable, we initialize our last frame position
		/// </summary>
		protected virtual void OnEnable()
		{
			_positionLastFrame = this.transform.position;
		}

		/// <summary>
		/// On fixedUpdate, checks the last movement and if needed casts a ray to detect obstacles
		/// </summary>
		protected virtual void Update() 
		{ 
			_lastMovement = _rigidbody.position - _positionLastFrame; 
			_lastMovementSquared = _lastMovement.sqrMagnitude;

			// if we've moved further than our bounds, we may have missed something
			if (_lastMovementSquared > _squaredBoundsWidth) 
			{ 
				float movementMagnitude = Mathf.Sqrt(_lastMovementSquared);

                // we cast a ray backwards to see if we should have hit something
                RaycastHit2D hitInfo = Physics2D.Raycast(_positionLastFrame, _lastMovement, movementMagnitude, ObstaclesLayerMask);

                if (hitInfo.collider != null)
				{				

					if (hitInfo.collider.isTrigger) 
					{
						hitInfo.collider.SendMessage("OnTriggerEnter2D", _collider, SendMessageOptions.DontRequireReceiver);
					}						

					if (!hitInfo.collider.isTrigger)
					{
						_rigidbody.position = hitInfo.point - (_lastMovement / movementMagnitude) * _adjustedSmallestBoundsWidth; 
					}
				}
			} 
			_positionLastFrame = _rigidbody.position; 
		}
	}
}


