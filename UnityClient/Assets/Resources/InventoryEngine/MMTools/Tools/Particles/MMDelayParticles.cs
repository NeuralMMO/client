using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MoreMountains.Tools
{
	[ExecuteAlways]
	/// <summary>
	/// MM delay particles.
	/// </summary>
	public class MMDelayParticles : MonoBehaviour 
	{
		[Header("Delay")]
		/// the duration of the delay, in seconds
		public float Delay;
		/// if this is true, this will delay by the same amount all children particle systems of this object
		public bool DelayChildren = true;
		/// if this is true, the delay will be applied on Start
		public bool ApplyDelayOnStart = false;

		[MMInspectorButtonAttribute("ApplyDelay")]
		public bool ApplyDelayButton;

		protected Component[] particleSystems;

		protected virtual void Start()
		{
			if (ApplyDelayOnStart)
			{
				ApplyDelay();
			}
		}

		protected virtual void ApplyDelay()
		{
			if (this.gameObject.GetComponent<ParticleSystem>() != null)
			{
				var main = this.gameObject.GetComponent<ParticleSystem>().main;
				main.startDelay = main.startDelay.constant + Delay;
			}

			particleSystems = GetComponentsInChildren<ParticleSystem>();
			foreach (ParticleSystem system in particleSystems)
			{
				var main = system.main;
				main.startDelay = main.startDelay.constant + Delay;
			}

		}		
	}
}
