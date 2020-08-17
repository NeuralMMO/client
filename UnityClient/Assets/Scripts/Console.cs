using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour
{
   InputField prompt;
	Text log;
	public string cmd;
	GameObject console;
	GameObject container;
	ScrollRect scrollRect;
	public List<string> commands;
	string toggle;

   // Start is called before the first frame update
   void Start()
   {
      this.prompt     = GameObject.Find("Console/Container/Prompt").GetComponent<InputField>();
		this.console    = GameObject.Find("Console");
		this.container  = GameObject.Find("Console/Container");
		this.scrollRect = GameObject.Find("Console/Container/Scroll View").GetComponent<ScrollRect>();
		this.log        = GameObject.Find("Console/Container/Scroll View/Viewport/Content/Log").GetComponent<Text>();
		this.container.SetActive(false);
		this.toggle = "1";
		this.cmd = "\t";

		this.Activate();
	}

	void Activate()
	{
		this.container.SetActive(true);
		this.prompt.ActivateInputField();
		this.prompt.MoveTextEnd(false);
	}

	// Update is called once per frame
	void Update()
    {
		string text = this.prompt.text;
		//Enable on enter
		if (Input.GetKeyDown(KeyCode.Return))
		{
			if (!this.container.activeSelf)
			{
				this.Activate();
			}
			this.prompt.text = text.Replace(toggle, "");
		}
		//Toggle on back quote
 	    if (Input.GetKeyDown(KeyCode.Tab))
  	    {
			if (!this.container.activeSelf)
			{
      			this.Activate();
			} else
			{
				this.container.SetActive(false);
			}
			this.prompt.text = text.Replace(toggle, "");
      	}

		//Process text
		if (this.container.activeSelf)
		{
			if (Input.GetKey(KeyCode.Return))
			{
				this.prompt.text = "> ";
				if (text != "> ")
				{
					text = text.Replace("> ", "");
					this.cmd = text;
					this.log.text += "\n" + text;
				} else
				{
					//Pretty sure this is a hack relying on key bouncing
					this.scrollRect. verticalNormalizedPosition = 0f;
				}
				this.prompt.ActivateInputField();
				this.prompt.MoveTextEnd(false);
			}
		}
		this.prompt.ActivateInputField();
   }

   public bool validateCommand(string cmd)
   {
		if (cmd.Length == 0)
		{
			return false;
		}
      return true;
   }

	public string consumeCommand()
   {
		string command = this.cmd;
		this.cmd = "";
		return command;
   }
	
	void SubmitText(string txt)
	{
		Debug.Log(txt);
	}
}
