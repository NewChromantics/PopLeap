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

	public LeapFinger	GetFinger(int Index)
	{
		return (Fingers!=null && Fingers.Count>Index) ? Fingers[Index] : null;
	}

	private void	AddJsonFinger(string Json)
	{
		if (Json == null)
			return;
		var Finger = LeapFinger.ParseJson (Json);
		if (Finger != null) {
			if (Fingers == null)
				Fingers = new List<LeapFinger> ();
			Fingers.Add (Finger);
		}
	}

	static public LeapHand		ParseJson(string Json)
	{
		if (Json == null)
			return null;

		var Hand = JsonUtility.FromJson<LeapHand> (Json);
		Hand.AddJsonFinger (Hand.Finger0);
		Hand.AddJsonFinger (Hand.Finger1);
		Hand.AddJsonFinger (Hand.Finger2);
		Hand.AddJsonFinger (Hand.Finger3);
		Hand.AddJsonFinger (Hand.Finger4);

		return Hand;
	}
};

//	serialisable to mix with json
public class LeapFrame
{
	//	json strings
	public string	LeftHand;
	public string	RightHand;
	public string	InteractionMinMax;	

	public string	Error;
	public string	Time;
	public string	TimeNow;
	public List<LeapHand>	Hands;
	public Bounds	InteractionBounds;


	public LeapHand	GetHand(int Index)
	{
		return (Hands!=null && Hands.Count>Index) ? Hands[Index] : null;
	}

	private void	AddJsonHand(string Json)
	{
		if (Json == null)
			return;
		var Hand = LeapHand.ParseJson (Json);
		if (Hand != null) {
			if (Hands == null)
				Hands = new List<LeapHand> ();
			Hands.Add (Hand);
		}
	}


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
			if ( lh != null )	Frame.Hands.Add (lh);
			if ( rh != null )	Frame.Hands.Add (rh);

			if ( Frame.InteractionMinMax != null )
				Frame.InteractionBounds = FastParse.MinMaxToBounds( Frame.InteractionMinMax );
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

	public Transform			MinMaxBox;
	public Transform			LeftHand;
	public Transform			RightHand;

	void SetHand(Transform HandObject,LeapHand HandData)
	{
		if ( HandObject == null )
			return;

		if (HandData == null) {
			HandObject.gameObject.SetActive (false);
			return;
		}

		HandObject.gameObject.SetActive (true);
		HandObject.transform.localPosition = HandData.PalmPosition;
	}


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

		if (MinMaxBox != null) {
			MinMaxBox.transform.localPosition = Frame.InteractionBounds.center;
			MinMaxBox.transform.localScale = Frame.InteractionBounds.size;
		}

		SetHand (LeftHand, Frame.GetHand (0));
		SetHand (RightHand, Frame.GetHand (1));

		Debug.Log ("Frame time: " + Frame.Time + ", Now: " + Frame.TimeNow);
	}

	public void SetFrame(string JsonString)
	{
		Debug.Log ("json: " + JsonString);
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
