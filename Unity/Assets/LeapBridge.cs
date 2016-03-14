using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class LeapFinger
{
	//	Json string
	public string	Bones;
	
	public int		Id;
	public string	Type;


	static public LeapFinger		ParseJson(string Json)
	{
		if (Json == null)
			return null;

		var Finger = JsonUtility.FromJson<LeapFinger> (Json);
		return Finger;
	}
};

public class LeapHand
{	
	//	json strings
	public string	Finger0;
	public string	Finger1;
	public string	Finger2;
	public string	Finger3;
	public string	Finger4;

	public int		Id;
	public float	Confidence;
	public float	GrabStrength;
	public Vector3	PalmNormal;
	public Vector3	PalmPosition;
	public List<LeapFinger>	Fingers;


	static public LeapHand		ParseJson(string Json)
	{
		if (Json == null)
			return null;

		var Hand = JsonUtility.FromJson<LeapHand> (Json);

		var Finger0 = LeapFinger.ParseJson(Hand.Finger0);
		var Finger1 = LeapFinger.ParseJson(Hand.Finger1);
		var Finger2 = LeapFinger.ParseJson(Hand.Finger2);
		var Finger3 = LeapFinger.ParseJson(Hand.Finger3);
		var Finger4 = LeapFinger.ParseJson(Hand.Finger4);
		Hand.Fingers = new List<LeapFinger> ();
		Hand.Fingers.Add (Finger0);
		Hand.Fingers.Add (Finger1);
		Hand.Fingers.Add (Finger2);
		Hand.Fingers.Add (Finger3);
		Hand.Fingers.Add (Finger4);

		return Hand;
	}
};

//	serialisable to mix with json
public class LeapFrame
{
	public string	LeftHand;		//	json string
	public string	RightHand;		//	json string

	public string	Error;
	public string	Time;
	public string	TimeNow;
	public List<LeapHand>	Hands;

	static public LeapFrame		ParseJson(string Json)
	{
		if (Json == null)
			return null;

		var Frame = JsonUtility.FromJson<LeapFrame> (Json);

		try
		{
			var lh = LeapHand.ParseJson (Frame.LeftHand);
			var rh = LeapHand.ParseJson (Frame.RightHand);
			Frame.Hands = new List<LeapHand> ();
			Frame.Hands.Add (lh);
			Frame.Hands.Add (rh);
		}
		catch(System.Exception e) {
			if (Frame.Error==null)
				Frame.Error = "";
			Frame.Error += "exception parsing json: " + e.Message;
		}

		return Frame;
	}

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
		var Frame = LeapFrame.ParseJson(JsonString);
		SetFrame (Frame);
	}

	public void OnError(string Error)
	{
		//	make error json
		string Json = "{ \"Error\":\"" + Error + "\" }";
		SetFrame (Json);
	}
}
