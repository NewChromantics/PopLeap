using UnityEngine;
using System.Collections;
using System.Collections.Generic;




//	raw flat json class
public class LeapFrameJson
{
	public int		HandLeft_Id;
	public float	HandLeft_Confidence;
	public float	HandLeft_GrabStrength;
	public string	HandLeft_PalmNormal;
	public string	HandLeft_PalmPosition;

	public int		HandLeft_Finger0_Id;
	public int		HandLeft_Finger0_Type;
	public string	HandLeft_Finger0_Bone0_Position;
	public string	HandLeft_Finger0_Bone0_PositionTip;
	public string	HandLeft_Finger0_Bone0_Direction;
	public string	HandLeft_Finger0_Bone1_Position;
	public string	HandLeft_Finger0_Bone1_PositionTip;
	public string	HandLeft_Finger0_Bone1_Direction;
	public string	HandLeft_Finger0_Bone2_Position;
	public string	HandLeft_Finger0_Bone2_PositionTip;
	public string	HandLeft_Finger0_Bone2_Direction;
	public string	HandLeft_Finger0_Bone3_Position;
	public string	HandLeft_Finger0_Bone3_PositionTip;
	public string	HandLeft_Finger0_Bone3_Direction;

	public int		HandLeft_Finger1_Id;
	public int		HandLeft_Finger1_Type;
	public string	HandLeft_Finger1_Bone0_Position;
	public string	HandLeft_Finger1_Bone0_PositionTip;
	public string	HandLeft_Finger1_Bone0_Direction;
	public string	HandLeft_Finger1_Bone1_Position;
	public string	HandLeft_Finger1_Bone1_PositionTip;
	public string	HandLeft_Finger1_Bone1_Direction;
	public string	HandLeft_Finger1_Bone2_Position;
	public string	HandLeft_Finger1_Bone2_PositionTip;
	public string	HandLeft_Finger1_Bone2_Direction;
	public string	HandLeft_Finger1_Bone3_Position;
	public string	HandLeft_Finger1_Bone3_PositionTip;
	public string	HandLeft_Finger1_Bone3_Direction;

	public string	InteractionMinMax;	

	public string	Error;
	public string	Time;
	public string	TimeNow;
};



public class LeapFinger
{
	public int		Id;
	public int		Type;

	//	change these to matrixes using direction
	public Vector3	Position0;
	public Vector3	Position1;
	public Vector3	Position2;
	public Vector3	Position3;
	public Vector3	Position4;
};

public class LeapHand
{
	public Vector3			Position;
	public Vector3			Normal;
	public int				Id;
	public float			Confidence;
	public float			GrabStrength;

	public List<LeapFinger>	Fingers;

	static bool GetVariable<T>(ref T Out,string VariableName,LeapFrameJson Data)
	{
		var Property = Data.GetType ().GetProperty (VariableName);
		if (Property == null)
			return false;
		Out = (T)Property.GetValue (Data,null) ;
		return true;
	}

	static bool ParseVariable<T>(ref T Out,string VariableName,LeapFrameJson Data,System.Func<string,T>  Parser)
	{
		string Value = "";
		if (!GetVariable (ref Value, VariableName, Data))
			return false;
		try
		{
			Out = Parser (Value);
			return true;
		}
		catch(System.Exception ) {
			return false;
		}
	}

	public LeapHand(string HandPrefix, LeapFrameJson RawFrame)
	{
		bool AnyData = false;
		AnyData |= GetVariable (ref Id, HandPrefix + "_Id", RawFrame);
		AnyData |= GetVariable (ref Confidence, HandPrefix + "_Confidence", RawFrame);
		AnyData |= GetVariable (ref GrabStrength, HandPrefix + "_GrabStrength", RawFrame);
		AnyData |= ParseVariable (ref Position, HandPrefix + "_PalmPosition", RawFrame, FastParse.Vector3);
		AnyData |= ParseVariable (ref Normal, HandPrefix + "_PalmNormal", RawFrame, FastParse.Vector3);

		if (!AnyData)
			throw new System.Exception ("No hand data");
	}

};

public class LeapFrame
{
	public LeapHand	LeftHand;
	public LeapHand	RightHand;
	public string	Error;
	public Bounds	InteractionBounds;
	public int		Time;			//	timestamp of the frame
	public int		TimeNow;		//	timestamp of the packet

	public LeapFrame(string Json)
	{
		var FrameData = JsonUtility.FromJson<LeapFrameJson> (Json);
		Error = FrameData.Error;

		try
		{
			LeftHand = new LeapHand ("HandLeft", FrameData);
		}
		catch(System.Exception){}

		try
		{
			RightHand = new LeapHand ("HandRight", FrameData);
		}
		catch(System.Exception){}

		if ( FrameData.InteractionMinMax != null )
			InteractionBounds = FastParse.MinMaxToBounds (FrameData.InteractionMinMax);
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
		HandObject.transform.localPosition = HandData.Position;
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

		SetHand (LeftHand, Frame.LeftHand);
		SetHand (RightHand, Frame.RightHand);

		Debug.Log ("Frame time: " + Frame.Time + ", Now: " + Frame.TimeNow);
	}

	public void SetFrame(string JsonString)
	{
		Debug.Log ("json: " + JsonString);
		var Frame = new LeapFrame(JsonString);
		SetFrame (Frame);
	}

	public void OnError(string Error)
	{
		//	make error json
		string Json = "{ \"Error\":\"" + Error + "\" }";
		SetFrame (Json);
	}
}
