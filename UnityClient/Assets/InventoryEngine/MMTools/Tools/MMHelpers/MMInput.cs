using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// Input helpers
	/// </summary>

	public class MMInput : MonoBehaviour 
	{
		/// <summary>
		/// All possible states for a button. Can be used in a state machine.
		/// </summary>
		public enum ButtonStates { Off, ButtonDown, ButtonPressed, ButtonUp }

        public enum AxisTypes { Positive, Negative }

		/// <summary>
		/// Takes an axis and returns a ButtonState depending on whether the axis is pressed or not (useful for xbox triggers for example), and when you need to use an axis/trigger as a binary thing
		/// </summary>
		/// <returns>The axis as button.</returns>
		/// <param name="axisName">Axis name.</param>
		/// <param name="threshold">Threshold value below which the button is off or released.</param>
		/// <param name="currentState">Current state of the axis.</param>
		public static ButtonStates ProcessAxisAsButton (string axisName, float threshold, ButtonStates currentState, AxisTypes AxisType = AxisTypes.Positive)
		{
			float axisValue = Input.GetAxis (axisName);
			ButtonStates returnState;

            bool comparison = (AxisType == AxisTypes.Positive) ? (axisValue < threshold) : (axisValue > threshold);
			
			if (comparison)
			{
				if (currentState == ButtonStates.ButtonPressed)
				{
					returnState = ButtonStates.ButtonUp;
				}
				else
				{
					returnState = ButtonStates.Off;
				}
			}
			else
			{
				if (currentState == ButtonStates.Off)
				{
					returnState = ButtonStates.ButtonDown;
				}
				else
				{
					returnState = ButtonStates.ButtonPressed;
				}
			}
			return returnState;
		}

		/// <summary>
		/// IM button, short for 
		/// </summary>
		public class IMButton
		{
			public MMStateMachine<MMInput.ButtonStates> State {get;protected set;}
			public string ButtonID;

			public delegate void ButtonDownMethodDelegate();
			public delegate void ButtonPressedMethodDelegate();
			public delegate void ButtonUpMethodDelegate();

			public ButtonDownMethodDelegate ButtonDownMethod;
			public ButtonPressedMethodDelegate ButtonPressedMethod;
			public ButtonUpMethodDelegate ButtonUpMethod;

            public IMButton(string playerID, string buttonID, ButtonDownMethodDelegate btnDown = null, ButtonPressedMethodDelegate btnPressed = null, ButtonUpMethodDelegate btnUp = null) 
			{
				ButtonID = playerID + "_" + buttonID;
				ButtonDownMethod = btnDown;
				ButtonUpMethod = btnUp;
				ButtonPressedMethod = btnPressed;
				State = new MMStateMachine<MMInput.ButtonStates> (null, false);
				State.ChangeState (MMInput.ButtonStates.Off);
			}

			public virtual void TriggerButtonDown()
			{
                if (ButtonDownMethod == null)
                {
                    State.ChangeState(MMInput.ButtonStates.ButtonDown);
                }
                else
                {
                    ButtonDownMethod();
                }
			}

			public virtual void TriggerButtonPressed()
			{
                if (ButtonPressedMethod == null)
                {
                    State.ChangeState(MMInput.ButtonStates.ButtonPressed);
                }
                else
                {
                    ButtonPressedMethod();
                }
			}

			public virtual void TriggerButtonUp()
            {
                if (ButtonUpMethod == null)
                {
                    State.ChangeState(MMInput.ButtonStates.ButtonUp);
                }
                else
                {
                    ButtonUpMethod();
                }
			}
		}
	}


}
