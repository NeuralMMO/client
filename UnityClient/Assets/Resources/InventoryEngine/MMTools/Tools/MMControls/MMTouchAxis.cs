using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{	
	[System.Serializable]
	public class AxisEvent : UnityEvent<float> {}

	[RequireComponent(typeof(Rect))]
	[RequireComponent(typeof(CanvasGroup))]
	/// <summary>
	/// Add this component to a GUI Image to have it act as an axis. 
	/// Bind pressed down, pressed continually and released actions to it from the inspector
	/// Handles mouse and multi touch
	/// </summary>
	public class MMTouchAxis : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
	{
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }
		[Header("Binding")]
		/// The method(s) to call when the axis gets pressed down
		public UnityEvent AxisPressedFirstTime;
		/// The method(s) to call when the axis gets released
		public UnityEvent AxisReleased;
		/// The method(s) to call while the axis is being pressed
		public AxisEvent AxisPressed;

		[Header("Pressed Behaviour")]
		[MMInformation("Here you can set the opacity of the button when it's pressed. Useful for visual feedback.",MMInformationAttribute.InformationType.Info,false)]
		/// the new opacity to apply to the canvas group when the axis is pressed
		public float PressedOpacity = 0.5f;
		/// the value to send the bound method when the axis is pressed
		public float AxisValue;

		[Header("Mouse Mode")]
		[MMInformation("If you set this to true, you'll need to actually press the axis for it to be triggered, otherwise a simple hover will trigger it (better for touch input).", MMInformationAttribute.InformationType.Info,false)]
		/// If you set this to true, you'll need to actually press the axis for it to be triggered, otherwise a simple hover will trigger it (better for touch input).
		public bool MouseMode = false;

		public ButtonStates CurrentState { get; protected set; }

	    protected CanvasGroup _canvasGroup;
	    protected float _initialOpacity;

	    /// <summary>
	    /// On Start, we get our canvasgroup and set our initial alpha
	    /// </summary>
	    protected virtual void Awake()
	    {
			_canvasGroup = GetComponent<CanvasGroup>();
			if (_canvasGroup!=null)
			{
				_initialOpacity = _canvasGroup.alpha;
			}
			ResetButton();
	    }

		/// <summary>
		/// Every frame, if the touch zone is pressed, we trigger the bound method if it exists
		/// </summary>
		protected virtual void Update()
	    {
			if (AxisPressed != null)
			{
				if (CurrentState == ButtonStates.ButtonPressed)
				{
					AxisPressed.Invoke(AxisValue);
				}
	        }
	    }

		/// <summary>
		/// At the end of every frame, we change our button's state if needed
		/// </summary>
		protected virtual void LateUpdate()
		{
			if (CurrentState == ButtonStates.ButtonUp)
			{
				CurrentState = ButtonStates.Off;
			}
			if (CurrentState == ButtonStates.ButtonDown)
			{
				CurrentState = ButtonStates.ButtonPressed;
			}
		}

		/// <summary>
		/// Triggers the bound pointer down action
		/// </summary>
		public virtual void OnPointerDown(PointerEventData data)
	    {
			if (CurrentState != ButtonStates.Off)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonDown;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=PressedOpacity;
			}
			if (AxisPressedFirstTime!=null)
	        {
				AxisPressedFirstTime.Invoke();
	        }
	    }

		/// <summary>
		/// Triggers the bound pointer up action
		/// </summary>
		public virtual void OnPointerUp(PointerEventData data)
		{
			if (CurrentState != ButtonStates.ButtonPressed && CurrentState != ButtonStates.ButtonDown)
			{
				return;
			}

			CurrentState = ButtonStates.ButtonUp;
			if (_canvasGroup!=null)
			{
				_canvasGroup.alpha=_initialOpacity;
			}
			if (AxisReleased != null)
			{
				AxisReleased.Invoke();
			}
			AxisPressed.Invoke(0);
	    }

		/// <summary>
	    /// OnEnable, we reset our button state
	    /// </summary>
		protected virtual void OnEnable()
	    {
			ResetButton();
	    }

	    /// <summary>
	    /// Resets the button's state and opacity
	    /// </summary>
	    protected virtual void ResetButton()
	    {
			CurrentState = ButtonStates.Off;
			_canvasGroup.alpha = _initialOpacity;
			CurrentState = ButtonStates.Off;
		}

		/// <summary>
		/// Triggers the bound pointer enter action when touch enters zone
		/// </summary>
		public void OnPointerEnter(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerDown (data);
			}
		}

		/// <summary>
		/// Triggers the bound pointer exit action when touch is out of zone
		/// </summary>
		public void OnPointerExit(PointerEventData data)
		{
			if (!MouseMode)
			{
				OnPointerUp(data);	
			}
		}
	}
}