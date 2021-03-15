using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// This class acts as an interface to allow the demo levels to work whether the environment (colliders, rigidbodies) are set as 2D or 3D.
	/// If you already know for sure that you're going for a 2D or 3D game, I suggest you replace the use of this class with the appropriate classes.
	/// </summary>
	public class MMRigidbodyInterface : MonoBehaviour 
	{	
		/// <summary>
		/// Returns the rigidbody's position
		/// </summary>
		/// <value>The position.</value>
		public Vector3 position
	    {
	        get
	        {
	            if (_rigidbody2D != null)
	            {
	                return _rigidbody2D.position;
	            }
	            if (_rigidbody != null)
	            {
	                return _rigidbody.position;
	            }
	            return Vector3.zero;
	        }
	        set { }
	    }

		/// <summary>
		/// Only use if you absolutely need to target the rigidbody2D specifically
		/// </summary>
		/// <value>The internal rigid body2 d.</value>
		public Rigidbody2D InternalRigidBody2D 
		{
			get {
				return _rigidbody2D;
			}
		}

		/// <summary>
		/// Only use if you absolutely need to target the rigidbody2D specifically
		/// </summary>
		/// <value>The internal rigid body.</value>
		public Rigidbody InternalRigidBody 
		{
			get {
				return _rigidbody;
			}
		} 

		/// <summary>
		/// Gets or sets the velocity of the rigidbody associated to the interface.
		/// </summary>
		/// <value>The velocity.</value>
		public Vector3 Velocity 
		{
			get 
			{ 
				if (_mode == "2D") 
				{
					return(_rigidbody2D.velocity);
				}
				else 
				{
					if (_mode == "3D") 
					{
						return(_rigidbody.velocity);
					}
					else
					{
						return new Vector3(0,0,0);
					}
				}
			}
			set 
			{
				if (_mode == "2D") {
					_rigidbody2D.velocity = value;
				}
				if (_mode == "3D") {
					_rigidbody.velocity = value;
				}
			}
		}

		/// <summary>
		/// Gets the collider bounds.
		/// </summary>
		/// <value>The collider bounds.</value>
		public Bounds ColliderBounds 
		{ 
			get 
			{  
				if (_rigidbody2D != null) 
				{
					return _collider2D.bounds;
				}
				if (_rigidbody != null) 
				{
					return _collider.bounds;
				}
				return new Bounds();
			}
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="MoreMountains.Tools.RigidbodyInterface"/> is kinematic.
		/// </summary>
		/// <value><c>true</c> if is kinematic; otherwise, <c>false</c>.</value>
		public bool isKinematic
		{
			get
			{
				if (_mode == "2D") 
				{
					return(_rigidbody2D.isKinematic);
				}
				if (_mode == "3D")
				{			
					return(_rigidbody.isKinematic);
				}
				return false;
			}
		}

		protected string _mode;
		protected Rigidbody2D _rigidbody2D;
		protected Rigidbody _rigidbody;
		protected Collider2D _collider2D;
		protected Collider _collider;
		protected Bounds _colliderBounds;

		/// <summary>
		/// Initialization
		/// </summary>
		protected virtual void Awake () 
		{
			// we check for rigidbodies, and depending on their presence determine if the interface will work with 2D or 3D rigidbodies and colliders.
			_rigidbody2D=GetComponent<Rigidbody2D>();
			_rigidbody=GetComponent<Rigidbody>();

			if (_rigidbody2D != null) 
			{
				_mode="2D";
				_collider2D = GetComponent<Collider2D> ();
			}
			if (_rigidbody != null) 
			{
				_mode="3D";
				_collider = GetComponent<Collider> ();
			}
			if (_rigidbody==null && _rigidbody2D==null)
			{
				Debug.LogWarning("A RigidBodyInterface has been added to "+gameObject+" but there's no Rigidbody or Rigidbody2D on it.", gameObject);
			}
		}
		
		/// <summary>
		/// Adds the specified force to the rigidbody associated to the interface..
		/// </summary>
		/// <param name="force">Force.</param>
		public virtual void AddForce(Vector3 force)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.AddForce(force,ForceMode2D.Impulse);
			}
			if (_mode == "3D")
			{
				_rigidbody.AddForce(force);
			}
		}

		/// <summary>
		/// Adds the specified relative force to the rigidbody associated to the interface..
		/// </summary>
		/// <param name="force">Force.</param>
		public virtual void AddRelativeForce(Vector3 force)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.AddRelativeForce(force,ForceMode2D.Impulse);
			}
			if (_mode == "3D")
			{
				_rigidbody.AddRelativeForce(force);
			}
		}



	    /// <summary>
	    /// Move the rigidbody to the position vector specified
	    /// </summary>
	    /// <param name="newPosition"></param>
	    public virtual void MovePosition(Vector3 newPosition)
	    {
	        if (_mode == "2D")
	        {
	            _rigidbody2D.MovePosition(newPosition);
	        }
	        if (_mode == "3D")
	        {
	            _rigidbody.MovePosition(newPosition);
	        }
	    }

		/// <summary>
		/// Resets the angular velocity.
		/// </summary>
		public virtual void ResetAngularVelocity()
		{
			if (_mode == "2D")
			{
				_rigidbody2D.angularVelocity = 0;
			}
			if (_mode == "3D")
			{
				_rigidbody.angularVelocity = Vector3.zero;
			}	
		}

		/// <summary>
		/// Resets the rotation.
		/// </summary>
		public virtual void ResetRotation()
		{
			if (_mode == "2D")
			{
				_rigidbody2D.rotation = 0;
			}
			if (_mode == "3D")
			{
				_rigidbody.rotation = Quaternion.identity;
			}	
		}
			
		
		/// <summary>
		/// Determines whether the rigidbody associated to the interface is kinematic
		/// </summary>
		/// <returns><c>true</c> if this instance is kinematic the specified status; otherwise, <c>false</c>.</returns>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void IsKinematic(bool status)
		{
			if (_mode == "2D") 
			{
				_rigidbody2D.isKinematic=status;
			}
			if (_mode == "3D")
			{			
				_rigidbody.isKinematic=status;
			}
		}

		
		/// <summary>
		/// Enables the box collider associated to the interface.
		/// </summary>
		/// <param name="status">If set to <c>true</c> status.</param>
		public virtual void EnableBoxCollider(bool status)
		{
			if (_mode == "2D") 
			{
				GetComponent<Collider2D>().enabled=status;
			}
			if (_mode == "3D")
			{			
				GetComponent<Collider>().enabled=status;
			}
		}

		/// <summary>
		/// Use this to check if you're dealing with a 3D object
		/// </summary>
		/// <value><c>true</c> if this instance is3 d; otherwise, <c>false</c>.</value>
		public bool Is3D 
		{ 
			get
	        {
				if (_mode=="3D") 
				{ 
					return true; 
				} 
				else 
				{ 
					return false; 
				}
			}
		}

		/// <summary>
		/// Use this to check if you're dealing with a 2D object
		/// </summary>
		/// <value>The position.</value>
		public bool Is2D 
		{ 
			get
	        {
				if (_mode=="2D") 
				{ 
					return true; 
				} 
				else
				{ 
					return false; 
				}
			}
		}
	}
}
