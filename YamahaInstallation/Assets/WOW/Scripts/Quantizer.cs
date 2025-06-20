using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quantizer : MonoBehaviour
{
	public static double Quantize16(double position, double durEighth)
	{
		return System.Math.Round(position / durEighth) * durEighth;
		//return System.Math.Floor((position - start) / q) * q + start;
	}
}