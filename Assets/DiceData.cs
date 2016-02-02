using UnityEngine;
using System.Collections;

public struct DiceData {

	private readonly int mValue;
	private readonly bool mDouble;
	public int number {
		get{
			return mValue;
		}
	}
	public bool isDouble{
		get{
			return mDouble;
		}
	}
	
	public DiceData (int val, bool dbl){
		mValue = val;
		mDouble = dbl;
	}
}
