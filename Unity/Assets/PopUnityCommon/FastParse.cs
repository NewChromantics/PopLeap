using UnityEngine;
using System.Collections;

public class FastParse : MonoBehaviour {

	//	fast alterantive to parse.Floats
	//	needs more error checking
	static public float	Float(string FloatStr)
	{
		float Major = 0;
		float Minor = 0;
		int Pos = 0;
		float Modifier = 1.0f;

		if (FloatStr [0] == '-') {
			Modifier = -1.0f;
			Pos++;
		}

		//	parse major
		while (Pos < FloatStr.Length) {
			if (FloatStr [Pos] == '.')
			{
				Pos++;
				break;
			}
			Major *= 10;
			Major += FloatStr [Pos] - '0';
			Pos++;
		}
		
		//	parse minor
		float MinorScale = 1.0f / 10.0f;
		while (Pos < FloatStr.Length) {
			if ( FloatStr[Pos] == 'f' )
			{
				Pos++;
				continue;
			}
			Minor += (FloatStr [Pos] - '0') * MinorScale;
			MinorScale /= 10.0f;
			Pos++;
		}
		
		return Modifier * (Major + Minor);
	}

	static public Vector3 Vector3(string Vec3f)
	{
		char[] comma = {','};
		var xyz = Vec3f.Split( comma );
		if (xyz.Length != 3)
			throw new System.Exception ("MinMax string not valid - doesnt split by , to 3");

		return new Vector3 (Float (xyz [0]), Float (xyz [1]), Float (xyz [2]));
	}


	static public Bounds MinMaxToBounds(string MinMax)
	{
		char[] x = {'x'};
		var MinAndMax = MinMax.Split( x );
		if (MinAndMax.Length != 2)
			throw new System.Exception ("MinMax string not valid - doesnt split by x to 2");

		Bounds b = new Bounds ();
		b.SetMinMax( Vector3 (MinAndMax [0]), Vector3 (MinAndMax [1]));
		return b;
	}

}
