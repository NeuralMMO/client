using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Add this class to a ParticleSystem so it auto destroys once it has stopped emitting.
	/// Make sure your ParticleSystem isn't looping, otherwise this script will be useless
	/// </summary>
	public class MMAutoDestroyParticleSystem : MonoBehaviour 
	{
		/// True if the ParticleSystem should also destroy its parent
		public bool DestroyParent=false;

		/// If for some reason your particles don't get destroyed automatically at the end of the emission, you can force a destroy after a delay. Leave it at zero otherwise.
		public float DestroyDelay=0f;
		
		protected ParticleSystem _particleSystem;
		protected float _startTime;
		
		/// <summary>
		/// Initialization, we get the ParticleSystem component
		/// </summary>
		protected virtual void Start()
		{
			_particleSystem = GetComponent<ParticleSystem>();
			if (DestroyDelay!=0)
			{
				_startTime = Time.time;
			}
		}
		
		/// <summary>
		/// When the ParticleSystem stops playing, we destroy it.
		/// </summary>
		protected virtual void Update()
		{	
			if ( (DestroyDelay!=0) && (Time.time - _startTime > DestroyDelay) )
			{
				DestroyParticleSystem();
			}	

			if (_particleSystem.isPlaying)
			{
				return;
			}

			DestroyParticleSystem();
		}

		/// <summary>
		/// Destroys the particle system.
		/// </summary>
		protected virtual void DestroyParticleSystem()
		{
			if (transform.parent!=null)
			{
				if(DestroyParent)
				{	
					Destroy(transform.parent.gameObject);	
				}
			}					
			Destroy (gameObject);
		}
	}
}
