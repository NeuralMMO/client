using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using MoreMountains.Tools;
using UnityEngine.Events;

namespace MoreMountains.Tools
{
	public class MMTriggerAndCollision : MonoBehaviour 
	{
		public LayerMask CollisionLayerMask;
		public UnityEvent OnCollisionEnterEvent;
		public UnityEvent OnCollisionExitEvent;
		public UnityEvent OnCollisionStayEvent;

		public LayerMask TriggerLayerMask;
		public UnityEvent OnTriggerEnterEvent;
		public UnityEvent OnTriggerExitEvent;
		public UnityEvent OnTriggerStayEvent;

		public LayerMask Collision2DLayerMask;
		public UnityEvent OnCollision2DEnterEvent;
		public UnityEvent OnCollision2DExitEvent;
		public UnityEvent OnCollision2DStayEvent;

		public LayerMask Trigger2DLayerMask;
		public UnityEvent OnTrigger2DEnterEvent;
		public UnityEvent OnTrigger2DExitEvent;
		public UnityEvent OnTrigger2DStayEvent;

		// Collision 2D ------------------------------------------------------------------------------------

		protected virtual void OnCollisionEnter2D (Collision2D collision)
		{
			if (Collision2DLayerMask.MMContains (collision.gameObject))
			{
				OnCollision2DEnterEvent.Invoke();
			}
		}

		protected virtual void OnCollisionExit2D (Collision2D collision)
		{
			if (Collision2DLayerMask.MMContains (collision.gameObject))
			{
				OnCollision2DExitEvent.Invoke();
			}
		}

		protected virtual void OnCollisionStay2D (Collision2D collision)
		{
			if (Collision2DLayerMask.MMContains (collision.gameObject))
			{
				OnCollision2DStayEvent.Invoke();
			}
		}

		// Trigger 2D ------------------------------------------------------------------------------------

		protected virtual void OnTriggerEnter2D (Collider2D collider)
		{
			if (Trigger2DLayerMask.MMContains (collider.gameObject))
			{
				OnTrigger2DEnterEvent.Invoke();
			}
		}

		protected virtual void OnTriggerExit2D (Collider2D collider)
		{
			if (Trigger2DLayerMask.MMContains (collider.gameObject))
			{
				OnTrigger2DExitEvent.Invoke();
			}
		}

		protected virtual void OnTriggerStay2D (Collider2D collider)
		{
			if (Trigger2DLayerMask.MMContains (collider.gameObject))
			{
				OnTrigger2DStayEvent.Invoke();
			}
		}

		// Collision ------------------------------------------------------------------------------------

		protected virtual void OnCollisionEnter(Collision c)
		{
			if (0 != (CollisionLayerMask.value & 1 << c.transform.gameObject.layer))
			{
				OnCollisionEnterEvent.Invoke();
			}
		}

		protected virtual void OnCollisionExit(Collision c)
		{
			if (0 != (CollisionLayerMask.value & 1 << c.transform.gameObject.layer))
			{
				OnCollisionExitEvent.Invoke();
			}
		}

		protected virtual void OnCollisionStay(Collision c)
		{
			if (0 != (CollisionLayerMask.value & 1 << c.transform.gameObject.layer))
			{
				OnCollisionStayEvent.Invoke();
			}
		}

		// Trigger  ------------------------------------------------------------------------------------

		protected virtual void OnTriggerEnter (Collider collider)
		{
			if (TriggerLayerMask.MMContains (collider.gameObject))
			{
				OnTriggerEnterEvent.Invoke();
			}
		}

		protected virtual void OnTriggerExit (Collider collider)
		{
			if (TriggerLayerMask.MMContains (collider.gameObject))
			{
				OnTriggerExitEvent.Invoke();
			}
		}

		protected virtual void OnTriggerStay (Collider collider)
		{
			if (TriggerLayerMask.MMContains (collider.gameObject))
			{
				OnTriggerStayEvent.Invoke();
			}
		}

		protected virtual void Reset()
		{
			Collision2DLayerMask = LayerMask.NameToLayer("Everything");
			CollisionLayerMask = LayerMask.NameToLayer("Everything");
		}
	}
}
