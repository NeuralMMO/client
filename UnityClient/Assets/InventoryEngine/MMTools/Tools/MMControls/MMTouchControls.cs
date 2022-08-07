using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace MoreMountains.Tools
{	
	[RequireComponent(typeof(CanvasGroup))]
	public class MMTouchControls : MonoBehaviour 
	{
		public enum InputForcedMode { None, Mobile, Desktop }
		[MMInformation("If you check Auto Mobile Detection, the engine will automatically switch to mobile controls when your build target is Android or iOS. You can also force mobile or desktop (keyboard, gamepad) controls using the dropdown below.\nNote that if you don't need mobile controls and/or GUI this component can also work on its own, just put it on an empty GameObject instead.", MMInformationAttribute.InformationType.Info,false)]
		/// If you check Auto Mobile Detection, the engine will automatically switch to mobile controls when your build target is Android or iOS. 
		/// You can also force mobile or desktop (keyboard, gamepad) controls using the dropdown below.Note that if you don't need mobile controls 
		/// and/or GUI this component can also work on its own, just put it on an empty GameObject instead.
		public bool AutoMobileDetection = true;
		/// Force desktop mode (gamepad, keyboard...) or mobile (touch controls) 
		public InputForcedMode ForcedMode;
		public bool IsMobile { get; protected set; }

		protected CanvasGroup _canvasGroup;
		protected float _initialMobileControlsAlpha;

		/// <summary>
	    /// We get the player from its tag.
	    /// </summary>
	    protected virtual void Start()
		{
			_canvasGroup = GetComponent<CanvasGroup>();

			_initialMobileControlsAlpha = _canvasGroup.alpha;
			SetMobileControlsActive(false);
			IsMobile=false;
			if (AutoMobileDetection)
			{
				#if UNITY_ANDROID || UNITY_IPHONE
					SetMobileControlsActive(true);
					IsMobile = true;
				 #endif
			}
			if (ForcedMode==InputForcedMode.Mobile)
			{
				SetMobileControlsActive(true);
				IsMobile = true;
			}
			if (ForcedMode==InputForcedMode.Desktop)
			{
				SetMobileControlsActive(false);
				IsMobile = false;		
			}
		}
		
		public virtual void SetMobileControlsActive(bool state)
		{
			if (_canvasGroup!=null)
			{
				_canvasGroup.gameObject.SetActive(state);
				if (state)
				{
					_canvasGroup.alpha=_initialMobileControlsAlpha;
				}
				else
				{
					_canvasGroup.alpha=0;
				}
			}
		}
	}
}