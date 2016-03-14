using UnityEngine;
using System.Collections;




public class PopLeapHttp : MonoBehaviour {

	public UnityEvent_String	OnNewJson;
	public UnityEvent_String	OnError;
	public string				Url = "localhost:8080/leap";
	private WWW					www;

	void Update () 
	{
		if ( www == null )
		{
			StartCoroutine( Fetch() );
		}
	}


	IEnumerator Fetch()
	{
		string url = Url;

		//	already waiting for existing
		if (www != null)
			yield break;

		www = new WWW (url);
		yield return www;

		if (www.error != null) {
			OnError.Invoke("Error fetching PopLeap(" + url  + ")" + www.error );
			www = null;
			yield break;
		}

		//	send text to new frame
		OnNewJson.Invoke( www.text );
		www = null;
		yield break;
	}
}

