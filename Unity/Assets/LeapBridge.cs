using UnityEngine;
using System.Collections;


//	serialisable to mix with json
public class LeapFrame
{
	public string	Error;
	public string	Time;
	public string	TimeNow;



	//	some slightly cleaner accessors
	public bool	HasError()		{	return (Error!=null && Error.Length >0);	}
};


public class LeapBridge : MonoBehaviour {

	public UnityEngine.UI.Text	ErrorText;


	public void	SetFrame(LeapFrame Frame)
	{
		//	update objects
		if (ErrorText != null ) 
		{
			ErrorText.text = Frame.Error;
		}
		else if ( Frame.HasError() )
		{
			Debug.LogError ("Frame error: " + Frame.Error);
		}

		Debug.Log ("Frame time: " + Frame.Time + ", Now: " + Frame.TimeNow);
	}

	public void SetFrame(string JsonString)
	{
		var Frame = JsonUtility.FromJson<LeapFrame> (JsonString);
		SetFrame (Frame);
	}

	public void OnError(string Error)
	{
		//	make error json
		string Json = "{ \"Error\":\"" + Error + "\" }";
		SetFrame (Json);
	}
}
