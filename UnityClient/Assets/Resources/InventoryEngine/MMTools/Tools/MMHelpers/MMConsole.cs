using UnityEngine;
using System.Collections;

namespace MoreMountains.Tools
{	
	/// <summary>
	/// This class displays an on-screen console for easier debugging
	/// DO NOT ADD THIS CLASS AS A COMPONENT.
	/// Instead, use the MMDebug.DebugOnScreen methods that will take care of everything
	/// </summary>
	public class MMConsole : MonoBehaviour 
	{
		protected string _messageStack;

		protected int _numberOfMessages=0;
		protected bool _messageStackHasBeenDisplayed=false;
		protected int _largestMessageLength=0;

		protected int _marginTop = 10;
		protected int _marginLeft = 10;
		protected int _padding = 10;

		protected int _fontSize = 10;
		protected int _characterHeight = 16;
		protected int _characterWidth = 6;

		/// <summary>
		/// Draws a box containing the current stack of messages on top of the screen.
		/// </summary>
		protected virtual void OnGUI()
		{
			// we define the style to use and the font size
	        GUIStyle style = GUI.skin.GetStyle ("label");
			style.fontSize = _fontSize;

	        // we determine our box dimension based on the number of lines and the length of the longest line
			int boxHeight = _numberOfMessages*_characterHeight;
			int boxWidth = _largestMessageLength * _characterWidth;

			// we draw a box and the message on top of it
			GUI.Box (new Rect (_marginLeft,_marginTop,boxWidth+_padding*2,boxHeight+_padding*2), "");
			GUI.Label(new Rect(_marginLeft+_padding, _marginTop+_padding, boxWidth, boxHeight), _messageStack);

	        // we set our flag to true, which will trigger the reset of the stack next time it's accessed
			_messageStackHasBeenDisplayed=true;
		}

		/// <summary>
		/// Sets the size of the font, and automatically deduces the character's height and width.
		/// </summary>
		/// <param name="fontSize">Font size.</param>
		public virtual void SetFontSize(int fontSize)
		{
			_fontSize = fontSize;
			_characterHeight = (int)Mathf.Round(1.6f * fontSize + 0.49f);
			_characterWidth = (int)Mathf.Round(0.6f * fontSize + 0.49f);

		}

		/// <summary>
		/// Replaces the content of the current message stack with the specified string 
		/// </summary>
		/// <param name="newMessage">New message.</param>
		public virtual void SetMessage(string newMessage)
		{
			_messageStack=newMessage;
			_numberOfMessages=1;
		}

		/// <summary>
		/// Adds the specified message to the message stack.
		/// </summary>
		/// <param name="newMessage">New message.</param>
		public virtual void AddMessage(string newMessage)
		{
			// if the message stack has been displayed, we empty it and reset our counters
			if (_messageStackHasBeenDisplayed)
			{
				_messageStack="";
				_messageStackHasBeenDisplayed=false;
				_numberOfMessages=0;
				_largestMessageLength=0;
			}

			// we add the specified message to the stack
			_messageStack+=newMessage+"\n";
			// if this new message is longer than our previous longer message, we store it (this will expand the box's width
			if (newMessage.Length > _largestMessageLength)
			{
				_largestMessageLength = newMessage.Length;
			}
			// we increment our counter
			_numberOfMessages++;
		}
	}
}
