using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Add this component to a UI rectangle and it'll act as a detection zone for a joystick.
	/// Note that this component extends the MMTouchJoystick class so you don't need to add another joystick to it. It's both the detection zone and the stick itself.
	/// </summary>
	public class MMTouchDynamicJoystick : MMTouchJoystick, IPointerDownHandler
	{
		[Header("Dynamic Joystick")]
		[MMInformation("Here you can select an image for your joystick's knob, and decide if the joystick's detection zone should reset its position whenever the drag ends.", MMInformationAttribute.InformationType.Info,false)]
		/// the image to use for your joystick's knob
		public Sprite JoystickKnobImage;
		/// if this is set to true, the joystick's touch zone will revert to its initial position whenever the player drops the joystick. If not, it'll stay where it was.
		public bool RestorePosition = true;

		protected Vector3 _initialPosition;
		protected Vector3 _newPosition;
		protected CanvasGroup _knobCanvasGroup;

		/// <summary>
		/// On Start, we instantiate our joystick's image if there's one
		/// </summary>
		protected override void Start()
		{
			base.Start();

			// we store the detection zone's initial position
			_initialPosition = GetComponent<RectTransform>().localPosition;	

			// we instantiate our joystick knob 
			if (JoystickKnobImage!=null)
			{
				GameObject knob = new GameObject();
				knob.transform.SetParent(gameObject.transform);
				knob.name="DynamicJoystickKnob";
				knob.transform.position = transform.position;
				knob.transform.localScale = transform.localScale;

				Image knobImage = knob.AddComponent<Image>();
				knobImage.sprite = JoystickKnobImage;

				_knobCanvasGroup = knob.AddComponent<CanvasGroup>();
			}
		}

		/// <summary>
		/// When the zone is pressed, we move our joystick accordingly
		/// </summary>
		/// <param name="data">Data.</param>
		public virtual void OnPointerDown(PointerEventData data)
	    {
			// if we're in "screen space - camera" render mode
			if (ParentCanvasRenderMode == RenderMode.ScreenSpaceCamera)
			{
				_newPosition = TargetCamera.ScreenToWorldPoint(data.position);
			}
			// otherwise
			else
			{
				_newPosition = data.position;
			}
			_newPosition.z = this.transform.position.z;

			// we define a new neutral position
			SetNeutralPosition(_newPosition);
	    }

	    /// <summary>
	    /// When the player lets go of the stick, we restore our stick's position if needed
	    /// </summary>
	    /// <param name="eventData">Event data.</param>
		public override void OnEndDrag(PointerEventData eventData)
		{
			base.OnEndDrag(eventData);
			if (RestorePosition)
			{
				GetComponent<RectTransform>().localPosition = _initialPosition;
			}
		}
	}
}
